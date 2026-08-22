using UnityEngine;
using System.Collections.Generic;
using Mirror;
using Steamworks;

namespace SVESimulator
{
    public class NetworkDataManager : NetworkBehaviour
    {
        private struct ProfileInfo
        {
            public Texture2D profilePic;
            public string username;

            public ProfileInfo(Texture2D profilePic, string username)
            {
                this.profilePic = profilePic;
                this.username = username;
            }
        }

        private readonly Dictionary<int, ProfileInfo> UserProfileInfo = new();

        // ------------------------------

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
            SVEGameNetworkManager.DataManager = this;
        }

        // ------------------------------

        public void SaveProfileInfo(int connectionId, Texture2D profilePic, string username)
        {
            Debug.Log($"Caching user info {username} with connection ID {connectionId}");
            UserProfileInfo.TryAdd(connectionId, new ProfileInfo(profilePic, username));
        }

        public bool TryGetProfileInfo(int connectionId, out Texture2D profilePic, out string username)
        {
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
