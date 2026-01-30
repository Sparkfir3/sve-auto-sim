using System;
using System.Collections.Generic;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public class GiveStatEndOfTurnEffect : GiveStatBoostEffect
    {
        public override void Resolve(PlayerController player, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, Action onComplete = null)
        {
            ResolveOnTarget(player, triggeringCardInstanceId, triggeringCardZone, sourceCardInstanceId, sourceCardZone, target, filter, onTargetFound: targets =>
            {
                foreach(CardObject card in targets)
                {
                    RegisteredPassiveAbility passive = new()
                    {
                        sourceCardInstanceId = card.RuntimeCard.instanceId,
                        targetsFormula = null,
                        filters = new Dictionary<SVEFormulaParser.CardFilterSetting, string>(),
                        effect = new GiveStatBoostPassive
                        {
                            duration = SVEProperties.PassiveDuration.EndOfTurn,
                            targetStats = targetStats,
                            amount = amount
                        },
                        affectedCards = new List<RuntimeCard>(),
                        target = SVEProperties.SVEEffectTarget.Self,
                        duration = SVEProperties.PassiveDuration.EndOfTurn
                    };
                    SVEEffectPool.Instance.RegisterPassiveAbility(passive);
                }
                onComplete?.Invoke();
            });
        }
    }
}
