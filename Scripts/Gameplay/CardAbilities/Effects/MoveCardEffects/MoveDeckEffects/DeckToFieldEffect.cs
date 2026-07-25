using System;
using System.Linq;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public class DeckToFieldEffect : SveEffect
    {
        [EnumField("Target", width = 100), Order(1)]
        public SVEProperties.SVEEffectTarget target;

        // ------------------------------

        public override void Resolve(PlayerController player, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, Action onComplete = null)
        {
            if(target != SVEProperties.SVEEffectTarget.TriggerCard)
            {
                Debug.LogError($"Effect {nameof(DeckToFieldEffect)} only supports using target mode TriggerCard (received mode {target})");
                onComplete?.Invoke();
                return;
            }

            CardObject card = CardManager.Instance.GetCardByInstanceId(triggeringCardInstanceId);
            if(!card)
            {
                RuntimeCard runtimeCard = player.GetPlayerInfo().namedZones[SVEProperties.Zones.Deck].cards.FirstOrDefault(x => x.instanceId == triggeringCardInstanceId);
                if(runtimeCard == null)
                {
                    Debug.LogError($"Effect {nameof(DeckToFieldEffect)} failed to find card with instance ID {triggeringCardInstanceId} in the player's deck");
                    onComplete?.Invoke();
                    return;
                }
                card = CardManager.Instance.RequestCard(runtimeCard);
            }
            player.LocalEvents.PlayCardToField(card, SVEProperties.Zones.Deck, payCost: false);
            onComplete?.Invoke();
        }
    }
}
