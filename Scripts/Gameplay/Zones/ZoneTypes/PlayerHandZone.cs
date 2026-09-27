using System.Collections.Generic;
using System.Linq;
using CCGKit;
using UnityEngine;
using Sirenix.OdinInspector;

namespace SVESimulator
{
    public class PlayerHandZone : CardZone
    {
        [TitleGroup("Runtime Data"), SerializeField]
        private List<CardObject> cardsWithActFromHand = new();

        [TitleGroup("Settings"), SerializeField]
        private float spacing;

        [TitleGroup("Object References"), SerializeField]
        private GameObject targetingSlot;

        public override Quaternion CardRotation => visible ? SVEProperties.CardFaceUpRotation : SVEProperties.CardFaceDownRotation;

        // ------------------------------

        public override void Initialize(RuntimeZone zone, PlayerCardZoneController controller)
        {
            base.Initialize(zone, controller);
            SetTargetSlotActive(false);
        }

        public override void AddCard(CardObject card)
        {
            if(card.LibraryCard.abilities.Any(x => x is ActivatedAbility ability && ability.zoneId == SVEProperties.ZoneIds.Hand && x.effect is SveEffect))
                cardsWithActFromHand.Add(card);
            base.AddCard(card);
        }

        public override void RemoveCard(CardObject card)
        {
            if(cardsWithActFromHand.Contains(card))
                cardsWithActFromHand.Remove(card);
            base.RemoveCard(card);
        }

        // ------------------------------

        public Vector3 GetCardPosition(int index)
        {
            return transform.position + (Vector3.right * (spacing * Mathf.Max(index, 0)));
        }

        public Vector3 GetCardPosition(CardObject card)
        {
            return !cards.Contains(card) ? transform.position : GetCardPosition(cards.IndexOf(card));
        }

        public Vector3 GetLastCardPosition()
        {
            return transform.position + (Vector3.right * (spacing * Mathf.Max(cards.Count - 1, 0)));
        }

        public bool HasActFromHand()
        {
            return cardsWithActFromHand.Count > 0;
        }

        public bool TryGetActFromHandAbilityData(out Dictionary<CardObject, List<ActivatedAbility>> abilityList)
        {
            abilityList = new();
            foreach(CardObject card in cardsWithActFromHand)
            {
                List<ActivatedAbility> abilities = card.LibraryCard.abilities.Where(x => x is ActivatedAbility ability && ability.zoneId == SVEProperties.ZoneIds.Hand && x.effect is SveEffect)
                    .Select(x => x as ActivatedAbility).ToList();
                if(abilities is not { Count: > 0 })
                {
                    Debug.LogError($"Card with instance ID {card.RuntimeCard.instanceId} in player's hand was marked as having ActFromHand, but a corresponding ability could not be found.");
                    continue;
                }
                abilityList.Add(card, abilities);
            }
            return abilityList.Count > 0;
        }

        // ------------------------------

        public void SetValidQuicksInteractable()
        {
            foreach(CardObject card in cards)
                card.Interactable = card.HasQuickKeyword() && Player.LocalEvents.CanPayPlayPointsCost(card.RuntimeCard.PlayPointCost(Player));
        }

        public void HighlightValidQuicks()
        {
            foreach(CardObject card in cards)
                card.SetHighlightMode(card.HasQuickKeyword() && Player.LocalEvents.CanPayPlayPointsCost(card.RuntimeCard.PlayPointCost(Player))
                    ? CardObject.HighlightMode.ValidTarget : CardObject.HighlightMode.None);
        }

        public void SetTargetSlotActive(bool active)
        {
            if(targetingSlot)
                targetingSlot.SetActive(active);
        }
    }
}
