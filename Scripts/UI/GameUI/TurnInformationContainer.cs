using System;
using UnityEngine;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine.UI;

namespace SVESimulator.UI
{
    public class TurnInformationContainer : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI currentPhaseTextBox;
        [SerializeField]
        private TextMeshProUGUI currentTurnNumberTextBox;
        [SerializeField]
        private TextMeshProUGUI currentPlayerTextBox;
        [SerializeField]
        private GameObject textDivider;
        [SerializeField]
        private Button endTurnButton;

        public event Action OnPressEndTurn;
        private int turnNumber;

        // ------------------------------

        public void Awake()
        {
            endTurnButton.onClick.AddListener(() =>
            {
                endTurnButton.interactable = false;
                OnPressEndTurn?.Invoke();
            });
            turnNumber = 0;
        }

        public void SetTurn(bool isLocalPlayerTurn, bool increment)
        {
            if(increment)
                turnNumber++;
            if(turnNumber <= 0)
            {
                currentPlayerTextBox.text = "End Turn";
                endTurnButton.interactable = false;
                currentTurnNumberTextBox.text = "";
                textDivider.SetActive(false);
                return;
            }

            currentPlayerTextBox.text = isLocalPlayerTurn ? "End Turn" : "Opponent's Turn";
            endTurnButton.interactable = isLocalPlayerTurn;
            currentTurnNumberTextBox.text = $"Turn {turnNumber}";
            textDivider.SetActive(true);
        }

        public void SetPhase(SVEProperties.GamePhase phase)
        {
            currentPhaseTextBox.text = phase == SVEProperties.GamePhase.Setup ? "Setting Up" : $"{phase} Phase";
        }
    }
}
