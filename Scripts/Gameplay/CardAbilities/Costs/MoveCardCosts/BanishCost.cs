using System;
using System.Collections.Generic;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public class BanishCost : SendTargetToZoneCost
    {
        protected override string TargetZoneName => SVEProperties.Zones.Banished;
        protected override string CostName => nameof(BanishCost);

        // ------------------------------

        protected override void MoveCardObjectsToTargetZone(PlayerController player, List<CardObject> cards)
        {
            foreach(CardObject card in cards)
                player.LocalEvents.BanishCard(card, sendMessage: false, onlyMoveObject: true);
        }
    }
}
