using UnityEngine;
using System.Collections;
using Mirror;

namespace SVESimulator
{
    public class GameConnectionManager : MonoBehaviour
    {
        private IEnumerator Start()
        {
            yield return new WaitUntil(() => NetworkClient.ready);
            while(!NetworkClient.activeHost)
            {
                yield return new WaitForSeconds(0.5f);
                // TODO - better way of client user waiting for host user to spawn their player object first
                if(FindObjectOfType<PlayerController>() != null)
                    break;
                yield return new WaitForSeconds(0.5f);
            }
            yield return new WaitForEndOfFrame();
            NetworkClient.AddPlayer();
        }
    }
}
