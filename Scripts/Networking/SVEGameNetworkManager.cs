using System;
using System.Collections;
using UnityEngine;
using CCGKit;
using Mirror;
using Steamworks;
using Sparkfire.AppStateSystem;

namespace SVESimulator
{
    [RequireComponent(typeof(NetworkSceneManager))]
    public class SVEGameNetworkManager : NetworkManager
    {
        #region Variables

        [Header("Network Manager Prefabs"), SerializeField]
        private SVEGameNetworkManager networkManagerSteamPrefab;
        [SerializeField]
        private SVEGameNetworkManager networkManagerKcpPrefab;

        // ---

        public static SVEGameNetworkManager Instance { get; private set; }
        public static SteamLobby SteamLobby { get; private set; }
        public static NetworkSceneManager SceneManager { get; private set; }

        public static int ConnectedPlayerCount => NetworkServer.connections.Count;
        public static bool IsSteamConnected => SteamManager.Initialized && SteamAPI.IsSteamRunning();
        public static bool IsSteamManager => SteamLobby != null;
        public static bool IsSteamManagerAndConnected => IsSteamManager && IsSteamConnected;
        public static bool IsKcpManager => SteamLobby == null;

        public static event Action<NetworkConnectionToClient> OnPlayerConnected;
        public static event Action<NetworkConnectionToClient> OnPlayerDisconnected;
        public static event Action OnLocalConnect;
        public static event Action OnLocalDisconnect;
        public static Action<string> OnStartHostSteamLobby;

        #endregion

        // ------------------------------

        #region Unity Functions

        public override void Awake()
        {
            base.Awake();
            if(Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            SteamLobby = GetComponent<SteamLobby>();
            SceneManager = GetComponent<NetworkSceneManager>();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if(Instance == this)
            {
                Instance = null;
                SteamLobby = null;
            }
        }

        #endregion

        // ------------------------------

        #region Init Network Managers

        public void InitSteamNetworkManager(Action onComplete = null)
        {
            if(IsSteamManager)
            {
                onComplete?.Invoke();
                return;
            }
            ApplicationStateManager.Instance.StartCoroutine(RebootNetworkManager(networkManagerSteamPrefab, onComplete));
        }

        public void InitKcpNetworkManager(Action onComplete = null)
        {
            if(IsKcpManager)
            {
                onComplete?.Invoke();
                return;
            }
            ApplicationStateManager.Instance.StartCoroutine(RebootNetworkManager(networkManagerKcpPrefab, onComplete));
        }

        private static IEnumerator RebootNetworkManager(SVEGameNetworkManager newNetworkManager, Action onComplete)
        {
            Destroy(Instance.gameObject);
            yield return null;
            Instantiate(newNetworkManager.gameObject);
            yield return null;
            yield return new WaitUntil(() => Instance);
            yield return null;
            onComplete?.Invoke();
        }

        #endregion

        // ------------------------------

        #region Network Management

        public void Disconnect()
        {
            if(!Instance.isNetworkActive)
                return;
            if(IsSteamManager)
                SteamMatchmaking.LeaveLobby(new CSteamID(SteamLobby.CurrentLobbyID));
            Instance.StopHost();
            Instance.StopServer();
        }

        #endregion

        // ------------------------------

        #region Network Events

        public override void OnServerConnect(NetworkConnectionToClient conn)
        {
            base.OnServerConnect(conn);
            Server server = FindObjectOfType<Server>();
            if(server)
                server.OnPlayerConnected(conn.connectionId);
            OnPlayerConnected?.Invoke(conn);
        }

        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            base.OnServerDisconnect(conn);
            Server server = FindObjectOfType<Server>();
            if(server)
                server.OnPlayerDisconnected(conn.connectionId);
            OnPlayerDisconnected?.Invoke(conn);
        }

        public override void OnClientConnect()
        {
            base.OnClientConnect();
            OnLocalConnect?.Invoke();
        }

        public override void OnClientDisconnect()
        {
            base.OnClientDisconnect();
            OnLocalDisconnect?.Invoke();
        }

        #endregion
    }
}
