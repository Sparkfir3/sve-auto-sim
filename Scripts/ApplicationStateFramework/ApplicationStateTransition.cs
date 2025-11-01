using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TitleGroup = UnityEngine.HeaderAttribute;
using FoldoutGroup = Sparkfire.Utility.BlankAttribute;
using HorizontalGroup = Sparkfire.Utility.BlankAttribute;
using TableList = Sparkfire.Utility.BlankAttribute;
using HideIf = Sparkfire.Utility.BlankAttribute;
#endif

namespace Sparkfire.AppStateSystem
{
    public class ApplicationStateTransition : MonoBehaviour
    {
        private enum StateBlendType
        {
            SwitchOnStart, // State will switch before transition starts
            SwitchInMiddle, // State will during the transition
            SwitchOnEnd, // State will switch after transition is over
            Blend // Both states will be active during the transition
        }

        // ---

        [field: TitleGroup("Transition Controls"), SerializeField]
        private ApplicationState nextState;
        [SerializeField]
        [Tooltip("Controls how state changes are handled during the transition")]
        private StateBlendType stateBlendSetting = StateBlendType.SwitchInMiddle;
        [field: SerializeField, FoldoutGroup("Transition Events")]
        public UnityEvent PreTransition { get; private set; }
        [field: SerializeField, FoldoutGroup("Transition Events")]
        public UnityEvent MidTransition { get; private set; }
        [field: SerializeField, FoldoutGroup("Transition Events")]
        public UnityEvent PostTransition { get; private set; }

        // ---

        public List<IEnumerator> CoroutinesPreTransition { get; private set; } = new();
        public List<IEnumerator> CoroutinesMidTransition { get; private set; } = new();
        public List<IEnumerator> CoroutinesPostTransition { get; private set; } = new();

        // ---

        [TitleGroup("Scene Loading Settings"), SerializeField]
        [Tooltip("Whether all the scenes to load should be added onto existing scenes")]
        private bool additiveSceneLoading;
        [SerializeField, HorizontalGroup("Scenes")]
        private float beforeSceneLoadDelay;
        [SerializeField, HorizontalGroup("Scenes")]
        private float afterSceneLoadDelay;
        [SerializeField, TableList]
        private List<SceneToLoad> scenesToLoad;

        // ---

        public bool IsTransitioning { get; private set; } = false;

        // ----------------------------------------------------------------------------------------------------------

        private void OnValidate()
        {
            beforeSceneLoadDelay = Mathf.Clamp(beforeSceneLoadDelay, 0, Mathf.Infinity);
            afterSceneLoadDelay = Mathf.Clamp(afterSceneLoadDelay, 0, Mathf.Infinity);
            if(scenesToLoad != null)
                foreach(SceneToLoad sceneToLoad in scenesToLoad)
                    if(sceneToLoad.SceneToLoadType == SceneType.Int && (sceneToLoad.SceneIndex < 0 || sceneToLoad.SceneIndex >= SceneManager.sceneCountInBuildSettings))
                    {
                        Debug.LogWarning($"Scene number {sceneToLoad.SceneIndex} is invalid, resetting to 0");
                        sceneToLoad.ResetIndex();
                    }
        }

        // ----------------------------------------------------------------------------------------------------------

        #region Next State

        public void Transition()
        {
            if(IsTransitioning)
            {
                Debug.LogWarning($"Attempting to call an AppState transition on object \"{gameObject.name}\" while a transition is still occuring, new transition has been cancelled");
                return;
            }

            if(ApplicationStateManager.Instance.IsBlocked(nextState))
            {
                Debug.Log($"Application State Transition could not move on to state {nextState.name} as it is blocked");
                return;
            }
            ApplicationStateManager.Instance.StartCoroutine(NextStateCoroutine());
        }

        private IEnumerator NextStateCoroutine()
        {
            IsTransitioning = true;
            if(stateBlendSetting == StateBlendType.Blend)
                ApplicationStateManager.Instance.StartCoroutine(ApplicationStateManager.Instance.BlendIntoState(nextState, () => !IsTransitioning));

            // Pre-transition
            if(stateBlendSetting == StateBlendType.SwitchOnStart)
                nextState.SetStateActive();
            PreTransition.Invoke();
            yield return RunListOfIEnumerators(CoroutinesPreTransition);

            // Actual transition
            yield return new WaitForSecondsRealtime(beforeSceneLoadDelay);
            yield return LoadScenes();

            MidTransition.Invoke();
            yield return RunListOfIEnumerators(CoroutinesMidTransition);
            if(stateBlendSetting == StateBlendType.SwitchInMiddle)
                nextState.SetStateActive();

            yield return new WaitForSecondsRealtime(afterSceneLoadDelay);

            // Post-transition
            if(stateBlendSetting == StateBlendType.SwitchOnEnd)
                nextState.SetStateActive();
            yield return RunListOfIEnumerators(CoroutinesPostTransition);
            IsTransitioning = false;
            PostTransition.Invoke();
        }

        private IEnumerator LoadScenes()
        {
            if(scenesToLoad.Count == 0) yield break;

            //Debug.Log("ApplicationStateManager is loading a scene(s) via Transition...");
            ApplicationStateManager.Instance.SetIsLoadingScene(true);
            bool firstScene = true;
            foreach(SceneToLoad sceneToLoad in scenesToLoad)
            {
                LoadSceneMode mode = !additiveSceneLoading && firstScene ? LoadSceneMode.Single : LoadSceneMode.Additive;
                if(sceneToLoad.SceneToLoadType == SceneType.Int)
                    yield return ApplicationStateManager.Instance.LoadScene(sceneToLoad.SceneIndex, mode);
                else
                    yield return ApplicationStateManager.Instance.LoadScene(sceneToLoad.SceneName, mode);

                firstScene = false;
                yield return new WaitForEndOfFrame();
            }
            ApplicationStateManager.Instance.SetIsLoadingScene(false);
        }

        #endregion

        // ----------------------------------------------------------------------------------------------------------

        #region Misc

        public void AddScene(int index, int order = 0)
        {
            scenesToLoad.Insert(order, new SceneToLoad(index));
        }

        public void AddScene(string sceneName, int order = 0)
        {
            scenesToLoad.Insert(order, new SceneToLoad(sceneName));
        }

        private IEnumerator RunListOfIEnumerators(List<IEnumerator> list)
        {
            foreach(IEnumerator iEnum in list) yield return ApplicationStateManager.Instance.StartCoroutine(iEnum);
        }

        #endregion
    }

    // ------------------------------------------------------------------------------------------------------------------------------------------------

    internal enum SceneType
    {
        Int,
        String
    }

    [System.Serializable]
    internal class SceneToLoad
    {
        [field: SerializeField]
        public SceneType SceneToLoadType { get; private set; }
        [field: SerializeField, HideIf("@SceneToLoadType != SceneType.Int")]
        public int SceneIndex { get; private set; }
        [field: SerializeField, HideIf("@SceneToLoadType != SceneType.String")]
        public string SceneName { get; private set; }

        // ---

        public SceneToLoad(int index)
        {
            SceneToLoadType = SceneType.Int;
            SceneIndex = index;
        }

        public SceneToLoad(string sceneName)
        {
            SceneToLoadType = SceneType.String;
            SceneName = sceneName;
        }

        public void ResetIndex()
        {
            SceneIndex = 0;
        }
    }
}
