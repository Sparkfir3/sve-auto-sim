using System;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SVESimulator
{
    [RequireComponent(typeof(SVEEffectPool))]
    public class EffectPoolDebugger : MonoBehaviour
    {
        [Serializable]
        private struct SerializedPassive
        {
            public string name;
            public int sourceCardInstanceId;
            public List<int> affectedCardIds;
            public SVEProperties.SVEEffectTarget target;
            public SVEProperties.PassiveDuration duration;
        }

        // ------------------------------

        [SerializeField, Required]
        private SVEEffectPool effectPool;

        [TitleGroup("Data"), SerializeField, ReadOnly]
        private List<SerializedPassive> registeredPassives = new();

        [TitleGroup("Debugging"), ShowInInspector, ReadOnly, HideInEditorMode]
        private ComplexEffect.LogMode complexEffectLogMode => ComplexEffect.CurrentLogMode;

        // ------------------------------

        [TitleGroup("Controls"), Button, DisableInEditorMode]
        public void ConfirmationTiming()
        {
            effectPool.CmdExecuteConfirmationTiming();
        }

        private void UpdateSerializedPassives()
        {
            registeredPassives.Clear();
            foreach(RegisteredPassiveAbility passive in effectPool.RegisteredPassives)
            {
                registeredPassives.Add(new SerializedPassive
                {
                    name = passive.effect.GetType().Name,
                    sourceCardInstanceId = passive.sourceCardInstanceId,
                    affectedCardIds = passive.affectedCards.Select(x => x.instanceId).ToList(),
                    target = passive.target,
                    duration = passive.duration
                });
            }
        }

#if UNITY_EDITOR
        [TitleGroup("Controls"), Button("Update Serialized Passives"), DisableInEditorMode]
        private void OnValidate()
        {
            if(!effectPool)
                effectPool = GetComponent<SVEEffectPool>();
            if(Application.isPlaying)
                UpdateSerializedPassives();
        }

        [TitleGroup("Debugging"), Button]
        private void ToggleComplexEffectLogs()
        {
            ComplexEffect.CurrentLogMode = ComplexEffect.CurrentLogMode == ComplexEffect.LogMode.None ? ComplexEffect.LogMode.All : ComplexEffect.LogMode.None;
        }
#endif
    }
}
