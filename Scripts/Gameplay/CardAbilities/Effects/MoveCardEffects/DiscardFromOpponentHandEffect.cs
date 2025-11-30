using System;
using System.Collections;
using UnityEngine;
using CCGKit;
using Sparkfire.Utility;

namespace SVESimulator
{
    public class SveDiscardFromOpponentHandEffect : SveEffect
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
                if(amount.IsNullOrWhiteSpace())
                    minDiscard = maxDiscard = 1;
                else
                    SVEFormulaParser.ParseValueAsMinMax(amount, player, out minDiscard, out maxDiscard);
                minDiscard = Mathf.Min(minDiscard, player.OppZoneController.handZone.AllCards.Count);
                CardSelectionArea selectionArea = player.ZoneController.selectionArea;

                selectionArea.Enable(CardSelectionArea.SelectionMode.SelectCardsFromOppHand, minDiscard, maxDiscard);
                selectionArea.SetFilter(null);
                selectionArea.SetConfirmAction(LibraryCardCache.GetCardFromInstanceId(sourceCardInstanceId).name,
                    "Discard",
                    text,
                    minDiscard, maxDiscard,
                    targets =>
                    {
                        foreach(CardObject target in targets)
                        {
                            player.LocalEvents.SendToCemetery(target, SVEProperties.Zones.Hand);
                        }
                        waiting = false;
                    });

                yield return new WaitUntil(() => !waiting);
                selectionArea.Disable();
                yield return null;
                onComplete?.Invoke();
            }
        }
    }
}
