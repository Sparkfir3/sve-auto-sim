using System;
using System.Collections.Generic;
using System.Linq;
using CCGKit;
using UnityEngine;
using Sirenix.OdinInspector;
using Sparkfire.Utility;
using UnityEngine.Serialization;

namespace SVESimulator
{
    public class CardPositionedZone : CardZone
    {
        [Serializable]
        protected class CardSlot
        {
            [LabelWidth(100)]
            public Transform transform;
            [ReadOnly, LabelWidth(100)]
            public TargetableSlot target;
            [ReadOnly, LabelWidth(100)]
            public CardObject card;

            public Vector3 position => transform.position;
            public Vector3 localPosition => transform.localPosition;
        }

        // ---

        #region Variables

        [TitleGroup("Runtime Data"), SerializeField]
        protected SerializedDictionary<int, CardSlot> cardSlots = new();

        [TitleGroup("Settings"), SerializeField]
        [FormerlySerializedAs("defaultInteractionType")]
        public TargetableSlot.InteractionType endInteractionType = TargetableSlot.InteractionType.None;
        [TitleGroup("Settings"), SerializeField]
        protected bool trackEngagedStatus;

        [TitleGroup("Debug"), SerializeField]
        protected bool drawGizmos;
        [HorizontalGroup("Gizmo"), SerializeField, LabelWidth(100)]
        protected Color32 gizmoColor = Color.red;
        [HorizontalGroup("Gizmo"), SerializeField, LabelWidth(100)]
        protected float gizmoRadius = 0.25f;

        #endregion

        // ------------------------------

        #region Unity Messages

        private void Start()
        {
            foreach(CardSlot slot in cardSlots.Values)
            {
                slot.target.CurrentInteractionType = endInteractionType;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            foreach(CardSlot slot in cardSlots.Values)
            {
                slot.target = slot.transform ? slot.transform.GetComponent<TargetableSlot>() : null;
            }
        }
#endif

        #endregion

        // ------------------------------

        #region Add/Remove/Movement Controls

        public override void AddCard(CardObject card)
        {
            base.AddCard(card);
            if(trackEngagedStatus)
                card.StartTrackingEngagedStatus();
        }

        public override void RemoveCard(CardObject card)
        {
            base.RemoveCard(card);
            if(trackEngagedStatus)
                card.StopTrackingEngagedStatus();
            IEnumerable<CardSlot> oldSlots = cardSlots.Where(kvPair => kvPair.Value.card == card).Select(x => x.Value);
            foreach(CardSlot oldSlot in oldSlots)
            {
                oldSlot.card = null;
                oldSlot.target.CurrentInteractionType = endInteractionType;
            }
        }

        public virtual void MoveCardToSlot(CardObject card, int slot, TargetableSlot.InteractionType newInteractionType)
        {
            #region Break Conditions
            if(!cards.Contains(card))
            {
                Debug.LogError($"Attempted to move card into slot {slot} when card is not in this zone ({gameObject.name})!");
                return;
            }
            if(!IsSlotNumberValid(slot))
            {
                Debug.LogError($"Attempted to move a card into invalid slot number {slot}");
                return;
            }
            #endregion

            IEnumerable<CardSlot> oldSlots = cardSlots.Where(kvPair => kvPair.Value.card == card).Select(x => x.Value);
            foreach(CardSlot oldSlot in oldSlots)
            {
                oldSlot.card = null;
            }
            cardSlots[slot].card = card;
            cardSlots[slot].target.CurrentInteractionType = newInteractionType;
        }

        public virtual bool SwapCardsInSlots(CardObject cardA, CardObject cardB)
        {
            if(!cardA || !cardB || !cards.Contains(cardA) || !cards.Contains(cardB))
                return false;

            int slotA = GetSlotNumber(cardA);
            int slotB = GetSlotNumber(cardB);
            if(slotA == slotB)
                return false;
            cardSlots[slotA].card = cardB;
            cardSlots[slotB].card = cardA;
            return true;
        }

        #endregion

        // ------------------------------

        #region Data Controls

        public void CalculateValidAttackTargets(CardObject attacker, out bool wardTargetExists, bool setHighlight = true)
        {
            ClearValidAttackTargets();
            if(!attacker || !attacker.CanAttack)
            {
                wardTargetExists = false;
                return;
            }

            wardTargetExists = !attacker.RuntimeCard.HasKeyword(SVEProperties.PassiveAbilities.IgnoreWard)
                && cardSlots.Values.Any(x => x.card && x.card.Engaged
                    && x.card.RuntimeCard.HasKeyword(SVEProperties.Keywords.Ward)
                    && !x.card.RuntimeCard.HasKeyword(SVEProperties.Keywords.Intimidate));
            foreach(CardSlot slot in cardSlots.Values)
            {
                if(!slot.card)
                    continue;

                if(slot.card.IsFollowerOrEvolvedFollower())
                {
                    if(wardTargetExists)
                        slot.card.IsValidDefender = slot.card.RuntimeCard.HasKeyword(SVEProperties.Keywords.Ward) && slot.card.Engaged;
                    else
                        slot.card.IsValidDefender = (attacker.RuntimeCard.HasKeyword(SVEProperties.Keywords.Assail) || slot.card.Engaged)
                            && !slot.card.RuntimeCard.HasKeyword(SVEProperties.Keywords.Intimidate);

                    if(slot.card.IsValidDefender)
                        slot.card.SetHighlightMode(CardObject.HighlightMode.ValidTarget);
                }
            }
        }

        public void ClearValidAttackTargets()
        {
            foreach(CardObject card in cards)
            {
                if(card.IsValidDefender)
                {
                    card.IsValidDefender = false;
                    card.SetHighlightMode(CardObject.HighlightMode.None);
                }
            }
        }

        public override void SetAllCardsInteractable(bool interactable)
        {
            foreach(CardObject card in GetAllPrimaryCards())
                card.Interactable = interactable;
        }

        public override void SetAllCardsInteractable(string filter)
        {
            var filterDict = SVEFormulaParser.ParseCardFilterFormula(filter);
            foreach(CardObject card in GetAllPrimaryCards())
                card.Interactable = filterDict.MatchesCard(card);
        }

        #endregion

        // ------------------------------

        #region Get Info

        public bool IsSlotNumberValid(int slotNumber) => cardSlots.ContainsKey(slotNumber);

        public int GetSlotNumber(TargetableSlot slot)
        {
            KeyValuePair<int, CardSlot>[] kvPair = cardSlots.Where(kvPair => kvPair.Value.target == slot).ToArray();
            if(kvPair.Length == 0)
                return -1;
            return kvPair[0].Key;
        }

        public int GetSlotNumber(CardObject card)
        {
            if(!card) return -1;
            KeyValuePair<int, CardSlot>[] kvPair = cardSlots.Where(kvPair => kvPair.Value.card == card).ToArray();
            if(kvPair.Length == 0)
                return -1;
            return kvPair[0].Key;
        }

        public Vector3 GetSlotPosition(TargetableSlot slot) => GetSlotPosition(slot.SlotNumber);
        public Vector3 GetSlotPosition(int slotNumber) => cardSlots[slotNumber].position;
        public Vector3 GetSlotLocalPosition(int slotNumber) => cardSlots[slotNumber].localPosition;

        public CardObject GetCard(int slotNumber) => (slotNumber >= 0 && slotNumber < cardSlots.Count) ? cardSlots[slotNumber].card : null;
        public List<CardObject> GetAllPrimaryCards() => cardSlots.Where(x => x.Value.card != null).Select(x => x.Value.card).ToList();

        public bool HasOpenSlot() => cardSlots.Any(x => !x.Value.card && x.Value.target.isActiveAndEnabled);
        public int FilledSlotCount() => cardSlots.Count(x => x.Value.card && x.Value.target.isActiveAndEnabled);
        public int OpenSlotCount() => cardSlots.Count(x => !x.Value.card && x.Value.target.isActiveAndEnabled);
        public int GetFirstOpenSlotId() => !HasOpenSlot() ? -1 : cardSlots.First(x => !x.Value.card && x.Value.target.isActiveAndEnabled).Key;

        public override int CountOfCardType(string cardType) => GetAllPrimaryCards().Count(x => x.IsCardType(cardType));

        #endregion

        // ------------------------------

        #region Graphics

        public void HighlightCardsCanAttack()
        {
            if(!Player.isActivePlayer)
                return;
            foreach(CardObject card in cards)
                card.SetHighlightMode(card.CanAttack ? CardObject.HighlightMode.ValidTarget : CardObject.HighlightMode.None);
        }

        #endregion

        // ------------------------------

        #region Debug

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if(!drawGizmos)
                return;

            Gizmos.color = gizmoColor;
            foreach(CardSlot slot in cardSlots.Values)
            {
                if(!slot.transform)
                    continue;
                Gizmos.DrawSphere(slot.position, gizmoRadius);
            }
        }
#endif

        [TitleGroup("Buttons"), Button, HideInPlayMode]
        private void InitializeDictionary(int amount = 5)
        {
            for(int i = 0; i < amount; i++)
            {
                cardSlots.TryAdd(i, new CardSlot());
            }
        }

        #endregion
    }
}
