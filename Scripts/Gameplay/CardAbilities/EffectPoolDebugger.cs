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
            public int sourceCardInstanceId;
            public string targetsFormula;
            public List<int> affectedCardIds;
            public SVEProperties.SVEEffectTarget target;
            public SVEProperties.PassiveDuration duration;
        }

        // ------------------------------

        [SerializeField, Required]
        private SVEEffectPool effectPool;

        [Title("Data"), SerializeField, ReadOnly]
        private List<SerializedPassive> registeredPassives = new();
        
        // ------------------------------

        private void UpdatePassives()
        {
            registeredPassives.Clear();
            foreach(RegisteredPassiveAbility passive in effectPool.RegisteredPassives)
            {
                registeredPassives.Add(new SerializedPassive
                {
                    sourceCardInstanceId = passive.sourceCardInstanceId,
                    targetsFormula = passive.targetsFormula,
                    affectedCardIds = passive.affectedCards.Select(x => x.instanceId).ToList(),
                    target = passive.target,
                    duration = passive.duration
                });
            }
        }
        
#if UNITY_EDITOR
        [Button("Update"), DisableInEditorMode]
        private void OnValidate()
        {
            if(!effectPool)
                effectPool = GetComponent<SVEEffectPool>();
            if(Application.isPlaying)
                UpdatePassives();
        }
#endif
    }
}
