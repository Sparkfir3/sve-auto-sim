using System.Collections;
using CCGKit;
using Mirror;
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
        private GameObject networkManagerSteam;
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
            SVEGameNetworkManager.OnPlayerConnected += HandlePlayerConnectedToServer;
            SVEGameNetworkManager.OnPlayerDisconnected += HandlePlayerDisconnectedFromServer;
            SVEGameNetworkManager.OnLocalDisconnect += HandleLocalPlayerDisconnected;
        }

        private void OnDestroy()
        {
            SVEGameNetworkManager.OnPlayerConnected -= HandlePlayerConnectedToServer;
            SVEGameNetworkManager.OnPlayerDisconnected -= HandlePlayerDisconnectedFromServer;
            SVEGameNetworkManager.OnLocalDisconnect -= HandleLocalPlayerDisconnected;
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
                case MainMenuButton.BackToMain:
                    if(SVEGameNetworkManager.Instance.isNetworkActive)
                        SVEGameNetworkManager.Instance.StopHost();
                    break;
                case MainMenuButton.StartGame:
                    SVEGameNetworkManager.SceneManager.LoadGameplay();
                    break;
                case MainMenuButton.Quit:
                    QuitGame();
                    break;
            }
        }

        // ------------------------------

        #region General Networking

        private void HandlePlayerConnectedToServer(NetworkConnectionToClient conn)
        {
            if(SVEGameNetworkManager.ConnectedPlayerCount >= 2 && mainMenuView.CurrentState == MainMenuViewState.ConnectingLocal)
                mainMenuView.PerformAction(MainMenuAction.ReadyLocal);
        }

        private void HandlePlayerDisconnectedFromServer(NetworkConnectionToClient conn)
        {
            if(mainMenuView.CurrentState == MainMenuViewState.ReadyLocal && conn.connectionId != 0) // other user disconnect
                mainMenuView.PerformAction(MainMenuAction.Back);
        }

        private void HandleLocalPlayerDisconnected()
        {
            if(mainMenuView.CurrentState is MainMenuViewState.ConnectingLocal or MainMenuViewState.ReadyLocal)
                mainMenuView.PerformAction(MainMenuAction.Back);
        }

        #endregion

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
                if(SVEGameNetworkManager.IsSteam)
                {
                    Destroy(SVEGameNetworkManager.Instance.gameObject);
                    yield return null;
                    Instantiate(networkManagerKcp);
                    yield return null;
                }
                SVEGameNetworkManager.Instance.StartHost();
            }
        }

        public void StartLocalClient()
        {
            if(!TryLoadSelectedDeck())
                return;
            LibraryCardCache.ClearCache();
            StopAllCoroutines();
            StartCoroutine(StartClientCoroutine());
            IEnumerator StartClientCoroutine()
            {
                if(SVEGameNetworkManager.IsSteam)
                {
                    Destroy(SVEGameNetworkManager.Instance.gameObject);
                    yield return null;
                    Instantiate(networkManagerKcp);
                    yield return null;
                }
                SVEGameNetworkManager.Instance.StartClient();
            }
        }

        #endregion

        // ------------------------------

        #region Steam Networking

        public void HostSteamLobby()
        {
            if((SVEGameNetworkManager.IsSteam && !SVEGameNetworkManager.SteamLobby.IsSteamConnected) || !TryLoadSelectedDeck())
                return;
            LibraryCardCache.ClearCache();
            StartCoroutine(StartHostCoroutine());
            IEnumerator StartHostCoroutine()
            {
                if(!SVEGameNetworkManager.IsSteam)
                {
                    Destroy(SVEGameNetworkManager.Instance.gameObject);
                    yield return null;
                    Instantiate(networkManagerSteam);
                    yield return null;
                }
                SVEGameNetworkManager.SteamLobby.HostLobby(mainMenuView.RoomCode);
            }
        }

        public void JoinSteamLobby()
        {
            if((SVEGameNetworkManager.IsSteam && !SVEGameNetworkManager.SteamLobby.IsSteamConnected) || !TryLoadSelectedDeck())
                return;
            LibraryCardCache.ClearCache();
            StartCoroutine(StartClientCoroutine());
            IEnumerator StartClientCoroutine()
            {
                if(!SVEGameNetworkManager.IsSteam)
                {
                    Destroy(SVEGameNetworkManager.Instance.gameObject);
                    yield return null;
                    Instantiate(networkManagerSteam);
                    yield return null;
                }
                SVEGameNetworkManager.SteamLobby.GetLobby(mainMenuView.RoomCode, lobbyID =>
                {
                    SteamMatchmaking.JoinLobby(lobbyID);
                });
            }

        }

        #endregion

        // ------------------------------

        #region Other

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

        #endregion
    }
}
