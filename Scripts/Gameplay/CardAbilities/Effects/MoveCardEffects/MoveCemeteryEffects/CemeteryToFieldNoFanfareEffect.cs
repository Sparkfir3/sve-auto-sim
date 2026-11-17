using System;
using System.Collections.Generic;
using UnityEngine;

namespace SVESimulator
{
    public class CemeteryToFieldNoFanfareEffect : CemeteryToFieldEffect
    {
        protected override string debugName => nameof(CemeteryToFieldNoFanfareEffect);

        protected override void ConfirmationAction(PlayerController player, List<CardObject> selectedCards, Action onComplete)
        {
            foreach(CardObject card in selectedCards)
            {
                if(!player.LocalEvents.PlayCardToField(card, SVEProperties.Zones.Cemetery, payCost: false, triggerFanfare: false))
                {
                    Debug.LogError($"{debugName} Effect - Failed to play target card with instance ID {card.RuntimeCard.instanceId}");
                    continue;
                }
                card.Interactable = player.isActivePlayer;
            }
            onComplete?.Invoke();
        }
    }
}
