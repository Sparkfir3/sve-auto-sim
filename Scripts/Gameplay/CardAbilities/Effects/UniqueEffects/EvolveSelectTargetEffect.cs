using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public class EvolveSelectTargetEffect : ChooseFromEvolveDeckEffect, IEvolveEffect
    {
        protected override bool TargetFaceDownCards => true;
        protected override bool TargetFaceUpCards => false;

        [NonSerialized]
        private int triggerInstanceId, sourceInstanceId;
        [NonSerialized]
        private string triggerZone, sourceZone;

        // ------------------------------

        public override void Resolve(PlayerController player, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, Action onComplete = null)
        {
            amount = "1";
            triggerInstanceId = triggeringCardInstanceId;
            triggerZone = triggeringCardZone;
            sourceInstanceId = sourceCardInstanceId;
            sourceZone = sourceCardZone;
            base.Resolve(player, triggeringCardInstanceId, triggeringCardZone, sourceCardInstanceId, sourceCardZone, onComplete);
        }

        protected override void ConfirmationAction(PlayerController player, List<CardObject> selectedCards, Action onComplete)
        {
            if(selectedCards.Count == 0)
            {
                onComplete?.Invoke();
                return;
            }

            ResolveOnTarget(player, triggerInstanceId, triggerZone, sourceInstanceId, sourceZone, SVEProperties.SVEEffectTarget.Self, filter, onTargetFound: targets =>
            {
                if(targets.Count == 0)
                {
                    onComplete?.Invoke();
                    return;
                }
                player.LocalEvents.EvolveCard(targets[0], evolvedCard: selectedCards[0], useEvolvePoint: false, useEvolveCost: false);
                onComplete?.Invoke();
            });
        }

        public bool CanEvolve(PlayerController player, RuntimeCard card)
        {
            var filterDict = SVEFormulaParser.ParseCardFilterFormula(filter);
            return player && !player.EvolvedThisTurn && player.ZoneController.evolveDeckZone.Runtime.cards.Any(x => filterDict.MatchesCard(x));
        }
    }
}
