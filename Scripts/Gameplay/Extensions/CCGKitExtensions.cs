using System.Collections.Generic;
using System.Linq;
using CCGKit;
using UnityEngine;
using Sirenix.OdinInspector;
using Sparkfire.Utility;

namespace SVESimulator
{
    public static class CCGKitExtensions
    {
        #region Is Card Type Checks

        public static bool IsCardType(this Card card, string cardType, GameConfiguration gameConfig = null)
        {
            if(card == null)
                return false;
            gameConfig ??= GameManager.Instance.config;
            if(gameConfig == null)
                return false;
            CardType ccgCardType = gameConfig.cardTypes.Find(x => x.id == card.cardTypeId);
            return ccgCardType != null && ccgCardType.name.Equals(cardType);
        }

        public static bool IsCardType(this CardObject card, string cardType) => card.RuntimeCard.IsCardType(cardType);
        public static bool IsCardType(this RuntimeCard card, string cardType) => card.cardType.name.Equals(cardType);

        public static bool IsFollowerOrEvolvedFollower(this Card card, GameConfiguration gameConfig = null)
            => IsCardType(card, SVEProperties.CardTypes.Follower, gameConfig) || IsCardType(card, SVEProperties.CardTypes.EvolvedFollower, gameConfig);
        public static bool IsFollowerOrEvolvedFollower(this CardObject card) => IsFollowerOrEvolvedFollower(card.RuntimeCard);
        public static bool IsFollowerOrEvolvedFollower(this RuntimeCard card)
        {
            if(card == null)
                return false;
            return card.IsCardType(SVEProperties.CardTypes.Follower) || card.IsCardType(SVEProperties.CardTypes.EvolvedFollower);
        }

        public static bool IsSpell(this Card card, GameConfiguration gameConfig = null) => IsCardType(card, SVEProperties.CardTypes.Spell, gameConfig);
        public static bool IsSpell(this CardObject card) => IsSpell(card.RuntimeCard);
        public static bool IsSpell(this RuntimeCard card)
        {
            if(card == null)
                return false;
            return card.IsCardType(SVEProperties.CardTypes.Spell);
        }

        public static bool IsAmulet(this Card card, GameConfiguration gameConfig = null) => IsCardType(card, SVEProperties.CardTypes.Amulet, gameConfig);
        public static bool IsAmulet(this CardObject card) => IsAmulet(card.RuntimeCard);
        public static bool IsAmulet(this RuntimeCard card)
        {
            if(card == null)
                return false;
            return card.IsCardType(SVEProperties.CardTypes.Amulet);
        }

        public static bool IsEvolvedType(this CardObject card) => card.RuntimeCard.IsEvolvedType();
        public static bool IsEvolvedType(this RuntimeCard card)
        {
            if(card == null)
                return false;
            return card.cardType.name.Equals(SVEProperties.CardTypes.EvolvedFollower);
        }

        public static bool IsToken(this CardObject card, GameConfiguration gameConfig = null) => card.RuntimeCard.IsToken(gameConfig);
        public static bool IsToken(this RuntimeCard card, GameConfiguration gameConfig = null)
        {
            if(card == null)
                return false;
            gameConfig ??= GameManager.Instance.config;
            if(gameConfig == null)
                return false;
            return LibraryCardCache.GetCard(card.cardId, gameConfig).GetStringProperty(SVEProperties.CardStats.Trait).Contains(SVEProperties.CardTypes.Token);
        }

        public static bool IsEvolvedVersionOf(this Card evolvedCard, Card baseCard)
        {
            return evolvedCard.name.Replace(" (Evolved)", "").Equals(baseCard.name);
        }

        #endregion

        // ------------------------------

        #region Get Card Info

        public static int PlayPointCost(this RuntimeCard card, PlayerController player = null)
        {
            int baseCost = card.namedStats.TryGetValue(SVEProperties.CardStats.Cost, out Stat costStat) ? costStat.effectiveValue : 0;
            int reduction = 0;

            // Reduced cost from abilities on card
            List<TriggeredAbility> reducedCostAbilities = LibraryCardCache.GetCard(card.cardId).abilities.Where(x => x is TriggeredAbility && x.effect is ReducedCostEffect)
                .Select(x => x as TriggeredAbility).ToList();
            foreach(TriggeredAbility ability in reducedCostAbilities)
            {
                SveTrigger trigger = ability.trigger as SveTrigger;
                ReducedCostEffect effect = ability.effect as ReducedCostEffect;
                if(trigger == null || effect == null || (!trigger.condition.IsNullOrWhiteSpace() && !SVEFormulaParser.ParseValueAsCondition(trigger.condition, player, card)))
                    continue;
                reduction += effect.ReduceCostAmount(player, card);
            }

            // Reduced cost from abilities of other cards on field
            reduction += SVEEffectPool.Instance.GetReducedCostFromActivePassives(card, player);

            return Mathf.Max(baseCost - reduction, 0);
        }

        public static bool HasAvailableAlternateCost(this RuntimeCard card, PlayerController player, out List<TriggeredAbility> alternateCostAbilities)
        {
            List<TriggeredAbility> allAlternateCostAbilities = LibraryCardCache.GetCard(card.cardId).abilities.Where(x => x is TriggeredAbility && x.effect is AlternateCostEffect)
                .Select(x => x as TriggeredAbility).ToList();
            if(allAlternateCostAbilities.Count == 0)
            {
                alternateCostAbilities = null;
                return false;
            }

            alternateCostAbilities = new List<TriggeredAbility>();
            foreach(TriggeredAbility ability in allAlternateCostAbilities)
            {
                SveTrigger trigger = ability.trigger as SveTrigger;
                AlternateCostEffect effect = ability.effect as AlternateCostEffect;
                if(trigger == null || effect == null || (!trigger.condition.IsNullOrWhiteSpace() && !SVEFormulaParser.ParseValueAsCondition(trigger.condition, player, card)))
                    continue;
                alternateCostAbilities.Add(ability);
            }
            return alternateCostAbilities.Count > 0;
        }

        public static int EvolveCost(this RuntimeCard card)
        {
            if(!card.namedStats.TryGetValue(SVEProperties.CardStats.EvolveCost, out Stat stat))
                return -1;
            return stat.baseValue < 0 ? stat.baseValue : stat.effectiveValue;
            // for SOME REASON, CCG kit clamps stats' min value to 0 (which only affects effectiveValue, and not baseValue) so we have to have an extra check for -1
        }

        public static bool HasQuickKeyword(this CardObject card) => card.RuntimeCard.HasQuickKeyword();
        public static bool HasQuickKeyword(this RuntimeCard card)
        {
            if(card == null)
                return false;
            return card.HasKeyword(SVEProperties.Keywords.Quick);
        }

        public static int CountOfKeyword(this CardObject card, int keyword, int value) => card ? CountOfKeyword(card.RuntimeCard, keyword, value) : 0;
        public static int CountOfKeyword(this RuntimeCard card, int keyword, int value)
        {
            int count = 0;
            for(int i = 0; i < card.keywords.Count; i++)
            {
                if(card.keywords[i].keywordId == keyword && card.keywords[i].valueId == value)
                    count++;
            }
            return count;
        }

        public static int CountOfCounter(this CardObject card, int keywordType) => card ? CountOfCounter(card.RuntimeCard, (SVEProperties.Counters)keywordType) : 0;
        public static int CountOfCounter(this CardObject card, SVEProperties.Counters counter) => card ? CountOfCounter(card.RuntimeCard, counter) : 0;
        public static int CountOfCounter(this RuntimeCard card, int keywordType) => card.CountOfCounter((SVEProperties.Counters)keywordType);
        public static int CountOfCounter(this RuntimeCard card, SVEProperties.Counters counter)
        {
            RuntimeKeyword counterAsKeyword = card.keywords.FirstOrDefault(x => x.keywordId == (int)counter);
            return counterAsKeyword?.valueId ?? 0;
        }

        public static bool HasCounter(this CardObject card, SVEProperties.Counters counter) => card.CountOfCounter(counter) > 0;
        public static bool HasCounter(this RuntimeCard card, SVEProperties.Counters counter) => card.CountOfCounter(counter) > 0;

        #endregion

        // ------------------------------

        #region Modify Card Info

        public static bool RemoveModifier(this Stat stat, Modifier modifier)
        {
            Modifier modToRemove = stat.modifiers.FirstOrDefault(x => x.value == modifier.value && x.duration == modifier.duration);
            if(modToRemove != null)
            {
                int oldValue = stat.effectiveValue;
                stat.modifiers.Remove(modToRemove);
                stat.onValueChanged?.Invoke(oldValue, stat.effectiveValue);
                return true;
            }
            return false;
        }

        public static void SetCounterAmount(this CardObject card, SVEProperties.Counters counter, int amount) => SetCounterAmount(card.RuntimeCard, counter, amount);
        public static void SetCounterAmount(this RuntimeCard card, SVEProperties.Counters counter, int amount)
        {
            RuntimeKeyword counterAsKeyword = card.keywords.FirstOrDefault(x => x.keywordId == (int)counter);
            if(counterAsKeyword != null)
                card.RemoveKeyword(counterAsKeyword.keywordId, counterAsKeyword.valueId);
            if(amount > 0)
                card.AddKeyword((int)counter, amount);
        }

        #endregion

        // ------------------------------

        #region Manage Zones

        public static void AddCardToTop(this RuntimeZone zone, RuntimeCard card)
        {
            if(zone.cards.Count < zone.maxCards && !zone.cards.Contains(card))
            {
                zone.cards.Insert(0, card);
                zone.numCards++; // calls onZoneChanged
                zone.onCardAdded?.Invoke(card);
            }
        }

        public static RuntimeCard GetCardOnFieldOrEXArea(this PlayerInfo player, int cardInstanceId)
        {
            RuntimeCard card = player.namedZones[SVEProperties.Zones.Field].cards.Find(x => x.instanceId == cardInstanceId);
            card ??= player.namedZones[SVEProperties.Zones.ExArea].cards.Find(x => x.instanceId == cardInstanceId);
            return card;
        }

        #endregion

        // ------------------------------

        #region Initialize Or Copy

        public static void InitializeFromLibraryCard(this RuntimeCard runtimeCard, Card libraryCard, int instanceId, PlayerInfo player)
        {
            runtimeCard.cardId = libraryCard.id;
            runtimeCard.instanceId = instanceId;
            runtimeCard.ownerPlayer = player;

            foreach(Stat stat in libraryCard.stats)
            {
                runtimeCard.stats[stat.statId] = stat.Copy();
                runtimeCard.namedStats[stat.name] = runtimeCard.stats[stat.statId];
            }
            foreach(RuntimeKeyword keyword in libraryCard.keywords)
            {
                runtimeCard.keywords.Add(keyword.Copy());
            }
        }

        public static Stat Copy(this Stat stat)
        {
            Stat newStat = new()
            {
                statId = stat.statId,
                name = stat.name,
                originalValue = stat.originalValue,
                baseValue = stat.baseValue,
                minValue = stat.minValue,
                maxValue = stat.maxValue
            };
            return newStat;
        }

        public static RuntimeKeyword Copy(this RuntimeKeyword keyword)
        {
            RuntimeKeyword newKeyword = new RuntimeKeyword
            {
                keywordId = keyword.keywordId,
                valueId = keyword.valueId
            };
            return newKeyword;
        }

        #endregion
    }
}
