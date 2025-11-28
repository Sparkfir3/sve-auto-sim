using System;
using System.Collections.Generic;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public class SveCemeteryToFieldEffect : SveChooseFromCemeteryEffect
    {
        [EnumField("Target", width = 100), Order(3)]
        public SVEProperties.SVEEffectTarget target;

        // ------------------------------

        public override void Resolve(PlayerController player, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, Action onComplete = null)
        {
            // Unique logic for target mode TriggerCard
            if(target == SVEProperties.SVEEffectTarget.TriggerCard)
            {
                CardObject targetCard = CardManager.Instance.GetCardByInstanceId(triggeringCardInstanceId);
                if(!targetCard.CurrentZone.Runtime.name.Equals(SVEProperties.Zones.Cemetery))
                {
                    Debug.LogError($"Attempted to resolve CemeteryToField effect with mode TriggerCard on card with instance ID {triggeringCardInstanceId}, " +
                        $"but target card is not in the cemetery" +
                        $"\nTrigger card zone: {targetCard.CurrentZone.Runtime.name}" +
                        $"\nEffect source: instance ID {sourceCardInstanceId} in zone {sourceCardZone}");
                    onComplete?.Invoke();
                    return;
                }
                ConfirmationAction(player, new List<CardObject>() { targetCard }, onComplete);
                return;
            }

            // Standard logic for non TriggerCard mode
            base.Resolve(player, triggeringCardInstanceId, triggeringCardZone, sourceCardInstanceId, sourceCardZone, onComplete);
        }

        protected override void ConfirmationAction(PlayerController player, List<CardObject> selectedCards, Action onComplete)
        {
            foreach(CardObject card in selectedCards)
            {
                card.Interactable = player.isActivePlayer;
                player.LocalEvents.PlayCardToField(card, SVEProperties.Zones.Cemetery);
            }
            onComplete?.Invoke();
        }
    }
}
