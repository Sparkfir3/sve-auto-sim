using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using Steamworks;

namespace SVESimulator
{
    /// <summary>
    /// Stores and handles general information about players when connected to the network
    /// (Specifically, handles storing Steam profile information)
    /// </summary>
    public class NetworkDataManager : NetworkBehaviour
    {
        public class ProfileInfo
        {
            public readonly Texture2D profilePic;
            public readonly string username;

            public ProfileInfo()
            {
                profilePic = null;
                username = null;
            }

            public ProfileInfo(Texture2D profilePic, string username)
            {
                this.profilePic = profilePic;
                this.username = username;
            }
        }

        public readonly SyncDictionary<int, ProfileInfo> UserProfileInfo = new();

        // ------------------------------

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
            SVEGameNetworkManager.DataManager = this;
        }

        // ------------------------------

        [Command(requiresAuthority = false)]
        public void CmdSaveProfileInfo(Texture2D profilePic, string username, NetworkConnectionToClient conn = null)
        {
            if(conn == null)
                return;
            Debug.Log($"Caching user info for {username} with connection ID {conn.connectionId}");
            UserProfileInfo.TryAdd(conn.connectionId, new ProfileInfo(profilePic, username));
        }

        // -----

        public bool TryGetLocalProfileInfo(out Texture2D profilePic, out string username)
        {
            if(NetworkClient.connection == null)
            {
                profilePic = null;
                username = null;
                return false;
            }

            int connectionId = NetworkClient.connection.connectionId;
            if(!UserProfileInfo.TryGetValue(connectionId, out ProfileInfo profileInfo))
            {
                profilePic = null;
                username = null;
                return false;
            }
            profilePic = profileInfo.profilePic;
            username = profileInfo.username;
            return true;
        }

        public bool TryGetOpponentProfileInfo(out Texture2D profilePic, out string username)
        {
            // assuming for an opponent to be connected, we must also be connected already
            if(UserProfileInfo.Count <= 1 || NetworkClient.connection == null)
            {
                profilePic = null;
                username = null;
                return false;
            }

            int connectionId = NetworkClient.connection.connectionId;
            ProfileInfo profileInfo = UserProfileInfo.FirstOrDefault(x => x.Key != connectionId).Value;
            if(profileInfo == null)
            {
                profilePic = null;
                username = null;
                return false;
            }
            profilePic = profileInfo.profilePic;
            username = profileInfo.username;
            return true;
        }

        // ------------------------------

        #region Utils

        public static bool TryGetUserProfileInfo(out Texture2D profilePic, out string username)
        {
            if(!SVEGameNetworkManager.IsSteamManagerAndConnected)
            {
                profilePic = null;
                username = null;
                return false;
            }

            int iImage = SteamFriends.GetMediumFriendAvatar(SteamUser.GetSteamID());
            profilePic = GetSteamImageAsTexture2D(iImage);
            username = SteamFriends.GetFriendPersonaName(SteamUser.GetSteamID());
            return true;
        }

        // Taken from open source Steamworks.NET-Test repo - SteamUtilsTest.cs
        private static Texture2D GetSteamImageAsTexture2D(int iImage)
        {
            Texture2D ret = null;
            uint ImageWidth;
            uint ImageHeight;
            bool bIsValid = SteamUtils.GetImageSize(iImage, out ImageWidth, out ImageHeight);

            if(bIsValid)
            {
                byte[] Image = new byte[ImageWidth * ImageHeight * 4];
                bIsValid = SteamUtils.GetImageRGBA(iImage, Image, (int)(ImageWidth * ImageHeight * 4));
                if(bIsValid)
                {
                    ret = new Texture2D((int)ImageWidth, (int)ImageHeight, TextureFormat.RGBA32, false, true);
                    ret.LoadRawTextureData(Image);
                    ret.Apply();
                }
            }

            return ret;
        }

        #endregion
    }
}
