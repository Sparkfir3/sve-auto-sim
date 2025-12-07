using System;
using UnityEngine;
using Sirenix.OdinInspector;
using Sparkfire.Utility;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using CCGKit;
using SVESimulator.Database;

namespace SVESimulator.DeckBuilder
{
    [DefaultExecutionOrder(-1000)]
    public class DeckBuilderModel : MonoBehaviour
    {
        #region Variables

        [field: Title("Runtime Data"), SerializeField]
        public DeckBuilderFilters Filters { get; set; }
        [field: SerializeField]
        public CardListSorting.SortMode SortMode;
        [field: SerializeField]
        public string DeckClass { get; set; }
        [ShowInInspector]
        public int FilteredListCount => _filteredCardList.Count;
        [ShowInInspector]
        public int MainDeckCount => CurrentMainDeck.Sum(x => x.Value);
        [ShowInInspector]
        public int EvolveDeckCount => CurrentEvolveDeck.Sum(x => x.Value);
        [field: SerializeField, ReadOnly]
        public bool IsDirty { get; set; }

        public GameConfiguration gameConfig;

        private List<Card> _filteredCardList = new();
        public List<Card> FilteredCardList => _filteredCardList;

        public Card CurrentLeader { get; private set; }
        public Dictionary<Card, int> CurrentMainDeck { get; private set; } = new();
        public Dictionary<Card, int> CurrentEvolveDeck { get; private set; } = new();

        public Action OnUpdateFilters;
        public event Action OnUpdateFilteredCardList;
        public event Action OnUpdateDeck;

        #endregion

        // ------------------------------

        #region Unity Functions

        private void Awake()
        {
            IsDirty = false;
            gameConfig = new GameConfiguration();
            gameConfig.LoadGameConfigurationAtRuntime();
            Debug.Log($"Deck builder loaded {gameConfig.cards.Count} cards in {gameConfig.cardSets.Count} sets from library");
        }

        #endregion

        // ------------------------------

        #region Filtering

        [TitleGroup("Buttons"), Button, HideInEditorMode]
        public void UpdateFilteredCardList()
        {
            _filteredCardList.Clear();
            foreach(CardSet set in gameConfig.cardSets)
            {
                if(Filters.sets is { Count: > 0 } && !Filters.sets.Any(x => x.Equals(set.name)))
                    continue;
                _filteredCardList.AddRange(set.cards.OrderBy(x => x.id));
            }

            FilterByName(ref _filteredCardList);
            FilterByCardType(ref _filteredCardList);
            FilterByClass(ref _filteredCardList);
            if(Filters.useCost)
                FilterByStat(ref _filteredCardList, Filters.minCost, Filters.maxCost, SVEProperties.CardStats.Cost, 8);
            if(Filters.useAttack)
                FilterByStat(ref _filteredCardList, Filters.minAttack, Filters.maxAttack, SVEProperties.CardStats.Attack);
            if(Filters.useDefense)
                FilterByStat(ref _filteredCardList, Filters.minDefense, Filters.maxDefense, SVEProperties.CardStats.Defense);
            // FilterByRarity(ref _filteredCardList); NOT CURRENTLY USED

            CardListSorting.SortCardList(ref _filteredCardList, SortMode);
            OnUpdateFilteredCardList?.Invoke();
        }

        private void FilterByName(ref List<Card> cardList)
        {
            if(Filters.text.IsNullOrWhiteSpace())
                return;

            cardList = cardList.Where(x => x.name.ToLower().Contains(Filters.text.ToLower())).ToList();
        }

        private void FilterByCardType(ref List<Card> cardList)
        {
            if(Filters.cardType <= 0)
                return;

            List<int> cardTypeIds = new();
            if(Filters.cardType.HasFlag(CardTypeFilter.Follower)) // Follower
                cardTypeIds.Add(0);
            if(Filters.cardType.HasFlag(CardTypeFilter.Evolved)) // Evolved Follower
                cardTypeIds.Add(1);
            if(Filters.cardType.HasFlag(CardTypeFilter.Spell)) // Spell & Evolved Spell
            {
                cardTypeIds.Add(2);
                cardTypeIds.Add(3);
            }
            if(Filters.cardType.HasFlag(CardTypeFilter.Amulet)) // Amulet
                cardTypeIds.Add(4);
            if(Filters.cardType.HasFlag(CardTypeFilter.Leader)) // Leader
                cardTypeIds.Add(5);

            if(Filters.cardType.HasFlag(CardTypeFilter.Leader)) // Leader
                cardTypeIds.Add(5);

            bool checkTokens = Filters.cardType.HasFlag(CardTypeFilter.Token);
            if(cardTypeIds.Count == 0 && !checkTokens)
                return;
            cardList = cardList.Where(x =>
            {
                if(x.properties.Any(y => y is StringProperty property && y.name.Equals(SVEProperties.CardStats.Trait) && property.value.Contains("Token")))
                    return checkTokens;
                return cardTypeIds.Count == 0 || cardTypeIds.Contains(x.cardTypeId);
            }).ToList();
        }

        private void FilterByClass(ref List<Card> cardList)
        {
            if(Filters.cardClass <= 0)
                return;

            List<string> classNames = new();
            if(Filters.cardClass.HasFlag(ClassFilter.Forest))
                classNames.Add(SVEProperties.CardClass.Forest);
            if(Filters.cardClass.HasFlag(ClassFilter.Sword))
                classNames.Add(SVEProperties.CardClass.Sword);
            if(Filters.cardClass.HasFlag(ClassFilter.Rune))
                classNames.Add(SVEProperties.CardClass.Rune);
            if(Filters.cardClass.HasFlag(ClassFilter.Dragon))
                classNames.Add(SVEProperties.CardClass.Dragon);
            if(Filters.cardClass.HasFlag(ClassFilter.Abyss))
                classNames.Add(SVEProperties.CardClass.Abyss);
            if(Filters.cardClass.HasFlag(ClassFilter.Haven))
                classNames.Add(SVEProperties.CardClass.Haven);
            if(Filters.cardClass.HasFlag(ClassFilter.Neutral))
                classNames.Add(SVEProperties.CardClass.Neutral);

            if(classNames.Count == 0)
                return;
            _filteredCardList = _filteredCardList.Where(x =>
            {
                string cardClass = x.GetStringProperty(SVEProperties.CardStats.Class);
                return classNames.Any(y => cardClass.Equals(y));
            }).ToList();
        }

        private void FilterByStat(ref List<Card> cardList, int min, int max, string statName, int maxLimit = 10)
        {
            if(min <= 0 && max >= maxLimit)
                return;
            if(max >= maxLimit)
                max = 99;

            cardList = cardList.Where(x =>
            {
                Stat stat = x.stats.FirstOrDefault(y => y.name.Equals(statName));
                return stat != null && (stat.baseValue >= min && stat.baseValue <= max);
            }).ToList();
        }

        // NOT CURRENTLY USED
        // private void FilterByRarity(ref List<Card> cardList)
        // {
        //     if(Filters.rarity <= 0)
        //         return;
        //
        //     List<string> rarities = new();
        //     if(Filters.rarity.HasFlag(RarityFilter.Legendary))
        //         rarities.Add(SVEProperties.CardRarity.Legendary);
        //     if(Filters.rarity.HasFlag(RarityFilter.Gold))
        //         rarities.Add(SVEProperties.CardRarity.Gold);
        //     if(Filters.rarity.HasFlag(RarityFilter.Silver))
        //         rarities.Add(SVEProperties.CardRarity.Silver);
        //     if(Filters.rarity.HasFlag(RarityFilter.Bronze))
        //         rarities.Add(SVEProperties.CardRarity.Bronze);
        //     if(Filters.rarity.HasFlag(RarityFilter.Token))
        //         rarities.Add(SVEProperties.CardRarity.Token);
        //
        //     if(rarities.Count == 0)
        //         return;
        //     cardList = cardList.Where(x =>
        //     {
        //         StringProperty rarityProperty = x.properties.Find(y => y.name.Equals(SVEProperties.CardStats.Rarity) && y is StringProperty) as StringProperty;
        //         if(rarityProperty == null)
        //             return false;
        //         string cardRarity = rarityProperty.value;
        //         return rarities.Any(y => cardRarity.Equals(y));
        //     }).ToList();
        // }

        #endregion

        // ------------------------------

        #region Deck

        [TitleGroup("Buttons"), Button, HideInEditorMode]
        public void ImportDeck(string data)
        {
            CurrentLeader = null;
            CurrentMainDeck.Clear();
            CurrentEvolveDeck.Clear();

            List<DeckSaveLoadUtils.CardAmountPair> cardAsArray = DeckSaveLoadUtils.LoadDeck(data, out string deckClass);
            DeckClass = deckClass;
            foreach(DeckSaveLoadUtils.CardAmountPair cardAmount in cardAsArray)
            {
                Card card = gameConfig.cards.FirstOrDefault(x => x.id == cardAmount.id);
                if(card == null)
                {
                    Debug.LogError($"Failed to find card in database with ID {cardAmount.id} (desired amount: {cardAmount.amount})");
                    continue;
                }

                switch(card.cardTypeId)
                {
                    case 5: // Leader
                        CurrentLeader = card;
                        break;
                    case 1: // Evolved
                    case 3:
                        CurrentEvolveDeck.Add(card, cardAmount.amount);
                        break;
                    default: // Main Deck
                        CurrentMainDeck.Add(card, cardAmount.amount);
                        break;
                }
            }
        }

        public void AddCard(Card card)
        {
            if(card == null)
                return;
            StringProperty traitProperty = card.properties.Find(x => x.name.Equals(SVEProperties.CardStats.Trait) && x is StringProperty) as StringProperty;
            if(traitProperty != null && (traitProperty.value?.Contains("/ Token") ?? false))
                return;

            // Leader
            if(card.cardTypeId == 5)
            {
                if(CurrentLeader == null || card.id != CurrentLeader.id)
                {
                    CurrentLeader = card;
                    IsDirty = true;
                    OnUpdateDeck?.Invoke();
                }
                return;
            }

            // Main/Evolve Deck
            var targetDeck = card.cardTypeId is 1 or 3 ? CurrentEvolveDeck : CurrentMainDeck;
            if(!targetDeck.TryAdd(card, 1))
            {
                if(targetDeck[card] >= 3)
                    return;
                targetDeck[card] = Mathf.Clamp(targetDeck[card] + 1, 1, 3);
                IsDirty = true;
            }
            OnUpdateDeck?.Invoke();
        }

        public void RemoveCard(Card card)
        {
            if(card == null)
                return;

            // Leader
            if(card.cardTypeId == 5 && CurrentLeader == card)
            {
                CurrentLeader = null;
                IsDirty = true;
                OnUpdateDeck?.Invoke();
                return;
            }

            // Main/Evolve Deck
            var targetDeck = card.cardTypeId is 1 or 3 ? CurrentEvolveDeck : CurrentMainDeck;
            if(!targetDeck.TryGetValue(card, out int count))
                return;
            if(count <= 1)
                targetDeck.Remove(card);
            else
                targetDeck[card] -= 1;
            IsDirty = true;
            OnUpdateDeck?.Invoke();
        }

        public string DeckAsString()
        {
            string leaderClass = !DeckClass.IsNullOrWhiteSpace()
                ? DeckClass
                : CurrentLeader == null ? "Neutral" : CurrentLeader.GetStringProperty(SVEProperties.CardStats.Class);
            string data = $"{leaderClass} v1\n\n" +

            "# Leader\n" +
            $"{(CurrentLeader != null ? $"1 {CurrentLeader.GetStringProperty(SVEProperties.CardStats.ID)}\n" : "")}\n" +
            "# Main Deck\n" +
            $"{string.Join("\n", CurrentMainDeck.Select(x => $"{x.Value} {x.Key.GetStringProperty(SVEProperties.CardStats.ID)}"))}\n\n" +
            "# Evolve Deck\n" +
            $"{string.Join("\n", CurrentEvolveDeck.Select(x => $"{x.Value} {x.Key.GetStringProperty(SVEProperties.CardStats.ID)}"))}";

            return data;
        }

        public int GetCardAmount(Card card)
        {
            if(card == null)
                return 0;
            if(card == CurrentLeader)
                return 1;
            if(CurrentMainDeck.TryGetValue(card, out int count) || CurrentEvolveDeck.TryGetValue(card, out count))
                return count;
            return 0;
        }

        #endregion

        // ------------------------------

        #region Validation

        public bool IsDeckValid(out DeckConstructionErrors errors)
        {
            errors = DeckConstructionErrors.None;

            if(CurrentLeader == null)
                errors |= DeckConstructionErrors.NoLeader;
            int mainDeckCount = CurrentMainDeck.Sum(x => x.Value);
            if(mainDeckCount < 40)
                errors |= DeckConstructionErrors.TooFewMainDeck;
            else if(mainDeckCount > 50)
                errors |= DeckConstructionErrors.TooMuchMainDeck;
            int evolveDeckCount = CurrentEvolveDeck.Sum(x => x.Value);
            if(evolveDeckCount > 10)
                errors |= DeckConstructionErrors.TooMuchEvolveDeck;

            return errors == DeckConstructionErrors.None;
        }

        #endregion
    }
}
