using System;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public class EvolveEffect : SveEffect, IEvolveEffect
    {
        [EnumField("Target", width = 200), Order(1)]
        public SVEProperties.SVEEffectTarget target = SVEProperties.SVEEffectTarget.Self;

        [StringField("Target Filter", width = 100), Order(2)]
        public string filter;

        // ------------------------------

        public override void Resolve(PlayerController player, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, Action onComplete = null)
        {
            if(target != SVEProperties.SVEEffectTarget.Self && !filter.Contains("#(hasEvolveDeckTarget)"))
                filter += "#(hasEvolveDeckTarget)";
            ResolveOnTarget(player, triggeringCardInstanceId, triggeringCardZone, sourceCardInstanceId, sourceCardZone, target, filter, onTargetFound: targets =>
            {
                foreach(CardObject card in targets)
                {
                    if(!player.ZoneController.EvolveDeckHasEvolvedVersionOf(card.RuntimeCard))
                    {
                        continue;
                    }
                    player.LocalEvents.EvolveCard(card, useEvolvePoint: false, useEvolveCost: false, useEvolveForTurn: target == SVEProperties.SVEEffectTarget.Self);
                }
                onComplete?.Invoke();
            });
        }

        public bool CanEvolve(PlayerController player, RuntimeCard card)
        {
            return player && !player.EvolvedThisTurn && player.ZoneController.EvolveDeckHasEvolvedVersionOf(card);
        }
    }
}
