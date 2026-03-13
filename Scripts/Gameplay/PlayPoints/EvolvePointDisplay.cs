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
                Initialize(true, count);
                return;
            }

            for(; currentEvolvePoints != count; currentEvolvePoints += (currentEvolvePoints > count ? -1 : 1))
            {
                if(currentEvolvePoints - 1 > evolvePointCards.Length)
                {
                    Debug.LogError($"Attempted to set evolve point display to {count}, which is higher than the supported displayed amount ${evolvePointCards.Length}");
                    continue;
                }
                if(currentEvolvePoints - 1 < count)
                {
                    evolvePointCards[currentEvolvePoints - 1].gameObject.SetActive(true);
                    continue;
                }
                FlipEvolvePoint(evolvePointCards[currentEvolvePoints - 1]);
            }
        }

        // ------------------------------

        private void FlipEvolvePoint(CardObject card)
        {
            card.transform.rotation *= Quaternion.Euler(0f, 0f, 180f);
        }
    }
}
