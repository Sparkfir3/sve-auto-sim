using System;
using System.Collections.Generic;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public class SummonFromEvolveDeckEffect : ChooseFromEvolveDeckEffect
    {
        protected override bool TargetFaceDownCards => true;
        protected override bool TargetFaceUpCards => false;

        // ------------------------------

        protected override void ConfirmationAction(PlayerController player, List<CardObject> selectedCards, Action onComplete)
        {
            foreach(CardObject card in selectedCards)
            {
                card.Interactable = player.isActivePlayer;
                player.LocalEvents.PlayCardToField(card, SVEProperties.Zones.EvolveDeck, payCost: false);
            }
            onComplete?.Invoke();
        }
    }
}
