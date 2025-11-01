using System;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public class SveExtraTurnEffect : SveEffect
    {
        public override void Resolve(PlayerController player, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, Action onComplete = null)
        {
            player.LocalEvents.ExtraTurn();
            onComplete?.Invoke();
        }
    }
}
