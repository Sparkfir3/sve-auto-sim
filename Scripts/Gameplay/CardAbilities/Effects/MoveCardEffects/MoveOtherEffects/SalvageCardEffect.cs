using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public class SalvageCardEffect : ChooseFromCemeteryEffect
    {
        protected override void ConfirmationAction(PlayerController player, List<CardObject> selectedCards, Action onComplete)
        {
            foreach(CardObject card in selectedCards)
            {
                card.Interactable = player.isActivePlayer;
                player.LocalEvents.ReturnToHand(card, SVEProperties.Zones.Cemetery);
            }
            onComplete?.Invoke();
        }
    }
}
