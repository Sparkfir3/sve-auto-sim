using UnityEngine;
using System.Collections;
using Mirror;

namespace SVESimulator
{
    public class GameConnectionManager : MonoBehaviour
    {
        #region Variables

        [field: Header("Runtime Data"), SerializeField]
        public bool IsConnected { get; private set; }
        [field: SerializeField]
        public bool IsHost { get; private set; }

        #endregion

        // ------------------------------

        #region Unity Functions

        private IEnumerator Start()
        {
            yield return new WaitUntil(() => NetworkClient.ready);
            yield return new WaitForSeconds(0.1f);
            while(!NetworkClient.activeHost)
            {
                yield return new WaitForSeconds(0.5f);
                // TODO - better way of client user waiting for host user to spawn their player object first
                if(FindObjectOfType<PlayerController>() != null)
                    break;
                yield return new WaitForSeconds(0.25f);
            }
            yield return new WaitForEndOfFrame();
            IsHost = NetworkClient.activeHost;
            NetworkClient.AddPlayer();

            SVEGameNetworkManager.OnPlayerDisconnected += HandleDisconnect;
            SVEGameNetworkManager.OnLocalDisconnect += HandleLocalDisconnected;
        }

        private void OnDestroy()
        {
            DisableDisconnectEvents();
        }

        private void Update()
        {
            IsConnected = NetworkClient.isConnected;
        }

        #endregion

        // ------------------------------

        #region Public Functions

        public void DisableDisconnectEvents()
        {
            SVEGameNetworkManager.OnPlayerDisconnected -= HandleDisconnect;
            SVEGameNetworkManager.OnLocalDisconnect -= HandleLocalDisconnected;
        }

        #endregion

        // ------------------------------

        #region Disconnect Events

        private void HandleDisconnect(NetworkConnectionToClient conn)
        {
            if(conn.connectionId == 0)
                return; // Let OnLocalDisconnect handle the logic
            HandleOpponentDisconnected();
        }

        private void HandleLocalDisconnected()
        {
            Debug.Log("Local Disconnected");
            // TODO - UI
        }

        private void HandleOpponentDisconnected()
        {
            if(!NetworkClient.isConnected)
                return;
            Debug.Log("Opponent Disconnected");
            // TODO - UI
        }

        #endregion
    }
}
