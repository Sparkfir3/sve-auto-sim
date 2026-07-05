using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CCGKit;
using Sparkfire.Utility;
using SVESimulator.UI;

namespace SVESimulator
{
    public class SearchDeckFilteredOptionsEffect : SearchDeckEffect
    {
        [StringField("Filter 2", width = 100), Order(20)]
        public string filter2;
        [StringField("Action 2", width = 100), Order(21)]
        public SearchDeckAction searchDeckAction2;

        [StringField("Filter 3", width = 100), Order(30)]
        public string filter3;
        [StringField("Action 3", width = 100), Order(31)]
        public SearchDeckAction searchDeckAction3;

        // ------------------------------

        protected override void InitializeSelectionArea(PlayerController player, CardSelectionArea selectionArea)
        {
            int cardCount = player.ZoneController.deckZone.Runtime.cards.Count;
            selectionArea.Enable(CardSelectionArea.SelectionMode.SelectCardsFromDeck, cardCount, cardCount, slotBackgroundsActive: false);
            string subfilter = !filter2.IsNullOrWhiteSpace() ? $"|({filter2}{(!filter3.IsNullOrWhiteSpace() ? $"|({filter3})" : "")})"
                : (!filter3.IsNullOrWhiteSpace() ? $"|({filter3})" : "");
            selectionArea.SetFilter($"{filter}{subfilter}");
            selectionArea.AddAllCardsInDeck();
        }

        protected override void SetSelectionAreaAction(PlayerController player, CardSelectionArea selectionArea, in string cardName, in int minSelect, in int maxSelect, Action onSelect)
        {
            selectionArea.SetConfirmAction(cardName, searchDeckAction.ToString(), text, minSelect, maxSelect, selectedCards =>
            {
                selectionArea.Disable();
                ConfirmationAction(player, selectedCards, onSelect);
            });
            if(searchDeckAction2 != SearchDeckAction.None)
            {
                selectionArea.AddAdditionalConfirmAction(searchDeckAction2.ToString(), selectedCards =>
                {
                    selectionArea.Disable();
                    ConfirmationAction2(player, selectedCards, onSelect);
                });
            }
            if(searchDeckAction3 != SearchDeckAction.None)
            {
                selectionArea.AddAdditionalConfirmAction(searchDeckAction3.ToString(), selectedCards =>
                {
                    selectionArea.Disable();
                    ConfirmationAction3(player, selectedCards, onSelect);
                });
            }
            selectionArea.UpdateActionButtonOverride = UpdateActionButton;
            UpdateActionButton(selectionArea);
        }

        // ------------------------------

        protected void ConfirmationAction2(PlayerController player, List<CardObject> selectedCards, Action onComplete)
        {
            PerformSearchDeckAction(player, searchDeckAction2, selectedCards);
            onComplete?.Invoke();
        }

        protected void ConfirmationAction3(PlayerController player, List<CardObject> selectedCards, Action onComplete)
        {
            PerformSearchDeckAction(player, searchDeckAction3, selectedCards);
            onComplete?.Invoke();
        }

        protected void UpdateActionButton(CardSelectionArea selectionArea)
        {
            // TODO - cache filter dictionaries and/or manual iterate instead of linq if it becomes problematic
            int i = 0;
            if(selectionArea.CurrentSelectedCards.Count == 0)
            {
                if(searchDeckAction != SearchDeckAction.None)
                    GameUIManager.MultipleChoice.SetButtonActive(i++, false);
                if(searchDeckAction2 != SearchDeckAction.None)
                    GameUIManager.MultipleChoice.SetButtonActive(i++, false);
                if(searchDeckAction3 != SearchDeckAction.None)
                    GameUIManager.MultipleChoice.SetButtonActive(i, false);
                return;
            }

            if(searchDeckAction != SearchDeckAction.None)
            {
                var filterDict = SVEFormulaParser.ParseCardFilterFormula(filter);
                GameUIManager.MultipleChoice.SetButtonActive(i++, selectionArea.CurrentSelectedCards.All(x => filterDict.MatchesCard(x)));
            }
            if(searchDeckAction2 != SearchDeckAction.None)
            {
                var filterDict = SVEFormulaParser.ParseCardFilterFormula(filter2);
                GameUIManager.MultipleChoice.SetButtonActive(i++, selectionArea.CurrentSelectedCards.All(x => filterDict.MatchesCard(x)));
            }
            if(searchDeckAction3 != SearchDeckAction.None)
            {
                var filterDict = SVEFormulaParser.ParseCardFilterFormula(filter3);
                GameUIManager.MultipleChoice.SetButtonActive(i, selectionArea.CurrentSelectedCards.All(x => filterDict.MatchesCard(x)));
            }
        }
    }
}
