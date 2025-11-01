using System;
using System.Collections.Generic;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    // Internal effect used by certain costs to select targets for that cost
    public class TargetCardForCostEffect : SveEffect
    {
        [EnumField("Target", width = 200), Order(1)]
        public SVEProperties.SVEEffectTarget target = SVEProperties.SVEEffectTarget.Self;

        [StringField("Target Filter", width = 100), Order(2)]
        public string filter;

        // ------------------------------

        public void GetTargets(PlayerController player, int cardInstanceId, string cardZone, Action<List<CardObject>> onComplete = null)
        {
            ResolveOnTarget(player, cardInstanceId, cardZone, cardInstanceId, cardZone, target, filter, targets =>
            {
                onComplete?.Invoke(targets);
            });
        }

        public override void Resolve(PlayerController player, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, Action onComplete = null)
        {
            Debug.LogError($"Calling Resolve() on {nameof(TargetCardForCostEffect)} is not supported, use GetTargets() instead.");
            onComplete?.Invoke();
        }
    }
}
