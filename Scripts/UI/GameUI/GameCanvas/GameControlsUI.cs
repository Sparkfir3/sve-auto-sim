using System;
using Mirror;
using UnityEngine;
using Sirenix.OdinInspector;
using Steamworks;
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

        public event Action OnPressEndTurn;

        // ------------------------------

        private void Awake()
        {
            SetTurnDisplayActive(false);
            turnInfoContainer.OnPressEndTurn += () => OnPressEndTurn?.Invoke();
            quitGameButton.onClick.AddListener(ReturnToMainMenu);
        }

        public void SetTurnDisplayActive(bool active)
        {
            turnInfoContainer.gameObject.SetActive(active);
            debugButtonContainer.gameObject.SetActive(active);
        }

        public void SetTurn(bool isLocalPlayerTurn, bool increment = false) => turnInfoContainer.SetTurn(isLocalPlayerTurn, increment);
        public void SetPhase(SVEProperties.GamePhase phase) => turnInfoContainer.SetPhase(phase);

        private void ReturnToMainMenu()
        {
            NetworkManager.singleton.StopHost();
            SteamMatchmaking.LeaveLobby(new CSteamID(SteamLobby.CurrentLobbyID));
        }
    }
}
