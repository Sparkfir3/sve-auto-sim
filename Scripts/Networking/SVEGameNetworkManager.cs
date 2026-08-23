using System;
using System.Collections;
using System.Collections.Generic;
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

        [Header("Prefabs"), SerializeField]
        private SVEGameNetworkManager networkManagerSteamPrefab;
        [SerializeField]
        private SVEGameNetworkManager networkManagerKcpPrefab;
        [SerializeField]
        private NetworkDataManager dataManagerPrefab;
        [SerializeField]
        private PlayerController gamePlayerPrefab;

        [Header("Timeout"), SerializeField]
        private float timeoutDuration = 10f;
        [SerializeField]
        private float timeoutTimer;

        // ---

        public static SVEGameNetworkManager Instance { get; private set; }
        public static SteamLobby SteamLobby { get; private set; }
        public static NetworkSceneManager SceneManager { get; private set; }
        public static NetworkDataManager DataManager { get; set; }

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
        public static event Action OnFindLobbyTimeout;

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

        public override void Update()
        {
            base.Update();
            if(timeoutTimer > 0f)
            {
                timeoutTimer -= Time.deltaTime;
                if(timeoutTimer <= 0f)
                    ConnectionTimeout();
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if(Instance == this)
            {
                Instance = null;
                SteamLobby = null;
            }
            NetworkServer.UnregisterHandler<SpawnGameplayPlayerControllerMsg>();
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
            CancelConnectionTimeout();
            if(IsSteamManager)
                SteamMatchmaking.LeaveLobby(new CSteamID(SteamLobby.CurrentLobbyID));
            if(!Instance.isNetworkActive)
                return;
            Instance.StopHost();
            Instance.StopServer();
            CancelConnectionTimeout();
        }

        #endregion

        // ------------------------------

        #region Network Events

        public override void OnStartServer()
        {
            base.OnStartServer();
            NetworkServer.RegisterHandler<SpawnGameplayPlayerControllerMsg>(SpawnGameplayPlayerController);
            NetworkServer.Spawn(Instantiate(dataManagerPrefab).gameObject);
        }

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
            if(DataManager)
                DataManager.CmdRemoveProfileInfo();
            OnPlayerDisconnected?.Invoke(conn);
        }

        public override void OnClientConnect()
        {
            base.OnClientConnect();
            CancelConnectionTimeout();
            OnLocalConnect?.Invoke();
        }

        public override void OnClientDisconnect()
        {
            base.OnClientDisconnect();
            OnLocalDisconnect?.Invoke();
        }

        #endregion

        // ------------------------------

        #region Connection Timeout

        public void StartConnectionTimeoutTimer()
        {
            timeoutTimer = timeoutDuration;
        }

        public void CancelConnectionTimeout()
        {
            OnFindLobbyTimeout = null;
            timeoutTimer = 0f;
        }

        private void ConnectionTimeout()
        {
            OnFindLobbyTimeout?.Invoke();
            Disconnect(); // leads to CancelConnectionTimeout()
        }

        #endregion

        // ------------------------------

        #region Internal Controls

        private void SpawnGameplayPlayerController(NetworkConnectionToClient conn, SpawnGameplayPlayerControllerMsg msg)
        {
            GameObject oldPlayer = conn.identity.gameObject;
            GameObject newPlayer = Instantiate(gamePlayerPrefab).gameObject;
            NetworkServer.ReplacePlayerForConnection(conn, newPlayer, true);
            Destroy(oldPlayer, 0.1f);
        }

        #endregion
    }
}
