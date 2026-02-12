using System;
using System.Collections.Generic;
using System.Linq;
using CCGKit;
using UnityEngine;
using Sirenix.OdinInspector;

namespace SVESimulator
{
    public abstract class CardZone : MonoBehaviour
    {
        public enum ZoneInteractionType { None = -1, MoveCard = 0, TargetEffect = 1 }

        // ---

        [TitleGroup("Runtime Data"), SerializeField, ReadOnly]
        protected List<CardObject> cards;
        [FoldoutGroup("Runtime Data/Runtime Zone Info", true), LabelText("Card Count"), ShowIf("@hasRuntimeZone"), ShowInInspector]
        private int runtimeCardCount => hasRuntimeZone ? runtimeZone.cards.Count : -1;
        [FoldoutGroup("Runtime Data/Runtime Zone Info", true), LabelText("Card IDs"), ShowIf("@hasRuntimeZone"), ShowInInspector]
        private List<int> runtimeCardIds => hasRuntimeZone ? runtimeZone.cards.Select(x => x.instanceId).ToList() : null;

        [field: TitleGroup("Settings"), SerializeField]
        public bool IsLocalPlayerZone { get; protected set; }
        [TitleGroup("Settings"), SerializeField]
        protected bool visible;
        [field: TitleGroup("Settings"), SerializeField]
        public bool Interactable { get; protected set; }
        [field: TitleGroup("Settings"), SerializeField, LabelText("Begin Interaction Type")]
        public ZoneInteractionType InteractionType { get; protected set; } = ZoneInteractionType.None;

        protected bool hasRuntimeZone => runtimeZone != null;
        protected RuntimeZone runtimeZone;
        public RuntimeZone Runtime => runtimeZone;
        public List<CardObject> AllCards => cards;

        public PlayerCardZoneController ZoneController { get; protected set; }
        public PlayerController Player => ZoneController.Player;

        public event Action OnInitialize;

        // ------------------------------

        public virtual Quaternion CardRotation => IsLocalPlayerZone ? SVEProperties.CardFaceUpRotation
            : SVEProperties.CardFaceDownRotation * SVEProperties.OpponentCardRotation;

        // ------------------------------

        public virtual void Initialize(RuntimeZone zone, PlayerCardZoneController controller)
        {
            runtimeZone = zone;
            ZoneController = controller;
            OnInitialize?.Invoke();
        }

        public virtual void AddCard(CardObject card)
        {
            cards.Add(card);
            card.transform.parent = transform;
            card.IsVisible = visible;
        }

        public virtual void RemoveCard(CardObject card)
        {
            cards.Remove(card);
        }

        public bool TryGetCard(int instanceId, out CardObject card)
        {
            card = cards.FirstOrDefault(x => x.RuntimeCard.instanceId == instanceId);
            return card != null;
        }

        public bool ContainsCard(CardObject card) => cards.Contains(card);
        public bool ContainsCard(RuntimeCard card) => cards.Any(x => x.RuntimeCard.instanceId == card.instanceId);

        public virtual int CountOfCardType(string cardType) => cards.Count(x => x.IsCardType(cardType));
        public virtual int CountOfCardByFilter(string filterFormula)
        {
            var filter = SVEFormulaParser.ParseCardFilterFormula(filterFormula);
            return cards.Count(x => filter.MatchesCard(x));
        }

        // ------------------------------

        public virtual void SetAllCardsInteractable(bool interactable)
        {
            foreach(CardObject card in cards)
                card.Interactable = interactable;
        }

        public virtual void SetAllCardsInteractable(string filter)
        {
            var filterDict = SVEFormulaParser.ParseCardFilterFormula(filter);
            foreach(CardObject card in cards)
                card.Interactable = filterDict.MatchesCard(card);
        }

        public void SetAllCardHighlights(CardObject.HighlightMode mode)
        {
            foreach(CardObject card in cards)
                card.SetHighlightMode(mode);
        }

        public void RemoveAllCardHighlights() => SetAllCardHighlights(CardObject.HighlightMode.None);
    }
}
