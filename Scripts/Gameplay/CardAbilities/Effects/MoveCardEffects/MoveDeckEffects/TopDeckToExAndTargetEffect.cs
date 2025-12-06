using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CCGKit;
using Sparkfire.Utility;

namespace SVESimulator
{
    public class TopDeckToExAndTargetEffect : TopDeckToExEffect
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
            int count = SVEFormulaParser.ParseValue(amount, player);
            List<CardObject> cards = new();
            for(int i = 0; i < count; i++)
                cards.Add(player.LocalEvents.TopDeckToExArea());
            if(cards.Count == 0)
            {
                onComplete?.Invoke();
                return;
            }

            SVEEffectPool.Instance.StartCoroutine(EffectSequence.ResolveEffectsAsSequence(allEffects, player, triggeringCardInstanceId, triggeringCardZone, sourceCardInstanceId, sourceCardZone,
                onComplete, additionalFilters: $"i({string.Join(",", cards.Select(x => x.RuntimeCard.instanceId))})"));
        }
    }
}
