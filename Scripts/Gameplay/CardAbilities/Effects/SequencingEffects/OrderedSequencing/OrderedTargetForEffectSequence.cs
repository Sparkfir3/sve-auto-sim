using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CCGKit;
using Sparkfire.Utility;

namespace SVESimulator
{
    public class OrderedTargetForEffectSequence : TargetForEffectSequence
    {
        public override void Resolve(PlayerController player, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, Action onComplete = null)
        {
            ResolveOnTarget(player, triggeringCardInstanceId, triggeringCardZone, sourceCardInstanceId, sourceCardZone, target, filter, onTargetFound: targets =>
            {
                SVEEffectPool.Instance.StartCoroutine(ResolveOverTime(player, triggeringCardInstanceId, triggeringCardZone, sourceCardInstanceId, sourceCardZone, targets, onComplete));
            });
        }

        private IEnumerator ResolveOverTime(PlayerController player, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, List<CardObject> targets, Action onComplete)
        {
            for(int i = 0; i < targets.Count && i < allEffects.Count; i++)
            {
                string effectName = allEffects[i];
                if(effectName.IsNullOrWhiteSpace())
                    continue;

                yield return ResolveEffectsAsSequence(new List<string>() { effectName },  player, triggeringCardInstanceId, triggeringCardZone,
                    sourceCardInstanceId, sourceCardZone, onComplete: null,
                    additionalFilters: $"i({targets[i].RuntimeCard.instanceId})");
                yield return new WaitForEndOfFrame();
            }
            yield return new WaitForEndOfFrame();
            onComplete?.Invoke();
        }
    }
}
