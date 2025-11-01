using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;

namespace SVESimulator
{
    public class EvolvePointDisplay : MonoBehaviour
    {
        [field: Title("Runtime Data"), SerializeField, ReadOnly]
        public bool IsActive { get; private set; }
        [SerializeField, Range(0, 3), ReadOnly]
        private int currentEvolvePoints;

        [Title("Object References"), SerializeField]
        private CardObject[] evolvePointCards;

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
        }

        [Button, HideInEditorMode]
        public void SetEvolvePointCount(int count)
        {
            if(!IsActive)
            {
                Debug.LogError("Attempted to update evolve point count on disabled evolve point display!");
                return;
            }

            int amountToFlip = currentEvolvePoints - count;
            for(int i = 1; i <= amountToFlip; i++)
            {
                FlipEvolvePoint(evolvePointCards[currentEvolvePoints - i]);
            }
            currentEvolvePoints -= amountToFlip;
        }

        // ------------------------------

        private void FlipEvolvePoint(CardObject card)
        {
            card.transform.rotation *= Quaternion.Euler(0f, 0f, 180f);
        }
    }
}
