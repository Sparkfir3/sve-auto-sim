using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public class SvePutExToFieldAndTargetEffect : SveEffect
    {
        [EnumField("Target", width = 200), Order(1)]
        public SVEProperties.SVEEffectTarget target = SVEProperties.SVEEffectTarget.TargetPlayerCardEx;

        [StringField("Target Filter", width = 100), Order(2)]
        public string filter;

        [StringField("Effect 1", width = 200), Order(11)]
        public string effectName1;
        [StringField("Effect 2", width = 200), Order(12)]
        public string effectName2;
        [StringField("Effect 3", width = 200), Order(13)]
        public string effectName3;
        [StringField("Effect 4", width = 200), Order(13)]
        public string effectName4;
        [StringField("Effect 5", width = 200), Order(14)]
        public string effectName5;

        public List<string> allEffects => new() { effectName1, effectName2, effectName3, effectName4, effectName5 };

        // ------------------------------

        public override void Resolve(PlayerController player, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, Action onComplete = null)
        {
            if(target is not SVEProperties.SVEEffectTarget.TargetPlayerCardEx or SVEProperties.SVEEffectTarget.AllPlayerCardsEx)
            {
                Debug.LogError($"Attempted to perform effect PutExToFieldAndTarget with invalid target mode: {target}" +
                    $"\nEffect Source: Instance ID {sourceCardInstanceId} in {sourceCardZone}, Triggered By: Instance ID {triggeringCardInstanceId} in {triggeringCardZone}" +
                    $"\nFilter: {filter}\nEffect List: {string.Join(", ", allEffects)}");
                onComplete?.Invoke();
                return;
            }

            ResolveOnTarget(player, triggeringCardInstanceId, triggeringCardZone, sourceCardInstanceId, sourceCardZone, target, filter, onTargetFound: targets =>
            {
                if(targets.Count == 0)
                {
                    onComplete?.Invoke();
                    return;
                }

                foreach(CardObject card in targets)
                {
                    player.LocalEvents.PlayCardToField(card, SVEProperties.Zones.ExArea, payCost: false);
                }
                SVEEffectPool.Instance.StartCoroutine(SveEffectSequence.ResolveEffectsAsSequence(allEffects, player, triggeringCardInstanceId, triggeringCardZone, sourceCardInstanceId, sourceCardZone,
                    onComplete, additionalFilters: $"i({string.Join(",", targets.Select(x => x.RuntimeCard.instanceId))})"));
            });
        }
    }
}
