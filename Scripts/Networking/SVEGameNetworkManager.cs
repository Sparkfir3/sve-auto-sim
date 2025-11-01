using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using CCGKit;
using Mirror;

namespace SVESimulator
{
    public class SVEGameNetworkManager : NetworkManager
    {
        public static SVEGameNetworkManager Instance { get; private set; }
        public static SteamLobby SteamLobby { get; private set; }

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

        public override void OnServerConnect(NetworkConnectionToClient conn)
        {
            base.OnServerConnect(conn);
            Server server = FindObjectOfType<Server>();
            if(server)
                server.OnPlayerConnected(conn.connectionId);
        }

        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            base.OnServerDisconnect(conn);
            Server server = FindObjectOfType<Server>();
            if(server)
                server.OnPlayerDisconnected(conn.connectionId);
        }
    }
}
