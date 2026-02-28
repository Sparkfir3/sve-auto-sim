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
using UnityEngine.UI;
using MultipleChoiceEntryData = SVESimulator.UI.MultipleChoiceWindow.MultipleChoiceEntryData;

namespace SVESimulator
{
    // Internal zone for selecting effect targets from a zone other than the field or EX area
    public class CardSelectionArea : CardPositionedZone
    {
        #region Variables

        public enum SelectionMode { PlaceCardsFromHand, SelectCardsFromDeck, SelectCardsFromCemetery, SelectCardsFromOppHand, MoveSelectionArea,
            ViewCardsCemetery, ViewCardsOppCemetery, ViewCardsEvolveDeck, ViewCardsOppEvolveDeck, ViewCardsBanished, ViewCardsOppBanished }

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

        [BoxGroup("Settings/Slot Spacing"), SerializeField]
        private float slotScale = 1f;
        [BoxGroup("Settings/Slot Spacing"), SerializeField]
        private Vector2 slotSpacing = new Vector2(7f, 14f);
        [BoxGroup("Settings/Slot Spacing"), SerializeField]
        private int maxRowLength = 10;
        [BoxGroup("Settings/Slot Spacing"), SerializeField]
        private float startHeight = 10f;
        [BoxGroup("Settings/Scrolling"), SerializeField]
        private float scrollAreaHeightPerRow = 15f;
        [BoxGroup("Settings/Scrolling"), SerializeField]
        private float scrollAreaMargin = 5f;
        [TitleGroup("Settings"), SerializeField]
        private float repositionTime = 0.5f;
        [field: TitleGroup("Settings"), SerializeField]
        public float AddRemoveCardDelay { get; private set; } = 0.02f;

        [TitleGroup("Object References"), SerializeField, AssetsOnly]
        private TargetableSlot slotPrefab;
        [SerializeField]
        private PlayerCardZoneController zoneController;
        [SerializeField]
        private GameObject targetSlot;
        [SerializeField]
        private PlayerInputSettings inputSettings;
        [SerializeField]
        private Transform slotContainer;
        [SerializeField]
        private Transform cardContainer;
        [BoxGroup("Scroll View"), SerializeField]
        private ScrollRect scrollRect;
        [BoxGroup("Scroll View"), SerializeField]
        private RectTransform scrollContent;
        [BoxGroup("Scroll View"), SerializeField]
        private CanvasGroup scrollViewTint;
        [BoxGroup("Scroll View"), SerializeField]
        private GameObject scrollViewRaycastBlocker;

        private Camera cam;
        private Dictionary<SVEFormulaParser.CardFilterSetting, string> currentFilter;
        private Vector3 contentPositionDiff;
        private Vector3 raycastHitPos;

        public bool IsActive => gameObject.activeInHierarchy;
        public int ValidTargetsCount => AllCards.Count(x => currentFilter.MatchesCard(x));
        public SelectionMode CurrentMode => currentMode;
        public float SlotScale => slotScale;

        #endregion

        // ------------------------------

        #region Enable/Disable

        public override void Initialize(RuntimeZone zone, PlayerCardZoneController controller)
        {
            base.Initialize(zone, controller);
            contentPositionDiff = scrollContent.transform.position - slotContainer.transform.position;
        }

        public void Enable(SelectionMode mode, int minSlotCount = 1, int maxSlotCount = 5, bool slotBackgroundsActive = true)
        {
            SwitchMode(mode);
            this.minSlotCount = minSlotCount;
            this.maxSlotCount = maxSlotCount;
            gameObject.SetActive(true);
            zoneController.fieldZone.RemoveAllCardHighlights();
            zoneController.handZone.RemoveAllCardHighlights();
            slotContainer.localPosition = Vector3.zero;
            cardContainer.localPosition = Vector3.zero;
            scrollContent.anchoredPosition = Vector2.zero;
            scrollRect.onValueChanged.AddListener(ScrollSelectionArea);
            raycastHitPos = Vector3.zero;
            SetScrollViewTintActive(false);

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
            SetSlotBackgroundsActive(slotBackgroundsActive);
        }

        [Button]
        public void Disable()
        {
            List<CardObject> cardsToMove = GetAllPrimaryCards();
            foreach(CardObject card in cardsToMove)
                card.SetHighlightMode(CardObject.HighlightMode.None);
            zoneController.handZone.SetTargetSlotActive(false);
            switch(currentMode)
            {
                // Local player zones modes
                case SelectionMode.PlaceCardsFromHand:
                    for(int i = 0; i < cardsToMove.Count; i++)
                        zoneController.AddCardToHand(cardsToMove[i], delay: i * AddRemoveCardDelay);
                    break;
                case SelectionMode.SelectCardsFromDeck:
                    for(int i = 0; i < cardsToMove.Count; i++)
                        zoneController.SendCardToTopDeck(cardsToMove[i], delay: i * AddRemoveCardDelay);
                    break;
                case SelectionMode.SelectCardsFromCemetery:
                case SelectionMode.ViewCardsCemetery:
                    for(int i = 0; i < cardsToMove.Count; i++)
                        zoneController.SendCardToCemetery(cardsToMove[i], delay: i * AddRemoveCardDelay);
                    break;
                case SelectionMode.ViewCardsEvolveDeck:
                    for(int i = 0; i < cardsToMove.Count; i++)
                        zoneController.SendCardToEvolveDeck(cardsToMove[i], delay: i * AddRemoveCardDelay);
                    break;
                case SelectionMode.ViewCardsBanished:
                    for(int i = 0; i < cardsToMove.Count; i++)
                        zoneController.SendCardToBanishedZone(cardsToMove[i], delay: i * AddRemoveCardDelay);
                    break;

                // Opponent zone modes
                case SelectionMode.SelectCardsFromOppHand:
                    for(int i = 0; i < cardsToMove.Count; i++)
                        Player.OppZoneController.AddCardToHand(cardsToMove[i], delay: i * AddRemoveCardDelay);
                    break;
                case SelectionMode.ViewCardsOppCemetery:
                    for(int i = 0; i < cardsToMove.Count; i++)
                        Player.OppZoneController.SendCardToCemetery(cardsToMove[i], delay: i * AddRemoveCardDelay);
                    break;
                case SelectionMode.ViewCardsOppEvolveDeck:
                    for(int i = 0; i < cardsToMove.Count; i++)
                        Player.OppZoneController.SendCardToEvolveDeck(cardsToMove[i], delay: i * AddRemoveCardDelay);
                    break;
                case SelectionMode.ViewCardsOppBanished:
                    for(int i = 0; i < cardsToMove.Count; i++)
                        Player.OppZoneController.SendCardToBanishedZone(cardsToMove[i], delay: i * AddRemoveCardDelay);
                    break;
            }

            DeselectAllCards();
            SetFilter("");
            scrollRect.onValueChanged.RemoveListener(ScrollSelectionArea);
            OnDisableZone();
            gameObject.SetActive(false);
        }

        protected virtual void OnDisableZone()
        {
            Player.InputController.allowedInputs = Player.isActivePlayer ? PlayerInputController.InputTypes.All : PlayerInputController.InputTypes.None;
            zoneController.fieldZone.HighlightCardsCanAttack();
            GameUIManager.NetworkedCalls.CmdCloseOpponentTargeting(zoneController.Player.GetOpponentInfo().netId);
        }

        #endregion

        // ------------------------------

        #region Mode/Action Controls

        public void SwitchMode(SelectionMode newMode)
        {
            if(currentMode == SelectionMode.PlaceCardsFromHand)
                zoneController.handZone.SetTargetSlotActive(true);

            currentMode = newMode;
            switch(currentMode)
            {
                case SelectionMode.PlaceCardsFromHand:
                    zoneController.handZone.SetTargetSlotActive(true);
                    goto case SelectionMode.MoveSelectionArea;
                case SelectionMode.MoveSelectionArea:
                    Interactable = true;
                    InteractionType = ZoneInteractionType.MoveCard;
                    endInteractionType = TargetableSlot.InteractionType.MoveCard;
                    Player.InputController.allowedInputs = PlayerInputController.InputTypes.MoveCards;
                    targetSlot.SetActive(true);
                    SetCardsInteractable(true);
                    break;
                case SelectionMode.SelectCardsFromDeck:
                case SelectionMode.SelectCardsFromCemetery:
                case SelectionMode.SelectCardsFromOppHand:
                case SelectionMode.ViewCardsCemetery:
                case SelectionMode.ViewCardsOppCemetery:
                case SelectionMode.ViewCardsEvolveDeck:
                case SelectionMode.ViewCardsOppEvolveDeck:
                case SelectionMode.ViewCardsBanished:
                case SelectionMode.ViewCardsOppBanished:
                    Interactable = false;
                    InteractionType = ZoneInteractionType.None;
                    endInteractionType = TargetableSlot.InteractionType.None;
                    Player.InputController.allowedInputs = PlayerInputController.InputTypes.None;
                    targetSlot.SetActive(false);
                    SetCardsInteractable(false);
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

        public void SetConfirmAction(string cardName, string actionText, string effectText, int minSelectionCount, int maxSelectionCount, Action<List<CardObject>> action,
            bool showTargetingToOpponent = true, ButtonDisplayPosition displayPosition = ButtonDisplayPosition.Top)
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
                showBackgroundTint: false, showTargetingToOpponent: showTargetingToOpponent, disablePlayerInputs: false,
                position: displayPosition);
            UpdateActionButton();
        }

        #endregion

        // ------------------------------

        #region Add Cards

        public void AddAllCardsInHand()
        {
            List<CardObject> cardsToMove = new(zoneController.handZone.AllCards);
            MoveCardsToSelectionArea(cardsToMove);
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

            RuntimeCard runtimeCard = zoneController.deckZone.Runtime.cards[FilledSlotCount()];
            CardObject cardObject = zoneController.CreateNewCardObjectTopDeck(runtimeCard);
            MoveCardToSelectionArea(cardObject);
        }

        public void AddAllCardsInDeck()
        {
            Debug.Assert(minSlotCount >= zoneController.deckZone.Runtime.cards.Count);
            List<RuntimeCard> cardsInZone = new(zoneController.deckZone.Runtime.cards);
            MoveCardsToSelectionAreaWithInstantiate(cardsInZone, runtimeCard =>
            {
                CardObject cardObject = CardManager.Instance.RequestCard(runtimeCard);
                cardObject.transform.SetPositionAndRotation(zoneController.deckZone.GetTopStackPosition(), SVEProperties.CardFaceDownRotation);
                cardObject.CurrentZone = zoneController.deckZone;
                return cardObject;
            });
            SetScrollViewTintActive(true);
        }

        public void AddCemetery()
        {
            List<CardObject> cardsToMove = new(zoneController.cemeteryZone.AllCards);
            MoveCardsToSelectionArea(cardsToMove);
            SetScrollViewTintActive(true);
        }

        public void AddEvolveDeck()
        {
            Debug.Assert(minSlotCount >= zoneController.evolveDeckZone.Runtime.cards.Count);
            List<RuntimeCard> cardsInZone = zoneController.evolveDeckZone.Runtime.cards.OrderByDescending(x => x.namedStats[SVEProperties.CardStats.FaceUp].effectiveValue)
                .ThenBy(x => x.cardId).ToList();
            MoveCardsToSelectionAreaWithInstantiate(cardsInZone, runtimeCard =>
            {
                CardObject cardObject = CardManager.Instance.RequestCard(runtimeCard);
                cardObject.transform.SetPositionAndRotation(zoneController.evolveDeckZone.GetTopStackPosition(), SVEProperties.CardFaceDownRotation);
                cardObject.CurrentZone = zoneController.evolveDeckZone;
                return cardObject;
            });
            SetScrollViewTintActive(true);
        }

        public void AddBanished()
        {
            List<CardObject> cardsToMove = new(zoneController.banishedZone.AllCards);
            MoveCardsToSelectionArea(cardsToMove);
            SetScrollViewTintActive(true);
        }

        #endregion

        #region Add Cards (Opponent)

        public void AddAllCardsInOpponentsHand()
        {
            List<CardObject> cardsToMove = new(Player.OppZoneController.handZone.AllCards);
            MoveCardsToSelectionArea(cardsToMove);
        }

        public void AddOpponentCemetery()
        {
            List<CardObject> cardsToMove = new(Player.OppZoneController.cemeteryZone.AllCards);
            MoveCardsToSelectionArea(cardsToMove);
            SetScrollViewTintActive(true);
        }

        public void AddOpponentEvolveDeck()
        {
            List<CardObject> cardsToMove = new(Player.OppZoneController.evolveDeckZone.AllCards);
            MoveCardsToSelectionArea(cardsToMove);
            SetScrollViewTintActive(true);
        }

        public void AddOpponentBanished()
        {
            List<CardObject> cardsToMove = new(Player.OppZoneController.banishedZone.AllCards);
            MoveCardsToSelectionArea(cardsToMove);
            SetScrollViewTintActive(true);
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

            if(Input.GetKeyDown(KeyCode.Mouse0))
            {
                raycastHitPos = Physics.Raycast(cam.ScreenToWorldPoint(Input.mousePosition), Vector3.down, out RaycastHit hit,
                    inputSettings.RaycastDistance, inputSettings.CardRaycastLayers | inputSettings.UIRaycastLayer)
                    ? hit.point
                    : Vector3.zero;
            }
            else if(Input.GetKeyUp(KeyCode.Mouse0) && Physics.Raycast(cam.ScreenToWorldPoint(Input.mousePosition), Vector3.down, out RaycastHit hit,
                    inputSettings.RaycastDistance, inputSettings.CardRaycastLayers | inputSettings.UIRaycastLayer) && Vector3.Distance(raycastHitPos, hit.point) < 5f)
            {
                if(hit.transform.TryGetComponent(out CardObject card) && cards.Contains(card) && currentFilter.MatchesCard(card))
                {
                    ToggleCardSelection(card);
                }
            }
        }

        #endregion

        // ------------------------------

        #region Zone Overrides

        public override void AddCard(CardObject card)
        {
            base.AddCard(card);
            card.transform.parent = cardContainer;
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

        #endregion

        // ------------------------------

        #region Internal Controls

        protected void MoveCardsToSelectionArea(List<CardObject> cardsToMove)
        {
            for(int i = 0; i < cardsToMove.Count; i++)
                MoveCardToSelectionArea(cardsToMove[i], i * AddRemoveCardDelay);
        }

        protected void MoveCardsToSelectionAreaWithInstantiate(List<RuntimeCard> cardsToMove, Func<RuntimeCard, CardObject> instantiateAction)
        {
            for(int i = 0; i < cardsToMove.Count; i++)
            {
                CardObject cardObject = CardManager.Instance.GetCardByInstanceId(cardsToMove[i].instanceId);
                if(!cardObject)
                {
                    cardObject = instantiateAction(cardsToMove[i]);
                    if(!cardObject)
                        continue;
                }
                MoveCardToSelectionArea(cardObject, i * AddRemoveCardDelay);
            }
        }

        protected virtual void MoveCardToSelectionArea(CardObject card, float delay = 0f)
        {
            zoneController.MoveCardToSelectionArea(card, delay: delay, rearrangeHand: false);
        }
        
        private void SetScrollViewTintActive(bool active)
        {
            scrollViewTint.alpha = active ? 1f : 0f;
            if(scrollViewRaycastBlocker)
                scrollViewRaycastBlocker.SetActive(active);
        }

        private void CreateNewSlot()
        {
            TargetableSlot target = Instantiate(slotPrefab, slotContainer);
            target.transform.localScale = Vector3.one * slotScale;
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
            int rowSize = Mathf.Min(slotCount, maxRowLength);
            float leftPosition = rowSize % 2 == 1
                ? -slotSpacing.x * Math.Min(rowSize / 2, maxRowLength / 2)
                : -slotSpacing.x * (Math.Min(rowSize / 2, maxRowLength / 2) - 0.5f);

            for(int i = 0; i < slotCount; i++)
            {
                TargetableSlot slot = cardSlots[i].target;
                Vector3 targetPosition = new Vector3(leftPosition + (slotSpacing.x * (i % maxRowLength)), 0f, startHeight + (-slotSpacing.y * (int)(i / maxRowLength)));
                if(instant)
                    slot.transform.localPosition = targetPosition;
                else
                    slot.transform.DOLocalMove(targetPosition, repositionTime, true);
            }

            scrollContent.sizeDelta = new Vector2(scrollContent.sizeDelta.x,
                (scrollAreaHeightPerRow * Mathf.Ceil((float)slotCount / maxRowLength)) + (scrollAreaMargin * 2f));
        }

        private void SetSlotBackgroundsActive(bool active)
        {
            foreach(CardSlot slot in cardSlots.Values)
            {
                if(slot.target.isActiveAndEnabled)
                    slot.target.SetBackgroundActive(active);
            }
        }

        private void ScrollSelectionArea(Vector2 value)
        {
            slotContainer.position = scrollContent.position - contentPositionDiff;
            cardContainer.position = scrollContent.position - contentPositionDiff;
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
