using System;
using System.Collections.Generic;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public class DiscardEffect : ChooseFromHandEffect
    {
        protected override string ActionText => "Discard";

        protected override void ConfirmationAction(PlayerController player, List<CardObject> selectedCards, Action onComplete)
        {
            foreach(CardObject target in selectedCards)
            {
                player.LocalEvents.SendToCemetery(target, SVEProperties.Zones.Hand);
            }
            onComplete?.Invoke();
        }
    }
}
