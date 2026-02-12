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
            { "BP02", "Booster Set #2 \\\"Reign of Bahamut\\\"" },
            { "BP03", "Booster Set #3 \\\"Flame of Laevateinn\\\"" },
            { "BP04", "Booster Set #4 \\\"Cosmic Mythos\\\"" },
            { "BP05", "Booster Set #5 \\\"Omens Eternal\\\"" },
            { "BP06", "Booster Set #6 \\\"Paragons of the Colosseum\\\"" },
            { "BP07", "Booster Set #7 \\\"Verdant Steel\\\"" },
            { "BP08", "Booster Set #8 \\\"Alterchaotica\\\"" },
            { "BP09", "Booster Set #9 \\\"Duet of Dawn and Dusk\\\"" },
            { "BP10", "Booster Set #10 \\\"Gods of the Arcana\\\"" },

            { "CSD01", "Crossover Starter Deck \\\"Ready, Set, Umamusume!\\\"" },
            { "CP01", "Crossover Set \\\"Umamusume: Pretty Derby\\\"" },

            { "CSD02a", "Crossover Starter Deck \\\"Cute\\\"" },
            { "CSD02b", "Crossover Starter Deck \\\"Cool\\\"" },
            { "CSD02c", "Crossover Starter Deck \\\"Passion\\\"" },
            { "CP02", "Crossover Set \\\"THE IDOLM@STER CINDERELLA GIRLS\\\"" },
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
    }
}
