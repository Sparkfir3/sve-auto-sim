using UnityEngine;
using Sirenix.OdinInspector;
using Sparkfire.Utility;

namespace SVESimulator.UI
{
    [DefaultExecutionOrder(-5000)]
    public class GameUIManager : Singleton<GameUIManager>
    {
        [Title("Main Controls"), SerializeField]
        private GameControlsUI gameControlsUI;

        [Title("Setup Menus"), SerializeField]
        private CanvasGroup setupMenuGroup;
        [SerializeField]
        private SelectGoingFirstScreen goingFirstScreen;
        [SerializeField]
        private MulliganScreen mulliganScreen;

        [Title("Main Game Info"), SerializeField]
        private CardInfoDisplay cardInfoDisplay;

        [Title("Gameplay Menus"), SerializeField]
        private CanvasGroup mainGameplayMenuGroup;
        [SerializeField]
        private QuickTimingDisplay quickTimingDisplay;
        [SerializeField]
        private ActivateEffectWindow activateEffectWindow;
        [SerializeField]
        private MultipleChoiceWindow multipleChoiceWindow;
        [SerializeField]
        private EffectTargetingUI effectTargetingUI;

        [Title("Other Game Info"), SerializeField]
        private ViewingZoneWindow viewingZoneWindow;
        [SerializeField]
        private MouseTooltip mouseTooltip;

        [Title("App State"), SerializeField]
        private DisconnectScreen disconnectScreen;
        [SerializeField]
        private PauseMenuController pauseMenu;
        [SerializeField]
        private WinLoseDisplay winLoseDisplay;

        [Title("Networking Objects"), SerializeField]
        private NetworkedUICalls networkedCalls;

        // ------------------------------

        // Controls
        public static GameControlsUI GameControlsUI => Instance.gameControlsUI;

        // Setup
        public static SelectGoingFirstScreen GoingFirstScreen => Instance.goingFirstScreen;
        public static MulliganScreen MulliganScreen => Instance.mulliganScreen;

        // Main Game Info
        public static CardInfoDisplay CardInfoDisplay => Instance.cardInfoDisplay;

        // Gameplay menus
        public static QuickTimingDisplay QuickTiming => Instance.quickTimingDisplay;
        public static ActivateEffectWindow ActivateEffect => Instance.activateEffectWindow;
        public static MultipleChoiceWindow MultipleChoice => Instance.multipleChoiceWindow;
        public static EffectTargetingUI EffectTargeting => Instance.effectTargetingUI;

        // Other Game Info
        public static ViewingZoneWindow ViewingZone => Instance.viewingZoneWindow;
        public static MouseTooltip MouseTooltip => Instance.mouseTooltip;

        // App State
        public static PauseMenuController PauseMenu => Instance.pauseMenu;
        public static DisconnectScreen DisconnectScreen => Instance.disconnectScreen;
        public static WinLoseDisplay WinLoseDisplay => Instance.winLoseDisplay;

        // Other
        public static NetworkedUICalls NetworkedCalls
        {
            get
            {
                if(!Instance.networkedCalls)
                    Instance.networkedCalls = FindObjectOfType<NetworkedUICalls>();
                return Instance.networkedCalls;
            }
        }

        // ------------------------------

        protected override void Awake()
        {
            base.Awake();

            // Controls
            gameControlsUI.gameObject.SetActive(true);

            // Setup
            SetSetupMenusVisible(true);
            goingFirstScreen.gameObject.SetActive(false);
            mulliganScreen.Close();

            // Main Game Info
            cardInfoDisplay.Hide();

            // Gameplay Menus
            SetMainGameplayMenusVisible(true);
            quickTimingDisplay.gameObject.SetActive(false);
            activateEffectWindow.Close();
            multipleChoiceWindow.Close();
            effectTargetingUI.gameObject.SetActive(true);

            // Other Game Info
            viewingZoneWindow.gameObject.SetActive(false);
            mouseTooltip.Disable();

            // App State
            disconnectScreen.gameObject.SetActive(false);
            pauseMenu.gameObject.SetActive(true);
            pauseMenu.ClosePauseMenu();
            winLoseDisplay.gameObject.SetActive(false);
        }

        public void Initialize(PlayerController player)
        {
            gameControlsUI.Initialize(player);
            activateEffectWindow.Initialize();
            multipleChoiceWindow.Initialize();
            effectTargetingUI.Initialize();
        }

        // ------------------------------

        public void SetSetupMenusVisible(bool visible)
        {
            setupMenuGroup.alpha = visible ? 1f : 0f;
            setupMenuGroup.ignoreParentGroups = visible;
        }

        public void SetMainGameplayMenusVisible(bool visible)
        {
            mainGameplayMenuGroup.alpha = visible ? 1f : 0f;
            mainGameplayMenuGroup.blocksRaycasts = visible;
            mainGameplayMenuGroup.interactable = visible;
        }
    }
}
