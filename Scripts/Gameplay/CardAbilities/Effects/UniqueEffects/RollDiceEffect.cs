using System;
using System.Collections.Generic;
using Sparkfire.Utility;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public class RollDiceEffect : SveEffect
    {
        [StringField("Effect 1", width = 200), Order(1)]
        public string effectName1;
        [StringField("Effect 2", width = 200), Order(2)]
        public string effectName2;
        [StringField("Effect 3", width = 200), Order(3)]
        public string effectName3;
        [StringField("Effect 4", width = 200), Order(3)]
        public string effectName4;
        [StringField("Effect 5", width = 200), Order(4)]
        public string effectName5;
        [StringField("Effect 6", width = 200), Order(5)]
        public string effectName6;

        public List<string> allEffects => new() { effectName1, effectName2, effectName3, effectName4, effectName5, effectName6 };

        // ------------------------------

        public override void Resolve(PlayerController player, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, Action onComplete = null)
        {
            int diceResult = player.LocalEvents.GetRandomNumber(0, 6);
            string targetEffect = allEffects[diceResult];
            Debug.Log($"Dice roll result = {diceResult}, target effect = {targetEffect}");

            if(targetEffect.IsNullOrWhiteSpace() || targetEffect.Equals("_"))
            {
                onComplete?.Invoke();
                return;
            }
            SVEEffectPool.Instance.StartCoroutine(EffectSequence.ResolveEffectsAsSequence(new List<string>() { targetEffect }, player,
                triggeringCardInstanceId, triggeringCardZone, sourceCardInstanceId, sourceCardZone, onComplete));
        }
    }
}
