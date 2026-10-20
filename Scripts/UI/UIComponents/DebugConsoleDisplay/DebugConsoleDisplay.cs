using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using Sparkfire.Utility;

namespace SVESimulator.UI
{
    public class DebugConsoleDisplay : MonoBehaviour
    {
        [Title("Object References"), SerializeField]
        private List<DebugConsoleEntry> entries = new();
        [SerializeField]
        private Transform entryContainer;

        [Title("Asset References"), SerializeField, AssetsOnly]
        private DebugConsoleEntry entryPrefab;
        [SerializeField]
        private SerializedDictionary<LogType, Sprite> logTypeIcons = new();

        // ------------------------------

        public void Clear()
        {
            foreach(DebugConsoleEntry entry in entries)
                Destroy(entry.gameObject);
            entries.Clear();
        }

        public void AddEntries(List<(LogType, string)> logMessages)
        {
            foreach((LogType logType, string message) in logMessages)
                AddEntry(logType, message);
        }

        public void AddEntry(in LogType logType, in string message)
        {
            DebugConsoleEntry newEntry = Instantiate(entryPrefab, entryContainer);
            newEntry.SetInfo(logTypeIcons[logType], message);
            entries.Add(newEntry);
        }
    }
}
