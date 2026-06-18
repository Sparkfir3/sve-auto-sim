using System;
using System.Collections;
using System.Net.Sockets;
using CCGKit;
using Mirror;
using UnityEngine;
using Sirenix.OdinInspector;
using Steamworks;

namespace SVESimulator.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [field: SerializeField]
        public bool IsConnecting { get; private set; }
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

        private Action onNextConnectionToServer;

        // ------------------------------

        private void Start()
        {
            GameManager.Instance.Initialize();
            deckSelectionController.Initialize();
            deckSelectionController.OnSelectDeck += () => selectDeckError.SetActive(false);
            mainMenuView.OnButtonClicked += HandleButtonClicked;
            SVEGameNetworkManager.OnPlayerConnected += HandlePlayerConnectedToServer;
            SVEGameNetworkManager.OnPlayerDisconnected += HandlePlayerDisconnectedFromServer;
            SVEGameNetworkManager.OnLocalConnect += HandleLocalPlayerConnected;
            SVEGameNetworkManager.OnLocalDisconnect += HandleLocalPlayerDisconnected;
        }

        private void OnDestroy()
        {
            SVEGameNetworkManager.OnPlayerConnected -= HandlePlayerConnectedToServer;
            SVEGameNetworkManager.OnPlayerDisconnected -= HandlePlayerDisconnectedFromServer;
            SVEGameNetworkManager.OnLocalConnect -= HandleLocalPlayerConnected;
            SVEGameNetworkManager.OnLocalDisconnect -= HandleLocalPlayerDisconnected;
        }

        private void HandleButtonClicked(MainMenuButton button)
        {
            switch(button)
            {
                // Connecting
                case MainMenuButton.PlayOnlineHost:
                    if(IsConnecting)
                        return;
                    onNextConnectionToServer = () => mainMenuView.PerformAction(MainMenuAction.Connecting);
                    HostSteamLobby();
                    break;
                case MainMenuButton.PlayOnlineJoin:
                    if(IsConnecting)
                        return;
                    // TODO - loading icon
                    onNextConnectionToServer = () => mainMenuView.PerformAction(MainMenuAction.Connecting);
                    JoinSteamLobby();
                    break;
                case MainMenuButton.PlayLocalHost:
                    if(IsConnecting)
                        return;
                    onNextConnectionToServer = null;
                    StartLocalHost(onStartSuccess: () => mainMenuView.PerformAction(MainMenuAction.Connecting));
                    break;
                case MainMenuButton.PlayLocalJoin:
                    if(IsConnecting)
                        return;
                    // TODO - loading icon
                    onNextConnectionToServer = () => mainMenuView.PerformAction(MainMenuAction.Connecting);
                    StartLocalClient();
                    break;

                // Other
                case MainMenuButton.BackToMain:
                    if(SVEGameNetworkManager.Instance.isNetworkActive)
                        SVEGameNetworkManager.Instance.StopHost();
                    onNextConnectionToServer = null;
                    IsConnecting = false;
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

        #region Networking Events

        private void HandlePlayerConnectedToServer(NetworkConnectionToClient conn)
        {
            if(SVEGameNetworkManager.ConnectedPlayerCount >= 2 && mainMenuView.CurrentState == MainMenuViewState.Connecting)
                mainMenuView.PerformAction(MainMenuAction.ReadyToStart);
        }

        private void HandlePlayerDisconnectedFromServer(NetworkConnectionToClient conn)
        {
            if(NetworkClient.active && mainMenuView.CurrentState == MainMenuViewState.ReadyToStart && conn.connectionId != 0) // other user disconnect
                mainMenuView.PerformAction(MainMenuAction.OppDisconnected);
        }

        private void HandleLocalPlayerConnected()
        {
            IsConnecting = false;
            onNextConnectionToServer?.Invoke();
            onNextConnectionToServer = null;
        }

        private void HandleLocalPlayerDisconnected()
        {
            IsConnecting = false;
            if(mainMenuView.CurrentState is MainMenuViewState.Connecting or MainMenuViewState.ReadyToStart)
                mainMenuView.PerformAction(MainMenuAction.Back);
        }

        #endregion

        // ------------------------------

        #region Local Networking

        public void StartLocalHost(Action onStartSuccess = null, Action onStartFail = null)
        {
            if(!TryLoadSelectedDeck())
                return;
            LibraryCardCache.ClearCache();
            IsConnecting = false;
            InitKcpNetworkManager(() =>
            {
                try
                {
                    SVEGameNetworkManager.Instance.StartHost();
                }
                catch(SocketException e)
                {
                    Debug.Log($"Attempted to start new a LAN connection instance when one is already active.\n{e.ToString()}");
                    onStartFail?.Invoke();
                    return;
                }
                onStartSuccess?.Invoke();
            });
        }

        public void StartLocalClient()
        {
            if(!TryLoadSelectedDeck())
                return;
            LibraryCardCache.ClearCache();
            IsConnecting = false;
            InitKcpNetworkManager(() =>
            {
                SVEGameNetworkManager.Instance.StartClient();
            });
        }

        private void InitKcpNetworkManager(Action onComplete)
        {
            if(!SVEGameNetworkManager.IsSteam)
            {
                onComplete?.Invoke();
                return;
            }
            StopAllCoroutines();
            StartCoroutine(ClientCoroutine());
            IEnumerator ClientCoroutine()
            {
                Destroy(SVEGameNetworkManager.Instance.gameObject);
                yield return null;
                Instantiate(networkManagerKcp);
                yield return null;
                onComplete?.Invoke();
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
            IsConnecting = false;
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
            IsConnecting = false;
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
