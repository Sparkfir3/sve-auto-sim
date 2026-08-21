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

        [FoldoutGroup("Network Manager Prefabs"), SerializeField, AssetsOnly]
        private GameObject networkManagerSteam;
        [FoldoutGroup("Network Manager Prefabs"), SerializeField, AssetsOnly]
        private GameObject networkManagerKcp;

        private Action onNextConnectionToServer;

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
                    onNextConnectionToServer = () => mainMenuView.PerformAction(MainMenuAction.Connecting);
                    HostSteamLobby();
                    break;
                case MainMenuButton.PlayOnlineJoin:
                    if(IsConnecting || !SVEGameNetworkManager.IsSteamConnected || mainMenuView.RoomCode.IsNullOrWhiteSpace())
                        return;
                    // TODO - loading icon
                    onNextConnectionToServer = () => mainMenuView.PerformAction(MainMenuAction.Connecting);
                    JoinSteamLobby();
                    break;

                // Play LAN
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

        #endregion

        // ------------------------------

        #region Network Event Handling

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

        #region Local Networking Controls

        public void StartLocalHost(Action onStartSuccess = null, Action onStartFail = null)
        {
            if(!TryLoadSelectedDeck())
                return;
            LibraryCardCache.ClearCache();
            IsConnecting = true;
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
            IsConnecting = true;
            InitKcpNetworkManager(() =>
            {
                SVEGameNetworkManager.Instance.StartClient();
            });
        }

        private void InitKcpNetworkManager(Action onComplete)
        {
            if(!SVEGameNetworkManager.IsSteamManager)
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

        #region Steam Networking Controls

        public void HostSteamLobby()
        {
            if(!SVEGameNetworkManager.IsSteamConnected || !TryLoadSelectedDeck())
                return;
            LibraryCardCache.ClearCache();
            IsConnecting = true;
            StartCoroutine(StartHostCoroutine());
            IEnumerator StartHostCoroutine()
            {
                if(!SVEGameNetworkManager.IsSteamManager)
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
            if(!SVEGameNetworkManager.IsSteamConnected || !TryLoadSelectedDeck())
                return;
            LibraryCardCache.ClearCache();
            IsConnecting = true;
            StartCoroutine(StartClientCoroutine());
            IEnumerator StartClientCoroutine()
            {
                if(!SVEGameNetworkManager.IsSteamManager)
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

        public bool IsConnecting
        {
            get => _isConnecting;
            set
            {
                if(value == _isConnecting)
                    return;
                _isConnecting = value;
                if(_isConnecting)
                    mainMenuView.OnStartConnecting();
                else
                    mainMenuView.OnEndConnecting();
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
