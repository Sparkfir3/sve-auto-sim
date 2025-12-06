using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public abstract class ChooseFromCardStackEffect : SveEffect
    {
        [StringField("Target Filter", width = 100), Order(1)]
        public string filter;

        [StringField("Amount", width = 100), Order(2)]
        public string amount;

        // ------------------------------

        public override void Resolve(PlayerController player, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, Action onComplete = null)
        {
            player.StartCoroutine(ResolveOverTime(player, sourceCardInstanceId, sourceCardZone, onComplete));
        }

        protected IEnumerator ResolveOverTime(PlayerController player, int sourceCardInstanceId, string sourceCardZone, Action onComplete = null)
        {
            // Init
            GetMinMax(player, out int minSelect, out int maxSelect);
            CardSelectionArea selectionArea = player.ZoneController.selectionArea;
            string cardName = LibraryCardCache.GetCardFromInstanceId(sourceCardInstanceId, GameManager.Instance.config)?.name ?? "";

            player.InputController.allowedInputs = PlayerInputController.InputTypes.None;
            InitializeSelectionArea(player, selectionArea);

            // Perform actions
            bool waiting = true;
            if(player.ZoneController.selectionArea.ValidTargetsCount == 0)
                minSelect = 0;
            selectionArea.SetConfirmAction(cardName, "Confirm Selection", text, minSelect, maxSelect, selectedCards =>
            {
                selectionArea.Disable();
                ConfirmationAction(player, selectedCards, () => waiting = false);
            });
            yield return new WaitUntil(() => !waiting);

            player.InputController.allowedInputs = player.isActivePlayer ? PlayerInputController.InputTypes.All : PlayerInputController.InputTypes.None;
            player.ZoneController.handZone.SetAllCardsInteractable(player.isActivePlayer);
            OnCompleteInternal(player);
            onComplete?.Invoke();
        }

        protected virtual void GetMinMax(PlayerController player, out int min, out int max)
        {
            SVEFormulaParser.ParseValueAsMinMax(amount, player, out min, out max);
        }

        protected abstract void InitializeSelectionArea(PlayerController player, CardSelectionArea selectionArea);
        protected abstract void ConfirmationAction(PlayerController player, List<CardObject> selectedCards, Action onComplete);

        protected virtual void OnCompleteInternal(PlayerController player) { }
    }
}
