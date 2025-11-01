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

        [Title("Opponent Targeting"), SerializeField]
        private GameObject opponentTargetingPopup;
        [SerializeField]
        private TextMeshProUGUI opponentCardTextbox;

        public static EffectTargetZoneScreen TargetZone => GameUIManager.EffectTargeting.targetZoneScreen;
        public static EffectTargetCardScreen TargetCard => GameUIManager.EffectTargeting.targetCardScreen;

        // ------------------------------

        public void Initialize()
        {
            targetZoneScreen.Initialize();
            targetZoneScreen.Close();
            targetCardScreen.Initialize();
            targetCardScreen.Close();
        }

        public void OpenOpponentIsTargeting(string cardName, string effectText)
        {
            opponentTargetingPopup.SetActive(true);
            opponentCardTextbox.text = $"{(string.IsNullOrWhiteSpace(cardName) ? "" : $"<b>{cardName}</b>")}" +
                $"{(string.IsNullOrWhiteSpace(effectText) || string.IsNullOrWhiteSpace(cardName) ? "" : $" - ")}" +
                $"{effectText}";
            GameUIManager.QuickTiming.SetAlpha(0f); // TODO - better solution for overlapping popups
        }

        public void CloseOpponentIsTargeting()
        {
            opponentTargetingPopup.SetActive(false);
            GameUIManager.QuickTiming.SetAlpha(1f);
        }
    }
}
