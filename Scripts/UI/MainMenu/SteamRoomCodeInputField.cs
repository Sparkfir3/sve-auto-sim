using UnityEngine;
using Sirenix.OdinInspector;
using Steamworks;
using TMPro;

namespace SVESimulator.UI
{
    public class SteamRoomCodeInputField : MonoBehaviour
    {
        [SerializeField]
        private TMP_InputField inputField;
        [SerializeField]
        private CanvasGroup canvasGroup;

        public string Text => inputField.text;
        public bool Interactable
        {
            get => inputField.interactable;
            set => inputField.interactable = value;
        }

        // ------------------------------

        private void Start()
        {
            SVEGameNetworkManager.SteamLobby.OnStartHostLobby += OnStartLobby;
        }

        private void OnDestroy()
        {
            SVEGameNetworkManager.SteamLobby.OnStartHostLobby -= OnStartLobby;
        }

        // ------------------------------

        public void Show(bool clearText = true)
        {
            if(clearText)
                Clear();
            canvasGroup.interactable = true;
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            canvasGroup.interactable = false;
            gameObject.SetActive(false);
        }

        public void Clear()
        {
            inputField.text = "";
        }

        private void OnStartLobby(string lobbyName)
        {
            inputField.SetTextWithoutNotify(lobbyName);
        }
    }
}
