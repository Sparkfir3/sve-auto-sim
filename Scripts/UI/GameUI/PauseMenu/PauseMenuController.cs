using UnityEngine;
using Sirenix.OdinInspector;
using Mirror;
using Steamworks;
using UnityEngine.UI;

namespace SVESimulator
{
    public class PauseMenuController : MonoBehaviour
    {
        [SerializeField]
        private Button quitGameButton;

        // ------------------------------

        private void Awake()
        {
            quitGameButton.onClick.AddListener(ReturnToMainMenu);
        }

        private void ReturnToMainMenu()
        {
            NetworkManager.singleton.StopHost();
            SteamMatchmaking.LeaveLobby(new CSteamID(SteamLobby.CurrentLobbyID));
        }
    }
}
