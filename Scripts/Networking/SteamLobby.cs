using System;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using Mirror;
using Sparkfire.AppStateSystem;
using Sparkfire.Utility;
using Steamworks;

namespace SVESimulator
{
    public class SteamLobby : MonoBehaviour
    {
        #region Variables

        public static ulong CurrentLobbyID;
        public static string CurrentLobbyName;

        private const int RandomLobbyCodeLength = 16;
        private const string HostAddressKey = "SveHostAddress";
        private const string LobbyNameKey = "SveLobbyName";
        
        // ---

        [Title("App States"), SerializeField]
        private ApplicationState appStateConnected;
        [SerializeField]
        private ApplicationState appStateDisconnected;

        private bool initialized;
        private NetworkManager networkManager;

        protected Callback<LobbyCreated_t> lobbyCreated;
        protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
        protected Callback<LobbyEnter_t> lobbyEntered;
        protected Callback<LobbyMatchList_t> getLobbyList;
        protected Callback<LobbyDataUpdate_t> getGetLobbyData;

        private Action<LobbyDataUpdate_t> OnLobbyDataFound;

        #endregion

        // ------------------------------

        #region Unity Functions

        private void Start()
        {
            networkManager = GetComponent<NetworkManager>();
            if(!SteamManager.Initialized || !SteamAPI.IsSteamRunning())
            {
                appStateConnected.SetStateInactive();
                appStateDisconnected.SetStateActive();
                return;
            }

            appStateDisconnected.SetStateInactive();
            appStateConnected.SetStateActive();
            lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
            gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
            lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
            getLobbyList = Callback<LobbyMatchList_t>.Create(OnGetLobbyList);
            getGetLobbyData = Callback<LobbyDataUpdate_t>.Create(OnGetLobbyData);
            initialized = true;
        }

        private void OnDestroy()
        {
            if(!initialized)
                return;

            lobbyCreated.Dispose();
            gameLobbyJoinRequested.Dispose();
            lobbyEntered.Dispose();
            getLobbyList.Dispose();
            getGetLobbyData.Dispose();
        }

        #endregion

        // ------------------------------

        #region Public Data Calls

        public void HostLobby(string lobbyName)
        {
            CurrentLobbyName = string.IsNullOrWhiteSpace(lobbyName) ? GeneralUtility.RandomAlphaString(RandomLobbyCodeLength) : lobbyName;
            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, networkManager.maxConnections);
        }

        public void GetLobby(string lobbyName, Action<CSteamID> onLobbyFound)
        {
            OnLobbyDataFound = null;
            OnLobbyDataFound += result =>
            {
                onLobbyFound?.Invoke(new CSteamID(result.m_ulSteamIDLobby));
            };

            SteamMatchmaking.AddRequestLobbyListResultCountFilter(1);
            SteamMatchmaking.AddRequestLobbyListStringFilter(LobbyNameKey, string.IsNullOrWhiteSpace(lobbyName) ? "" : lobbyName, ELobbyComparison.k_ELobbyComparisonEqual);
            SteamMatchmaking.RequestLobbyList();
        }

        #endregion

        // ------------------------------

        #region Steam Networking Callbacks

        private void OnLobbyCreated(LobbyCreated_t callback)
        {
            if(callback.m_eResult != EResult.k_EResultOK)
            {
                return;
            }
            networkManager.StartHost();
            CurrentLobbyID = callback.m_ulSteamIDLobby;
            SteamMatchmaking.SetLobbyData(new CSteamID(CurrentLobbyID), HostAddressKey, SteamUser.GetSteamID().ToString());
            SteamMatchmaking.SetLobbyData(new CSteamID(CurrentLobbyID), LobbyNameKey, string.IsNullOrWhiteSpace(CurrentLobbyName) ? SteamUser.GetSteamID().ToString() : CurrentLobbyName);

            Debug.Log($"Created Steam lobby, name: {CurrentLobbyName}");
        }

        private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
        {
            SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
        }

        private void OnLobbyEntered(LobbyEnter_t callback)
        {
            if(NetworkServer.active)
                return;

            CurrentLobbyID = callback.m_ulSteamIDLobby;
            networkManager.networkAddress = SteamMatchmaking.GetLobbyData(new CSteamID(CurrentLobbyID), HostAddressKey);
            CurrentLobbyName = SteamMatchmaking.GetLobbyData(new CSteamID(CurrentLobbyID), LobbyNameKey);

            networkManager.StartClient();
        }

        private void OnGetLobbyList(LobbyMatchList_t result)
        {
            if(result.m_nLobbiesMatching == 0)
                return;

            for(int i = 0; i < result.m_nLobbiesMatching; i++)
            {
                CSteamID lobbyID = SteamMatchmaking.GetLobbyByIndex(i);
                SteamMatchmaking.RequestLobbyData(lobbyID);
            }
        }

        private void OnGetLobbyData(LobbyDataUpdate_t result)
        {
            OnLobbyDataFound?.Invoke(result);
        }

        #endregion
    }
}
