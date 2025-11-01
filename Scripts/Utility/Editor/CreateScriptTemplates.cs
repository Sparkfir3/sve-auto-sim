using UnityEditor;

namespace Sparkfire.Utility
{
    public static class CreateScriptTemplates
    {
        private const string FILE_PATH = "Assets/_Main/Scripts/Utility/Editor/ScriptTemplates/";
        
        [MenuItem("Assets/Create/Script/MonoBehaviour", priority = 40)]
        public static void CreateMonoBehaviour()
        {
            string path = FILE_PATH + "MonoBehaviour.cs.txt";
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(path, "NewMonoBehaviour.cs");
        }
        
        [MenuItem("Assets/Create/Script/ScriptableObject", priority = 40)]
        public static void CreateScriptableObject()
        {
            string path = FILE_PATH + "ScriptableObject.cs.txt";
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(path, "NewScriptableObject.cs");
        }

        [MenuItem("Assets/Create/Script/StaticClass", priority = 40)]
        public static void CreateStaticClass()
        {
            string path = FILE_PATH + "StaticClass.cs.txt";
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(path, "NewStaticClass.cs");
        }

        [MenuItem("Assets/Create/Script/SVE Effect", priority = 50)]
        public static void CreateSveEffect()
        {
            string path = FILE_PATH + "SveEffect.cs.txt";
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(path, "NewEffect.cs");
        }

        [MenuItem("Assets/Create/Script/SVE Passive", priority = 51)]
        public static void CreateSvePassive()
        {
            string path = FILE_PATH + "SvePassive.cs.txt";
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(path, "NewPassive.cs");
        }

        [MenuItem("Assets/Create/Script/Effect Cost", priority = 52)]
        public static void CreateEffectCost()
        {
            string path = FILE_PATH + "EffectCost.cs.txt";
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(path, "EffectCost.cs");
        }
    }
}
