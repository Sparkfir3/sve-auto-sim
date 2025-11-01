using System;
using CCGKit;
using UnityEngine;
using StatBoostType = SVESimulator.SVEProperties.StatBoostType;

namespace SVESimulator
{
    public class SveSetStatEffect : SveEffect
    {
        [EnumField("Target", width = 200), Order(1)]
        public SVEProperties.SVEEffectTarget target = SVEProperties.SVEEffectTarget.Self;

        [StringField("Target Filter", width = 100), Order(2)]
        public string filter;

        [EnumField("Stat", width = 100), Order(3)]
        public StatBoostType targetStats;

        [StringField("Amount", width = 100), Order(4)]
        public string amount;

        // ------------------------------

        public override void Resolve(PlayerController player, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, Action onComplete = null)
        {
            int boostAmount = SVEFormulaParser.ParseValue(amount, player);

            // Target leader
            if(target.IsLeader(out bool local, out bool opponent))
            {
                switch(targetStats)
                {
                    case StatBoostType.Defense:
                    case StatBoostType.AttackDefense:
                        Debug.LogError("Directly setting leader defense via SetStat is not currently supported");
                        break;
                    case StatBoostType.MaxPlayPoint:
                        Debug.LogError("Directly setting max play points via SetStat effect is not supported.");
                        break;
                    case StatBoostType.PlayPoint:
                        player.LocalEvents.SetCurrentPlayPoints(boostAmount);
                        break;
                    default:
                        Debug.LogError($"Attempted to apply invalid stat boost {targetStats} to {target}!");
                        break;
                }
                if(!target.IsFieldCard())
                {
                    onComplete?.Invoke();
                    return;
                }
            }

            // Target cards (on field)
            if(targetStats is StatBoostType.MaxPlayPoint or StatBoostType.PlayPoint)
            {
                Debug.LogError($"Attempted to apply invalid stat boost {targetStats} to {target}!");
                onComplete?.Invoke();
                return;
            }
            ResolveOnTarget(player, triggeringCardInstanceId, triggeringCardZone, sourceCardInstanceId, sourceCardZone, target, filter, onTargetFound: targets =>
            {
                foreach(CardObject card in targets)
                {
                    foreach(string stat in targetStats.AsNamedStatArray())
                    {
                        player.LocalEvents.SetCardStat(card.RuntimeCard, card.RuntimeCard.namedStats[stat].statId, boostAmount);
                    }
                }
                onComplete?.Invoke();
            });
        }
    }
}
