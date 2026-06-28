using UnityEngine;

namespace SVESimulator.UI
{
    public class WinLoseDisplay : MonoBehaviour
    {
        [SerializeField]
        private GameObject winDisplay;
        [SerializeField]
        private GameObject loseDisplay;

        // ------------------------------

        public void WinGame()
        {
            GameUIManager.DisconnectScreen.SetNotVisible();
            gameObject.SetActive(true);
            winDisplay.SetActive(true);
            loseDisplay.SetActive(false);
        }

        public void LoseGame()
        {
            GameUIManager.DisconnectScreen.SetNotVisible();
            gameObject.SetActive(true);
            winDisplay.SetActive(false);
            loseDisplay.SetActive(true);
        }

        // ------------------------------

        public void ReturnToMainMenu()
        {
            GameUIManager.PauseMenu.ReturnToMainMenu();
        }
    }
}
