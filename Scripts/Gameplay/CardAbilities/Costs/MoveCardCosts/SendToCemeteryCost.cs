using System;
using System.Collections.Generic;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public class SendToCemeteryCost : SendTargetToZoneCost
    {
        protected override string TargetZoneName => SVEProperties.Zones.Cemetery;
        protected override string CostName => nameof(SendToCemeteryCost);

        // ------------------------------

        protected override void MoveCardObjectsToTargetZone(PlayerController player, List<CardObject> cards)
        {
            foreach(CardObject card in cards)
                player.LocalEvents.SendToCemetery(card, onlyMoveObject: true);
        }
    }
}
