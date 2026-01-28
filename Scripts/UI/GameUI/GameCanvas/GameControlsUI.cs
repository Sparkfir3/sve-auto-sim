using System;
using Mirror;
using UnityEngine;
using Sirenix.OdinInspector;
using Steamworks;
using TMPro;
using UnityEngine.UI;

namespace SVESimulator.UI
{
    public class GameControlsUI : MonoBehaviour
    {
        [SerializeField]
        private TurnInformationContainer turnInfoContainer;
        [SerializeField]
        private GameDebugControls debugButtonContainer;
        [SerializeField]
        private Button quitGameButton;

        [Title("Start Turn Animation"), SerializeField]
        private Animator startTurnAnimator;
        [SerializeField]
        private TextMeshProUGUI startTurnTextBox;
        [SerializeField]
        private string startTurnAnimTrigger = "Play";

        // ------------------------------

        private void Awake()
        {
            SetTurnDisplayActive(false);
            quitGameButton.onClick.AddListener(ReturnToMainMenu);
        }

        public void Initialize(PlayerController player)
        {
            turnInfoContainer.Initialize(player);
            SetTurnDisplayActive(true);

            // Player events
            player.OnStartLocalTurn += _ => { PlayStartTurnAnimation(true); };
            player.OnStartOpponentTurn += _ => { PlayStartTurnAnimation(false); };
        }

        // ------------------------------

        public void SetTurnDisplayActive(bool active)
        {
            turnInfoContainer.gameObject.SetActive(active);
            debugButtonContainer.gameObject.SetActive(active);
        }

        public void SetPhase(SVEProperties.GamePhase phase) => turnInfoContainer.SetPhase(phase);

        private void PlayStartTurnAnimation(bool isLocalPlayer)
        {
            startTurnTextBox.text = isLocalPlayer ? "Your Turn" : "Opponent's Turn";
            startTurnAnimator.SetTrigger(startTurnAnimTrigger);
        }

        private void ReturnToMainMenu()
        {
            NetworkManager.singleton.StopHost();
            SteamMatchmaking.LeaveLobby(new CSteamID(SteamLobby.CurrentLobbyID));
        }
    }
}
