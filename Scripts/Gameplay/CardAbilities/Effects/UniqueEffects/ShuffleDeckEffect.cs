using System;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public class ShuffleDeckEffect : SveEffect
    {
        [EnumField("Target", width = 200), Order(1)]
        public SVEProperties.SVEEffectTarget target = SVEProperties.SVEEffectTarget.Self;

        // ------------------------------

        public override void Resolve(PlayerController player, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, Action onComplete = null)
        {
            target.IsLeader(out bool selfShuffle, out bool oppShuffle);
            if(selfShuffle)
                player.LocalEvents.ShuffleDeck();
            if(oppShuffle)
                Debug.LogError($"{nameof(ShuffleDeckEffect)} does not support shuffling opponent's deck using target mode {target}");
            onComplete?.Invoke();
        }
    }
}
