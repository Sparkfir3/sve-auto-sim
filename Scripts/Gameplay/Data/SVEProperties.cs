using UnityEngine;

namespace SVESimulator
{
    public static class SVEProperties
    {
        public enum GamePhase { Setup, Start, Main, End }

        public enum Counters
        {
            Stack = 3,
            Spell = 4
        }

        public enum SVEEffectTarget
        {
            Self,
            Leader,
            AllPlayerCards,
            AllPlayerCardsEx,
            AllPlayerCardsFieldAndEx,
            TargetPlayerCard,
            TargetPlayerCardOrLeader,
            TargetPlayerCardEx,
            TriggerCard,

            Opponent,
            OpponentLeader,
            AllOpponentCards,
            TargetOpponentCard,
            TargetOpponentCardsDivided,
            TargetOpponentCardOrLeader,

            AllCards,
            TargetCard,

            AllPlayers,
            AllLeaders
        }

        public enum TokenCreationOption
        {
            Field = 0,
            ExArea = 1,
            FieldOverflowToEx = 2,
            ChooseForEachFieldOrEx = 3
        }

        public enum StatBoostType { Attack, Defense, AttackDefense, Cost, EvolveCost, MaxPlayPoint, PlayPoint }
        public enum PassiveDuration  { WhileOnField, OpponentTurn, EndOfTurn }

        public static Quaternion CardFaceUpRotation = Quaternion.Euler(0f, 0f, 0f);
        public static Quaternion CardFaceDownRotation = Quaternion.Euler(0f, 0f, 180f);
        public static Quaternion OpponentCardRotation = Quaternion.Euler(0f, 180f, 0f);
        public static Quaternion CardEngagedRotation = Quaternion.Euler(0f, 90f, 0f);

        public const int StartingHandSize = 4;
        public const int MaxPlayPointsAmount = 10;
        public const float CardThickness = 0.01f;

        // ------------------------------

        public static class Zones
        {
            public const string Hand = "Hand";
            public const string Deck = "Deck";
            public const string Field = "Field";
            public const string ExArea = "EX Area";
            public const string Cemetery = "Cemetery";
            public const string EvolveDeck = "Evolve Deck";
            public const string Banished = "Banished";
            public const string Leader = "Leader";
            public const string Resolution = "Resolution";
        }

        public static class PlayerStats
        {
            public const string Defense = "Defense";
            public const string MaxPlayPoints = "Max Play Points";
            public const string PlayPoints = "Play Points";
            public const string EvolutionPoints = "Evolution Points";
        }

        public static class CardTypes
        {
            public const string Follower = "Follower";
            public const string EvolvedFollower = "Evolved Follower";
            public const string Spell = "Spell";
            public const string Amulet = "Amulet";
            public const string Token = "Token";
            public const string Leader = "Leader";
        }

        public static class CardStats
        {
            public const string Cost = "Cost";
            public const string Attack = "Attack";
            public const string Defense = "Defense";
            public const string EvolveCost = "Evolve Cost";
            public const string Engaged = "Engaged";
            public const string AttachedCardInstanceIDs = "Attached Instance IDs";
            public const string FaceUp = "Face Up";

            public const string ID = "ID";
            public const string Class = "Class";
            public const string Name = "Name";
            public const string Text = "Text";
            public const string Trait = "Trait";
            public const string Rarity = "Rarity";
        }

        public static class CardClass
        {
            public const string Forest = "Forestcraft";
            public const string Sword = "Swordcraft";
            public const string Rune = "Runecraft";
            public const string Dragon = "Dragoncraft";
            public const string Abyss = "Abysscraft";
            public const string Haven = "Havencraft";
            public const string Neutral = "Neutral";
        }

        public static class CardRarity
        {
            public const string Legendary = "Legendary";
            public const string Gold = "Gold";
            public const string Silver = "Silver";
            public const string Bronze = "Bronze";
            public const string Token = "Token";
        }

        public static class Keywords
        {
            public const string Ward = "Ward";
            public const string Storm = "Storm";
            public const string Rush = "Rush";
            public const string Intimidate = "Intimidate";
            public const string Assail = "Assail";
            public const string Drain = "Drain";
            public const string Bane = "Bane";
            public const string Aura = "Aura";
            public const string Quick = "Quick";
        }

        public static class PassiveAbilities
        {
            public const string IgnoreWard = "IgnoreWard";
            public const string PutOnFieldEngaged = "PutOnFieldEngaged";
            public const string CannotDealDamage = "CannotDealDamage";
            public const string CannotAttack = "CannotAttack";

            public const string Plus1Damage = "Plus1Damage";
            public const string Plus2Damage = "Plus2Damage";
            public const string Plus3Damage = "Plus3Damage";
            public const string Plus4Damage = "Plus4Damage";

            public const string DamageReduction1 = "DamageReduction1";
        }
    }
}
