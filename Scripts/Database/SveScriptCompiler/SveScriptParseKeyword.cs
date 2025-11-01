using System;
using System.Collections.Generic;
using static SVESimulator.SveScript.SveScriptData;

namespace SVESimulator.SveScript
{
    internal static class SveScriptKeywordCompiler
    {
        public static void ParseAndAddKeywords(in string text, ref CardInfo cardInfo)
        {
            string[] words = text.Trim().Split(new string[] {", ", " "}, StringSplitOptions.RemoveEmptyEntries);
            foreach(string word in words)
            {
                cardInfo.keywords.Add(GetKeyword(word));
            }
        }

        public static Keyword GetKeyword(in string keyword)
        {
            return KeywordList[keyword];
        }

        // ------------------------------

        public readonly struct Keyword
        {
            public readonly int keywordId;
            public readonly int valueId;

            public Keyword(int keywordId, int valueId)
            {
                this.keywordId = keywordId;
                this.valueId = valueId;
            }

            public override string ToString()
            {
                return $"keywordId: {keywordId}, valueId: {valueId}";
            }
        }

        private static Dictionary<string, Keyword> KeywordList = new()
        {
            { "Ward",               new Keyword(0, 0) },
            { "Storm",              new Keyword(0, 1) },
            { "Rush",               new Keyword(0, 2) },
            { "Assail",             new Keyword(0, 3) },
            { "Intimidate",         new Keyword(0, 4) },
            { "Drain",              new Keyword(0, 5) },
            { "Bane",               new Keyword(0, 6) },
            { "Aura",               new Keyword(0, 7) },
            { "Quick",              new Keyword(0, 8) },

            { "IgnoreWard",         new Keyword(1, 0) },
            { "PutOnFieldEngaged",  new Keyword(1, 1) },
            { "CannotDealDamage",   new Keyword(1, 2) },

            { "Plus1Damage",        new Keyword(2, 0) },
            { "Plus2Damage",        new Keyword(2, 1) },

            // Counters
            { "Stack",              new Keyword(3, 1) },
            { "Spell",              new Keyword(4, 1) },
        };
    }
}
