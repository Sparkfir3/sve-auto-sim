using System;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public class RevealTopPerformWithRevealedAmount : EffectSequence
    {
        [StringField("Amount", width = 100), Order(11)]
        public string amount;

        [EnumField("Target", width = 200), Order(12)]
        public SVEProperties.SVEEffectTarget target = SVEProperties.SVEEffectTarget.Self;

        [StringField("Target Filter", width = 100), Order(13)]
        public string filter;

        // ------------------------------

        public override void Resolve(PlayerController player, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, Action onComplete = null)
        {
            player.LocalEvents.RevealTopDeck(revealedCard =>
            {
                int targetAmount = SVEFormulaParser.ParseValue(amount, player, revealedCard);
                ResolveOnTarget(player, triggeringCardInstanceId, triggeringCardZone, sourceCardInstanceId, sourceCardZone, target, filter, onTargetFound: targets =>
                {
                    player.LocalEvents.FlipTopDeckToFaceDown(revealedCard);
                    SVEEffectPool.Instance.StartCoroutine(ResolveEffectsAsSequence(allEffects, player, triggeringCardInstanceId, triggeringCardZone, sourceCardInstanceId, sourceCardZone,
                        onComplete, overrideAmount: targetAmount.ToString()));
                });
            });
        }
    }
}
