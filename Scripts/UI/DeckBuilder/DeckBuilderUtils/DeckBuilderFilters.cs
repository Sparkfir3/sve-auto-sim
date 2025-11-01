using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SVESimulator.DeckBuilder
{
    [Serializable]
    public class DeckBuilderFilters
    {
        public List<string> sets;
        public string text;
        public CardTypeFilter cardType;
        [LabelText("Class")]
        public ClassFilter cardClass;

        [HorizontalGroup("Cost"), LabelWidth(100)]
        public bool useCost = true;
        [HorizontalGroup("Cost"), LabelWidth(100), Range(0, 8)]
        public int minCost;
        [HorizontalGroup("Cost"), LabelWidth(100), Range(0, 8)]
        public int maxCost = 8;

        [HorizontalGroup("Attack"), LabelWidth(100)]
        public bool useAttack = true;
        [HorizontalGroup("Attack"), LabelWidth(100), Range(0, 10)]
        public int minAttack;
        [HorizontalGroup("Attack"), LabelWidth(100), Range(0, 10)]
        public int maxAttack = 10;

        [HorizontalGroup("Defense"), LabelWidth(100)]
        public bool useDefense = true;
        [HorizontalGroup("Defense"), LabelWidth(100), Range(0, 10)]
        public int minDefense;
        [HorizontalGroup("Defense"), LabelWidth(100), Range(0, 10)]
        public int maxDefense = 10;

        // public RarityFilter rarity; UNUSED
    }

    [Flags]
    public enum CardTypeFilter
    {
        Follower = 1,
        Amulet = 2,
        Spell = 4,
        Evolved = 8,
        Leader = 16,
        Token = 32,
    }

    [Flags]
    public enum ClassFilter
    {
        Forest = 1,
        Sword = 2,
        Rune = 4,
        Dragon = 8,
        Abyss = 16,
        Haven = 32,
        Neutral = 64,
    }

    [Flags]
    public enum RarityFilter
    {
        Legendary = 1,
        Gold = 2,
        Silver = 4,
        Bronze = 8,
        Token = 16,
    }
}
