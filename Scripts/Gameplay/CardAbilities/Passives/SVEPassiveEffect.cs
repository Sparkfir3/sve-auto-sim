using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using CCGKit;

namespace SVESimulator
{
    public abstract class SvePassiveEffect : Effect
    {
        [EnumField("Duration"), Order(10)]
        public SVEProperties.PassiveDuration duration;

        public abstract void ApplyPassive(RuntimeCard card, PlayerController player);
        public abstract void RemovePassive(RuntimeCard card, PlayerController player);
    }
}
