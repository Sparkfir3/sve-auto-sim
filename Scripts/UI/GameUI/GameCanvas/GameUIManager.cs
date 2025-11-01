using UnityEngine;
using Sirenix.OdinInspector;
using Sparkfire.Utility;

namespace SVESimulator.UI
{
    [DefaultExecutionOrder(-5000)]
    public class GameUIManager : Singleton<GameUIManager>
    {
        [Title("Screens"), SerializeField]
        private SelectGoingFirstScreen goingFirstScreen;
        [SerializeField]
        private MulliganScreen mulliganScreen;
        [SerializeField]
        private CardInfoDisplay cardInfoDisplay;
        [SerializeField]
        private WinLoseDisplay winLoseDisplay;
        [SerializeField]
        private GameControlsUI gameControlsUI;
        [SerializeField]
        private ActivateEffectWindow activateEffectWindow;
        [SerializeField]
        private QuickTimingDisplay quickTimingDisplay;
        [SerializeField]
        private MultipleChoiceWindow multipleChoiceWindow;
        [SerializeField]
        private EffectTargetingUI effectTargetingUI;

        [Title("Networking Objects"), SerializeField]
        private NetworkedUICalls networkedCalls;

        // ------------------------------

        public static SelectGoingFirstScreen GoingFirstScreen => Instance.goingFirstScreen;
        public static MulliganScreen MulliganScreen => Instance.mulliganScreen;
        public static CardInfoDisplay CardInfoDisplay => Instance.cardInfoDisplay;
        public static WinLoseDisplay WinLoseDisplay => Instance.winLoseDisplay;
        public static GameControlsUI GameControlsUI => Instance.gameControlsUI;
        public static ActivateEffectWindow ActivateEffect => Instance.activateEffectWindow;
        public static QuickTimingDisplay QuickTiming => Instance.quickTimingDisplay;
        public static MultipleChoiceWindow MultipleChoice => Instance.multipleChoiceWindow;
        public static EffectTargetingUI EffectTargeting => Instance.effectTargetingUI;

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
            goingFirstScreen.gameObject.SetActive(false);
            mulliganScreen.Close();
            cardInfoDisplay.Hide();
            winLoseDisplay.gameObject.SetActive(false);
            gameControlsUI.gameObject.SetActive(true);
            activateEffectWindow.Initialize();
            activateEffectWindow.Close();
            quickTimingDisplay.gameObject.SetActive(false);
            multipleChoiceWindow.Initialize();
            multipleChoiceWindow.Close();
            effectTargetingUI.Initialize();
            effectTargetingUI.gameObject.SetActive(true);
        }
    }
}
