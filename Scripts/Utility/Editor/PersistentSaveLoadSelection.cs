using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Sparkfire.Utility
{
    public static class PersistentSaveLoadSelection
    {
        private enum PersistentSelectionType { None = 0, Hierarchy = 1, Asset = 2 }

        private const string SelectionDefaultPath = "Main Menu/Edit/Selection/";
        private const string SelectionPersistentPath = SelectionDefaultPath + "Persistent/";
        private const string SelectionLoad = "Load Selection ";
        private const string SelectionSave = "Save Selection ";
        
        private const string ShortcutPathLoad = "Edit/Selection/Persistent/Load Persistent Selection ";
        private const string ShortcutPathSave = "Edit/Selection/Persistent/Save Persistent Selection ";

        // ------------------------------

        #region Initialize

        [MenuItem("Edit/Selection/Persistent/Initialize Persistent Selection Shortcuts", priority = -120)]
        private static void InitializePersistentSelectionShortcuts()
        {
            for(int i = 0; i <= 9; i++)
            {
                string loadPath = $"{SelectionDefaultPath}{SelectionLoad}{i}";
                ShortcutBinding binding = ShortcutManager.instance.GetShortcutBinding(loadPath);
                ShortcutManager.instance.RebindShortcut($"{SelectionPersistentPath}Load Persistent Selection {i}", binding);
                ShortcutManager.instance.RebindShortcut(loadPath, new ShortcutBinding());

                string savePath = $"{SelectionDefaultPath}{SelectionSave}{i}";
                binding = ShortcutManager.instance.GetShortcutBinding(savePath);
                ShortcutManager.instance.RebindShortcut($"{SelectionPersistentPath}Save Persistent Selection {i}", binding);
                ShortcutManager.instance.RebindShortcut(savePath, new ShortcutBinding());
            }
        }

        [MenuItem("Edit/Selection/Persistent/Reset To Defaults", priority = -20)]
        private static void ResetShortcuts()
        {
            foreach(string shortcut in ShortcutManager.instance.GetAvailableShortcutIds())
            {
                if(shortcut.StartsWith("Main Menu/Edit/Selection/"))
                {
                    ShortcutManager.instance.ClearShortcutOverride(shortcut);
                }
            }
        }

        #endregion

        // ------------------------------

        #region Load

        [MenuItem(ShortcutPathLoad + "1", priority = -99)]
        private static void LoadSelection1() => LoadSelection(1);
        [MenuItem(ShortcutPathLoad + "2", priority = -98)]
        private static void LoadSelection2() => LoadSelection(2);
        [MenuItem(ShortcutPathLoad + "3", priority = -97)]
        private static void LoadSelection3() => LoadSelection(3);
        [MenuItem(ShortcutPathLoad + "4", priority = -96)]
        private static void LoadSelection4() => LoadSelection(4);
        [MenuItem(ShortcutPathLoad + "5", priority = -95)]
        private static void LoadSelection5() => LoadSelection(5);
        [MenuItem(ShortcutPathLoad + "6", priority = -94)]
        private static void LoadSelection6() => LoadSelection(6);
        [MenuItem(ShortcutPathLoad + "7", priority = -93)]
        private static void LoadSelection7() => LoadSelection(7);
        [MenuItem(ShortcutPathLoad + "8", priority = -92)]
        private static void LoadSelection8() => LoadSelection(8);
        [MenuItem(ShortcutPathLoad + "9", priority = -91)]
        private static void LoadSelection9() => LoadSelection(9);
        [MenuItem(ShortcutPathLoad + "0", priority = -90)]
        private static void LoadSelection0() => LoadSelection(0);

        [MenuItem(ShortcutPathLoad + "1", validate = true)]
        private static bool ValidateLoadSelection1() => ValidateLoadSelection(1);
        [MenuItem(ShortcutPathLoad + "2", validate = true)]
        private static bool ValidateLoadSelection2() => ValidateLoadSelection(2);
        [MenuItem(ShortcutPathLoad + "3", validate = true)]
        private static bool ValidateLoadSelection3() => ValidateLoadSelection(3);
        [MenuItem(ShortcutPathLoad + "4", validate = true)]
        private static bool ValidateLoadSelection4() => ValidateLoadSelection(4);
        [MenuItem(ShortcutPathLoad + "5", validate = true)]
        private static bool ValidateLoadSelection5() => ValidateLoadSelection(5);
        [MenuItem(ShortcutPathLoad + "6", validate = true)]
        private static bool ValidateLoadSelection6() => ValidateLoadSelection(6);
        [MenuItem(ShortcutPathLoad + "7", validate = true)]
        private static bool ValidateLoadSelection7() => ValidateLoadSelection(7);
        [MenuItem(ShortcutPathLoad + "8", validate = true)]
        private static bool ValidateLoadSelection8() => ValidateLoadSelection(8);
        [MenuItem(ShortcutPathLoad + "9", validate = true)]
        private static bool ValidateLoadSelection9() => ValidateLoadSelection(9);
        [MenuItem(ShortcutPathLoad + "0", validate = true)]
        private static bool ValidateLoadSelection0() => ValidateLoadSelection(0);

        #endregion

        // ------------------------------

        #region Save

        [MenuItem(ShortcutPathSave + "1", priority = -59)]
        private static void SaveSelection1() => SaveSelection(1);
        [MenuItem(ShortcutPathSave + "2", priority = -58)]
        private static void SaveSelection2() => SaveSelection(2);
        [MenuItem(ShortcutPathSave + "3", priority = -57)]
        private static void SaveSelection3() => SaveSelection(3);
        [MenuItem(ShortcutPathSave + "4", priority = -56)]
        private static void SaveSelection4() => SaveSelection(4);
        [MenuItem(ShortcutPathSave + "5", priority = -55)]
        private static void SaveSelection5() => SaveSelection(5);
        [MenuItem(ShortcutPathSave + "6", priority = -54)]
        private static void SaveSelection6() => SaveSelection(6);
        [MenuItem(ShortcutPathSave + "7", priority = -53)]
        private static void SaveSelection7() => SaveSelection(7);
        [MenuItem(ShortcutPathSave + "8", priority = -52)]
        private static void SaveSelection8() => SaveSelection(8);
        [MenuItem(ShortcutPathSave + "9", priority = -51)]
        private static void SaveSelection9() => SaveSelection(9);
        [MenuItem(ShortcutPathSave + "0", priority = -50)]
        private static void SaveSelection0() => SaveSelection(0);

        [MenuItem(ShortcutPathSave + "1", validate = true)]
        private static bool ValidateSaveSelection1() => ValidateSaveSelection(1);
        [MenuItem(ShortcutPathSave + "2", validate = true)]
        private static bool ValidateSaveSelection2() => ValidateSaveSelection(2);
        [MenuItem(ShortcutPathSave + "3", validate = true)]
        private static bool ValidateSaveSelection3() => ValidateSaveSelection(3);
        [MenuItem(ShortcutPathSave + "4", validate = true)]
        private static bool ValidateSaveSelection4() => ValidateSaveSelection(4);
        [MenuItem(ShortcutPathSave + "5", validate = true)]
        private static bool ValidateSaveSelection5() => ValidateSaveSelection(5);
        [MenuItem(ShortcutPathSave + "6", validate = true)]
        private static bool ValidateSaveSelection6() => ValidateSaveSelection(6);
        [MenuItem(ShortcutPathSave + "7", validate = true)]
        private static bool ValidateSaveSelection7() => ValidateSaveSelection(7);
        [MenuItem(ShortcutPathSave + "8", validate = true)]
        private static bool ValidateSaveSelection8() => ValidateSaveSelection(8);
        [MenuItem(ShortcutPathSave + "9", validate = true)]
        private static bool ValidateSaveSelection9() => ValidateSaveSelection(9);
        [MenuItem(ShortcutPathSave + "0", validate = true)]
        private static bool ValidateSaveSelection0() => ValidateSaveSelection(0);

        #endregion

        // ------------------------------

        #region Save/Load Functions

        private static void LoadSelection(int index)
        {
            string data = EditorPrefs.GetString($"persistentSelection{index}");
            PersistentSelectionType selectionType = (PersistentSelectionType)EditorPrefs.GetInt($"persistentSelectionType{index}");

            List<Object> objects = new();
            switch(selectionType)
            {
                case PersistentSelectionType.Asset:
                    string[] guids = JsonConvert.DeserializeObject<string[]>(data);
                    foreach(string guid in guids)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        Object asset = AssetDatabase.LoadAssetAtPath(path, typeof(Object));
                        if(!asset)
                            continue;
                        objects.Add(asset);
                    }
                    break;

                case PersistentSelectionType.Hierarchy:
                    string[] objectPath = data?.Split("\n");
                    if(objectPath == null || objectPath.Length == 0)
                        return;
                    GameObject target = GameObject.Find(objectPath[0]);
                    if(!target)
                        return;
                    for(int i = 1; i < objectPath.Length; i++)
                    {
                        target = target.transform.Find(objectPath[i]).gameObject;
                        if(!target)
                            return;
                    }
                    objects.Add(target);
                    break;

                case PersistentSelectionType.None:
                    Selection.objects = objects.ToArray();
                    return;
            }

            if(objects.Count > 0)
                Selection.objects = objects.ToArray();
        }

        private static void SaveSelection(int index)
        {
            string data = null;
            PersistentSelectionType selectionType = PersistentSelectionType.None;
            if(Selection.activeTransform) // Hierarchy
            {
                Transform current = Selection.activeTransform;
                data = Selection.activeTransform.name;
                while(current.parent)
                {
                    current = current.parent;
                    data = $"{current.name}\n{data}"; // "\n" cannot be used in GameObject names, so it's a safe split identifier
                }
                selectionType = PersistentSelectionType.Hierarchy;
            }
            else if(Selection.assetGUIDs != null && Selection.assetGUIDs.Length > 0) // Asset
            {
                data = JsonConvert.SerializeObject(Selection.assetGUIDs);
                selectionType = PersistentSelectionType.Asset;
            }
            else
            {
                selectionType = PersistentSelectionType.None;
            }

            EditorPrefs.SetString($"persistentSelection{index}", data);
            EditorPrefs.SetInt($"persistentSelectionType{index}", (int)selectionType);
        }

        private static bool ValidateLoadSelection(int index)
        {
            PersistentSelectionType selectionType = (PersistentSelectionType)EditorPrefs.GetInt($"persistentSelectionType{index}");
            return selectionType != PersistentSelectionType.None;
        }

        private static bool ValidateSaveSelection(int index)
        {
            return Selection.activeTransform || (Selection.assetGUIDs != null && Selection.assetGUIDs.Length > 0);
        }

        #endregion

        // ------------------------------

        #region Other

        [MenuItem("Edit/Selection/Persistent/Log Selections", priority = -119)]
        private static void LogSelections()
        {
            string[] selectionStrings = new string[10];
            for(int i = 0; i < 10; i++)
            {
                int index = (i + 1) % 10;
                List<Object> objects = new();
                PersistentSelectionType selectionType = (PersistentSelectionType)EditorPrefs.GetInt($"persistentSelectionType{index}");

                switch(selectionType)
                {
                    case PersistentSelectionType.Asset:
                        string data = EditorPrefs.GetString($"persistentSelection{index}");
                        string[] guids = JsonConvert.DeserializeObject<string[]>(data);
                        foreach(string guid in guids)
                        {
                            string path = AssetDatabase.GUIDToAssetPath(guid);
                            Object asset = AssetDatabase.LoadAssetAtPath(path, typeof(Object));
                            if(!asset)
                                continue;
                            objects.Add(asset);
                        }
                        selectionStrings[i] = $"Selection {index} = ({selectionType}) {string.Join(", ", objects.Select(x => x.name))}";
                        break;

                    case PersistentSelectionType.Hierarchy:
                        selectionStrings[i] = $"Selection {index} = ({selectionType}) " +
                            $"{string.Join(", ", EditorPrefs.GetString($"persistentSelection{index}").Split('\n'))}";
                        break;

                    case PersistentSelectionType.None:
                        selectionStrings[i] = $"Selection {index} = None";
                        break;
                }
            }
            Debug.Log(string.Join("\n", selectionStrings).Trim());
        }

        #endregion
    }
}
