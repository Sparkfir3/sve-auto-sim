using System;
using System.Collections;
using System.Net.Sockets;
using CCGKit;
using Mirror;
using UnityEngine;
using Sirenix.OdinInspector;
using Sparkfire.Utility;
using Steamworks;

namespace SVESimulator.UI
{
    public class MainMenuController : MonoBehaviour
    {
        #region Variables

        [Title("Runtime Data"), SerializeField]
        private bool _isConnecting;

        [Title("Object References"), SerializeField]
        private MainMenuView mainMenuView;
        [SerializeField]
        private DeckSelectionController deckSelectionController;
        [SerializeField]
        private GameObject selectDeckError;

        public event Action OnClientConnected;
        public event Action OnClientDisconnected;
        public event Action OnOpponentConnected;
        public event Action OnOpponentDisconnected;
        public event Action<bool> OnTryConnection;
        public event Action<string> OnConnectionFailed;

        private Action onNextConnectionToServerSuccess;
        private Action onNextConnectionToServerFailed;

        #endregion

        // ------------------------------

        #region Unity Functions

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

        #endregion

        // ------------------------------

        #region UI Event Handling

        private void HandleButtonClicked(MainMenuButton button)
        {
            switch(button)
            {
                // Play Online
                case MainMenuButton.PlayOnlineHost:
                    if(IsConnecting || !SVEGameNetworkManager.IsSteamConnected)
                        return;
                    onNextConnectionToServerSuccess = () => mainMenuView.PerformAction(MainMenuAction.Connecting);
                    HostSteamLobby();
                    break;
                case MainMenuButton.PlayOnlineJoin:
                    if(IsConnecting || !SVEGameNetworkManager.IsSteamConnected || mainMenuView.RoomCode.IsNullOrWhiteSpace())
                        return;
                    // TODO - loading icon
                    onNextConnectionToServerSuccess = () => mainMenuView.PerformAction(MainMenuAction.Connecting);
                    JoinSteamLobby();
                    break;

                // Play LAN
                case MainMenuButton.PlayLocalHost:
                    if(IsConnecting)
                        return;
                    onNextConnectionToServerSuccess = null;
                    StartLocalHost(onStartSuccess: () => mainMenuView.PerformAction(MainMenuAction.Connecting));
                    break;
                case MainMenuButton.PlayLocalJoin:
                    if(IsConnecting)
                        return;
                    // TODO - loading icon
                    onNextConnectionToServerSuccess = () => mainMenuView.PerformAction(MainMenuAction.Connecting);
                    StartLocalClient();
                    break;

                // Other
                case MainMenuButton.BackToMain:
                    SVEGameNetworkManager.Instance.Disconnect();
                    onNextConnectionToServerSuccess = null;
                    IsConnecting = false;
                    break;
                case MainMenuButton.StartGame:
                    TryLoadSelectedDeck();
                    SVEGameNetworkManager.SceneManager.LoadGameplay();
                    break;
                case MainMenuButton.Quit:
                    QuitGame();
                    break;
            }
        }

        #endregion

        // ------------------------------

        #region Network Event Handling

        private void HandlePlayerConnectedToServer(NetworkConnectionToClient conn)
        {
            if(SVEGameNetworkManager.ConnectedPlayerCount >= 2 && mainMenuView.CurrentState == MainMenuViewState.Connecting)
            {
                mainMenuView.PerformAction(MainMenuAction.ReadyToStart);
                OnOpponentConnected?.Invoke();
            }
        }

        private void HandlePlayerDisconnectedFromServer(NetworkConnectionToClient conn)
        {
            if(NetworkClient.active && mainMenuView.CurrentState == MainMenuViewState.ReadyToStart && conn.connectionId != 0) // other user disconnect
            {
                mainMenuView.PerformAction(MainMenuAction.OppDisconnected);
                OnOpponentDisconnected?.Invoke();
            }
        }

        private void HandleLocalPlayerConnected()
        {
            IsConnecting = false;
            onNextConnectionToServerSuccess?.Invoke();
            onNextConnectionToServerSuccess = null;
            onNextConnectionToServerFailed = null;
            OnClientConnected?.Invoke();
        }

        private void HandleLocalPlayerDisconnected()
        {
            IsConnecting = false;
            onNextConnectionToServerFailed?.Invoke();
            onNextConnectionToServerSuccess = null;
            onNextConnectionToServerFailed = null;
            if(mainMenuView.CurrentState is MainMenuViewState.Connecting or MainMenuViewState.ReadyToStart)
                mainMenuView.PerformAction(MainMenuAction.Back);
            OnClientDisconnected?.Invoke();
        }

        #endregion

        // ------------------------------

        #region Local Networking Controls

        public void StartLocalHost(Action onStartSuccess = null, Action onStartFail = null)
        {
            if(!TryLoadSelectedDeck())
                return;
            LibraryCardCache.ClearCache();
            IsConnecting = true;
            SVEGameNetworkManager.Instance.InitKcpNetworkManager(() =>
            {
                try
                {
                    SVEGameNetworkManager.Instance.StartHost();
                }
                catch(SocketException e)
                {
                    Debug.Log($"Attempted to start new a LAN connection instance when one is already active.\n{e.ToString()}");
                    OnConnectionFailed?.Invoke("An active LAN connection was found, but a second one cannot be started on the same network.");
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
            IsConnecting = true;
            onNextConnectionToServerFailed = () => OnConnectionFailed?.Invoke("Failed to find an active LAN connection.");
            SVEGameNetworkManager.Instance.InitKcpNetworkManager(() =>
            {
                SVEGameNetworkManager.Instance.StartClient();
            });
        }

        #endregion

        // ------------------------------

        #region Steam Networking Controls

        public void HostSteamLobby()
        {
            if(!SVEGameNetworkManager.IsSteamConnected || !TryLoadSelectedDeck())
                return;
            LibraryCardCache.ClearCache();
            IsConnecting = true;
            SVEGameNetworkManager.OnFindLobbyTimeout += () =>
            {
                Debug.Log($"Connection to Steam lobby timed out.");
                IsConnecting = false;
                OnConnectionFailed?.Invoke("Connection timed out.");
            };
            SVEGameNetworkManager.Instance.InitSteamNetworkManager(() =>
            {
                SVEGameNetworkManager.SteamLobby.HostLobby(mainMenuView.RoomCode);
            });
        }

        public void JoinSteamLobby()
        {
            if(!SVEGameNetworkManager.IsSteamConnected || !TryLoadSelectedDeck())
                return;
            LibraryCardCache.ClearCache();
            IsConnecting = true;
            SVEGameNetworkManager.OnFindLobbyTimeout += () =>
            {
                Debug.Log($"Failed to find lobby/Connection to Steam lobby timed out.");
                IsConnecting = false;
                OnConnectionFailed?.Invoke("Failed to find a game lobby.");
            };
            SVEGameNetworkManager.Instance.InitSteamNetworkManager(() =>
            {
                SVEGameNetworkManager.SteamLobby.GetLobby(mainMenuView.RoomCode, lobbyID =>
                {
                    SteamMatchmaking.JoinLobby(lobbyID);
                });
            });
        }

        #endregion

        // ------------------------------

        #region Other

        public bool IsConnecting
        {
            get => _isConnecting;
            set
            {
                if(value == _isConnecting)
                    return;
                _isConnecting = value;
                OnTryConnection?.Invoke(_isConnecting);
            }
        }

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
