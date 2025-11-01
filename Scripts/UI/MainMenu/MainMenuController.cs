using System.Collections;
using CCGKit;
using UnityEngine;
using Sirenix.OdinInspector;
using Steamworks;
using TMPro;

namespace SVESimulator.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField]
        private GameObject networkManagerKcp;
        [SerializeField]
        private TMP_InputField steamRoomCodeInputField;
        [SerializeField]
        private DeckSelectionController deckSelectionController;
        [SerializeField]
        private GameObject selectDeckError;

        // ------------------------------

        private void Start()
        {
            GameManager.Instance.Initialize();
            deckSelectionController.Initialize();
            deckSelectionController.OnSelectDeck += () => selectDeckError.SetActive(false);
        }

        // ------------------------------

        #region Local Networking

        public void StartLocalHost()
        {
            if(!TryLoadSelectedDeck())
                return;
            StopAllCoroutines();
            StartCoroutine(StartHostCoroutine());
            IEnumerator StartHostCoroutine()
            {
                if(SVEGameNetworkManager.Instance)
                {
                    Destroy(SVEGameNetworkManager.Instance.gameObject);
                    yield return null;
                }
                Instantiate(networkManagerKcp);
                SVEGameNetworkManager.Instance.StartHost();
            }
        }

        public void StartLocalClient()
        {
            if(!TryLoadSelectedDeck())
                return;
            StopAllCoroutines();
            StartCoroutine(StartHostCoroutine());
            IEnumerator StartHostCoroutine()
            {
                if(SVEGameNetworkManager.Instance)
                {
                    Destroy(SVEGameNetworkManager.Instance.gameObject);
                    yield return null;
                }
                Instantiate(networkManagerKcp);
                SVEGameNetworkManager.Instance.StartClient();
            }
        }

        #endregion

        // ------------------------------

        #region Steam Networking

        public void HostSteamLobby()
        {
            if(!TryLoadSelectedDeck())
                return;
            SVEGameNetworkManager.SteamLobby.HostLobby(steamRoomCodeInputField.text);
        }

        public void JoinSteamLobby()
        {
            if(!TryLoadSelectedDeck())
                return;
            SVEGameNetworkManager.SteamLobby.GetLobby(steamRoomCodeInputField.text, lobbyID =>
            {
                SteamMatchmaking.JoinLobby(lobbyID);
            });
        }

        #endregion

        // ------------------------------

        private bool TryLoadSelectedDeck()
        {
            if(!deckSelectionController.HasSelectedDeck)
            {
                selectDeckError.SetActive(true);
                return false;
            }
            selectDeckError.SetActive(false);
            deckSelectionController.LoadCurrentDeck();
            return true;
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            Debug.Log("Attempted to quit application in editor mode");
#else
            Application.Quit();
#endif
        }
    }
}
