using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using CCGKit;
using Sparkfire.Utility;

namespace SVESimulator
{
    public class SearchMultiFilterEffect : SearchDeckEffect
    {
        [StringField("Amount 2", width = 100), Order(21)]
        public string amount2;
        [StringField("Filter 2", width = 100), Order(22)]
        public string filter2;

        [StringField("Amount 3", width = 100), Order(31)]
        public string amount3;
        [StringField("Filter 3", width = 100), Order(32)]
        public string filter3;

        private int parsedAmount1;
        private Dictionary<SVEFormulaParser.CardFilterSetting, string> filterDict1;
        private int parsedAmount2;
        private Dictionary<SVEFormulaParser.CardFilterSetting, string> filterDict2;
        private int parsedAmount3;
        private Dictionary<SVEFormulaParser.CardFilterSetting, string> filterDict3;

        private CardSelectionArea selectionArea;

        // ------------------------------

        protected override void GetMinMax(PlayerController player, out int min, out int max)
        {
            SVEFormulaParser.ParseValueAsMinMax(amount, player, out _, out parsedAmount1);
            SVEFormulaParser.ParseValueAsMinMax(amount2, player, out _, out parsedAmount2);
            SVEFormulaParser.ParseValueAsMinMax(amount3, player, out _, out parsedAmount3);
            min = 0; // searches can always fail or be declined
            max = parsedAmount1 + parsedAmount2 + parsedAmount3;
            if(searchDeckAction == SearchDeckAction.Field)
                max = Mathf.Min(max, player.ZoneController.fieldZone.OpenSlotCount());
            else if(searchDeckAction == SearchDeckAction.ExArea)
                max = Mathf.Min(max, player.ZoneController.exAreaZone.OpenSlotCount());
        }

        protected override void InitializeSelectionArea(PlayerController player, CardSelectionArea selectionArea)
        {
            this.selectionArea = selectionArea;
            int cardCount = player.ZoneController.deckZone.Runtime.cards.Count;
            selectionArea.Enable(CardSelectionArea.SelectionMode.SelectCardsFromDeck, cardCount, cardCount, slotBackgroundsActive: false);
            selectionArea.SetFilter($"{filter}|({filter2}|({filter3}))");
            selectionArea.AddAllCardsInDeck();
            selectionArea.OnUpdateSelection += UpdateFilters;
            selectionArea.alwaysAllowDeselect = true;

            filterDict1 = !filter.IsNullOrWhiteSpace() ? SVEFormulaParser.ParseCardFilterFormula(filter) : null;
            filterDict2 = !filter2.IsNullOrWhiteSpace() ? SVEFormulaParser.ParseCardFilterFormula(filter2) : null;
            filterDict3 = !filter3.IsNullOrWhiteSpace() ? SVEFormulaParser.ParseCardFilterFormula(filter3) : null;
        }

        // ------------------------------

        private void UpdateFilters()
        {
            List<CardObject> selectedCards = selectionArea.CurrentSelectedCards;
            List<int> filterSlots = new();
            AddFilterSlots(0, filterDict1, parsedAmount1);
            AddFilterSlots(1, filterDict2, parsedAmount2);
            AddFilterSlots(2, filterDict3, parsedAmount3);

            int[] assignedCardIndices = Enumerable.Repeat(-1, filterSlots.Count).ToArray();
            for(int cardIndex = 0; cardIndex < selectedCards.Count; cardIndex++)
                TryAssignCard(cardIndex, new bool[filterSlots.Count]);

            int matchingFilter1 = 0;
            int matchingFilter2 = 0;
            int matchingFilter3 = 0;
            for(int slotIndex = 0; slotIndex < assignedCardIndices.Length; slotIndex++)
            {
                if(assignedCardIndices[slotIndex] < 0)
                    continue;

                switch(filterSlots[slotIndex])
                {
                    case 0:
                        matchingFilter1++;
                        break;
                    case 1:
                        matchingFilter2++;
                        break;
                    case 2:
                        matchingFilter3++;
                        break;
                }
            }

            bool filter1Open = matchingFilter1 < parsedAmount1;
            bool filter2Open = matchingFilter2 < parsedAmount2;
            bool filter3Open = matchingFilter3 < parsedAmount3;

            string assignedFilter = filter1Open ? filter : null;
            if(filter2Open)
                assignedFilter = assignedFilter == null ? filter2 : $"{assignedFilter}|({filter2})";
            if(filter3Open)
                assignedFilter = assignedFilter == null ? filter3 : $"{assignedFilter}|({filter3})";
            selectionArea.SetFilter(assignedFilter);

            // -----

            void AddFilterSlots(int filterIndex, Dictionary<SVEFormulaParser.CardFilterSetting, string> filterDict, int amount)
            {
                if(filterDict == null)
                    return;

                for(int i = 0; i < Mathf.Min(Mathf.Max(amount, 0), selectedCards.Count); i++)
                    filterSlots.Add(filterIndex);
            }

            bool TryAssignCard(int cardIndex, bool[] visitedSlots)
            {
                for(int slotIndex = 0; slotIndex < filterSlots.Count; slotIndex++)
                {
                    if(visitedSlots[slotIndex] || !MatchesFilter(selectedCards[cardIndex], filterSlots[slotIndex]))
                        continue;

                    visitedSlots[slotIndex] = true;
                    if(assignedCardIndices[slotIndex] < 0 || TryAssignCard(assignedCardIndices[slotIndex], visitedSlots))
                    {
                        assignedCardIndices[slotIndex] = cardIndex;
                        return true;
                    }
                }

                return false;
            }

            bool MatchesFilter(CardObject card, int filterIndex)
            {
                return filterIndex switch
                {
                    0 => filterDict1.MatchesCard(card),
                    1 => filterDict2.MatchesCard(card),
                    _ => filterDict3.MatchesCard(card)
                };
            }
        }
    }
}
