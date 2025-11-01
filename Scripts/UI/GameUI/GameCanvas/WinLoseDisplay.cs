using Mirror;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.SceneManagement;

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
            gameObject.SetActive(true);
            winDisplay.SetActive(true);
            loseDisplay.SetActive(false);
        }

        public void LoseGame()
        {
            gameObject.SetActive(true);
            winDisplay.SetActive(false);
            loseDisplay.SetActive(true);
        }

        // ------------------------------

        public void ReturnToMainMenu()
        {
            NetworkManager.singleton.StopHost();
            NetworkManager.singleton.StopServer();
            SceneManager.LoadScene("MainMenu");
        }
    }
}
