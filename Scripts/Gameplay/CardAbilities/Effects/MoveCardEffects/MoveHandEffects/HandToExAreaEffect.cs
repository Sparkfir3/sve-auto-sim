using System;
using System.Collections.Generic;
using UnityEngine;

namespace SVESimulator
{
    public class HandToExAreaEffect : ChooseFromHandEffect
    {
        protected override string ActionText => "Put into EX Area";

        protected override void GetMinMax(PlayerController player, out int min, out int max)
        {
            base.GetMinMax(player, out min, out max);
            max = Mathf.Min(max, player.ZoneController.exAreaZone.OpenSlotCount());
            min = Mathf.Min(min, max);
        }

        protected override void ConfirmationAction(PlayerController player, List<CardObject> selectedCards, Action onComplete)
        {
            foreach(CardObject target in selectedCards)
            {
                player.LocalEvents.SendToExArea(target, SVEProperties.Zones.Hand);
                target.Interactable = player.isActivePlayer;
            }
            onComplete?.Invoke();
        }
    }
}
