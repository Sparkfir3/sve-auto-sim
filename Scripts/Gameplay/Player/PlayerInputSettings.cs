using UnityEngine;
using Sirenix.OdinInspector;

namespace SVESimulator
{
    [CreateAssetMenu(menuName = "Input Settings", fileName = "InputSettings", order = 1)]
    public class PlayerInputSettings : ScriptableObject
    {
        [field: SerializeField]
        public float RaycastDistance { get; private set; } = 100f;
        [field: SerializeField]
        public LayerMask CardRaycastLayers { get; private set; } = ~0;
        [field: SerializeField]
        public LayerMask TargetSlotRaycastLayers { get; private set; } = ~0;
        [field: SerializeField]
        public LayerMask FieldRaycastLayers { get; private set; } = ~0;
        [field: SerializeField, LabelText("UI Raycast Layer")]
        public LayerMask UIRaycastLayer { get; private set; } = 16;
    }
}
