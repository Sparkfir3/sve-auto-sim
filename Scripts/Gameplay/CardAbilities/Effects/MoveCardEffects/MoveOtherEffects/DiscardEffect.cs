using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CCGKit;
using Sparkfire.Utility;

namespace SVESimulator
{
    public class DiscardEffect : SveEffect
    {
        [StringField("Amount", width = 100), Order(1)]
        public string amount;

        [StringField("Target Filter", width = 100), Order(2)]
        public string filter;

        // ------------------------------

        public override void Resolve(PlayerController player, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, Action onComplete = null)
        {
            SVEEffectPool.Instance.StartCoroutine(ResolveCoroutine());
            IEnumerator ResolveCoroutine()
            {
                bool waiting = true;
                int minDiscard, maxDiscard;
                if(amount.IsNullOrWhiteSpace())
                    minDiscard = maxDiscard = 1;
                else
                    SVEFormulaParser.ParseValueAsMinMax(amount, player, out minDiscard, out maxDiscard);
                minDiscard = Mathf.Min(minDiscard, player.ZoneController.handZone.AllCards.Count);
                CardSelectionArea selectionArea = player.ZoneController.selectionArea;

                selectionArea.Enable(CardSelectionArea.SelectionMode.PlaceCardsFromHand, minDiscard, maxDiscard);
                selectionArea.SetFilter(filter);
                selectionArea.SetConfirmAction(LibraryCardCache.GetCardFromInstanceId(sourceCardInstanceId).name,
                    ActionText,
                    text,
                    minDiscard, maxDiscard,
                    targets => ConfirmationAction(player, targets, () => waiting = false));

                yield return new WaitUntil(() => !waiting);
                selectionArea.Disable();
                yield return null;
                onComplete?.Invoke();
            }
        }

        protected virtual string ActionText => "Discard";

        protected virtual void ConfirmationAction(PlayerController player, List<CardObject> selectedCards, Action onComplete)
        {
            foreach(CardObject target in selectedCards)
            {
                player.LocalEvents.SendToCemetery(target, SVEProperties.Zones.Hand);
            }
            onComplete?.Invoke();
        }
    }
}
