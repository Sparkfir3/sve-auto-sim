using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CCGKit;
using Sparkfire.Utility;

namespace SVESimulator
{
    public class CheckTopDeckEffect : SveEffect
    {
        #region Enums/Structs

        public enum CheckCardAction
        {
            None,

            // Send to zone
            Hand,
            Cemetery,
            Field,
            TopOrBottomDeck,

            // Rearrange and send
            TopDeckAnyOrder,
            TopDeckSameOrder,
            BottomDeckAnyOrder,
        }

        private struct CheckActionParameters
        {
            public CheckCardAction action;
            public string filter;
            public string amount;

            public CheckActionParameters(CheckCardAction action, string filter, string amount)
            {
                this.action = action;
                this.filter = filter;
                this.amount = amount;
            }
        }

        #endregion

        // -----

        #region Variables

        [StringField("Check Amt", width = 100), Order(1)]
        public string amount;

        [EnumField("Action 1", width = 200), Order(11)]
        public CheckCardAction checkAction1;
        [StringField("A1 Filter", width = 100), Order(12)]
        public string checkFilter1;
        [StringField("A1 Amount", width = 100), Order(12)]
        public string checkAmount1;

        [EnumField("Action 2", width = 200), Order(21)]
        public CheckCardAction checkAction2;
        [StringField("A2 Filter", width = 100), Order(22)]
        public string checkFilter2;
        [StringField("A2 Amount", width = 100), Order(22)]
        public string checkAmount2;

        [EnumField("Action 3", width = 200), Order(31)]
        public CheckCardAction checkAction3;
        [StringField("A3 Filter", width = 100), Order(32)]
        public string checkFilter3;
        [StringField("A3 Amount", width = 100), Order(32)]
        public string checkAmount3;

        private CheckActionParameters action1 => new CheckActionParameters(checkAction1, checkFilter1, checkAmount1);
        private CheckActionParameters action2 => new CheckActionParameters(checkAction2, checkFilter2, checkAmount2);
        private CheckActionParameters action3 => new CheckActionParameters(checkAction3, checkFilter3, checkAmount3);
        private List<CheckActionParameters> allActions => new() { action1, action2, action3 };

        #endregion

        // ------------------------------

        #region Resolve

        public override void Resolve(PlayerController player, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, Action onComplete = null)
        {
            player.StartCoroutine(ResolveOverTime(player, sourceCardInstanceId, sourceCardZone, onComplete));
        }

        private IEnumerator ResolveOverTime(PlayerController player, int sourceCardInstanceId, string sourceCardZone, Action onComplete = null)
        {
            // Init
            SVEFormulaParser.ParseValueAsMinMax(amount, player, out int minCheck, out int maxCheck);
            CardSelectionArea selectionArea = player.ZoneController.selectionArea;
            string cardName = LibraryCardCache.GetCardFromInstanceId(sourceCardInstanceId, GameManager.Instance.config)?.name ?? "";
            player.InputController.allowedInputs = PlayerInputController.InputTypes.None;

            // Reveal initial
            selectionArea.Enable(CardSelectionArea.SelectionMode.SelectCardsFromDeck, minCheck, maxCheck);
            for(int i = 0; i < minCheck; i++)
            {
                selectionArea.AddCardFromTopDeck();
                yield return new WaitForSeconds(selectionArea.AddRemoveCardDelay);
            }

            // Perform actions
            foreach(CheckActionParameters action in allActions)
            {
                if(action.action == CheckCardAction.None)
                    continue;

                // Get actions
                SVEFormulaParser.ParseValueAsMinMax(action.amount, player, out int minSelect, out int maxSelect);
                if(!GetActionInfo(action, player, ref minSelect, ref maxSelect, out string actionText, out Action<List<CardObject>> confirmAction))
                    continue;
                bool hasSecondaryActions = GetSecondaryActionsInfo(action, player, out List<string> secondaryActionTexts, out List<Action<List<CardObject>>> secondaryConfirmActions);

                // Auto-complete if applicable
                if(ActionCanAutoComplete(action.action) && !hasSecondaryActions && action.filter == null && action.amount == null)
                {
                    confirmAction?.Invoke(new List<CardObject>(selectionArea.AllCards));
                    break;
                }

                // Select targets and perform effect
                bool waiting = true;
                selectionArea.SetFilter(action.filter);
                selectionArea.SetConfirmAction(cardName, actionText, text, minSelect, maxSelect, selectedCards =>
                {
                    confirmAction?.Invoke(selectedCards);
                    waiting = false;
                });
                if(hasSecondaryActions)
                    for(int i = 0; i < secondaryActionTexts.Count && i < secondaryConfirmActions.Count; i++)
                        selectionArea.AddAdditionalConfirmAction(secondaryActionTexts[i], secondaryConfirmActions[i]);
                yield return new WaitUntil(() => !waiting);
            }

            selectionArea.Disable();
            player.InputController.allowedInputs = player.isActivePlayer ? PlayerInputController.InputTypes.All : PlayerInputController.InputTypes.None;
            player.ZoneController.handZone.SetAllCardsInteractable(player.isActivePlayer);
            onComplete?.Invoke();
        }

        #endregion

        // ------------------------------

        #region Get Info

        private bool ActionCanAutoComplete(CheckCardAction action) => action is CheckCardAction.Hand or CheckCardAction.Cemetery;

        private bool GetActionInfo(CheckActionParameters action, PlayerController player, ref int minSelect, ref int maxSelect, out string actionText, out Action<List<CardObject>> confirmAction)
        {
            CardSelectionArea selectionArea = player.ZoneController.selectionArea;
            minSelect = 0;
            switch(action.action)
            {
                // Send to zone
                case CheckCardAction.Hand:
                    actionText = "Add to Hand";
                    confirmAction = selectedCards =>
                    {
                        foreach(CardObject card in selectedCards)
                        {
                            card.Interactable = player.isActivePlayer; // TODO - this might cause problems later
                            player.LocalEvents.DrawCard(card, reveal: !action.filter.IsNullOrWhiteSpace());
                        }
                    };
                    return true;
                case CheckCardAction.Cemetery:
                    actionText = "Send to Cemetery";
                    confirmAction = selectedCards =>
                    {
                        foreach(CardObject card in selectedCards)
                            player.LocalEvents.SendToCemetery(card, SVEProperties.Zones.Deck);
                    };
                    return true;
                case CheckCardAction.Field:
                    actionText = "Place on Field";
                    confirmAction = selectedCards =>
                    {
                        foreach(CardObject card in selectedCards)
                        {
                            card.Interactable = player.isActivePlayer;
                            player.LocalEvents.PlayCardToField(card, SVEProperties.Zones.Deck, payCost: false);
                        }
                    };
                    return true;
                case CheckCardAction.TopOrBottomDeck:
                    actionText = "Send to Top Deck";
                    confirmAction = selectedCards =>
                    {
                        foreach(CardObject card in selectedCards)
                            player.LocalEvents.SendToTopDeck(card, SVEProperties.Zones.Deck);
                    };
                    return true;

                // Rearrange
                case CheckCardAction.BottomDeckAnyOrder:
                    actionText = "Send All to Bottom Deck";
                    confirmAction = _ =>
                    {
                        List<CardObject> cardsToMove = new(selectionArea.GetAllPrimaryCards());
                        foreach(CardObject card in cardsToMove)
                            player.LocalEvents.SendToBottomDeck(card, SVEProperties.Zones.Deck);
                    };
                    minSelect = 1; // don't allow selecting cards, only allow moving cards
                    maxSelect = 0;
                    selectionArea.SwitchMode(CardSelectionArea.SelectionMode.MoveSelectionArea);
                    return true;

                // Other
                default:
                    actionText = null;
                    confirmAction = null;
                    return false;
            }
        }

        private bool GetSecondaryActionsInfo(CheckActionParameters action, PlayerController player, out List<string> actionTexts, out List<Action<List<CardObject>>> confirmActions)
        {
            switch(action.action)
            {
                // Send to zone
                case CheckCardAction.TopOrBottomDeck:
                    actionTexts = new List<string> { "Send to Bottom Deck" };
                    confirmActions = new List<Action<List<CardObject>>>();
                    confirmActions.Add(selectedCards =>
                    {
                        foreach(CardObject card in selectedCards)
                            player.LocalEvents.SendToBottomDeck(card, SVEProperties.Zones.Deck);
                    });
                    return true;

                // Other
                default:
                    actionTexts = null;
                    confirmActions = null;
                    return false;
            }
        }

        #endregion
    }
}
