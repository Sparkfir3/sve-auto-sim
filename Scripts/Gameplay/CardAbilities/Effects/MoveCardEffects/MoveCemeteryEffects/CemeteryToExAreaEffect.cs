using System;
using System.Collections.Generic;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public class CemeteryToExAreaEffect : CemeteryToFieldEffect
    {
        protected override string debugName => "CemeteryToExArea";

        // ------------------------------

        protected override void ConfirmationAction(PlayerController player, List<CardObject> selectedCards, Action onComplete)
        {
            foreach(CardObject card in selectedCards)
            {
                player.LocalEvents.SendToExArea(card, SVEProperties.Zones.Cemetery);
                card.Interactable = player.isActivePlayer;
            }
            onComplete?.Invoke();
        }

        protected override void GetMinMax(PlayerController player, out int min, out int max)
        {
            // Can't call base() because it would call CemeteryToFieldEffect and not ChooseFromCardStackEffect, too lazy to refactor it to be "proper" when it's one line of code
            SVEFormulaParser.ParseValueAsMinMax(amount, player, out min, out max);

            max = Mathf.Min(max, player.ZoneController.exAreaZone.OpenSlotCount());
            min = Mathf.Min(min, max);
        }
    }
}
