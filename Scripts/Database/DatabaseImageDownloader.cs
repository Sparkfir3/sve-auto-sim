using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Linq;
using Sirenix.OdinInspector;

namespace SVESimulator.Database.Scraper
{
    public class DatabaseImageDownloader : MonoBehaviour
    {
        #region Variables

        private const string BASE_IMAGE_URL = "https://cdn.dingdongdb.me/images/{0}/{1}/{2}.avif";

        [TitleGroup("Runtime Data"), SerializeField, ReadOnly]
        private int currentDownloadingCount;
        [SerializeField, ReadOnly]
        private int totalDownloadedCount;
        [SerializeField, ReadOnly]
        private int targetDownloadCount;

        [TitleGroup("Settings"), SerializeField, TableList]
        private List<SetDownloadSettings> downloadSettings;
        [SerializeField, LabelText("Card Back URL")]
        private string cardBackURL = "";
        [SerializeField, LabelText("Evolve Point URL")]
        private string evolvePointURL = "https://tcgplayer-cdn.tcgplayer.com/product/509622_in_1000x1000.jpg";
        [SerializeField]
        private int maxConcurrentDownloads = 5;
        [SerializeField]
        private float delayBetweenDownloads = 0.1f;

        [SerializeField, InlineEditor]
        private FilePathInfo pathInfo;

        private List<SetDownloadSettings> runtimeDownloadSettings = new();

        public event Action<int, int> OnDownloadProgressUpdate; // Current downloaded count, total download target

        #endregion

        // ------------------------------

        #region Controls

        public void DownloadAllImages(Action onComplete = null)
        {
            if(!Application.isPlaying)
                return;
            CalculateDownloadSize();
            StartCoroutine(DownloadAllCards(onComplete));
        }

        #endregion

        // ------------------------------

        #region Download Handling

        private void CalculateDownloadSize()
        {
            foreach(SetDownloadSettings settings in downloadSettings)
            {
                string folderPath = Path.Combine(pathInfo.ImagesPath, settings.setCode);
                if(!Directory.Exists(folderPath))
                {
                    runtimeDownloadSettings.Add(settings);
                    continue;
                }

                int i = settings.startIndex;
                for(; i < settings.endIndex; i++)
                {
                    string fileName = $"{settings.setCode}-{settings.GetCardIdNumberWithLanguage(i)}.png";
                    if(!File.Exists(Path.Combine(folderPath, fileName)))
                        break;
                }
                if(i >= settings.endIndex)
                    continue;

                runtimeDownloadSettings.Add(new SetDownloadSettings()
                {
                    setCode = settings.setCode,
                    cardType = settings.cardType,
                    startIndex = i,
                    endIndex = settings.endIndex,
                    language = settings.language
                });
            }

            targetDownloadCount = runtimeDownloadSettings.Sum(x => x.endIndex - x.startIndex + 1);
            if(!string.IsNullOrWhiteSpace(cardBackURL) && !File.Exists(Path.Join(pathInfo.MiscImagesPath, pathInfo.CardBackFile)))
                targetDownloadCount++;
            if(!File.Exists(Path.Join(pathInfo.MiscImagesPath, pathInfo.EvolvePointFile)))
                targetDownloadCount++;
        }

        private IEnumerator DownloadAllCards(Action onComplete = null)
        {
            // Create root data folders
            Directory.CreateDirectory(pathInfo.ImagesPath);
            Directory.CreateDirectory(pathInfo.MiscImagesPath);

            // Download card back & evolve point
            string cardBackFilePath = Path.Join(pathInfo.MiscImagesPath, pathInfo.CardBackFile);
            if(!string.IsNullOrWhiteSpace(cardBackURL) && !File.Exists(cardBackFilePath))
            {
                StartCoroutine(DownloadImage(cardBackURL, cardBackFilePath));
                yield return new WaitForSeconds(delayBetweenDownloads);
            }
            string evolvePointFilePath = Path.Join(pathInfo.MiscImagesPath, pathInfo.EvolvePointFile);
            if(!File.Exists(evolvePointFilePath))
            {
                StartCoroutine(DownloadImage(evolvePointURL, evolvePointFilePath));
                yield return new WaitForSeconds(delayBetweenDownloads);
            }
            yield return new WaitUntil(() => currentDownloadingCount < maxConcurrentDownloads);

            // Download sets
            foreach(SetDownloadSettings settings in runtimeDownloadSettings)
            {
                for(int i = settings.startIndex; i <= settings.endIndex; i++)
                {
                    DownloadSetCard(pathInfo.ImagesPath, settings.setCode, settings.GetCardIdNumberWithLanguage(i), settings.language);
                    yield return new WaitForSeconds(delayBetweenDownloads);
                    if(currentDownloadingCount >= maxConcurrentDownloads)
                        yield return new WaitUntil(() => currentDownloadingCount < maxConcurrentDownloads);
                }
            }
            onComplete?.Invoke();
        }

        #endregion

        // ------------------------------

        #region Web Request & File Handling

        private void DownloadSetCard(string folderPath, string setCode, string cardId, SetDownloadSettings.CardLanguage language)
        {
            string outputFolderPath = Path.Combine(folderPath, setCode);
            if(!Directory.Exists(outputFolderPath))
                Directory.CreateDirectory(outputFolderPath);
            string outputFilePath = Path.Combine(outputFolderPath, $"{setCode}-{cardId}.png");
            if(File.Exists(outputFilePath))
                return;

            string downloadURL = string.Format(BASE_IMAGE_URL, language == SetDownloadSettings.CardLanguage.EN ? "en" : "jp", setCode, $"{setCode}-{cardId}");
            StartCoroutine(DownloadImage(downloadURL, outputFilePath));
        }

        private IEnumerator DownloadImage(string url, string outputFilePath)
        {
            currentDownloadingCount++;
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
            yield return request.SendWebRequest();

            if(request.result is not UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error downloading image to {outputFilePath}: {request.error}\nURL: {url}");
                currentDownloadingCount--;
                yield break;
            }

            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            SaveTextureToDisk(texture, outputFilePath);
            currentDownloadingCount--;
            totalDownloadedCount++;
            OnDownloadProgressUpdate?.Invoke(totalDownloadedCount, targetDownloadCount);
        }

        private void SaveTextureToDisk(Texture2D texture, string filePath)
        {
            byte[] bytes = texture.EncodeToPNG();
            File.WriteAllBytes(filePath, bytes);
        }

        #endregion
    }
}
