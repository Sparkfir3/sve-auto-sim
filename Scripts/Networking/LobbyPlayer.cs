using UnityEngine;
using Mirror;

namespace SVESimulator
{
    public class LobbyPlayer : NetworkBehaviour
    {
        private void Start()
        {
            if(SVEGameNetworkManager.IsSteamManagerAndConnected && NetworkDataManager.TryGetUserProfileInfo(out Texture2D profilePic, out string username))
            {
                SVEGameNetworkManager.DataManager.SaveProfileInfo(connectionToClient.connectionId, profilePic, username);
            }
        }
    }
}
