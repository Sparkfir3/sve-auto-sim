using System;
using CCGKit;
using UnityEngine;
using StatBoostType = SVESimulator.SVEProperties.StatBoostType;

namespace SVESimulator
{
    public class GiveStatBoostEffect : SveEffect
    {
        [EnumField("Target", width = 200), Order(1)]
        public SVEProperties.SVEEffectTarget target = SVEProperties.SVEEffectTarget.Self;

        [StringField("Target Filter", width = 100), Order(2)]
        public string filter;

        [EnumField("Stat Boost"), Order(3)]
        public StatBoostType targetStats;

        [StringField("Amount", width = 100), Order(4)]
        public string amount;

        // ------------------------------

        public override void Resolve(PlayerController player, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, Action onComplete = null)
        {
            int boostAmount = SVEFormulaParser.ParseValue(amount, player, sourceCardInstanceId, sourceCardZone);

            // Target leader
            if(target.IsLeader(out bool local, out bool opponent) || targetStats is StatBoostType.MaxPlayPoint or StatBoostType.PlayPoint)
            {
                local |= target == SVEProperties.SVEEffectTarget.Self;
                opponent |= target == SVEProperties.SVEEffectTarget.Opponent;
                switch(targetStats)
                {
                    case StatBoostType.Defense:
                    case StatBoostType.AttackDefense:
                        if(local)
                            player.LocalEvents.AddLeaderDefense(player.GetPlayerInfo(), boostAmount);
                        if(opponent)
                            player.LocalEvents.AddLeaderDefense(player.GetOpponentInfo(), boostAmount);
                        break;
                    case StatBoostType.MaxPlayPoint:
                        if(boostAmount <= 0)
                            Debug.LogError("Decreasing max play points via GiveStat effect is not supported.");
                        else
                            player.LocalEvents.IncrementMaxPlayPoints(amount: boostAmount);
                        break;
                    case StatBoostType.PlayPoint:
                        player.LocalEvents.IncrementCurrentPlayPoints(boostAmount);
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
                if(target is not (SVEProperties.SVEEffectTarget.Self or SVEProperties.SVEEffectTarget.Opponent))
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
                        player.LocalEvents.ApplyModifierToCard(card.RuntimeCard, card.RuntimeCard.namedStats[stat].statId, boostAmount, true);
                    }
                }
                onComplete?.Invoke();
            });
        }
    }
}
