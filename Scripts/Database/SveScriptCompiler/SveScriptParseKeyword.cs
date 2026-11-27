using System;
using System.Collections.Generic;
using System.Linq;
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
                Keyword keyword = GetKeyword(word);
                if(!cardInfo.keywords.Any(x => x.Equals(keyword)))
                    cardInfo.keywords.Add(keyword);
            }
        }

        public static Keyword GetKeyword(in string keyword)
        {
            return KeywordList[keyword.Replace("Cant", "Cannot").Replace("Doesnt", "DoesNot")];
        }

        // ------------------------------

        public readonly struct Keyword : IEquatable<Keyword>
        {
            public readonly int keywordId;
            public readonly int valueId;

            public Keyword(int keywordId, int valueId)
            {
                this.keywordId = keywordId;
                this.valueId = valueId;
            }

            public bool Equals(Keyword other)
            {
                return keywordId == other.keywordId && valueId == other.valueId;
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

            // Ability Keywords
            { "Quick",                      new Keyword(0, 8) },
            { "Evolve",                     new Keyword(0, 9) },
            { "EarthRite",                  new Keyword(0, 10) },

            // Racing
            { "serve",                      new Keyword(1, 7) },
            { "serve1",                     new Keyword(1, 7) },
            { "serve2",                     new Keyword(1, 8) },
            { "serve3",                     new Keyword(1, 9) },

            // Passive Abilities
            { "IgnoreWard",                         new Keyword(1, 0) },
            { "PutOnFieldEngaged",                  new Keyword(1, 1) },
            { "UseDefAsAtk",                        new Keyword(1, 2) },
            { "RuneFollowersForSpellchain",         new Keyword(1, 3) },
            { "CannotDestroyByAbilities",           new Keyword(1, 4) },
            { "CannotAttack",                       new Keyword(1, 5) },
            { "CannotAttackLeaders",                new Keyword(1, 6) },
            { "CannotAct",                          new Keyword(1, 11) },
            { "OpponentMustSelectAsTarget",         new Keyword(1, 12) },
            { "CannotPlayFromEx",                   new Keyword(1, 13) },
            { "PlayerChooseAnyNumberOfOptions",     new Keyword(1, 14) },
            { "TriggerStartEndPhaseOnceMore",       new Keyword(1, 15) },

            // Plus Damage
            { "Plus1Damage",                new Keyword(2, 1) },
            { "Plus2Damage",                new Keyword(2, 2) },
            { "Plus3Damage",                new Keyword(2, 3) },
            { "Plus4Damage",                new Keyword(2, 4) },

            // Damage Reduction
            { "DamageReduction1",           new Keyword(3, 1) },
            { "DamageReductionAbilities1",  new Keyword(4, 1) },
            { "DamageCap1",                 new Keyword(5, 1) },
            { "DamageCap2",                 new Keyword(5, 2) },
            { "DamageCap3",                 new Keyword(5, 3) },

            // Take Reduced/Increased Damage
            { "TakeReducedDamage1",         new Keyword(6, 1) },
            { "TakeIncreasedDamage1",       new Keyword(7, 1) },

            // Other Damage Mods
            { "CannotDealDamage",           new Keyword(8, 0) },
            { "DoesNotTakeDamage",          new Keyword(8, 1) },
            { "DoesNotTakeCombatDamage",    new Keyword(8, 2) },
            { "DoubleCombatDamage",         new Keyword(8, 3) },
            { "DoubleLeaderDamage",         new Keyword(8, 4) },
            { "DoesNotTakeAbilityDamage",   new Keyword(8, 5) },

            // Counters
            { "Stack",                      new Keyword(10, 1) },
            { "Spell",                      new Keyword(11, 1) },
            { "Prayer",                     new Keyword(12, 1) },
            { "Fable",                      new Keyword(13, 1) },
            { "Soul",                       new Keyword(14, 1) },
            { "Lightning",                  new Keyword(15, 1) },
            { "Grace",                      new Keyword(16, 1) },
            { "Calamity",                   new Keyword(17, 1) },
            { "Mana",                       new Keyword(18, 1) },
            { "Dormancy",                   new Keyword(19, 1) },
            { "Seasonal",                   new Keyword(20, 1) },
            { "Divine Water",               new Keyword(21, 1) },
            { "Fighting Spirit",            new Keyword(22, 1) },
            { "Arrow",                      new Keyword(23, 1) },
            { "Gigabyte",                   new Keyword(24, 1) },
            { "Curse",                      new Keyword(25, 1) },
            { "Fusion",                     new Keyword(26, 1) },
            { "Pain",                       new Keyword(27, 1) },
            { "Reversal",                   new Keyword(28, 1) },
            { "Passion",                    new Keyword(29, 1) },
            { "Bunclie",                    new Keyword(30, 1) },
            { "Battery",                    new Keyword(31, 1) },
        };
    }
}
