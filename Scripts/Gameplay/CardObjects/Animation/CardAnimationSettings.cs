using UnityEngine;
using Sirenix.OdinInspector;

namespace SVESimulator
{
    [CreateAssetMenu(menuName = "SVE Simulator/Card Animation Settings", fileName = "CardAnim_TYPE", order = 1)]
    public class CardAnimationSettings : ScriptableObject
    {
        [field: SerializeField]
        public float MoveDuration { get; private set; } = 0.25f;
        [field: SerializeField]
        public AnimationCurve MoveCurveXZ { get; private set; } = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        [field: SerializeField]
        public AnimationCurve MoveCurveY { get; private set; } = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [field: SerializeField]
        public Vector3 InitialRotateOffset { get; private set; }
        [field: SerializeField]
        public AnimationCurve RotateCurve { get; private set; } = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        [field: SerializeField]
        public AnimationCurve ScaleCurve { get; private set; } = AnimationCurve.Linear(0f, 1f, 1f, 1f);

        // ------------------------------

        public Vector3 GetLerpedPosition(Vector3 startPosition, Vector3 endPosition, float t)
        {
            Vector3 targetPosition = Vector3.LerpUnclamped(startPosition, endPosition, MoveCurveXZ.Evaluate(t));
            targetPosition.y = Mathf.LerpUnclamped(startPosition.y, endPosition.y, MoveCurveY.Evaluate(t));
            return targetPosition;
        }

        public Quaternion GetLerpedRotation(Quaternion startRotation, Quaternion endRotation, float t)
        {
            return Quaternion.LerpUnclamped(startRotation, endRotation, RotateCurve.Evaluate(t));
        }
    }
}
