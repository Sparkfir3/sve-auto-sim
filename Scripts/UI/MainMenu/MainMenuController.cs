using System.Collections;
using CCGKit;
using UnityEngine;
using Sirenix.OdinInspector;
using Steamworks;

namespace SVESimulator.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField]
        private MainMenuView mainMenuView;
        [SerializeField]
        private GameObject networkManagerKcp;
        [SerializeField]
        private DeckSelectionController deckSelectionController;
        [SerializeField]
        private GameObject selectDeckError;

        // ------------------------------

        private void Start()
        {
            GameManager.Instance.Initialize();
            deckSelectionController.Initialize();
            deckSelectionController.OnSelectDeck += () => selectDeckError.SetActive(false);
            mainMenuView.OnButtonClicked += HandleButtonClicked;
        }

        private void HandleButtonClicked(MainMenuButton button)
        {
            switch(button)
            {
                case MainMenuButton.PlayOnlineHost:
                    HostSteamLobby();
                    break;
                case MainMenuButton.PlayOnlineJoin:
                    JoinSteamLobby();
                    break;
                case MainMenuButton.PlayLocalHost:
                    StartLocalHost();
                    break;
                case MainMenuButton.PlayLocalJoin:
                    StartLocalClient();
                    break;
                case MainMenuButton.Quit:
                    QuitGame();
                    break;
            }
        }

        // ------------------------------

        #region Local Networking

        public void StartLocalHost()
        {
            if(!TryLoadSelectedDeck())
                return;
            LibraryCardCache.ClearCache();
            StopAllCoroutines();
            StartCoroutine(StartHostCoroutine());
            IEnumerator StartHostCoroutine()
            {
                if(SVEGameNetworkManager.Instance)
                {
                    Destroy(SVEGameNetworkManager.Instance.gameObject);
                    yield return null;
                }
                Instantiate(networkManagerKcp);
                SVEGameNetworkManager.Instance.StartHost();
            }
        }

        public void StartLocalClient()
        {
            if(!TryLoadSelectedDeck())
                return;
            LibraryCardCache.ClearCache();
            StopAllCoroutines();
            StartCoroutine(StartHostCoroutine());
            IEnumerator StartHostCoroutine()
            {
                if(SVEGameNetworkManager.Instance)
                {
                    Destroy(SVEGameNetworkManager.Instance.gameObject);
                    yield return null;
                }
                Instantiate(networkManagerKcp);
                SVEGameNetworkManager.Instance.StartClient();
            }
        }

        #endregion

        // ------------------------------

        #region Steam Networking

        public void HostSteamLobby()
        {
            if(!TryLoadSelectedDeck())
                return;
            LibraryCardCache.ClearCache();
            SVEGameNetworkManager.SteamLobby.HostLobby(mainMenuView.RoomCode);
        }

        public void JoinSteamLobby()
        {
            if(!TryLoadSelectedDeck())
                return;
            LibraryCardCache.ClearCache();
            SVEGameNetworkManager.SteamLobby.GetLobby(mainMenuView.RoomCode, lobbyID =>
            {
                SteamMatchmaking.JoinLobby(lobbyID);
            });
        }

        #endregion

        // ------------------------------

        private bool TryLoadSelectedDeck()
        {
            if(!deckSelectionController.HasSelectedDeck)
            {
                selectDeckError.SetActive(true);
                return false;
            }
            selectDeckError.SetActive(false);
            deckSelectionController.LoadCurrentDeck();
            return true;
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            Debug.Log("Attempted to quit application in editor mode");
#else
            Application.Quit();
#endif
        }
    }
}
