using System;
using System.Collections.Generic;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public class DiscardToBottomDeckEffect : DiscardEffect
    {
        protected override void ConfirmationAction(PlayerController player, List<CardObject> selectedCards, Action onComplete)
        {
            foreach(CardObject target in selectedCards)
            {
                player.LocalEvents.SendToBottomDeck(target, SVEProperties.Zones.Hand);
            }
            onComplete?.Invoke();
        }
    }
}
