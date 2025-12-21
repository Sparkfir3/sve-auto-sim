using System;
using System.Collections.Generic;
using UnityEngine;
using CCGKit;
using Sparkfire.Utility;

namespace SVESimulator
{
    public class SearchDeckEffect : ChooseFromCardStackEffect
    {
        public enum SearchDeckAction { Hand, Cemetery }

        [StringField("Action", width = 100), Order(10)]
        public SearchDeckAction searchDeckAction;

        // ------------------------------

        protected override void GetMinMax(PlayerController player, out int min, out int max)
        {
            SVEFormulaParser.ParseValueAsMinMax(amount, player, out _, out max);
            min = 0; // searches can always fail or be declined
        }

        protected override void InitializeSelectionArea(PlayerController player, CardSelectionArea selectionArea)
        {
            int cardCount = player.ZoneController.deckZone.Runtime.cards.Count;
            selectionArea.Enable(CardSelectionArea.SelectionMode.SelectCardsFromDeck, cardCount, cardCount);
            selectionArea.SetFilter(filter);
            selectionArea.AddAllCardsInDeck();
        }

        protected override void ConfirmationAction(PlayerController player, List<CardObject> selectedCards, Action onComplete)
        {
            foreach(CardObject card in selectedCards)
            {
                switch(searchDeckAction)
                {
                    case SearchDeckAction.Hand:
                        card.Interactable = player.isActivePlayer;
                        player.LocalEvents.DrawCard(card, reveal: !filter.IsNullOrWhiteSpace());
                        break;
                    case SearchDeckAction.Cemetery:
                        card.Interactable = false;
                        player.LocalEvents.SendToCemetery(card, SVEProperties.Zones.Deck);
                        break;
                    default:
                        Debug.LogError($"Search deck action {searchDeckAction} is not implemented.");
                        break;
                }
            }
            onComplete?.Invoke();
        }

        protected override void OnCompleteInternal(PlayerController player)
        {
            player.LocalEvents.ShuffleDeck();
        }
    }
}
