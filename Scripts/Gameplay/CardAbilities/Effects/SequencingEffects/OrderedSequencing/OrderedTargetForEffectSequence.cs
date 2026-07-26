using System;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public class OrderedTargetForEffectSequence : TargetForEffectSequence
    {
        public override void Resolve(PlayerController player, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, Action onComplete = null)
        {
            // TODO
            base.Resolve(player, triggeringCardInstanceId, triggeringCardZone, sourceCardInstanceId, sourceCardZone, onComplete);
        }
    }
}
