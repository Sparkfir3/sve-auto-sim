using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CCGKit;
using Sparkfire.Utility;

namespace SVESimulator
{
    public abstract class ChooseFromHandEffect : SveEffect
    {
        [StringField("Amount", width = 100), Order(1)]
        public string amount;

        [StringField("Target Filter", width = 100), Order(2)]
        public string filter;

        protected abstract string ActionText { get; }

        // ------------------------------

        public override void Resolve(PlayerController player, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, Action onComplete = null)
        {
            SVEEffectPool.Instance.StartCoroutine(ResolveCoroutine(player, triggeringCardInstanceId, triggeringCardZone, sourceCardInstanceId, sourceCardZone, onComplete));
        }

        protected virtual IEnumerator ResolveCoroutine(PlayerController player, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, Action onComplete = null)
        {
            bool waiting = true;
            int minTargets, maxTargets;
            if(amount.IsNullOrWhiteSpace())
                minTargets = maxTargets = 1;
            else
                GetMinMax(player, out minTargets, out maxTargets);
            minTargets = Mathf.Min(minTargets, player.ZoneController.handZone.AllCards.Count);
            CardSelectionArea selectionArea = player.ZoneController.selectionArea;

            selectionArea.Enable(CardSelectionArea.SelectionMode.PlaceCardsFromHand, minTargets, maxTargets);
            selectionArea.SetFilter(filter);
            selectionArea.SetConfirmAction(LibraryCardCache.GetCardFromInstanceId(sourceCardInstanceId).name,
                ActionText,
                text,
                minTargets, maxTargets,
                targets => ConfirmationAction(player, targets, () => waiting = false));

            yield return new WaitUntil(() => !waiting);
            selectionArea.Disable();
            yield return null;
            onComplete?.Invoke();
        }

        protected virtual void GetMinMax(PlayerController player, out int min, out int max)
        {
            SVEFormulaParser.ParseValueAsMinMax(amount, player, out min, out max);
        }

        protected abstract void ConfirmationAction(PlayerController player, List<CardObject> selectedCards, Action onComplete);
    }
}
