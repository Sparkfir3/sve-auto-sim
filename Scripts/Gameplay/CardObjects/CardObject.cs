using System;
using System.Collections.Generic;
using CCGKit;
using UnityEngine;
using Sirenix.OdinInspector;
using Sparkfire.Utility;

namespace SVESimulator
{
    public class CardObject : MonoBehaviour
    {
        #region Variables

        private static readonly int MainTex = Shader.PropertyToID("_MainTex");
        public enum HighlightMode { None, Hover, ValidTarget, Selected }

        // ---

        [field: TitleGroup("Runtime Data"), SerializeField, ReadOnly]
        public CardZone CurrentZone { get; set; }
        [field: SerializeField, ReadOnly]
        public bool IsVisible { get; set; }
        [field: HorizontalGroup("Attack"), SerializeField, ReadOnly, LabelWidth(120)]
        public bool CanAttack { get; set; }
        [field: HorizontalGroup("Attack"), SerializeField, ReadOnly, LabelWidth(120)]
        public bool CanAttackLeader { get; set; }
        [field: SerializeField, ReadOnly]
        public bool IsValidDefender { get; set; }
        [SerializeField]
        private bool _interactable = true;
        [SerializeField, ReadOnly]
        private bool isTrackingEngagedStatus;
        [field: SerializeField, ReadOnly]
        public int NumberOfTurnsOnBoard { get; set; }

        [TitleGroup("Runtime Data"), ShowInInspector]
        private bool hasRuntimeCard => _runtimeCard != null;
        [FoldoutGroup("Runtime Card Info", true), LabelText("Card ID"), ShowIf("@hasRuntimeCard"), ShowInInspector]
        private int runtimeCardId => hasRuntimeCard ? _runtimeCard.cardId : -1;
        [FoldoutGroup("Runtime Card Info", true), LabelText("Card Instance ID"), ShowIf("@hasRuntimeCard"), ShowInInspector]
        private int runtimeInstanceId => hasRuntimeCard ? _runtimeCard.instanceId : -1;

        [BoxGroup("Attachment"), SerializeField]
        private CardObject parentCard;
        [BoxGroup("Attachment"), SerializeField]
        private List<CardObject> attachedCards = new();

        // ---

        [TitleGroup("Object References"), SerializeField]
        private MeshRenderer cardFront;
        [SerializeField]
        private MeshRenderer cardBack;
        [SerializeField]
        private CardObjectStatsDisplay statsDisplay;
        [SerializeField]
        private SerializedDictionary<HighlightMode, GameObject> highlights = new();

        // ---

        public bool Interactable
        {
            get => _interactable && CurrentZone.Interactable;
            set => _interactable = value;
        }
        public bool Engaged => RuntimeCard != null &&
            RuntimeCard.namedStats.TryGetValue(SVEProperties.CardStats.Engaged, out Stat engagedStat) && engagedStat.baseValue > 0;

        private RuntimeCard _runtimeCard;
        public RuntimeCard RuntimeCard
        {
            get => _runtimeCard;
            set
            {
                if(value == null)
                    ResetRuntimeCard(_runtimeCard);
                else
                    InitRuntimeCard(value);
                _runtimeCard = value;
            }
        }
        public Card LibraryCard { get; protected set; }

        #endregion

        // ------------------------------

        #region Event Functions

        public void Initialize(RuntimeCard runtimeCard)
        {
            RuntimeCard = runtimeCard;
            SetStatOverlayActive(false);
            SetCostOverlayActive(false);
            SetHighlightMode(HighlightMode.None);

            _interactable = true;
            parentCard = null;
            NumberOfTurnsOnBoard = 0;
            CanAttack = false;
            CanAttackLeader = false;
            IsValidDefender = false;
            attachedCards.Clear();
        }

        public void OnStartTurn()
        {
            NumberOfTurnsOnBoard++;
            CanAttack = this.IsFollowerOrEvolvedFollower() && NumberOfTurnsOnBoard > 0;
            CanAttackLeader = this.IsFollowerOrEvolvedFollower() && NumberOfTurnsOnBoard > 0;
        }

        public void OnMoveZone()
        {
            NumberOfTurnsOnBoard = 0;
            CanAttack = false;
            CanAttackLeader = false;
            IsValidDefender = false;
            if(parentCard)
            {
                if(parentCard.RuntimeCard.namedStats.TryGetValue(SVEProperties.CardStats.Engaged, out Stat engagedStat))
                {
                    engagedStat.onValueChanged -= UpdateEngagedStatus;
                }
                parentCard = null;
            }
        }

        #endregion

        // ------------------------------

        #region Get Info

        public bool CanEvolve()
        {
            if(!RuntimeCard.namedStats.TryGetValue(SVEProperties.CardStats.EvolveCost, out Stat stat) || stat.baseValue < 0)
                return false;
            return RuntimeCard.IsCardType(SVEProperties.CardTypes.Follower);
        }

        public int GetEvolveCost()
        {
            return RuntimeCard.EvolveCost();
        }

        public List<CardObject> GetAttachedCards()
        {
            return attachedCards;
        }

        #endregion

        // ------------------------------

        #region Reserved/Engaged Controls

        public void StartTrackingEngagedStatus()
        {
            Debug.Assert(RuntimeCard != null);
            isTrackingEngagedStatus = true;
            RuntimeCard.namedStats[SVEProperties.CardStats.Engaged].onValueChanged += UpdateEngagedStatus;
        }

        public void StopTrackingEngagedStatus()
        {
            Debug.Assert(RuntimeCard != null);
            isTrackingEngagedStatus = false;
            RuntimeCard.namedStats[SVEProperties.CardStats.Engaged].onValueChanged -= UpdateEngagedStatus;
        }

        public void SetReserved() // untapped
        {
            Quaternion targetRot = Quaternion.identity;
            if(!CurrentZone.IsLocalPlayerZone)
                targetRot *= SVEProperties.OpponentCardRotation;
            CardManager.Animator.RotateCard(this, targetRot);
        }

        public void SetEngaged() // tapped
        {
            Quaternion targetRot = SVEProperties.CardEngagedRotation;
            if(!CurrentZone.IsLocalPlayerZone)
                targetRot *= SVEProperties.OpponentCardRotation;
            CardManager.Animator.RotateCard(this, targetRot);

            CanAttack = false;
            CanAttackLeader = false;
            SetHighlightMode(HighlightMode.None);
        }

        private void UpdateEngagedStatus(int prev, int engaged)
        {
            if(prev == engaged)
                return;
            if(engaged == 0)
                SetReserved();
            else
                SetEngaged();
        }

        #endregion

        // ------------------------------

        #region Set Graphics

        public void SetCardFront(Texture texture)
        {
            cardFront.material.SetTexture(MainTex, texture);
        }

        public void SetCardBack(Texture texture)
        {
            cardBack.material.SetTexture(MainTex, texture);
        }

        public void SetStatOverlayActive(bool active)
        {
            statsDisplay.MainStatContainer.SetActive(active);
        }

        public void SetCostOverlayActive(bool active)
        {
            statsDisplay.CostStatContainer.SetActive(active);
        }

        [TitleGroup("Buttons"), Button]
        public void SetHighlightMode(HighlightMode mode)
        {
            foreach(KeyValuePair<HighlightMode, GameObject> kvPair in highlights)
            {
                (HighlightMode m, GameObject highlight) = (kvPair.Key, kvPair.Value);
                if(!highlight) continue;
                highlight.SetActive(mode != HighlightMode.None && mode == m);
            }
        }

        #endregion

        // ------------------------------

        #region Set Stats & State

        public void AttachToCard(CardObject newParent)
        {
            if(!newParent)
                return;

            if(parentCard)
            {
                parentCard.attachedCards.Remove(this);
            }
            parentCard = newParent;
            newParent.attachedCards.Add(this);

            _interactable = false;
            SetStatOverlayActive(false);
            SetCostOverlayActive(false);
            if(parentCard.RuntimeCard.namedStats.TryGetValue(SVEProperties.CardStats.Engaged, out Stat engagedStat))
            {
                engagedStat.onValueChanged += UpdateEngagedStatus;
            }
        }

        public void CalculateCanAttackStatus()
        {
            if(!CurrentZone.IsLocalPlayerZone || !this.IsFollowerOrEvolvedFollower() || RuntimeCard.namedStats[SVEProperties.CardStats.Engaged].effectiveValue == 1)
            {
                CanAttack = false;
                CanAttackLeader = false;
                SetHighlightMode(HighlightMode.None);
                return;
            }

            CanAttack = RuntimeCard.IsCardType(SVEProperties.CardTypes.EvolvedFollower) || NumberOfTurnsOnBoard > 0 ||
                RuntimeCard.HasKeyword(SVEProperties.Keywords.Rush) || RuntimeCard.HasKeyword(SVEProperties.Keywords.Storm);
            CanAttackLeader = RuntimeCard.IsCardType(SVEProperties.CardTypes.EvolvedFollower) || NumberOfTurnsOnBoard > 0 || // TODO - check if evolved cards can attack leader immediately
                RuntimeCard.HasKeyword(SVEProperties.Keywords.Storm);
            SetHighlightMode(CanAttack ? HighlightMode.ValidTarget : HighlightMode.None);
        }

        #endregion

        // ------------------------------

        #region Hover Enter/Exit

        public void OnHoverEnter()
        {

        }

        public void OnHoverExit()
        {

        }

        #endregion

        // ------------------------------

        #region Runtime Card Controls

        private void InitRuntimeCard(RuntimeCard card)
        {
            LibraryCard = GameManager.Instance.config.GetCard(card.cardId);
            LibraryCardCache.CacheCard(LibraryCard);
            statsDisplay.SetCard(card);
        }

        private void ResetRuntimeCard(RuntimeCard card)
        {
            LibraryCard = null;
            if(card == null)
                return;
            if(isTrackingEngagedStatus)
                StopTrackingEngagedStatus();
            statsDisplay.Reset();
        }

        #endregion
    }
}
