using System;
using System.Collections.Generic;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public class SetStatEndOfTurnEffect : SetStatEffect
    {
        public override void Resolve(PlayerController player, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, Action onComplete = null)
        {
            // Leader check
            if(target.IsLeader())
            {
                Debug.LogError($"{nameof(SetStatEndOfTurnEffect)} does not support targeting leaders using target mode {target}");
                onComplete?.Invoke();
                return;
            }

            // TODO - support stats other than cost
            if(targetStats != SVEProperties.StatBoostType.Cost)
            {
                Debug.LogError($"Using {nameof(SetStatEndOfTurnEffect)} for stats other than Cost has not been implemented yet.");
                onComplete?.Invoke();
                return;
            }

            // Resolve
            ResolveOnTarget(player, triggeringCardInstanceId, triggeringCardZone, sourceCardInstanceId, sourceCardZone, target, filter, onTargetFound: targets =>
            {
                // Currently only works on cost stat
                foreach(CardObject card in targets)
                {
                    int amountDiff = SVEFormulaParser.ParseValue(amount, player, card) - card.RuntimeCard.namedStats[SVEProperties.CardStats.Cost].effectiveValue;
                    RegisteredPassiveAbility passive = new()
                    {
                        sourceCardInstanceId = card.RuntimeCard.instanceId,
                        targetsFormula = null,
                        filters = new Dictionary<SVEFormulaParser.CardFilterSetting, string>()
                        {
                            { SVEFormulaParser.CardFilterSetting.InstanceID, $"{card.RuntimeCard.instanceId}" }
                        },
                        effect = new MinusCostOtherPassive
                        {
                            amount = (amountDiff * -1).ToString()
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
