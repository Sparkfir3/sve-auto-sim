using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SVESimulator.SveScript;

namespace SVESimulator.Database
{
    public class CustomSetController : MonoBehaviour
    {
        public static bool UseCustomSets = false;

        [ShowInInspector, ReadOnly]
        private bool useCustomSets => UseCustomSets;
        [SerializeField]
        private string playerPrefsKey = "UseCustomSets";
        [SerializeField, InlineEditor]
        private FilePathInfo pathInfo;

        // ------------------------------

        public void Initialize()
        {
            CreateTemplateFolder();
            if(PlayerPrefs.HasKey(playerPrefsKey))
                UseCustomSets = PlayerPrefs.GetInt(playerPrefsKey) == 1;
            else
                SetCustomSetsEnabled(false);
        }

        public void SetCustomSetsEnabled(bool customSetsEnabled)
        {
            UseCustomSets = customSetsEnabled;
            PlayerPrefs.SetInt(playerPrefsKey, customSetsEnabled ? 1 : 0);
            PlayerPrefs.Save();
        }

        [Button]
        public void CompileCustomSets()
        {
            SveScriptDebugger.Reset();
            IEnumerable<string> folders = Directory.EnumerateDirectories(pathInfo.CustomSetFolderPath).Select(x => Path.GetRelativePath(pathInfo.CustomSetFolderPath, x));
            foreach(string folder in folders)
            {
                if(folder.Equals(pathInfo.CustomSetTemplateFolder))
                    continue;

                string mainFolder = Path.Combine(pathInfo.CustomSetFolderPath, folder);
                string dataFolder = Path.Combine(mainFolder, "CardData");
                Directory.CreateDirectory(dataFolder);

                string scriptsFolder = Path.Combine(mainFolder, pathInfo.CustomSetScriptsFolder);
                IEnumerable<string> scriptFiles = Directory.EnumerateFiles(scriptsFolder, "*.txt", SearchOption.TopDirectoryOnly);
                foreach(string script in scriptFiles)
                {
                    try
                    {
                        SveScriptCompiler.ParseSveScriptCardSet(File.ReadAllText(Path.Combine(scriptsFolder, script)), dataFolder, out List<string> outputFilePaths, sortIntoSubfolders: false);
                        // TODO - generate text data files
                    }
                    catch(System.Exception e)
                    {
                        SveScriptDebugger.LogError($"Error compiling script {script} in custom set {folder}: {e.Message}");
                    }
                }

                if(SveScriptDebugger.HasErrors())
                    break;

                SveScriptCompiler.SaveSetFile(dataFolder, Path.Combine(mainFolder, "FullSet.json"), $"Custom Set - {folder}");
                SveScriptDebugger.Log($"Finished Compiling Custom Set {folder}");
            }
        }

        public void LoadCustomSets()
        {
            // TODO - load to ccg kit runtime library
        }

        // ------------------------------

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
            File.Create(Path.Combine(templateFolder, pathInfo.CustomSetDownloadSettingsFileName));
        }
    }
}
