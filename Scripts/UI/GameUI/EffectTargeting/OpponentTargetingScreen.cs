using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using Sparkfire.Utility;
using TMPro;

namespace SVESimulator.UI
{
    public class OpponentTargetingScreen : MonoBehaviour
    {
        [Title("Runtime Data & Settings"), SerializeField]
        private ButtonDisplayPosition currentDisplayPosition;
        [SerializeField]
        private SerializedDictionary<ButtonDisplayPosition, ButtonPositionData> displayPositions = new();

        [Title("Object References"), SerializeField]
        private RectTransform opponentTargetingPopup;
        [SerializeField]
        private TextMeshProUGUI opponentCardTextbox;

        // ------------------------------

        public void Initialize(ButtonDisplayPosition position = ButtonDisplayPosition.Center)
        {
            SetDisplayPosition(position);
        }

        public void OpenOpponentIsTargeting(string cardName, string effectText, ButtonDisplayPosition displayPosition = ButtonDisplayPosition.Center)
        {
            opponentCardTextbox.text = $"{(string.IsNullOrWhiteSpace(cardName) ? "" : $"<b>{cardName}</b>")}" +
                $"{(string.IsNullOrWhiteSpace(effectText) || string.IsNullOrWhiteSpace(cardName) ? "" : $" - ")}" +
                $"{effectText}";
            SetDisplayPosition(displayPosition);
            gameObject.SetActive(true);
            GameUIManager.QuickTiming.SetAlpha(0f); // TODO - better solution for overlapping popups
        }

        public void CloseOpponentIsTargeting()
        {
            gameObject.SetActive(false);
            GameUIManager.QuickTiming.SetAlpha(1f);
        }

        private void SetDisplayPosition(ButtonDisplayPosition displayPosition)
        {
            if(currentDisplayPosition == displayPosition)
                return;
            currentDisplayPosition = displayPosition;
            ButtonPositionData positionData = displayPositions[displayPosition];

            opponentTargetingPopup.anchorMin = positionData.textAnchor;
            opponentTargetingPopup.anchorMax = positionData.textAnchor;
            opponentTargetingPopup.pivot = positionData.textPivot;
            opponentTargetingPopup.anchoredPosition = new Vector3(0f, positionData.textPosition, 0f);
        }
    }
}
