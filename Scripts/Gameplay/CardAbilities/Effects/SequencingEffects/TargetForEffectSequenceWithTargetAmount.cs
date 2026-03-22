using System;
using System.Linq;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public class TargetForEffectSequenceWithTargetAmount : EffectSequence
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
                Debug.Assert(targets.Count == 1, $"More than 1 target was selected for effect {nameof(TargetForEffectSequenceWithTargetAmount)}, " +
                    $"only the first one (instance ID {targets[0].RuntimeCard.instanceId}) will be used to parse the amount.");

                int targetAmount = SVEFormulaParser.ParseValue(amount, player, targets[0]);
                SVEEffectPool.Instance.StartCoroutine(ResolveEffectsAsSequence(allEffects, player, triggeringCardInstanceId, triggeringCardZone, sourceCardInstanceId, sourceCardZone,
                    onComplete, additionalFilters: $"i({string.Join(",", targets.Select(x => x.RuntimeCard.instanceId))})",
                    overrideAmount: targetAmount.ToString()));
            });
        }
    }
}
