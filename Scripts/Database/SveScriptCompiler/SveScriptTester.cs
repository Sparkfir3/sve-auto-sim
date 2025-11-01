using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SVESimulator.SveScript
{
    public class SveScriptTester : MonoBehaviour
    {
        [SerializeField]
        private TextAsset input;
        [SerializeField]
        private FilePathInfo filePathInfo;

        [Button]
        public void TestScript()
        {
            SveScriptCompiler.ParseSveScriptCardSet(input.text, filePathInfo.CardDataPath);
        }
    }
}
