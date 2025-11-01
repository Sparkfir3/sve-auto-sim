using System;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public abstract class SveModifiedCostEffect : SveEffect
    {
        public override void Resolve(PlayerController player, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, Action onComplete = null)
        {
            Debug.LogError("Attempted to resolve a modified cost effect, this should not happen.");
            onComplete?.Invoke();
        }
    }
}
