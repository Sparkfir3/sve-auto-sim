using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using System.Linq;
using UnityEngine.UI;

namespace SVESimulator.UI
{
    public class SelectGoingFirstScreen : MonoBehaviour
    {
        public enum Mode { None = -1, LocalSelectGoingFirst, OpponentSelectGoingFirst }

        [TitleGroup("Object References"), SerializeField]
        private GameObject selectGoingFirstContainer;
        [SerializeField]
        private Button youButton;
        [SerializeField]
        private Button opponentButton;
        [SerializeField]
        private GameObject opponentIsSelectingContainer;

        private bool isInitialized = false;

        // ------------------------------

        #region Initialize & Unity Messages

        private void Initialize()
        {
            if(isInitialized)
                return;

            isInitialized = true;
            youButton.onClick.AddListener(() => SelectGoingFirst(true));
            opponentButton.onClick.AddListener(() => SelectGoingFirst(false));
        }

        public void Awake()
        {
            Initialize();
        }

        #endregion

        // ------------------------------

        #region Controls

        [TitleGroup("Buttons"), Button]
        public void SetDisplayMode(Mode mode, bool setActive = true)
        {
            switch(mode)
            {
                case Mode.LocalSelectGoingFirst:
                    selectGoingFirstContainer.SetActive(true);
                    opponentIsSelectingContainer.SetActive(false);
                    break;
                case Mode.OpponentSelectGoingFirst:
                    selectGoingFirstContainer.SetActive(false);
                    opponentIsSelectingContainer.SetActive(true);
                    break;
                default:
                    break;
            }
            gameObject.SetActive(setActive);
        }

        private void SelectGoingFirst(bool isLocalUserFirst)
        {
            PlayerController player = FindObjectsOfType<PlayerController>().FirstOrDefault(x => x.isLocalPlayer);
            Debug.Assert(player != null);
            player.LocalEvents.SetGoingFirst(isLocalUserFirst);
        }

        #endregion
    }
}
