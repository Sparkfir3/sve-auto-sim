using System;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using TMPro;

namespace SVESimulator
{
    public abstract class ZoneInfoDisplayBase : MonoBehaviour
    {
        [SerializeField]
        protected CardZone zone;
        [SerializeField]
        protected TextMeshProUGUI textCardCount;

        protected PlayerController player;

        // ------------------------------

        protected void Awake()
        {
            zone.OnInitialize += Initialize;
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if(!zone)
                zone = GetComponentInParent<CardZone>();
        }
#endif

        // ------------------------------

        protected virtual void Initialize()
        {
            player = zone.Player;
        }

        protected virtual void UpdateCardCount(int count)
        {
            textCardCount.text = count.ToString();
        }
    }
}
