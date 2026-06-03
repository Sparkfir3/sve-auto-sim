using System;
using System.Collections.Generic;
using Sparkfire.Utility;
using UnityEngine;

namespace SVESimulator
{
    public partial class ComplexEffect
    {
        [Flags]
        public enum LogMode
        {
            All = ~0,
            None = 0,
            Main = 1,
            Value = 2,
            Perform = 4,
        }

        public LogMode LOG_MODE { get; set; } = LogMode.All;
        private readonly Dictionary<LogMode, Color32> logModeColors = new()
        {
            { LogMode.Main,     Color.cyan },
            { LogMode.Value,    Color.blue },
            { LogMode.Perform,  new Color32(189, 34, 237, 255) }, // ourple
        };

        private void ComplexLog(LogMode mode, string message)
        {
            if(LOG_MODE.HasFlag(mode))
            {
                string modePrefix = $"[CE/{mode.ToString()}]";
                if(logModeColors.TryGetValue(mode, out Color32 color))
                    modePrefix = $"<color={color.ToHex()}>{modePrefix}</color>";
                Debug.Log($"{modePrefix} {message}\nPointers: {pointerL}, {pointerR}");
            }
        }
    }
}
