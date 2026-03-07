using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public class SearchDeckAndTargetEffect : SearchDeckEffect
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

        [NonSerialized]
        private int triggerInstanceId, sourceInstanceId;
        [NonSerialized]
        private string triggerZone, sourceZone;

        // ------------------------------

        public override void Resolve(PlayerController player, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, Action onComplete = null)
        {
            triggerInstanceId = triggeringCardInstanceId;
            triggerZone = triggeringCardZone;
            sourceInstanceId = sourceCardInstanceId;
            sourceZone = sourceCardZone;
            base.Resolve(player, triggeringCardInstanceId, triggeringCardZone, sourceCardInstanceId, sourceCardZone, onComplete);
        }

        protected override void ConfirmationAction(PlayerController player, List<CardObject> selectedCards, Action onComplete)
        {
            PerformSearchDeckAction(player, selectedCards);
            if(searchDeckAction != SearchDeckAction.Field && searchDeckAction != SearchDeckAction.ExArea)
            {
                Debug.LogError($"SearchDeckAndTargetEffect does not support targeting cards after search action {searchDeckAction}. Only Field and ExArea are supported");
                onComplete?.Invoke();
                return;
            }
            if(selectedCards.Count == 0)
            {
                onComplete?.Invoke();
                return;
            }

            SVEEffectPool.Instance.StartCoroutine(EffectSequence.ResolveEffectsAsSequence(allEffects, player, triggerInstanceId, triggerZone, sourceInstanceId, sourceZone,
                onComplete, additionalFilters: $"i({string.Join(",", selectedCards.Select(x => x.RuntimeCard.instanceId))})"));
        }
    }
}
