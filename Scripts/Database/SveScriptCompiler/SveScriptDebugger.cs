using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SVESimulator.SveScript
{
    public static class SveScriptDebugger
    {
        private static List<(LogType, string)> LogMessages = new();

        // ------------------------------

        public static void Reset()
        {
            LogMessages.Clear();
        }

        public static bool HasErrors()
        {
            foreach((LogType type, string _) in LogMessages)
            {
                if(type == LogType.Error)
                    return true;
            }
            return false;
        }

        public static List<(LogType, string)> GetLogs()
        {
            return new List<(LogType, string)>(LogMessages);
        }

        // ------------------------------

        public static void Log(string msg)
        {
            Debug.Log(msg);
            LogMessages.Add((LogType.Log, msg));
        }

        public static void LogWarning(string msg)
        {
            Debug.LogWarning(msg);
            LogMessages.Add((LogType.Warning, msg));
        }

        public static void LogError(string msg)
        {
            Debug.LogError(msg);
            LogMessages.Add((LogType.Error, msg));
        }
    }
}
