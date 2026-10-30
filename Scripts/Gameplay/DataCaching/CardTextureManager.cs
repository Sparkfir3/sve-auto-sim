using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Sparkfire.Utility;

namespace SVESimulator
{
    public static class CardTextureManager
    {
        private const string RESOURCE_PATH_INFO = "FilePathInfo";
        private const string RESOURCE_MISSING_TEXTURE = "Textures/MissingTexture";

        private static FilePathInfo pathInfo;
        private static Texture missingTexture;
        private static Texture cardBackTexture;
        private static Dictionary<string, Texture> cardTextureCache = new();
        private static List<string> customCardTextureFolderPaths = new();

        // ------------------------------

        #region Get Textures

        public static Texture GetCardBackTexture()
        {
            if(cardBackTexture)
                return cardBackTexture;
            ValidatePathInfo();

            // temp
            cardBackTexture = GetMissingCardTexture(); // LoadTextureFromFile(Path.Combine(pathInfo.MiscImagesPath, pathInfo.CardBackFile));
            return cardBackTexture;
        }

        public static Texture GetCardTexture(string cardId)
        {
            if(cardId.IsNullOrWhiteSpace())
                return GetMissingCardTexture();
            if(cardTextureCache.TryGetValue(cardId, out Texture texture))
                return texture;
            ValidatePathInfo();

            string filePath = GetTextureFilePath(cardId);
            texture = LoadTextureFromFile(filePath);
            CacheCardTexture(cardId, texture);
            return texture;
        }

        public static Texture GetMissingCardTexture()
        {
            if(missingTexture)
                return missingTexture;

            missingTexture = Resources.Load<Texture>(RESOURCE_MISSING_TEXTURE);
            return missingTexture;
        }

        #endregion

        // ------------------------------

        #region Cache Controls

        public static void CacheCardTexture(string cardId, Texture texture)
        {
            cardTextureCache.Add(cardId, texture);
        }

        public static void CacheCustomCardTextureFolderPath(string path)
        {
            if(!customCardTextureFolderPaths.Contains(path))
                customCardTextureFolderPaths.Add(path);
        }

        public static void ClearCache()
        {
            foreach(Texture texture in cardTextureCache.Values)
                Object.Destroy(texture);
            cardTextureCache.Clear();
            customCardTextureFolderPaths.Clear();
        }

        #endregion

        // ------------------------------

        #region Loading & Handling

        private static void ValidatePathInfo()
        {
            if(!pathInfo)
                pathInfo = Resources.Load<FilePathInfo>(RESOURCE_PATH_INFO);
#if UNITY_EDITOR
            Debug.Assert(pathInfo);
#endif
        }

        private static string GetTextureFilePath(in string cardId)
        {
            string filePath = Path.Combine(Path.Combine(pathInfo.BuildFolderImagesPath, GetSubfolderFromID(cardId), $"{cardId}{pathInfo.DefaultFileExtension}")); // check main data folder
            if(!File.Exists(filePath)) // check build folder
                filePath = Path.Combine(pathInfo.ImagesPath, GetSubfolderFromID(cardId), $"{cardId}{pathInfo.DefaultFileExtension}");
            if(!File.Exists(filePath) && customCardTextureFolderPaths is { Count: > 0 }) // check custom images
            {
                foreach(string folderPath in customCardTextureFolderPaths)
                {
                    string customFilePath = Path.Combine(folderPath, $"{cardId}{pathInfo.DefaultFileExtension}");
                    if(File.Exists(customFilePath))
                    {
                        filePath = customFilePath;
                        break;
                    }
                }
            }
            return filePath;
        }

        private static Texture LoadTextureFromFile(string filePath)
        {
            filePath = Path.GetFullPath(filePath); // validates forward and backslashes
            if(!File.Exists(filePath))
            {
                Debug.LogError($"Missing texture at {filePath}");
                return GetMissingCardTexture();
            }

            Texture2D texture = new(2, 2); // width/height doesn't matter, gets overridden immediately
            byte[] fileData = File.ReadAllBytes(filePath);
            texture.LoadImage(fileData);
            return texture;
        }

        private static string GetSubfolderFromID(string id)
        {
            return !string.IsNullOrWhiteSpace(id) ? id.Split('-')[0] : null;
        }

        #endregion
    }
}
