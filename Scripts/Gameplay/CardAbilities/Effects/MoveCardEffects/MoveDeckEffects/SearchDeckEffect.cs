using System;
using System.Collections.Generic;
using UnityEngine;
using CCGKit;
using Sparkfire.Utility;

namespace SVESimulator
{
    public class SearchDeckEffect : ChooseFromCardStackEffect
    {
        public enum SearchDeckAction { Hand, Cemetery, Field, ExArea }

        [StringField("Action", width = 100), Order(10)]
        public SearchDeckAction searchDeckAction;

        // ------------------------------

        protected override void GetMinMax(PlayerController player, out int min, out int max)
        {
            SVEFormulaParser.ParseValueAsMinMax(amount, player, out _, out max);
            min = 0; // searches can always fail or be declined
            if(searchDeckAction == SearchDeckAction.ExArea)
                max = Mathf.Min(max, player.ZoneController.exAreaZone.OpenSlotCount());
        }

        protected override void InitializeSelectionArea(PlayerController player, CardSelectionArea selectionArea)
        {
            int cardCount = player.ZoneController.deckZone.Runtime.cards.Count;
            selectionArea.Enable(CardSelectionArea.SelectionMode.SelectCardsFromDeck, cardCount, cardCount, slotBackgroundsActive: false);
            selectionArea.SetFilter(filter);
            selectionArea.AddAllCardsInDeck();
        }

        protected override void ConfirmationAction(PlayerController player, List<CardObject> selectedCards, Action onComplete)
        {
            PerformSearchDeckAction(player, selectedCards);
            onComplete?.Invoke();
        }

        protected virtual void PerformSearchDeckAction(PlayerController player, List<CardObject> selectedCards)
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
                    case SearchDeckAction.Field:
                        card.Interactable = player.isActivePlayer;
                        player.LocalEvents.PlayCardToField(card, SVEProperties.Zones.Deck, payCost: false);
                        break;
                    case SearchDeckAction.ExArea:
                        card.Interactable = player.isActivePlayer;
                        player.LocalEvents.SendToExArea(card, SVEProperties.Zones.Deck);
                        break;
                    default:
                        Debug.LogError($"Search deck action {searchDeckAction} is not implemented.");
                        break;
                }
            }
        }

        protected override void OnCompleteInternal(PlayerController player)
        {
            player.LocalEvents.ShuffleDeck();
        }
    }
}
