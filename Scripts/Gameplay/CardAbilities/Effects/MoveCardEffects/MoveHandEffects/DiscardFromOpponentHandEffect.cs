using System;
using System.Collections;
using UnityEngine;
using CCGKit;
using Sparkfire.Utility;

namespace SVESimulator
{
    public class DiscardFromOpponentHandEffect : SveEffect
    {
        [StringField("Amount", width = 100), Order(1)]
        public string amount;

        // ------------------------------

        public override void Resolve(PlayerController player, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, Action onComplete = null)
        {
            if(player.OppZoneController.handZone.AllCards.Count == 0)
            {
                onComplete?.Invoke();
                return;
            }
            SVEEffectPool.Instance.StartCoroutine(ResolveCoroutine());

            IEnumerator ResolveCoroutine()
            {
                bool waiting = true;
                int minDiscard, maxDiscard;
                int handSize = player.OppZoneController.handZone.AllCards.Count;
                if(amount.IsNullOrWhiteSpace())
                    minDiscard = maxDiscard = 1;
                else
                    SVEFormulaParser.ParseValueAsMinMax(amount, player, out minDiscard, out maxDiscard);
                minDiscard = Mathf.Min(minDiscard, handSize);
                CardSelectionArea selectionArea = player.ZoneController.selectionArea;

                selectionArea.Enable(CardSelectionArea.SelectionMode.SelectCardsFromOppHand, handSize, handSize);
                selectionArea.SetFilter(null);
                selectionArea.AddAllCardsInOpponentsHand();
                selectionArea.SetConfirmAction(LibraryCardCache.GetCardFromInstanceId(sourceCardInstanceId).name,
                    "Discard",
                    text,
                    minDiscard, maxDiscard,
                    targets =>
                    {
                        selectionArea.Disable(); // disable here returns cards to opponent's hand before sending to cemetery, required for race condition with card's zone's owner player
                        foreach(CardObject target in targets)
                        {
                            player.LocalEvents.SendToCemetery(target, SVEProperties.Zones.Hand);
                        }
                        waiting = false;
                    });

                yield return new WaitUntil(() => !waiting);
                onComplete?.Invoke();
            }
        }
    }
}
