using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TitleGroup = UnityEngine.HeaderAttribute;
using FoldoutGroup = Sparkfire.Utility.BlankAttribute;
using HideIf = Sparkfire.Utility.BlankAttribute;
using ShowIf = Sparkfire.Utility.BlankAttribute;
#endif

namespace Sparkfire.AppStateSystem
{
    public enum StateMatchCase
    {
        ContainsNone, // Must have none of the given states
        ContainsOne, // Must have one of the given states
        ContainsAll, // Must have all of the given states
        MatchExactly // Must match the given states exactly
    }

    /// <summary>
    /// Place this script on the UI panel object
    /// </summary>
    public class ApplicationStateBasedActivity : MonoBehaviour
    {
        /// <summary>
        /// The states that enable this panel
        /// </summary>
        [field: TitleGroup("State Controls"), SerializeField, HideIf("@activeStates.Count == 0")]
        [field: Tooltip("Match case required for activeStates")]
        public StateMatchCase ActiveMatchCase { get; private set; } = StateMatchCase.ContainsOne;
        [TitleGroup("State Controls"), SerializeField, ShowIf("@activeStates.Count == 0")]
        [Tooltip("If no active states are given, whether or not the activity is active by default (and only disabled by inactive states)")]
        private bool activeByDefault;
        [SerializeField]
        [Tooltip("The states that enable this panel")]
        private List<ApplicationState> activeStates;

        [field: SerializeField, HideIf("@inactiveStates.Count == 0")]
        [field: Tooltip("Match case required for inactiveStates")]
        public StateMatchCase InactiveMatchCase { get; private set; } = StateMatchCase.ContainsOne;
        [SerializeField]
        [Tooltip("The states that disable this panel")]
        private List<ApplicationState> inactiveStates;

        // ---

        [TitleGroup("Settings"), SerializeField]
        private bool controlSelfActivity = true;
        [SerializeField]
        private bool showDebugLogs = false;
        [SerializeField, FoldoutGroup("Unity Events")]
        public UnityEngine.Events.UnityEvent OnInitialize;
        [SerializeField, FoldoutGroup("Unity Events")]
        public UnityEngine.Events.UnityEvent OnEnter;
        [SerializeField, FoldoutGroup("Unity Events")]
        public UnityEngine.Events.UnityEvent OnExit;

        // ---

        private bool initialized = false;
        private bool isActive = false;

        // ----------------------------------------------------------------------------------------------------------

        private void Awake()
        {
            Initialize();
        }

        private void OnDestroy()
        {
            if(ApplicationStateManager.Instance)
                ApplicationStateManager.Instance.OnStateChanged -= CheckState;
        }

        // ----------------------------------------------------------------------------------------------------------

        public bool Initialize()
        {
            if(initialized)
                return false;
            initialized = true;

            OnEnter.AddListener(() =>
            {
                if(showDebugLogs)
                    Debug.Log($"ApplicationStateBasedActivity \"{gameObject.name}\" <color=#00ff00>activated</color>");
            });
            OnExit.AddListener(() =>
            {
                if(showDebugLogs)
                    Debug.Log($"ApplicationStateBasedActivity \"{gameObject.name}\" <color=#ff0000>deactivated</color>");
            });

            if(controlSelfActivity)
            {
                OnEnter.AddListener(() => { gameObject.SetActive(true); });
                OnExit.AddListener(() => { gameObject.SetActive(false); });
            }

            isActive = ShouldEnter();
            if(isActive)
                OnEnter.Invoke();
            else
                OnExit.Invoke();

            ApplicationStateManager.Instance.OnStateChanged += CheckState;
            OnInitialize?.Invoke();
            return true;
        }

        // ----------------------------------------------------------------------------------------------------------

        private void CheckState()
        {
            bool shouldEnter = ShouldEnter();
            if(shouldEnter == isActive)
                return;
            isActive = shouldEnter;

            if(shouldEnter)
                OnEnter.Invoke();
            else
                OnExit.Invoke();
        }

        private bool ShouldEnter()
        {
            if(inactiveStates.Count > 0)
                switch(InactiveMatchCase)
                {
                    case StateMatchCase.ContainsNone:
                        if(!ApplicationStateManager.Instance.HasAMatchingState(inactiveStates))
                            return false;
                        break;
                    case StateMatchCase.ContainsOne:
                        if(ApplicationStateManager.Instance.HasAMatchingState(inactiveStates))
                            return false;
                        break;
                    case StateMatchCase.ContainsAll:
                        if(ApplicationStateManager.Instance.AreAllStatesActive(inactiveStates))
                            return false;
                        break;
                    case StateMatchCase.MatchExactly:
                        if(ApplicationStateManager.Instance.AreAllStatesActive(inactiveStates, true))
                            return false;
                        break;
                }

            if(activeByDefault && activeStates.Count == 0)
                return true;

            return ActiveMatchCase switch
            {
                StateMatchCase.ContainsNone => !ApplicationStateManager.Instance.HasAMatchingState(activeStates),
                StateMatchCase.ContainsOne => ApplicationStateManager.Instance.HasAMatchingState(activeStates),
                StateMatchCase.ContainsAll => ApplicationStateManager.Instance.AreAllStatesActive(activeStates),
                StateMatchCase.MatchExactly => ApplicationStateManager.Instance.AreAllStatesActive(activeStates, true),
                _ => false
            };
        }
    }
}
