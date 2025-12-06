using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public class SveCemeteryToFieldAndTargetEffect : SveCemeteryToFieldEffect
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
            for(int i = 0; i < selectedCards.Count; i++)
            {
                if(!player.LocalEvents.PlayCardToField(selectedCards[i], SVEProperties.Zones.Cemetery, payCost: false))
                {
                    Debug.LogError($"CemeteryToField Effect - Failed to play target card with instance ID {selectedCards[i].RuntimeCard.instanceId}");
                    selectedCards.RemoveAt(i);
                    i--;
                    continue;
                }
                selectedCards[i].Interactable = player.isActivePlayer;
            }

            if(selectedCards.Count == 0)
            {
                onComplete?.Invoke();
                return;
            }
            SVEEffectPool.Instance.StartCoroutine(SveEffectSequence.ResolveEffectsAsSequence(allEffects, player, triggerInstanceId, triggerZone, sourceInstanceId, sourceZone,
                onComplete, additionalFilters: $"i({string.Join(",", selectedCards.Select(x => x.RuntimeCard.instanceId))})"));
        }
    }
}
