using UnityEngine;
using Sirenix.OdinInspector;

namespace SVESimulator
{
    [CreateAssetMenu(menuName = "SVE Simulator/Card Stat Color Settings", fileName = "CardStatColorSettings", order = 1)]
    public class CardStatColorSettings : ScriptableObject
    {
        [field: Title("Colors"), SerializeField]
        public Color32 StatBaseColor { get; private set; } = Color.white;
        [field: SerializeField]
        public Color32 StatBuffedColor { get; private set; } = Color.green;
        [field: SerializeField]
        public Color32 StatDownColor { get; private set; } = Color.red;

        [field: Title("Materials"), SerializeField]
        public Material BlackOutlineMaterial { get; private set; }
        [field: SerializeField]
        public Material WhiteOutlineMaterial { get; private set; }
    }
}
