using System;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CCGKit;
using FullSerializer;
using Sparkfire.Utility;
using SVESimulator.CardTextData;
using SVESimulator.Database.ImageDownloader;
using SVESimulator.SveScript;

namespace SVESimulator.Database
{
    public class CustomSetController : MonoBehaviour
    {
        #region Variables

        private const string RESOURCE_PATH_INFO = "FilePathInfo";

        public static bool UseCustomSets = false;
        private static List<string> CustomSetFolderNames;
        private static FilePathInfo PathInfo;

        [ShowInInspector, ReadOnly]
        private bool useCustomSets => UseCustomSets;
        [SerializeField]
        private string playerPrefsKey = "UseCustomSets";
        [SerializeField, InlineEditor]
        private FilePathInfo pathInfo;

        #endregion

        // ------------------------------

        #region Initialization & Controls

        public void Initialize()
        {
            CreateTemplateFolder();
            if(PlayerPrefs.HasKey(playerPrefsKey))
                UseCustomSets = PlayerPrefs.GetInt(playerPrefsKey) == 1;
            else
                SetCustomSetsEnabled(false);
            if(pathInfo)
                PathInfo = pathInfo;
        }

        public void SetCustomSetsEnabled(bool customSetsEnabled)
        {
            UseCustomSets = customSetsEnabled;
            PlayerPrefs.SetInt(playerPrefsKey, customSetsEnabled ? 1 : 0);
            PlayerPrefs.Save();
        }

        #endregion

        // ------------------------------

        #region Data Loading

        [Button]
        public void CompileCustomSets()
        {
            SveScriptDebugger.Reset();
            GetCustomSetFolderNames();
            foreach(string folder in CustomSetFolderNames)
            {
                if(folder.Equals(pathInfo.CustomSetTemplateFolder))
                    continue;

                string mainFolder = Path.Combine(pathInfo.CustomSetFolderPath, folder);
                string dataFolder = Path.Combine(mainFolder, "CardData");
                Directory.CreateDirectory(dataFolder);

                string scriptsFolder = Path.Combine(mainFolder, pathInfo.CustomSetScriptsFolder);
                IEnumerable<string> scriptFiles = Directory.EnumerateFiles(scriptsFolder, "*.txt", SearchOption.TopDirectoryOnly);
                int cardCount = 0;
                foreach(string script in scriptFiles)
                {
                    try
                    {
                        string scriptPath = Path.Combine(scriptsFolder, script);
                        SveScriptCompiler.ParseSveScriptCardSet(File.ReadAllText(scriptPath), dataFolder, out List<string> outputFilePaths, sortIntoSubfolders: false);
                        cardCount += outputFilePaths?.Count ?? 0;
                        string textDataPath = Path.Combine(mainFolder, pathInfo.CustomSetTextDataFolder, Path.GetFileNameWithoutExtension(script) + "_TextData.json");
                        if(!File.Exists(textDataPath))
                            using(FileStream _ = File.Create(textDataPath)) { }
                        CardTextCompiler.CompileTextJson(textDataPath, outputFilePaths);
                    }
                    catch(System.Exception e)
                    {
                        SveScriptDebugger.LogError($"Error compiling script {script} in custom set {folder}:\n{e.Message}");
                    }
                }

                if(SveScriptDebugger.HasErrors())
                    break;

                SveScriptCompiler.SaveSetFile(dataFolder, Path.Combine(mainFolder, "FullSet.json"), $"Custom Set - {folder}");
                SveScriptDebugger.Log($"Successfully Finished Compiling Custom Set - {folder}\nTotal Cards Compiled: {cardCount}");
            }
        }

        public static void LoadCustomSets(GameConfiguration gameConfig)
        {
            if(!UseCustomSets)
                Debug.LogError("Loading customs sets while custom sets are disabled!");

            GetCustomSetFolderNames();
            foreach(string folder in CustomSetFolderNames)
            {
                if(folder.Equals(PathInfo.CustomSetTemplateFolder))
                    continue;

                string dataFile = Path.Combine(PathInfo.CustomSetFolderPath, folder, "FullSet.json");
                if(!File.Exists(dataFile))
                {
                    Debug.LogError($"Custom set {folder} does not contain a FullSet.json file, skipping load");
                    continue;
                }

                fsData setData = fsJsonParser.Parse(File.ReadAllText(dataFile));
                if(!setData.IsList)
                    setData = new fsData(new List<fsData> { setData });
                object deserialized = null;
                new fsSerializer().TryDeserialize(setData, typeof(List<CardSet>), ref deserialized).AssertSuccessWithoutWarnings();
                List<CardSet> setLibrary = deserialized as List<CardSet>;
                Debug.Assert(setLibrary != null, $"Failed to deserialize card set library for custom set {folder}");
                gameConfig.cardSets.AddRange(setLibrary);
                foreach(CardSet set in setLibrary)
                    gameConfig.cards.AddRange(set.cards);
            }

            // Text data & images
            LibraryCardCache.CacheCustomSetTextDataList(GetTextDataPaths(CustomSetFolderNames, PathInfo));
            foreach(string folder in GetCustomSetFolderPaths())
                CardTextureManager.CacheCustomCardTextureFolderPath(Path.Combine(folder, PathInfo.CustomSetImagesFolder));
        }

        public static List<string> GetTextDataPaths(List<string> folders = null, FilePathInfo pathInfo = null)
        {
            if(!pathInfo)
                pathInfo = PathInfo;
            folders ??= GetCustomSetFolderNames();

            List<string> paths = new();
            foreach(string folder in folders)
            {
                if(folder.Equals(pathInfo.CustomSetTemplateFolder))
                    continue;

                string textDataFolder = Path.Combine(pathInfo.CustomSetFolderPath, folder, pathInfo.CustomSetTextDataFolder);
                IEnumerable<string> textDataFiles = Directory.EnumerateFiles(textDataFolder, "*.json", SearchOption.TopDirectoryOnly);
                paths.AddRange(textDataFiles);
            }
            return paths;
        }

        public void DownloadCustomSetImages(Action onComplete = null)
        {
            DatabaseImageDownloader imageDownloader = FindObjectOfType<DatabaseImageDownloader>();
            if(!imageDownloader)
            {
                Debug.LogError("DatabaseImageDownloader was not found, cannot download custom set images");
                onComplete?.Invoke();
                return;
            }
            StartCoroutine(Download());
            IEnumerator Download()
            {
                GetCustomSetFolderNames();
                foreach(string folder in CustomSetFolderNames)
                {
                    if(folder.Equals(pathInfo.CustomSetTemplateFolder))
                        continue;
                    string folderPath = Path.Combine(pathInfo.CustomSetFolderPath, folder);
                    string downloadSettingsFile = Path.Combine(folderPath, pathInfo.CustomSetDownloadSettingsFileName);
                    if(!File.Exists(downloadSettingsFile))
                        continue;
                    string downloadSettings = File.ReadAllText(downloadSettingsFile);
                    if(downloadSettings.IsNullOrWhiteSpace())
                        continue;

                    bool waiting = true;
                    imageDownloader.DownloadCustomSetImages(downloadSettings, Path.Combine(folderPath, pathInfo.CustomSetImagesFolder), () => waiting = false);
                    yield return new WaitUntil(() => !waiting);
                }
                onComplete?.Invoke();
            }
        }

        #endregion

        // ------------------------------

        #region Get Info

        public static List<string> GetCustomSetFolderNames()
        {
            CachePathInfo();
            CustomSetFolderNames = Directory.EnumerateDirectories(PathInfo.CustomSetFolderPath).Select(x => Path.GetRelativePath(PathInfo.CustomSetFolderPath, x)).ToList();
            CustomSetFolderNames.Remove(PathInfo.CustomSetTemplateFolder);
            return CustomSetFolderNames;
        }

        public static List<string> GetCustomSetFolderPaths()
        {
            GetCustomSetFolderNames();
            return CustomSetFolderNames.Select(x => Path.Combine(PathInfo.CustomSetFolderPath, x)).ToList();
        }

        #endregion

        // ------------------------------

        #region File/Data Management

        private void CreateTemplateFolder()
        {
            Directory.CreateDirectory(pathInfo.CustomSetFolderPath);
            string templateFolder = Path.Combine(pathInfo.CustomSetFolderPath, pathInfo.CustomSetTemplateFolder);
            Directory.CreateDirectory(templateFolder);

            string scriptsFolder = Path.Combine(templateFolder, pathInfo.CustomSetScriptsFolder);
            Directory.CreateDirectory(scriptsFolder);
            if(!File.Exists(Path.Combine(scriptsFolder, "TemplateScript.txt")))
                File.Create(Path.Combine(scriptsFolder, "TemplateScript.txt"));
            string textDataFolder = Path.Combine(templateFolder, pathInfo.CustomSetTextDataFolder);
            Directory.CreateDirectory(textDataFolder);
            string imagesFolder = Path.Combine(templateFolder, pathInfo.CustomSetImagesFolder);
            Directory.CreateDirectory(imagesFolder);
            File.Create(Path.Combine(templateFolder, pathInfo.CustomSetDownloadSettingsFileName));
        }

        private static void CachePathInfo()
        {
            if(!PathInfo)
                PathInfo = Resources.Load<FilePathInfo>(RESOURCE_PATH_INFO);
        }

        #endregion
    }
}
