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
            gameObject.SetActive(true);
        }

        public void OpenOpponentDisconnect()
        {
            textBox.text = oppDisconnectText;
            gameObject.SetActive(true);
        }

        public void Disable()
        {
            canvasGroup.alpha = 0f;
        }

        public void ReturnToMainMenu()
        {
            GameUIManager.PauseMenu.ReturnToMainMenu();
        }
    }
}
