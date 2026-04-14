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
            return KeywordList[keyword.Replace("Cant", "Cannot").Replace("Doesnt", "DoesNot")];
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
            // Regular Keywords
            { "Ward",                       new Keyword(0, 0) },
            { "Storm",                      new Keyword(0, 1) },
            { "Rush",                       new Keyword(0, 2) },
            { "Assail",                     new Keyword(0, 3) },
            { "Intimidate",                 new Keyword(0, 4) },
            { "Drain",                      new Keyword(0, 5) },
            { "Bane",                       new Keyword(0, 6) },
            { "Aura",                       new Keyword(0, 7) },
            { "Quick",                      new Keyword(0, 8) },

            // Racing (TODO: Evolve)
            { "Serve",                      new Keyword(1, 7) },
            { "Serve1",                     new Keyword(1, 7) },
            { "Serve2",                     new Keyword(1, 8) },
            { "Serve3",                     new Keyword(1, 9) },

            // Passive Abilities
            { "IgnoreWard",                 new Keyword(1, 0) },
            { "PutOnFieldEngaged",          new Keyword(1, 1) },
            { "UseDefAsAtk",                new Keyword(1, 2) },
            { "RuneFollowersForSpellchain", new Keyword(1, 3) },
            { "CannotDestroyByAbilities",   new Keyword(1, 4) },
            { "CannotAttack",               new Keyword(1, 5) },
            { "CannotAttackLeaders",        new Keyword(1, 6) },

            // Plus Damage
            { "Plus1Damage",                new Keyword(2, 0) },
            { "Plus2Damage",                new Keyword(2, 1) },
            { "Plus3Damage",                new Keyword(2, 2) },
            { "Plus4Damage",                new Keyword(2, 3) },

            // Damage Reduction
            { "DamageReduction1",           new Keyword(3, 0) },
            { "DamageReductionAbilities1",  new Keyword(4, 0) },

            // Other Damage Mods
            { "CannotDealDamage",           new Keyword(5, 0) },
            { "DoesNotTakeDamage",          new Keyword(5, 1) },
            { "DoesNotTakeCombatDamage",    new Keyword(5, 2) },
            { "DoubleCombatDamage",         new Keyword(5, 3) },
            { "DoubleLeaderDamage",         new Keyword(5, 4) },

            // Counters
            { "Stack",                      new Keyword(6, 1) },
            { "Spell",                      new Keyword(7, 1) },
            { "Prayer",                     new Keyword(8, 1) },
        };
    }
}
