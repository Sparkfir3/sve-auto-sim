using System;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public class SveEvolveEffect : SveEffect
    {
        [EnumField("Target", width = 200), Order(1)]
        public SVEProperties.SVEEffectTarget target = SVEProperties.SVEEffectTarget.Self;

        [StringField("Target Filter", width = 100), Order(2)]
        public string filter;

        // ------------------------------

        public override void Resolve(PlayerController player, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, Action onComplete = null)
        {
            ResolveOnTarget(player, triggeringCardInstanceId, triggeringCardZone, sourceCardInstanceId, sourceCardZone, target, filter, onTargetFound: targets =>
            {
                foreach(CardObject card in targets)
                {
                    if(!player.ZoneController.EvolveDeckHasEvolvedVersionOf(card.RuntimeCard))
                    {
                        continue;
                    }
                    player.LocalEvents.EvolveCard(card, useEvolvePoint: false, useEvolveCost: false);
                }
                onComplete?.Invoke();
            });
        }
    }
}
