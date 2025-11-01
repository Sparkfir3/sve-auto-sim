using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Sparkfire.Utility
{
    public class OnSceneLoadedEvent : MonoBehaviour
    {
        [SerializeField, InfoBox("Calls when the scene gets loaded. Does not call on initial start scene.")]
        private UnityEvent OnSceneLoaded;

        private void Awake()
        {
            SceneManager.sceneLoaded += Invoke;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= Invoke;
        }

        private void Invoke(Scene scene, LoadSceneMode mode)
        {
            OnSceneLoaded?.Invoke();
        }
    }
}
