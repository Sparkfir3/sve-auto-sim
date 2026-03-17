using System;
using System.Collections;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public class RevealTopPerformWithRevealedAmount : EffectSequence
    {
        [StringField("Amount", width = 100), Order(11)]
        public string amount;

        // ------------------------------

        public override void Resolve(PlayerController player, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, Action onComplete = null)
        {
            player.LocalEvents.RevealTopDeck(revealedCard =>
            {
                int targetAmount = SVEFormulaParser.ParseValue(amount, player, revealedCard);
                SVEEffectPool.Instance.StartCoroutine(ResolveWithDelays(revealedCard, targetAmount));
            });

            IEnumerator ResolveWithDelays(CardObject revealedCard, int targetAmount)
            {
                yield return new WaitForSeconds(0.4f);
                player.LocalEvents.FlipTopDeckToFaceDown(revealedCard);
                yield return new WaitForSeconds(0.1f);
                SVEEffectPool.Instance.StartCoroutine(ResolveEffectsAsSequence(allEffects, player, triggeringCardInstanceId, triggeringCardZone, sourceCardInstanceId, sourceCardZone,
                    onComplete, overrideAmount: targetAmount.ToString()));
            }
        }
    }
}
