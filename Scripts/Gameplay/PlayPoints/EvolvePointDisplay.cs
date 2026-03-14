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

        private int currentMaxEvolvePointCards = 3;

        // ------------------------------

        [Button, HideInEditorMode]
        public void Initialize(bool active, int count = 3)
        {
            IsActive = active;
            currentEvolvePoints = count;
            for(int i = 0; i < evolvePointCards.Length; i++)
            {
                evolvePointCards[i].gameObject.SetActive(active && i < count);
            }
            currentMaxEvolvePointCards = count;
        }

        [Button, HideInEditorMode]
        public void SetEvolvePointCount(int count)
        {
            if(!IsActive)
            {
                Initialize(true, count);
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

        // ------------------------------

        private void FlipEvolvePoint(CardObject card)
        {
            card.transform.rotation *= Quaternion.Euler(0f, 0f, 180f);
        }
    }
}
