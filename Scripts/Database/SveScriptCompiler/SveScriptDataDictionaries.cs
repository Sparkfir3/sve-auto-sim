using System;
using System.Collections;
using System.Collections.Generic;

namespace SVESimulator.SveScript
{
    internal static partial class SveScriptData
    {
        public static readonly Dictionary<string, string> SetNames = new()
        {
            { "SD01", "Starter Deck #1 \\\"Regal Fairy Princess\\\"" },
            { "SD02", "Starter Deck #2 \\\"Blade of Resentment\\\"" },
            { "SD03", "Starter Deck #3 \\\"Mysteries of Conjuration\\\"" },
            { "SD04", "Starter Deck #4 \\\"Wrath of the Greatwyrm\\\"" },
            { "SD05", "Starter Deck #5 \\\"Waltz of the Undying Night\\\"" },
            { "SD06", "Starter Deck #6 \\\"Maculate Ablution\\\"" },
            { "BP01", "Booster Set #1 \\\"Advent of Genesis\\\"" },
        };

        public static readonly Dictionary<string, int> SpecialCardTypeToID = new()
        {
            { "-T", 8 },
            { "-LD", 9 }
        };

        public static readonly Dictionary<string, int> SetTypeToID = new()
        {
            { "SD", 1 },
            { "BP", 2 }
        };

        // ------------------------------

        public static readonly Dictionary<string, int> CardTypeIDs = new()
        {
            { "follower", 0 },
            { "evolved follower", 1 },
            { "spell", 2 },
            { "evolved spell", 3 },
            { "amulet", 4 },
            { "leader", 5 },
        };

        public static readonly Dictionary<string, string> ClassList = new()
        {
            { "forest", "Forestcraft" },
            { "sword", "Swordcraft" },
            { "rune", "Runecraft" },
            { "dragon", "Dragoncraft" },
            { "abyss", "Abysscraft" },
            { "haven", "Havencraft" },
            { "neutral", "Neutral" },
        };

        public static readonly Dictionary<string, string> UniverseList = new()
        {
            { "uma", "Umamusume: Pretty Derby" },
            { "umamusume", "Umamusume: Pretty Derby" },
            { "imas", "THE IDOLM@STER CINDERELLA GIRLS" },
            { "idolmaster", "THE IDOLM@STER CINDERELLA GIRLS" },
        };

        public static readonly Dictionary<string, string> RarityList = new()
        {
            { "sl", "Legendary" },
            { "super legendary", "Super Legendary" },

            { "l", "Legendary" },
            { "legendary", "Legendary" },

            { "u", "Ultimate" },
            { "ultimate", "Ultimate" },

            { "g", "Gold" },
            { "gold", "Gold" },

            { "s", "Silver" },
            { "silver", "Silver" },

            { "b", "Bronze" },
            { "bronze", "Bronze" },
        };

        // ------------------------------

        public static readonly Dictionary<string, string> TextFormatting = new()
        {
            // { "Ward", "<b>Ward</b>" },
            // { "Storm", "<b>Storm</b>" },
            // { "Rush", "<b>Rush</b>" },
            // { "Intimidate", "<b>Intimidate</b>" },
            // { "Assail", "<b>Assail</b>" },
            // { "Drain", "<b>Drain</b>" },
            // { "Bane", "<b>Bane</b>" },
            // { "Aura", "<b>Aura</b>" },
            //
            // { "Combo", "<b>Combo</b>" },
            // { "Spellchain", "<b>Spellchain</b>" },
            // { "Overflow", "<b>Overflow</b>" },
            // { "Strike", "<b>Strike</b>" },
            // { "On Evolve:", "<b>On Evolve</b>:" },
            //
            // { "<bi>", "<b><i>" },
            // { "</bi>", "</i></b>" },

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
        };
    }
}
