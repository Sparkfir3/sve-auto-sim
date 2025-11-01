using System;
using System.Collections.Generic;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public class ReturnToHandCost : SendTargetToZoneCost
    {
        protected override string TargetZoneName => SVEProperties.Zones.Hand;
        protected override string CostName => nameof(ReturnToHandCost);

        // ------------------------------

        protected override void MoveCardObjectsToTargetZone(PlayerController player, List<CardObject> cards)
        {
            foreach(CardObject card in cards)
                player.LocalEvents.ReturnToHand(card, card.CurrentZone.Runtime.name, onlyMoveObject: true);
        }
    }
}
