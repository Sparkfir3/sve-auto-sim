using System;
using CCGKit;
using Mirror;
using UnityEngine;
using Sirenix.OdinInspector;
using Steamworks;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SVESimulator.UI
{
    public class GameControlsUI : MonoBehaviour
    {
        [SerializeField]
        private GameObject turnInfoContainer;
        [SerializeField]
        private TextMeshProUGUI currentTurnTextBox;
        [SerializeField]
        private Button endTurnButton;
        [SerializeField]
        private Button quitGameButton;

        public event Action OnPressEndTurn;

        // ------------------------------

        private void Awake()
        {
            SetTurnDisplayActive(false);
            endTurnButton.onClick.AddListener(() =>
            {
                endTurnButton.interactable = false;
                OnPressEndTurn?.Invoke();
            });
            quitGameButton.onClick.AddListener(ReturnToMainMenu);
        }

        public void SetTurnDisplayActive(bool active)
        {
            turnInfoContainer.SetActive(active);
        }

        public void SetTurn(bool isLocalPlayerTurn)
        {
            currentTurnTextBox.text = isLocalPlayerTurn ? "Your Turn" : "Opponent's Turn";
            endTurnButton.interactable = isLocalPlayerTurn;
        }

        private void ReturnToMainMenu()
        {
            NetworkManager.singleton.StopHost();
            SteamMatchmaking.LeaveLobby(new CSteamID(SteamLobby.CurrentLobbyID));
        }
    }
}
