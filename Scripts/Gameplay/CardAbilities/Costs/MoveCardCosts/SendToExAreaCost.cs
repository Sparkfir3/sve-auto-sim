using System;
using System.Collections.Generic;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public class SendToExAreaCost : SendTargetToZoneCost
    {
        protected override string TargetZoneName => SVEProperties.Zones.ExArea;
        protected override string CostName => nameof(SendToExAreaCost);

        // ------------------------------

        protected override void MoveCardObjectsToTargetZone(PlayerController player, List<CardObject> cards)
        {
            foreach(CardObject card in cards)
                player.LocalEvents.SendToExArea(card, onlyMoveObject: true);
        }
    }
}
