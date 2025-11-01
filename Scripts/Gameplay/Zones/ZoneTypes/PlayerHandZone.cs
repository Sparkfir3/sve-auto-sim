using UnityEngine;
using Sirenix.OdinInspector;

namespace SVESimulator
{
    public class PlayerHandZone : CardZone
    {
        [TitleGroup("Settings"), SerializeField]
        private float spacing;

        public override Quaternion CardRotation => visible ? SVEProperties.CardFaceUpRotation : SVEProperties.CardFaceDownRotation;

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

        // ------------------------------

        public void SetValidQuicksInteractable()
        {
            foreach(CardObject card in cards)
                card.Interactable = card.HasQuickKeyword() && Player.LocalEvents.CanPayPlayPointsCost(card.RuntimeCard.PlayPointCost());
        }

        public void HighlightValidQuicks()
        {
            foreach(CardObject card in cards)
                card.SetHighlightMode(card.HasQuickKeyword() && Player.LocalEvents.CanPayPlayPointsCost(card.RuntimeCard.PlayPointCost())
                    ? CardObject.HighlightMode.ValidTarget : CardObject.HighlightMode.None);
        }
    }
}
