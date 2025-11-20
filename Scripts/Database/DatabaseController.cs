using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SVESimulator.Database.Scraper;
using SVESimulator.SveScript;
using SVESimulator.UI;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SVESimulator.Database
{
    [DefaultExecutionOrder(-1000)]
    public class DatabaseController : MonoBehaviour
    {
        #region Variables/Classes

        [field: ShowInInspector, ReadOnly, HideInEditorMode]
        public static bool HasRunThisSession { get; private set; }
        // -----

        [field: TitleGroup("Settings"), SerializeField]
        public string DatabaseVersion { get; private set; }
        [SerializeField, LabelText("SVE Script Resources path")]
        private string sveScriptResourcesPath = "SveScripts";
        [HorizontalGroup("Run"), LabelWidth(175f), SerializeField]
        private bool checkToRunDownloader = true;
        [HorizontalGroup("Run"), LabelWidth(175f), SerializeField]
        private bool runInEditor;
        [HorizontalGroup("Generate"), LabelWidth(175f), SerializeField]
        private bool shouldGenerateLibrary = true;
        [HorizontalGroup("Generate"), LabelWidth(175f), SerializeField]
        private bool generateLibraryInBuild;

        [TitleGroup("Object References"), SerializeField]
        private DatabaseImageDownloader imageDownloader;
        [SerializeField, InlineEditor]
        private FilePathInfo paths;
        [SerializeField]
        private TextAsset cardLibrary;
        [BoxGroup("UI"), SerializeField]
        private GameObject loadingOverlay;
        [BoxGroup("UI"), SerializeField]
        private LoadingBar loadingBar;
        [BoxGroup("UI"), SerializeField]
        private TextMeshProUGUI infoText;

        #endregion

        // ------------------------------

        #region Unity Functions

        private void Awake()
        {
            if(HasRunThisSession || !checkToRunDownloader || (!runInEditor && Application.isEditor))
            {
                DisableOverlay();
                HasRunThisSession = true;
                return;
            }
            StartCoroutine(RunScraper());
        }

        #endregion

        // ------------------------------

        #region Database Download & Processing

        private IEnumerator RunScraper()
        {
            // if(File.Exists(paths.DatabaseVersionFilePath) && File.ReadAllText(paths.DatabaseVersionFilePath).Equals(DatabaseVersion))
            // {
            //     HasRunThisSession = true;
            //     yield break;
            // }

            EnableOverlay();

            bool downloading = true;
            imageDownloader.OnDownloadProgressUpdate += SetLoadingPercent;
            imageDownloader.DownloadAllImages(() => downloading = false);
            yield return new WaitUntil(() => !downloading);

            if(shouldGenerateLibrary && (Application.isEditor || generateLibraryInBuild))
            {
                GenerateLibrary();
                yield return null;
            }
            SaveDatabaseVersion();
            yield return null;

            DisableOverlay();
            HasRunThisSession = true;
        }

        [TitleGroup("Buttons"), Button]
        private void GenerateLibrary()
        {
            TextAsset[] sveScripts = Resources.LoadAll<TextAsset>(sveScriptResourcesPath);
            Debug.Log($"Loaded {sveScripts.Length} SVE scripts");
            foreach(TextAsset script in sveScripts)
            {
                SveScriptCompiler.ParseSveScriptCardSet(script.text, paths.CardDataPath);
            }
#if UNITY_EDITOR
            SveScriptCompiler.GenerateAndSaveFullCardLibrary(paths, Path.GetFullPath(Path.Join(Application.dataPath.Replace("/Assets", ""), AssetDatabase.GetAssetPath(cardLibrary))));
#else
            SveScriptCompiler.GenerateAndSaveFullCardLibrary(paths, Path.Combine(paths.MainFolderPath, "card_library.json"));
#endif
        }

        private void SaveDatabaseVersion()
        {
            File.WriteAllText(paths.DatabaseVersionFilePath, DatabaseVersion);
        }

        #endregion

        // ------------------------------

        #region UI Controls

        private void EnableOverlay()
        {
            loadingOverlay.SetActive(true);
        }

        private void DisableOverlay()
        {
            loadingOverlay.SetActive(false);
        }

        private void SetLoadingPercent(int progress, int target)
        {
            float percent = (float)progress / target;
            loadingBar.SetPercent(percent);
            if(percent > 1f || Mathf.Approximately(percent, 1f))
                SetInfoText("Finalizing...");
        }

        public void SetInfoText(string text)
        {
            infoText.text = text;
        }

        #endregion
    }
}
