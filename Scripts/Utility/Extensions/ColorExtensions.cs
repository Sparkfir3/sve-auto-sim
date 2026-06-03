using UnityEngine;

namespace Sparkfire.Utility
{
    public static class ColorExtensions
    {
        public static string ToHex(this Color color)
        {
            return $"#{(byte)(color.r * 255):X2}{(byte)(color.g * 255):X2}{(byte)(color.b * 255):X2}";
        }

        public static string ToHex(this Color32 color)
        {
            return $"#{color.r:X2}{color.g:X2}{color.b:X2}";
        }
    }
}
