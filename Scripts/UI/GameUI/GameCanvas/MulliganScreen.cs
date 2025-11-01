using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using System.Linq;
using UnityEngine.UI;

namespace SVESimulator.UI
{
    public class MulliganScreen : MonoBehaviour
    {
        [SerializeField]
        private GameObject localSelectingContainer;
        [SerializeField]
        private GameObject opponentSelectingContainer;
        [SerializeField]
        private Button yesButton;
        [SerializeField]
        private Button noButton;

        // ------------------------------

        private void Awake()
        {
            yesButton.onClick.AddListener(() => PerformMulligan(true));
            noButton.onClick.AddListener(() => PerformMulligan(false));
        }

        public void ShowLocalMulligan()
        {
            localSelectingContainer.SetActive(true);
            opponentSelectingContainer.SetActive(false);
            gameObject.SetActive(true);
        }

        public void ShowOpponentMulligan()
        {
            localSelectingContainer.SetActive(false);
            opponentSelectingContainer.SetActive(true);
            gameObject.SetActive(true);
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }

        private void PerformMulligan(bool shouldMulligan)
        {
            PlayerController player = FindObjectsOfType<PlayerController>().FirstOrDefault(x => x.isLocalPlayer);
            Debug.Assert(player != null);
            player.LocalEvents.Mulligan(shouldMulligan);
            Close();
        }
    }
}
