using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;

namespace SVESimulator
{
    public class EvolvePointDisplay : MonoBehaviour
    {
        [field: Title("Runtime Data"), SerializeField, ReadOnly]
        public bool IsActive { get; private set; }
        [SerializeField, Range(0, 5), ReadOnly]
        private int currentEvolvePoints;

        [Title("Object References"), SerializeField]
        private CardObject[] evolvePointCards;
        [SerializeField]
        private CardObject superEvolvePointCard;
        [SerializeField]
        private Transform superEvolveBasePosition;
        [SerializeField]
        private Transform superEvolveSidePosition;

        private int currentMaxEvolvePointCards = 3;
        private bool superEvolveActive;

        // ------------------------------

        [Button, HideInEditorMode]
        public void Initialize(bool active, int count = 3, bool superEvolve = true)
        {
            IsActive = active;
            currentEvolvePoints = count;
            for(int i = 0; i < evolvePointCards.Length; i++)
            {
                evolvePointCards[i].gameObject.SetActive(active && i < count);
            }
            currentMaxEvolvePointCards = count;
            superEvolveActive = superEvolve;
            superEvolvePointCard.gameObject.SetActive(superEvolve);
            superEvolvePointCard.transform.position = count > 0 ? superEvolveSidePosition.position : superEvolveBasePosition.position;
        }

        [Button, HideInEditorMode]
        public void SetEvolvePointCount(int count, bool superEvolve = true)
        {
            if(!IsActive)
            {
                Initialize(true, count, superEvolve);
                return;
            }

            count = Mathf.Clamp(count, 0, evolvePointCards.Length);
            if(currentEvolvePoints < count) // Add evolve points
            {
                if(count > currentMaxEvolvePointCards) // Add new card
                {
                    for(int i = currentEvolvePoints; i < count; i++)
                        evolvePointCards[i].gameObject.SetActive(true);
                    currentMaxEvolvePointCards = count;
                }
                else // Flip old card
                {
                    for(int i = currentMaxEvolvePointCards - currentEvolvePoints - 1; i >= currentMaxEvolvePointCards - count; i--)
                        FlipEvolvePoint(evolvePointCards[i]);
                }
            }
            else // Remove evolve points
            {
                int currentFlippedCards = currentMaxEvolvePointCards - currentEvolvePoints;
                int targetFlippedCards = currentMaxEvolvePointCards - count;
                for(int i = currentFlippedCards; i < targetFlippedCards; i++)
                    FlipEvolvePoint(evolvePointCards[i]);
            }
            currentEvolvePoints = count;
        }

        public void SetSuperEvolvePointActive(bool active)
        {
            if(active == superEvolveActive)
                return;
            superEvolveActive = active;
            FlipEvolvePoint(superEvolvePointCard);
        }

        // ------------------------------

        private void FlipEvolvePoint(CardObject card)
        {
            card.transform.rotation *= Quaternion.Euler(0f, 0f, 180f);
        }
    }
}
