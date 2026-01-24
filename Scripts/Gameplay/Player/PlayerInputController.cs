using System;
using System.Collections.Generic;
using System.Linq;
using CCGKit;
using DG.Tweening;
using UnityEngine;
using Sirenix.OdinInspector;
using SVESimulator.UI;

namespace SVESimulator
{
    // TODO - This file is a mess and needs refactoring
    // i am so sorry to whoever might touch this script in the future (including myself)
    public class PlayerInputController : MonoBehaviour
    {
        #region Variables

        [System.Flags]
        public enum InputTypes
        {
            None = 0,
            PlayCards = 1,
            Attack = 2,
            ActivateAbilities = 4,
            MoveCards = 8,
            OnlyQuicks = 16,
            All = ~OnlyQuicks
        }

        [TitleGroup("Runtime Data"), SerializeField]
        public InputTypes allowedInputs = InputTypes.All;
        [SerializeField, ReadOnly]
        private CardObject currentSelectedCard;
        [SerializeField, ReadOnly]
        private CardObject currentHoveredCard;
        [HorizontalGroup("Interact"), SerializeField, ReadOnly, LabelWidth(150)]
        public bool canInteract;
        [HorizontalGroup("Interact"), SerializeField, ReadOnly, LabelWidth(150)]
        private bool isInteracting;
        [FoldoutGroup("Drag Data"), SerializeField, ReadOnly]
        private Vector3 originalCardWorldPosition;
        [FoldoutGroup("Drag Data"), SerializeField, ReadOnly]
        private Vector3 originalCardLocalPosition;
        [FoldoutGroup("Drag Data"), SerializeField, ReadOnly]
        private Transform originalCardParent;
        [SerializeField, ReadOnly]
        private TargetableSlot currentTargetSlot;

        [TitleGroup("Settings & References"), SerializeField]
        private GameObject cardHighlightObject;
        [SerializeField, ReadOnly]
        private Transform cardHighlightTransform;
        [SerializeField]
        private Vector3 cardHighlightOffset = Vector3.down;
        [SerializeField]
        private float cardLiftDistance = 1f;
        [SerializeField, InlineEditor]
        private PlayerInputSettings inputSettings;

        [TitleGroup("Debug"), SerializeField]
        private bool debugMode;
        [SerializeField]
        private Transform debugSphere;

        private Camera cam;
        private RaycastHit[] raycastHits;

        public event Action<CardObject> OnChangeSelectedCard;
        public event Action<CardObject> OnBeginHoverOverCard;

        private PlayerController _player;
        private PlayerController Player
        {
            get
            {
                if(!_player)
                    InitializePlayerControllerReference();
                return _player;
            }
        }

        #endregion

        // ------------------------------

        #region Unity Callbacks

        private void Start()
        {
            cam = Camera.main;
            ConnectToCardInfoDisplay();
        }

        private void Update()
        {
            Vector3 mousePosition = cam.ScreenToWorldPoint(Input.mousePosition);
            if(allowedInputs == InputTypes.None)
            {
                if(!isInteracting)
                {
                    FindSelectedCard(mousePosition); // update the current hovered card info display
                    FindTargetSlot(mousePosition); // handle input for viewing zones
                }
                return;
            }

            #region Card
            if(canInteract && currentSelectedCard)
            {
                if(Input.GetMouseButtonDown(0))
                    BeginInteract();
                else if(Input.GetMouseButtonUp(0))
                    EndInteract();
            }
            else if(currentHoveredCard && currentHoveredCard.Interactable && Input.GetMouseButtonDown(0))
            {
                TryOpenActivateEffectWindow(currentHoveredCard);
            }

            if(isInteracting)
                InteractionUpdate(currentSelectedCard, mousePosition);
            else
                FindSelectedCard(mousePosition);

            // Highlight
            UpdateSelectionHighlight();
            #endregion

            // ---

            #region Target Slot
            FindTargetSlot(mousePosition);
            #endregion

            // ---

            DebugUpdate();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if(cardHighlightObject)
                cardHighlightTransform = cardHighlightObject.transform;
        }
#endif

        #endregion

        // ------------------------------

        #region Initialization

        private void InitializePlayerControllerReference()
        {
            _player = FindObjectsOfType<PlayerController>().First(x => x.isLocalPlayer);
            Debug.Assert(_player);

            _player.onEndGameEvent += OnEndGame;
        }

        private void ConnectToCardInfoDisplay()
        {
            OnBeginHoverOverCard += GameUIManager.CardInfoDisplay.Display;
        }

        #endregion

        // ------------------------------

        #region Event Handling

        private void OnEndGame()
        {
            allowedInputs = InputTypes.None;
        }

        #endregion

        // ------------------------------

        #region Interaction Handling

        private void BeginInteract()
        {
            if(isInteracting)
                return;

            isInteracting = true;
            cardHighlightObject.SetActive(false);
            currentSelectedCard.transform.DOKill();

            switch(currentSelectedCard.CurrentZone.InteractionType)
            {
                case CardZone.ZoneInteractionType.MoveCard:
                    originalCardWorldPosition = currentSelectedCard.transform.position;
                    originalCardLocalPosition = currentSelectedCard.transform.localPosition;
                    originalCardParent = currentSelectedCard.transform.parent;
                    currentSelectedCard.transform.parent = null;
                    break;
                case CardZone.ZoneInteractionType.TargetEffect:
                    Player.OppZoneController.fieldZone.CalculateValidAttackTargets(currentSelectedCard, out bool wardTargetsExist);
                    if(currentSelectedCard.CanAttackLeader && !wardTargetsExist)
                    {
                        CardObject oppLeader = Player.OppZoneController.leaderZone.AllCards[0];
                        Debug.Assert(oppLeader);
                        oppLeader.SetHighlightMode(CardObject.HighlightMode.ValidTarget);
                        oppLeader.IsValidDefender = true;
                    }
                    break;
            }
        }

        private void EndInteract()
        {
            if(!isInteracting)
                return;

            isInteracting = false;
            CardManager.Animator.SetTargetingLineActive(false);

            // Open ability menu
            if(currentSelectedCard.CurrentZone == Player.ZoneController.fieldZone && currentTargetSlot && currentTargetSlot.Card == currentSelectedCard)
            {
                TryOpenActivateEffectWindow(currentSelectedCard);
            }

            // Interact with new slot
            else if(currentTargetSlot)
            {
                switch(currentTargetSlot.CurrentInteractionType)
                {
                    case TargetableSlot.InteractionType.None:
                        break;

                    case TargetableSlot.InteractionType.MoveCard:
                        if(!allowedInputs.HasFlag(InputTypes.MoveCards))
                            break;
                        if(currentSelectedCard.CurrentZone.InteractionType == CardZone.ZoneInteractionType.MoveCard)
                        {
                            if(currentTargetSlot.ParentZone is PlayerHandZone)
                            {
                                if(currentSelectedCard.CurrentZone is PlayerHandZone)
                                    break;
                                Player.ZoneController.AddCardToHand(currentSelectedCard);
                                return;
                            }
                            if(currentTargetSlot.ParentZone is not CardSelectionArea)
                            {
                                Debug.LogError($"Unsupported action: attempted to MoveCard into zone {currentTargetSlot.ParentZone}, which is not supported!");
                                break;
                            }
                            if(currentSelectedCard.CurrentZone is CardSelectionArea)
                            {
                                if(Player.ZoneController.SwapCardSlots(currentSelectedCard, currentTargetSlot.Card))
                                    return;
                                else
                                    break;
                            }
                            if(currentTargetSlot.SlotNumber > -1)
                                Player.ZoneController.MoveCardToSelectionArea(currentSelectedCard, currentTargetSlot.SlotNumber);
                            else
                                Player.ZoneController.MoveCardToSelectionArea(currentSelectedCard);
                            return;
                        }
                        break;

                    case TargetableSlot.InteractionType.PlayCard:
                    case TargetableSlot.InteractionType.PlaySpell:
                        if(!allowedInputs.HasFlag(InputTypes.PlayCards))
                            break;
                        if(currentSelectedCard.CurrentZone.InteractionType == CardZone.ZoneInteractionType.MoveCard)
                        {
                            if(currentSelectedCard.IsSpell())
                            {
                                if(Player.LocalEvents.PlaySpell(currentHoveredCard, currentSelectedCard.CurrentZone.Runtime.name))
                                    return;
                            }
                            else
                            {
                                bool playCardSuccess = currentTargetSlot.SlotNumber > -1
                                    ? Player.LocalEvents.PlayCardToField(currentSelectedCard, currentTargetSlot.SlotNumber)
                                    : Player.LocalEvents.PlayCardToField(currentSelectedCard);
                                if(playCardSuccess)
                                    return;
                            }
                        }
                        break;

                    case TargetableSlot.InteractionType.AttackCard:
                        if(!allowedInputs.HasFlag(InputTypes.Attack))
                            break;
                        if(Player.isActivePlayer && currentSelectedCard.CanAttack && currentTargetSlot.ParentZone is CardPositionedZone zone)
                        {
                            CardObject defendingCard = zone.GetCard(currentTargetSlot.SlotNumber);
                            if(defendingCard.IsValidDefender)
                            {
                                Player.LocalEvents.AttackFollower(currentSelectedCard, defendingCard);
                                Player.ZoneController.fieldZone.RemoveAllCardHighlights();
                                allowedInputs = InputTypes.None; // wait for quick timing
                            }
                        }
                        break;

                    case TargetableSlot.InteractionType.AttackLeader:
                        if(!allowedInputs.HasFlag(InputTypes.Attack))
                            break;
                        if(Player.isActivePlayer && currentSelectedCard.CanAttack && currentSelectedCard.CanAttackLeader)
                        {
                            Player.LocalEvents.AttackLeader(currentSelectedCard);
                            Player.ZoneController.fieldZone.RemoveAllCardHighlights();
                            allowedInputs = InputTypes.None; // wait for quick timing
                        }
                        break;
                }
            }

            // ---

            // Cleanup on end interact
            if(currentSelectedCard.CurrentZone.InteractionType == CardZone.ZoneInteractionType.MoveCard)
            {
                currentSelectedCard.transform.parent = originalCardParent;
                currentSelectedCard.transform.localPosition = originalCardLocalPosition;
            }
            else if(currentSelectedCard.CurrentZone.InteractionType == CardZone.ZoneInteractionType.TargetEffect)
            {
                CardObject oppLeader = Player.OppZoneController.leaderZone.AllCards[0];
                Debug.Assert(oppLeader);
                oppLeader.SetHighlightMode(CardObject.HighlightMode.None);
                oppLeader.IsValidDefender = false;
            }
            Player.OppZoneController.fieldZone.RemoveAllCardHighlights();
        }

        private void InteractionUpdate(CardObject card, Vector3 mousePosition)
        {
            if(!card || !isInteracting)
                return;

            if(card.CurrentZone.InteractionType == CardZone.ZoneInteractionType.MoveCard)
                MoveCard(currentSelectedCard, mousePosition);
            else if(card.CurrentZone.InteractionType == CardZone.ZoneInteractionType.TargetEffect)
                SetTarget(currentSelectedCard, mousePosition);
        }

        // -----

        #region Movement/Targeting

        private void MoveCard(CardObject card, Vector3 mousePosition)
        {
            Vector3 targetPosition = mousePosition;
            targetPosition.y = originalCardWorldPosition.y + cardLiftDistance;
            CardManager.Animator.LerpCardToPosition(card, targetPosition);
        }

        private void SetTarget(CardObject card, Vector3 mousePosition)
        {
            if(!Physics.Raycast(mousePosition, Vector3.down, out RaycastHit hit, inputSettings.RaycastDistance, inputSettings.FieldRaycastLayers))
            {
                CardManager.Animator.SetTargetingLineActive(false);
                return;
            }

            CardManager.Animator.SetTargetingLineActive(true);
            CardManager.Animator.SetTargetingLine(card, hit.point);
        }

        #endregion

        // -----

        #region Activated Abilities

        private bool TryOpenActivateEffectWindow(CardObject card)
        {
            if(!card.CurrentZone.IsLocalPlayerZone || !allowedInputs.HasFlag(InputTypes.ActivateAbilities))
                return false;
            bool onlyQuicks = allowedInputs.HasFlag(InputTypes.OnlyQuicks);
            List<ActivatedAbility> activatedAbilities = card.LibraryCard.abilities.Where(x => x is ActivatedAbility && x.effect is SveEffect).Select(x => x as ActivatedAbility).ToList();

            if(onlyQuicks && !activatedAbilities.Any(x => x.costs.Any(y => y is QuickEffectAsCost)))
                return false;
            if(card.HasEvolveCost() || activatedAbilities.Count > 0 || card.RuntimeCard.HasCounter(SVEProperties.Counters.Stack))
            {
                GameUIManager.ActivateEffect.Open(Player, card, activatedAbilities, onlyQuicks: onlyQuicks);
                return true;
            }
            return false;
        }

        #endregion

        #endregion

        // ------------------------------

        #region Selection

        private void FindSelectedCard(in Vector3 mousePosition)
        {
            if(Physics.Raycast(mousePosition, Vector3.down, out RaycastHit hit, inputSettings.RaycastDistance, inputSettings.CardRaycastLayers.value))
            {
                if(hit.transform.TryGetComponent(out CardObject card))
                {
                    SetHoveredCard(card);
                    if(!card.Interactable || card.Engaged)
                        goto deselectCurrent;
                    if(card.CurrentZone.InteractionType == CardZone.ZoneInteractionType.TargetEffect)
                    {
                        if(!(allowedInputs.HasFlag(InputTypes.Attack) || allowedInputs.HasFlag(InputTypes.ActivateAbilities)))
                            goto deselectCurrent;
                        if(!card.CanAttack)
                            goto deselectCurrent;
                    }
                    SelectCard(card);
                    return;
                }
                if(card == null)
                    SetHoveredCard(null);
                deselectCurrent:
                DeselectCurrentCard();
            }
            else
            {
                SetHoveredCard(null);
                DeselectCurrentCard();
            }
        }

        private void SelectCard(CardObject card)
        {
            if(currentSelectedCard == card)
                return;
            if(currentSelectedCard != null)
                DeselectCurrentCard(invokeEvent: false);
            if(card == null)
                return;

            currentSelectedCard = card;
            currentSelectedCard.OnHoverEnter();
            canInteract = currentSelectedCard.CurrentZone.IsLocalPlayerZone;

            OnChangeSelectedCard?.Invoke(card);
        }

        private void DeselectCurrentCard(bool invokeEvent = true)
        {
            if(currentSelectedCard == null)
                return;

            currentSelectedCard.OnHoverExit();
            currentSelectedCard = null;
            canInteract = false;

            if(invokeEvent)
                OnChangeSelectedCard?.Invoke(null);
        }

        private void SetHoveredCard(CardObject card)
        {
            if(card == currentHoveredCard)
                return;
            currentHoveredCard = card;
            OnBeginHoverOverCard?.Invoke(card);
        }

        // -----

        private void FindTargetSlot(in Vector3 mousePosition)
        {
            if(Physics.Raycast(mousePosition, Vector3.down, out RaycastHit hit, inputSettings.RaycastDistance, inputSettings.TargetSlotRaycastLayers.value))
            {
                TargetableSlot slot = hit.transform.GetComponentInParent<TargetableSlot>();
                if(slot != currentTargetSlot)
                {
                    if(currentTargetSlot)
                        currentTargetSlot.OnHoverEnd();
                    currentTargetSlot = slot;
                    if(currentTargetSlot)
                        currentTargetSlot.OnHoverBegin();
                }
                else if(!slot && Input.GetKeyDown(KeyCode.Mouse0) && hit.transform.TryGetComponent(out ViewZoneControllerBase viewZoneCollider)
                    && (!currentHoveredCard || currentHoveredCard.CurrentZone == viewZoneCollider.Zone))
                {
                    viewZoneCollider.ViewZone();
                }
            }
            else
            {
                if(currentTargetSlot)
                    currentTargetSlot.OnHoverEnd();
                currentTargetSlot = null;
            }
        }

        private void UpdateSelectionHighlight()
        {
            if(!currentSelectedCard || !currentSelectedCard.CurrentZone.IsLocalPlayerZone)
            {
                cardHighlightObject.SetActive(false);
                return;
            }

            cardHighlightObject.SetActive(true);
            cardHighlightTransform.position = currentSelectedCard.transform.position + cardHighlightOffset;
        }

        #endregion

        // ------------------------------

        #region Debug

        private void DebugUpdate()
        {
            if(!debugMode)
            {
                debugSphere.gameObject.SetActive(false);
                return;
            }

            Vector3 mousePosition = cam.ScreenToWorldPoint(Input.mousePosition);
            if(Physics.Raycast(mousePosition, Vector3.down, out RaycastHit hit))
            {
                debugSphere.position = hit.point;
            }
        }

        #endregion

    }
}
