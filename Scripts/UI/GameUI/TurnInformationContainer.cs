using System;
using UnityEngine;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine.UI;

namespace SVESimulator.UI
{
    public class TurnInformationContainer : MonoBehaviour
    {
        [Title("Turn Info Box"), SerializeField]
        private TextMeshProUGUI currentPhaseTextBox;
        [SerializeField]
        private TextMeshProUGUI currentTurnNumberTextBox;
        [SerializeField]
        private TextMeshProUGUI currentPlayerTextBox;
        [SerializeField]
        private GameObject textDivider;
        [SerializeField]
        private Button endTurnButton;

        private bool isLocalPlayerTurn;
        private int turnNumber;

        // ------------------------------

        public void Initialize(PlayerController player)
        {
            // Internal init
            endTurnButton.onClick.AddListener(() =>
            {
                endTurnButton.interactable = false;
                player.EnterEndPhase();
            });
            turnNumber = 0;

            // Player events
            player.OnStartLocalTurn += incrementTurnCount => { SetTurn(true, incrementTurnCount); };
            player.OnStartOpponentTurn += incrementTurnCount => { SetTurn(false, incrementTurnCount); };

            // Game state events
            SVEEffectPool.Instance.OnConfirmationTimingStartConstant += () => SetEndTurnButtonActive(false);
            SVEEffectPool.Instance.OnConfirmationTimingEndConstant += () => SetEndTurnButtonActive(!SVEQuickTimingController.Instance.IsActive);
        }

        // ------------------------------

        public void SetPhase(SVEProperties.GamePhase phase)
        {
            currentPhaseTextBox.text = phase == SVEProperties.GamePhase.Setup ? "Setting Up" : $"{phase} Phase";
        }

        private void SetTurn(bool isLocalTurn, bool increment)
        {
            if(increment)
                turnNumber++;
            if(turnNumber <= 0)
            {
                currentPlayerTextBox.text = "End Turn";
                endTurnButton.interactable = false;
                currentTurnNumberTextBox.text = "";
                textDivider.SetActive(false);
                isLocalPlayerTurn = false;
                return;
            }

            isLocalPlayerTurn = isLocalTurn;
            currentPlayerTextBox.text = isLocalPlayerTurn ? "End Turn" : "Opponent's Turn";
            endTurnButton.interactable = isLocalPlayerTurn;
            currentTurnNumberTextBox.text = $"Turn {turnNumber}";
            textDivider.SetActive(true);
        }

        private void SetEndTurnButtonActive(bool active)
        {
            endTurnButton.interactable = isLocalPlayerTurn && active;
        }
    }
}
