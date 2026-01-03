using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using TMPro;

namespace SVESimulator
{
    public class ZoneInfoDisplay : MonoBehaviour
    {
        [SerializeField]
        private CardZone zone;
        [SerializeField]
        private TextMeshProUGUI textCardCount;

        // ------------------------------

        protected void Awake()
        {
            zone.OnInitialize += Initialize;
        }

        protected virtual void Initialize()
        {
            zone.Runtime.onZoneChanged += UpdateCardCount;
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if(!zone)
                zone = GetComponentInParent<CardZone>();
        }
#endif

        // ------------------------------

        protected virtual void UpdateCardCount(int count)
        {
            textCardCount.text = zone.Runtime.numCards.ToString();
        }
    }
}
