using System;
using System.Collections.Generic;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public class HandToFieldEffect : ChooseFromHandEffect
    {
        protected override string ActionText => "Put onto Field";

        protected override void GetMinMax(PlayerController player, out int min, out int max)
        {
            base.GetMinMax(player, out min, out max);
            max = Mathf.Max(max, player.ZoneController.fieldZone.OpenSlotCount());
            min = Mathf.Min(min, max);
        }

        protected override void ConfirmationAction(PlayerController player, List<CardObject> selectedCards, Action onComplete)
        {
            foreach(CardObject target in selectedCards)
            {
                if(!player.LocalEvents.PlayCardToField(target, SVEProperties.Zones.Hand, payCost: false))
                {
                    Debug.LogError($"HandToFieldEffect Effect - Failed to play target card with instance ID {target.RuntimeCard.instanceId}");
                    continue;
                }
                target.Interactable = player.isActivePlayer;
            }
            onComplete?.Invoke();
        }
    }
}
