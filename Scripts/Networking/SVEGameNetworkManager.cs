using System;
using UnityEngine;
using CCGKit;
using Mirror;

namespace SVESimulator
{
    [RequireComponent(typeof(NetworkSceneManager))]
    public class SVEGameNetworkManager : NetworkManager
    {
        public static SVEGameNetworkManager Instance { get; private set; }
        public static SteamLobby SteamLobby { get; private set; }
        public static NetworkSceneManager SceneManager { get; private set; }

        public static int ConnectedPlayerCount => NetworkServer.connections.Count;
        public static bool IsSteam => SteamLobby != null;

        public static event Action<NetworkConnectionToClient> OnPlayerConnected;
        public static event Action<NetworkConnectionToClient> OnPlayerDisconnected;
        public static event Action OnLocalDisconnect;

        // ------------------------------

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

        // ------------------------------

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

        public override void OnClientDisconnect()
        {
            base.OnClientDisconnect();
            OnLocalDisconnect?.Invoke();
        }
    }
}
