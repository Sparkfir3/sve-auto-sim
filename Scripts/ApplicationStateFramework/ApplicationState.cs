using System;
using System.Collections.Generic;
using UnityEngine;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TitleGroup = UnityEngine.HeaderAttribute;
using ReadOnly = Sparkfire.Utility.BlankAttribute;
using LabelText = Sparkfire.Utility.BlankAttribute;
using FoldoutGroup = Sparkfire.Utility.BlankAttribute;
using HideIf = Sparkfire.Utility.BlankAttribute;
using ShowIf = Sparkfire.Utility.BlankAttribute;
#endif

namespace Sparkfire.AppStateSystem
{
    public enum TimeScaleControlOptions
    {
        None,
        Set,
        Revert
    }

    // ---------------------------------------------------------------------------------------------------------

    [CreateAssetMenu(menuName = "Application State", fileName = "Application State")]
    public class ApplicationState : ScriptableObject
    {
        [field: ShowInInspector, ReadOnly, HideInEditorMode]
        [field: Tooltip("Controlled and used by the ApplicationStateManager, and is managed only at runtime. Should not be used anywhere else.")]
        public int StateID { get; set; } = -1;

        // ---

        [field: TitleGroup("State Interactions"), SerializeField]
        [field: Tooltip("Requires all of the following states in order to be active")]
        public List<ApplicationState> SubstateOf { get; private set; }
        [field: SerializeField]
        [field: Tooltip("Cannot be set if any of these states are active")]
        public List<ApplicationState> BlockedBy { get; private set; }

        #region Override

        [field: SerializeField, FoldoutGroup("Override Controls")]
        [field: Tooltip("If true, does not override any state, except for those in the AlwaysOverride list")]
        public bool DoNotOverrideAny { get; private set; }
        [field: SerializeField, LabelText("Can\'t Override By Default"), FoldoutGroup("Override Controls")]
        [field: Tooltip("Whether or not the state can be overriden by default. Can still be overwritten by AlwaysOverride")]
        public bool CantOverrideByDefault { get; private set; }

        [field: SerializeField, HideIf("@DoNotOverrideAny"), FoldoutGroup("Override Controls")]
        [field: Tooltip("Does not override the selected states")]
        public List<ApplicationState> DoNotOverride { get; private set; }
        [field: SerializeField, FoldoutGroup("Override Controls")]
        [field: Tooltip("States to always override, even if CantOverrideByDefault is true")]
        public List<ApplicationState> AlwaysOverride { get; private set; }

        #endregion

        // ---

        [field: TitleGroup("Time Scale"), SerializeField]
        public bool ControlTimeScale { get; private set; } = false;

        [field: SerializeField, ShowIf("@ControlTimeScale"), FoldoutGroup("Time Scale Options")]
        public TimeScaleControlOptions UpdateTimeScaleOnEnable { get; private set; } = TimeScaleControlOptions.Set;
        [field: SerializeField, ShowIf("@ControlTimeScale && UpdateTimeScaleOnEnable == TimeScaleControlOptions.Set"), FoldoutGroup("Time Scale Options")]
        public float TimeScaleOnEnable { get; private set; } = 0f;

        [field: SerializeField, ShowIf("@ControlTimeScale"), FoldoutGroup("Time Scale Options")]
        public TimeScaleControlOptions UpdateTimeScaleOnDisable { get; private set; } = TimeScaleControlOptions.None;
        [field: SerializeField, ShowIf("@ControlTimeScale && UpdateTimeScaleOnDisable == TimeScaleControlOptions.Set"), FoldoutGroup("Time Scale Options")]
        public float TimeScaleOnDisable { get; private set; } = 1f;

        // ---

        [field: TitleGroup("Misc"), SerializeField]
        public bool LockCursor { get; private set; } = false;

        // ----------------------------------------------------------------------------------------------------------

        private void OnValidate()
        {
            if(!DoNotOverrideAny)
                foreach(ApplicationState state in SubstateOf)
                    if(!DoNotOverride.Contains(state))
                        DoNotOverride.Add(state);
        }

        // ----------------------------------------------------------------------------------------------------------

        public void SetStateActive()
        {
            if(!IsStateActive)
                ApplicationStateManager.Instance.SetStateActive(this);
        }

        public void SetStateInactive()
        {
            if(IsStateActive)
                ApplicationStateManager.Instance.SetStateInactive(this);
        }

        public void ToggleState()
        {
            ApplicationStateManager.Instance.ToggleState(this);
        }

        public bool IsStateActive => ApplicationStateManager.Instance.IsStateActive(this);
    }
}
