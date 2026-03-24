using System;
using CCGKit;
using UnityEngine;

namespace SVESimulator
{
    public class PerformWithTargetAmountEffect : EffectSequence
    {
        [StringField("Amount", width = 100), Order(11)]
        public string amount;

        [EnumField("Target", width = 200), Order(12)]
        public SVEProperties.SVEEffectTarget target = SVEProperties.SVEEffectTarget.Self;

        [StringField("Target Filter", width = 100), Order(13)]
        public string filter;

        // ------------------------------

        public override void Resolve(PlayerController player, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, Action onComplete = null)
        {
            ResolveOnTarget(player, triggeringCardInstanceId, triggeringCardZone, sourceCardInstanceId, sourceCardZone, target, filter, onTargetFound: targets =>
            {
                if(targets is not { Count: > 0 })
                {
                    onComplete?.Invoke();
                    return;
                }
                Debug.Assert(targets.Count == 1, $"More than 1 target was selected for effect PerformWithTargetAmountEffect, only the first one (instance ID {targets[0].RuntimeCard.instanceId}) will be used.");

                int targetAmount = SVEFormulaParser.ParseValue(amount, player, targets[0]);
                SVEEffectPool.Instance.StartCoroutine(ResolveEffectsAsSequence(allEffects, player, triggeringCardInstanceId, triggeringCardZone, sourceCardInstanceId, sourceCardZone,
                    onComplete, overrideAmount: targetAmount.ToString()));
            });
        }
    }
}
