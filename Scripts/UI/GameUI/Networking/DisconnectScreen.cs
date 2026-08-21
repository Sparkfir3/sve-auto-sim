using UnityEngine;
using TMPro;

namespace SVESimulator.UI
{
    public class DisconnectScreen : MonoBehaviour
    {
        [SerializeField]
        private CanvasGroup canvasGroup;
        [SerializeField]
        private TextMeshProUGUI textBox;
        [SerializeField, TextArea(1, 2)]
        private string localDisconnectText = "You have been disconnected";
        [SerializeField, TextArea(1, 2)]
        private string oppDisconnectText = "Your opponent has disconnected";

        // ------------------------------

        public void OpenLocalDisconnect()
        {
            textBox.text = localDisconnectText;
            OpenInternal();
        }

        public void OpenOpponentDisconnect()
        {
            textBox.text = oppDisconnectText;
            OpenInternal();
        }

        public void SetNotVisible()
        {
            canvasGroup.alpha = 0f;
        }

        public void ReturnToMainMenu()
        {
            GameUIManager.PauseMenu.ReturnToMainMenu();
        }

        // ------------------------------

        private void OpenInternal()
        {
            GameUIManager.Instance.SetSetupMenusVisible(false);
            GameUIManager.Instance.SetMainGameplayMenusVisible(false);
            GameUIManager.ViewingZone.gameObject.SetActive(false);
            gameObject.SetActive(true);
        }
    }
}
