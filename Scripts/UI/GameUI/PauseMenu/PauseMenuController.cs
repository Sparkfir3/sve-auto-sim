using UnityEngine;
using Sirenix.OdinInspector;
using Mirror;
using Sparkfire.AppStateSystem;
using Steamworks;
using UnityEngine.UI;
using ReadOnly = Sirenix.OdinInspector.ReadOnlyAttribute;
using ShowInInspector = Sirenix.OdinInspector.ShowInInspectorAttribute;

namespace SVESimulator
{
    [DefaultExecutionOrder(1000)]
    public class PauseMenuController : MonoBehaviour
    {
        [Title("Runtime Data"), ShowInInspector, ReadOnly]
        public bool PointerInside { get; set; }

        [Title("Object References"), SerializeField]
        private GameObject pauseContainer;
        [SerializeField]
        private Button pauseButton;
        [SerializeField]
        private Button quitGameButton;
        [SerializeField]
        private ApplicationStateTransition quitTransition;

        public bool IsOpen => pauseContainer.activeSelf;

        // ------------------------------

        private void Awake()
        {
            pauseContainer.SetActive(false);
            pauseButton.onClick.AddListener(TogglePauseMenu);
            quitGameButton.onClick.AddListener(ReturnToMainMenu);
        }

        private void Update()
        {
            if(IsOpen && !PointerInside && (Input.GetButtonDown("Cancel") || Input.GetKeyDown(KeyCode.Mouse0)))
                ClosePauseMenu();
        }

        // ------------------------------

        public void TogglePauseMenu()
        {
            if(!IsOpen)
                OpenPauseMenu();
            else
                ClosePauseMenu();
        }

        public void OpenPauseMenu()
        {
            pauseContainer.SetActive(true);
        }

        public void ClosePauseMenu()
        {
            PointerInside = false;
            pauseContainer.SetActive(false);
        }

        private void ReturnToMainMenu()
        {
            NetworkManager.singleton.StopHost();
            SteamMatchmaking.LeaveLobby(new CSteamID(SteamLobby.CurrentLobbyID));
            quitTransition.Transition();
        }
    }
}
