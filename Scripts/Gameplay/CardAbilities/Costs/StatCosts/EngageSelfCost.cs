using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public class EngageSelfCost : SveCost
    {
        public override string GetReadableString(GameConfiguration config)
        {
            return $"Engage Self";
        }

        public override bool CanPayCost(PlayerController player, RuntimeCard card, string abilityName)
        {
            return card.namedStats.TryGetValue(SVEProperties.CardStats.Engaged, out Stat engageStat) && engageStat.effectiveValue == 0;
        }

        public override IEnumerator PayCost(PlayerController player, CardObject card, string abilityName, List<MoveCardToZoneData> cardsToMove)
        {
            CardManager.Animator.RotateCard(card, SVEProperties.CardEngagedRotation);
            yield return null;
        }
    }
}
