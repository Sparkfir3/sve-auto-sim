using System.Collections;
using System.Collections.Generic;

namespace SVESimulator.CardTextData
{
    public static class TextFormatting
    {
        private static readonly Dictionary<string, string> TextFormattingItems = new()
        {
            { "[attack]",       "<sprite index=0>" },
            { "[defense]",      "<sprite index=1>" },
            { "[fanfare]",      "<sprite index=2>" },
            { "[lastwords]",    "<sprite index=3>" },
            { "[act]",          "<sprite index=4>" },
            { "[evolve]",       "<sprite index=5>" },

            { "[cost00]",       "<sprite index=6>" },
            { "[cost01]",       "<sprite index=7>" },
            { "[cost02]",       "<sprite index=8>" },
            { "[cost03]",       "<sprite index=9>" },
            { "[cost04]",       "<sprite index=10>" },
            { "[cost05]",       "<sprite index=11>" },
            { "[cost06]",       "<sprite index=12>" },
            { "[cost07]",       "<sprite index=13>" },
            { "[cost08]",       "<sprite index=14>" },
            { "[cost09]",       "<sprite index=15>" },
            { "[cost10]",       "<sprite index=16>" },

            { "[forestcraft]",  "<sprite index=17>" },
            { "[forest]",       "<sprite index=17>" },
            { "[swordcraft]",   "<sprite index=18>" },
            { "[sword]",        "<sprite index=18>" },
            { "[runecraft]",    "<sprite index=19>" },
            { "[rune]",         "<sprite index=19>" },
            { "[dragoncraft]",  "<sprite index=20>" },
            { "[dragon]",       "<sprite index=20>" },
            { "[abysscraft]",   "<sprite index=21>" },
            { "[abyss]",        "<sprite index=21>" },
            { "[havencraft]",   "<sprite index=22>" },
            { "[haven]",        "<sprite index=22>" },
            { "[neutral]",      "<sprite index=25>" },

            { "[engage]",       "<sprite index=23>" },
            { "[quick]",        "<sprite index=24>" },
            { "[q]",            "<sprite index=27>" },
        };

        public static string FormatCardText(string text)
        {
            if(!text.Contains("["))
                return text;
            foreach(var formattingInfo in TextFormattingItems)
            {
                (string key, string value) = (formattingInfo.Key, formattingInfo.Value);
                text = text.Replace(key, value);
                if(!text.Contains("["))
                    break;
            }
            return text;
        }
    }
}
