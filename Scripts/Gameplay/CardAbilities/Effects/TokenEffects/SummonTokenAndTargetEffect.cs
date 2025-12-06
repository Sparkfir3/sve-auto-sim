using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CCGKit;
using SVESimulator.UI;
using UnityEngine;
using Sparkfire.Utility;

namespace SVESimulator
{
    public class SummonTokenAndTargetEffect : SummonTokenEffect
    {
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
            if(createTokenOption == SVEProperties.TokenCreationOption.ChooseForEachFieldOrEx)
            {
                Debug.LogError($"SummonTokenAndTarget effect does not support CreateTokenOption ChooseForFieldOrEx");
                onComplete?.Invoke();
                return;
            }
            base.Resolve(player, triggeringCardInstanceId, triggeringCardZone, sourceCardInstanceId, sourceCardZone, onComplete);
        }

        protected override void OnTokensCreated(List<CardObject> tokens, PlayerController player, int triggeringCardInstanceId, string triggeringCardZone,
            int sourceCardInstanceId, string sourceCardZone, Action onComplete)
        {
            // Reference: SveEffectSequence.cs
            SVEEffectPool.Instance.StartCoroutine(ResolveCoroutine());
            IEnumerator ResolveCoroutine()
            {
                yield return EngageWardTokens(tokens, player);
                yield return EffectSequence.ResolveEffectsAsSequence(allEffects, player, triggeringCardInstanceId, triggeringCardZone, sourceCardInstanceId, sourceCardZone,
                    onComplete, additionalFilters: $"i({string.Join(",", tokens.Select(x => x.RuntimeCard.instanceId))})");
            }
        }
    }
}
