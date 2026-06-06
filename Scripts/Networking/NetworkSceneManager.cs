using UnityEngine;
using Sirenix.OdinInspector;
using Mirror;

namespace SVESimulator
{
    public class NetworkSceneManager : MonoBehaviour
    {
        [SerializeField]
        private SVEGameNetworkManager networkManager;
        [SerializeField]
        private string mainMenuScene = "MainMenu";
        [SerializeField]
        private string gameplayScene = "Gameplay";

        // ------------------------------

        [TitleGroup("Debug"), Button, HideInEditorMode]
        public void LoadMainMenu()
        {
            networkManager.ServerChangeScene(mainMenuScene);
        }

        [TitleGroup("Debug"), Button, HideInEditorMode]
        public void LoadGameplay()
        {
            networkManager.ServerChangeScene(gameplayScene);
        }

        // ------------------------------

#if UNITY_EDITOR
        private void OnValidate()
        {
            if(!networkManager)
                networkManager = GetComponent<SVEGameNetworkManager>();
        }
#endif
    }
}
