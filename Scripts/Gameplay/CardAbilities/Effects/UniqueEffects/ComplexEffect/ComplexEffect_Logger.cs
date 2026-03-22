using System;
using UnityEngine;

namespace SVESimulator
{
    public partial class ComplexEffect
    {
        [Flags]
        private enum LogMode
        {
            All = ~0,
            None = 0,
            Main = 1,
            Value = 2,
            Perform = 4,
        }

        private const LogMode LOG_MODE = LogMode.All;

        private void ComplexLog(LogMode mode, string message)
        {
            if(LOG_MODE.HasFlag(mode))
                Debug.Log($"[CE/{mode.ToString()}] {message}");
        }
    }
}
