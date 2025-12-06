using System;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CCGKit;
using DG.Tweening;
using Sparkfire.Utility;
using SVESimulator.UI;

using MultipleChoiceEntryData = SVESimulator.UI.MultipleChoiceWindow.MultipleChoiceEntryData;

namespace SVESimulator
{
    public class CardSelectionArea : CardPositionedZone
    {
        #region Variables

        public enum SelectionMode { PlaceCardsFromHand, SelectCardsFromDeck, SelectCardsFromCemetery, SelectCardsFromOppHand, MoveSelectionArea }

        [TitleGroup("Runtime Data"), SerializeField, ReadOnly]
        private SelectionMode currentMode;
        [TitleGroup("Runtime Data"), SerializeField, ReadOnly]
        private List<CardObject> currentSelectedCards = new();
        [TitleGroup("Runtime Data"), SerializeField]
        private int minSlotCount;
        [TitleGroup("Runtime Data"), SerializeField]
        private int maxSlotCount;
        [TitleGroup("Runtime Data"), SerializeField]
        private int minSelectCount;
        [TitleGroup("Runtime Data"), SerializeField]
        private int maxSelectCount;
#if UNITY_EDITOR
        [TitleGroup("Runtime Data"), ShowInInspector, HideInEditorMode, ReadOnly, LabelText("Current Filters")]
        private SerializedDictionary<SVEFormulaParser.CardFilterSetting, string> debug_SerializedFilter = new();
#endif

        [TitleGroup("Settings"), SerializeField]
        private Vector2 slotSpacing = new Vector2(7f, 14f);
        [TitleGroup("Settings"), SerializeField]
        private int maxRowLength = 10;
        [TitleGroup("Settings"), SerializeField]
        private SerializedDictionary<int, float> startHeightByRowCount;
        [TitleGroup("Settings"), SerializeField]
        private float repositionTime = 0.5f;

        [TitleGroup("Object References"), SerializeField, AssetsOnly]
        private TargetableSlot slotPrefab;
        [SerializeField]
        private PlayerCardZoneController zoneController;
        [SerializeField]
        private PlayerInputSettings inputSettings;

        private Camera cam;
        private Dictionary<SVEFormulaParser.CardFilterSetting, string> currentFilter;

        public int ValidTargetsCount => AllCards.Count(x => currentFilter.MatchesCard(x));

        #endregion

        // ------------------------------

        #region Enable/Disable

        public void Enable(SelectionMode mode, int minSlotCount = 1, int maxSlotCount = 5)
        {
            SwitchMode(mode);
            this.minSlotCount = minSlotCount;
            this.maxSlotCount = maxSlotCount;
            gameObject.SetActive(true);
            zoneController.fieldZone.RemoveAllCardHighlights();

            int i;
            for(i = 0; i < minSlotCount; i++)
            {
                if(i < cardSlots.Count)
                    cardSlots[i].target.gameObject.SetActive(true);
                else
                    CreateNewSlot();
            }
            for(; i < cardSlots.Count; i++)
            {
                cardSlots[i].target.gameObject.SetActive(false);
            }
            RepositionSlots(true);
        }

        [Button]
        public void Disable()
        {
            List<CardObject> cardsToMove = GetAllPrimaryCards();
            foreach(CardObject card in cardsToMove)
                card.SetHighlightMode(CardObject.HighlightMode.None);
            switch(currentMode)
            {
                case SelectionMode.PlaceCardsFromHand:
                    foreach(CardObject card in cardsToMove)
                        zoneController.AddCardToHand(card);
                    break;
                case SelectionMode.SelectCardsFromDeck:
                    foreach(CardObject card in cardsToMove)
                        zoneController.SendCardToTopDeck(card);
                    break;
                case SelectionMode.SelectCardsFromCemetery:
                    foreach(CardObject card in cardsToMove)
                        zoneController.SendCardToCemetery(card);
                    break;
                case SelectionMode.SelectCardsFromOppHand:
                    foreach(CardObject card in cardsToMove)
                        Player.OppZoneController.AddCardToHand(card);
                    break;
            }

            DeselectAllCards();
            SetFilter("");
            Player.InputController.allowedInputs = Player.isActivePlayer ? PlayerInputController.InputTypes.All : PlayerInputController.InputTypes.None;
            zoneController.fieldZone.HighlightCardsCanAttack();
            GameUIManager.NetworkedCalls.CmdCloseOpponentTargeting(zoneController.Player.GetOpponentInfo().netId);
            gameObject.SetActive(false);
        }

        #endregion

        // ------------------------------

        #region Controls

        public void SwitchMode(SelectionMode newMode)
        {
            currentMode = newMode;
            switch(currentMode)
            {
                case SelectionMode.PlaceCardsFromHand:
                    Interactable = true;
                    InteractionType = ZoneInteractionType.MoveCard;
                    endInteractionType = TargetableSlot.InteractionType.MoveCard;
                    Player.InputController.allowedInputs = PlayerInputController.InputTypes.MoveCards;
                    SetCardsInteractable(false);
                    break;
                case SelectionMode.SelectCardsFromDeck:
                case SelectionMode.SelectCardsFromCemetery:
                case SelectionMode.SelectCardsFromOppHand:
                    Interactable = false;
                    InteractionType = ZoneInteractionType.None;
                    endInteractionType = TargetableSlot.InteractionType.None;
                    Player.InputController.allowedInputs = PlayerInputController.InputTypes.None;
                    SetCardsInteractable(false);
                    break;
                case SelectionMode.MoveSelectionArea:
                    Interactable = true;
                    InteractionType = ZoneInteractionType.MoveCard;
                    endInteractionType = TargetableSlot.InteractionType.MoveCard;
                    Player.InputController.allowedInputs = PlayerInputController.InputTypes.MoveCards;
                    SetCardsInteractable(true);
                    break;
            }
            foreach(CardSlot slot in cardSlots.Values)
                slot.target.CurrentInteractionType = endInteractionType;

            void SetCardsInteractable(bool interactable)
            {
                foreach(CardObject card in GetAllPrimaryCards())
                {
                    card.Interactable = interactable;
                    card.SetHighlightMode(CardObject.HighlightMode.None);
                }
            }
        }

        public void SetFilter(string filter)
        {
            currentFilter = SVEFormulaParser.ParseCardFilterFormula(filter);
            if(currentMode == SelectionMode.PlaceCardsFromHand)
            {
                Player.ZoneController.handZone.SetAllCardsInteractable(filter);
            }

#if UNITY_EDITOR
            debug_SerializedFilter.Clear();
            foreach(var kvPair in currentFilter)
                debug_SerializedFilter.Add(kvPair.Key, kvPair.Value);
#endif
        }

        public void AddAllCardsInHand()
        {
            List<CardObject> cardsToMove = new(zoneController.handZone.AllCards);
            foreach(CardObject card in cardsToMove)
                zoneController.MoveCardToSelectionArea(card, false);
        }

        public void AddAllCardsInOpponentsHand()
        {
            List<CardObject> cardsToMove = new(Player.OppZoneController.handZone.AllCards);
            foreach(CardObject card in cardsToMove)
                zoneController.MoveCardToSelectionArea(card, false);
        }

        public void AddCardFromTopDeck()
        {
            int currentCount = FilledSlotCount();
            if(currentCount >= maxSlotCount)
                return;
            if(currentCount >= minSlotCount)
            {
                if(currentCount < cardSlots.Count)
                    cardSlots[currentCount].target.gameObject.SetActive(true);
                else
                    CreateNewSlot();
                RepositionSlots(false);
            }

            // assuming that if we're adding cards from top deck, cards are *only* from top deck
            RuntimeCard runtimeCard = zoneController.deckZone.Runtime.cards[FilledSlotCount()];
            CardObject cardObject = CardManager.Instance.RequestCard(runtimeCard);
            cardObject.transform.SetPositionAndRotation(zoneController.deckZone.GetTopStackPosition(), SVEProperties.CardFaceDownRotation);
            cardObject.CurrentZone = zoneController.deckZone;
            zoneController.MoveCardToSelectionArea(cardObject, false);
        }

        public void AddAllCardsInDeck()
        {
            Debug.Assert(minSlotCount >= zoneController.deckZone.Runtime.cards.Count);
            foreach(RuntimeCard runtimeCard in zoneController.deckZone.Runtime.cards)
            {
                CardObject cardObject = CardManager.Instance.RequestCard(runtimeCard);
                cardObject.transform.SetPositionAndRotation(zoneController.deckZone.GetTopStackPosition(), SVEProperties.CardFaceDownRotation);
                cardObject.CurrentZone = zoneController.deckZone;
                zoneController.MoveCardToSelectionArea(cardObject, false);
            }
        }

        public void AddCemetery()
        {
            List<CardObject> cardsToMove = new(zoneController.cemeteryZone.AllCards);
            foreach(CardObject card in cardsToMove)
                zoneController.MoveCardToSelectionArea(card, false);
        }

        public void SetConfirmAction(string cardName, string actionText, string effectText, int minSelectionCount, int maxSelectionCount, Action<List<CardObject>> action, bool showTargetingToOpponent = true)
        {
            GameUIManager.MultipleChoice.Close();
            minSelectCount = minSelectionCount;
            maxSelectCount = maxSelectionCount;

            MultipleChoiceEntryData entryData = new()
            {
                text = actionText,
                onSelect = () =>
                {
                    action?.Invoke(new List<CardObject>(currentSelectedCards));
                    DeselectAllCards();
                    GameUIManager.MultipleChoice.Close();
                }
            };
            List<MultipleChoiceEntryData> entries = new() { entryData };
            if(minSelectCount == 0)
            {
                entries.Add(new MultipleChoiceEntryData()
                {
                    text = "Skip",
                    onSelect = () =>
                    {
                        action?.Invoke(new List<CardObject>());
                        GameUIManager.MultipleChoice.Close();
                    }
                });
            }

            GameUIManager.MultipleChoice.Open(zoneController.Player, cardName, entries, effectText,
                showBackgroundTint: false, showTargetingToOpponent: showTargetingToOpponent, disablePlayerInputs: false);
            UpdateActionButton();
        }

        #endregion

        // ------------------------------

        #region Unity Functions

        private void Awake()
        {
            cam = Camera.main;
        }

        private void Update()
        {
            if(currentMode is not SelectionMode.SelectCardsFromDeck and not SelectionMode.SelectCardsFromCemetery and not SelectionMode.SelectCardsFromOppHand)
                return;

            if(Input.GetKeyDown(KeyCode.Mouse0) && Physics.Raycast(cam.ScreenToWorldPoint(Input.mousePosition), Vector3.down, out RaycastHit hit, inputSettings.RaycastDistance, inputSettings.CardRaycastLayers.value))
            {
                if(hit.transform.TryGetComponent(out CardObject card) && cards.Contains(card) && currentFilter.MatchesCard(card))
                {
                    ToggleCardSelection(card);
                }
            }
        }

        #endregion

        // ------------------------------

        #region Internal Controls

        public override void AddCard(CardObject card)
        {
            base.AddCard(card);
            if(currentMode == SelectionMode.PlaceCardsFromHand)
            {
                currentSelectedCards.Add(card);
                UpdateActionButton();
            }
        }

        public override void RemoveCard(CardObject card)
        {
            base.RemoveCard(card);
            if(currentMode == SelectionMode.PlaceCardsFromHand)
            {
                currentSelectedCards.Remove(card);
                UpdateActionButton();
            }
        }

        public override void MoveCardToSlot(CardObject card, int slot, TargetableSlot.InteractionType newInteractionType)
        {
            CardObject oldCard = cardSlots[slot].card;
            if(oldCard)
            {
                currentSelectedCards.Remove(oldCard);
                switch(currentMode)
                {
                    case SelectionMode.PlaceCardsFromHand:
                        Player.ZoneController.AddCardToHand(oldCard, onComplete: () =>
                        {
                            // i hate this but it works and we need the one frame delay so whatever i'm done
                            StartCoroutine(TryReEnableCard());
                            IEnumerator TryReEnableCard()
                            {
                                yield return null;
                                if(gameObject.activeInHierarchy && currentFilter.MatchesCard(oldCard)) // if selection area is still open
                                    oldCard.Interactable = true;
                            }
                        });
                        break;
                    case SelectionMode.SelectCardsFromDeck:
                        Player.ZoneController.SendCardToTopDeck(oldCard);
                        break;
                }
            }
            base.MoveCardToSlot(card, slot, newInteractionType);
        }

        private void CreateNewSlot()
        {
            TargetableSlot target = Instantiate(slotPrefab, transform);
            CardSlot newSlot = new()
            {
                transform = target.transform,
                target = target
            };
            int index = cardSlots.Keys.Max() + 1;
            cardSlots.Add(index, newSlot);
        }

        private void RepositionSlots(bool instant)
        {
            int slotCount = cardSlots.Count(x => x.Value.target.isActiveAndEnabled);
            float leftPosition = slotCount % 2 == 1
                ? -slotSpacing.x * Math.Min(slotCount / 2, maxRowLength / 2)
                : -slotSpacing.x * Math.Min(slotCount / 2, maxRowLength / 2) - 0.5f;
            float topPosition = startHeightByRowCount.GetValueOrDefault((int)(slotCount / maxRowLength) + 1, 0f);

            for(int i = 0; i < slotCount; i++)
            {
                TargetableSlot slot = cardSlots[i].target;
                Vector3 targetPosition = new Vector3(leftPosition + (slotSpacing.x * (i % maxRowLength)), 0f, topPosition + (-slotSpacing.y * (int)(i / maxRowLength)));
                if(instant)
                    slot.transform.localPosition = targetPosition;
                else
                    slot.transform.DOLocalMove(targetPosition, repositionTime, true);
            }
        }

        private void ToggleCardSelection(CardObject card)
        {
            if(!currentSelectedCards.Contains(card))
            {
                if(currentSelectedCards.Count < maxSelectCount)
                {
                    currentSelectedCards.Add(card);
                    card.SetHighlightMode(CardObject.HighlightMode.Selected);
                }
            }
            else
            {
                currentSelectedCards.Remove(card);
                card.SetHighlightMode(CardObject.HighlightMode.None);
            }
            UpdateActionButton();
        }

        private void UpdateActionButton()
        {
            // If minSelectCount is 0, the "Close" button is an option, so the action button should be disabled if none are selected
            GameUIManager.MultipleChoice.SetButtonActive(0, currentSelectedCards.Count >= Mathf.Max(minSelectCount, 1) || maxSelectCount == 0);
        }

        private void DeselectAllCards()
        {
            foreach(CardObject card in currentSelectedCards)
                card.SetHighlightMode(CardObject.HighlightMode.None);
            currentSelectedCards.Clear();
        }

        #endregion
    }
}
