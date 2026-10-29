using System;
using System.Collections;
using System.Collections.Generic;

namespace SVESimulator.SveScript
{
    internal static partial class SveScriptData
    {
        public static readonly Dictionary<string, string> SetNames = new()
        {
            // Starter Decks
            { "SD01",           "Starter Deck #1 \\\"Regal Fairy Princess\\\"" },
            { "SD02",           "Starter Deck #2 \\\"Blade of Resentment\\\"" },
            { "SD03",           "Starter Deck #3 \\\"Mysteries of Conjuration\\\"" },
            { "SD04",           "Starter Deck #4 \\\"Wrath of the Greatwyrm\\\"" },
            { "SD05",           "Starter Deck #5 \\\"Waltz of the Undying Night\\\"" },
            { "SD06",           "Starter Deck #6 \\\"Maculate Ablution\\\"" },

            // Booster Sets
            { "BP01",           "Booster Set #1 \\\"Advent of Genesis\\\"" },
            { "BP02",           "Booster Set #2 \\\"Reign of Bahamut\\\"" },
            { "BP03",           "Booster Set #3 \\\"Flame of Laevateinn\\\"" },
            { "BP04",           "Booster Set #4 \\\"Cosmic Mythos\\\"" },
            { "BP05",           "Booster Set #5 \\\"Omens Eternal\\\"" },
            { "BP06",           "Booster Set #6 \\\"Paragons of the Colosseum\\\"" },
            { "BP07",           "Booster Set #7 \\\"Verdant Steel\\\"" },
            { "BP08",           "Booster Set #8 \\\"Alterchaotica\\\"" },
            { "BP09",           "Booster Set #9 \\\"Duet of Dawn and Dusk\\\"" },
            { "BP10",           "Booster Set #10 \\\"Gods of the Arcana\\\"" },
            { "BP11",           "Booster Set #11 \\\"Bullet of Fate\\\"" },
            { "BP12",           "Booster Set #12 \\\"Worldreaver's Descent\\\"" },
            { "BP13",           "Booster Set #13 \\\"Dominion of Darkness\\\"" },
            { "BP14",           "Booster Set #14 \\\"Banquet of Dreams\\\"" },
            { "BP15",           "Booster Set #15 \\\"Trial of the Omens\\\"" },
            { "BP16",           "Booster Set #16 \\\"New World Genesis\\\"" },
            { "BP17",           "Booster Set #17 \\\"Convergent Destinies\\\"" },
            { "BP18",           "Booster Set #18 \\\"Neometropolis\\\"" },
            { "BP19",           "Booster Set #19 \\\"Eightfold Retribution\\\"" },
            { "BP20",           "Booster Set #20 \\\"Omens and Heirs\\\"" },

            // Crossover Sets
            { "CSD01",          "Crossover Starter Deck \\\"Ready, Set, Umamusume!\\\"" },
            { "CP01",           "Crossover Set \\\"Umamusume: Pretty Derby\\\"" },
            { "ECP01",          "EX Crossover Set \\\"Umamusume: Pretty Derby\\\"" },

            { "CSD02a",         "Crossover Starter Deck \\\"Cute\\\"" },
            { "CSD02b",         "Crossover Starter Deck \\\"Cool\\\"" },
            { "CSD02c",         "Crossover Starter Deck \\\"Passion\\\"" },
            { "CP02",           "Crossover Set \\\"THE IDOLM@STER CINDERELLA GIRLS\\\"" },
            { "ECP02",          "EX Crossover Set \\\"THE IDOLM@STER CINDERELLA GIRLS\\\"" },

            { "CSD03a",         "Crossover Starter Deck \\\"Sanctuary Knight Brigade\\\"" },
            { "CSD03b",         "Crossover Starter Deck \\\"Apocalyptic Fire\\\"" },
            { "CP03",           "Crossover Set \\\"Cardfight!! Vanguard\\\"" },

            { "CP04",           "Crossover Set \\\"Princess Connect! Re: Dive\\\"" },

            // Showdown decks
            { "SDD01",          "Showdown Deck: Forestcraft" },
            { "SDD02",          "Showdown Deck: Swordcraft" },
            { "SDD03",          "Showdown Deck: Runecraft" },
            { "SDD04",          "Showdown Deck: Dragoncraft" },
            { "SDD05",          "Showdown Deck: Abysscraft" },
            { "SDD06",          "Showdown Deck: Havencraft" },

            // Other
            { "PR",             "Promo Cards" },
            { "SS01",           "Worlds Beyond Swordcraft Starter Set" },
            { "SS02",           "Worlds Beyond Dragoncraft Starter Set" },
            { "SP01",           "Special Set \\\"Seaside Memories\\\"" },
            { "PCS01",          "Premium Card Set \\\"Summer Edition Vol. 1\\\"" },
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
            { "advanced", 6 },
            { "advanced follower", 6 },
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
            { "vanguard", "Cardfight!! Vanguard" },
            { "priconne", "Princess Connect! Re: Dive" },
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
