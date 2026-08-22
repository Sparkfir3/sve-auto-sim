using UnityEngine;
using System.Collections;
using Mirror;

namespace SVESimulator
{
    public class NetworkSessionDataManager : NetworkBehaviour
    {
        private void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("a");
                ClientRpcTest();
                TargetRpcTest(connectionToClient);
            }
        }

        [ClientRpc]
        public void ClientRpcTest()
        {
            Debug.Log("Test ClientRpc");
        }

        [TargetRpc]
        public void TargetRpcTest(NetworkConnectionToClient conn)
        {
            Debug.Log("Test TargetRpc");
        }
    }
}
