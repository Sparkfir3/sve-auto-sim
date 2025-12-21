using System;
using System.Linq;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public class OncePerTurnCost : SveCost
    {
        public override bool IsInternalCost => true;

        public override string GetReadableString(GameConfiguration config)
        {
            return "Once Per Turn";
        }

        public override bool CanPayCost(PlayerController player, RuntimeCard card, string abilityName)
        {
            return !player.AbilitiesUsedThisTurn.Any(x => x.instanceId == card.instanceId && x.cardId == card.cardId && x.abilityName.Equals(abilityName));
        }
    }
}
