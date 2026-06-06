using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using Sparkfire.Utility;
using TMPro;

namespace SVESimulator.UI
{
    public class EffectTargetingUI : MonoBehaviour
    {
        [SerializeField]
        private EffectTargetZoneScreen targetZoneScreen;
        [SerializeField]
        private EffectTargetCardScreen targetCardScreen;
        [SerializeField]
        private OpponentTargetingScreen opponentTargetingScreen;

        public static EffectTargetZoneScreen TargetZone => GameUIManager.EffectTargeting.targetZoneScreen;
        public static EffectTargetCardScreen TargetCard => GameUIManager.EffectTargeting.targetCardScreen;
        public static OpponentTargetingScreen OpponentTargeting => GameUIManager.EffectTargeting.opponentTargetingScreen;

        // ------------------------------

        public void Initialize()
        {
            targetZoneScreen.Initialize();
            targetZoneScreen.Close();
            targetCardScreen.Initialize();
            targetCardScreen.Close();
            opponentTargetingScreen.Initialize();
            opponentTargetingScreen.gameObject.SetActive(false);
        }
    }
}
