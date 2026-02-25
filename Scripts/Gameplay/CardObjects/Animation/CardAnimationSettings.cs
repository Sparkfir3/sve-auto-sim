using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace SVESimulator
{
    [CreateAssetMenu(menuName = "SVE Simulator/Card Animation Settings", fileName = "CardAnim_TYPE", order = 1)]
    public class CardAnimationSettings : ScriptableObject
    {
        [System.Serializable]
        private class AdvancedCurve
        {
            [SerializeField, MinMaxSlider(0f, 1f, true)]
            public Vector2 time ;
            public AnimationCurve curve;

            public float startTime => time.x;
            public float endTime => time.y;

            public AdvancedCurve(float startTime, float endTime, AnimationCurve curve)
            {
                time = new Vector2(startTime, endTime);
                this.curve = curve;
            }
        }

        [field: SerializeField]
        public float MoveDuration { get; private set; } = 0.25f;
        [field: TitleGroup("Movement"), SerializeField]
        public AnimationCurve MoveCurveXZ { get; private set; } = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [field: SerializeField]
        public bool UseAdvancedYMovement { get; private set; }
        [field: SerializeField, HideIf("UseAdvancedYMovement")]
        public AnimationCurve MoveCurveY { get; private set; } = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        [field: BoxGroup("Advanced Y Movement"), SerializeField, ShowIf("UseAdvancedYMovement")]
        public float DropHeight { get; private set; }
        [field: BoxGroup("Advanced Y Movement"), SerializeField, ShowIf("UseAdvancedYMovement")]
        private AdvancedCurve MoveRiseCurve { get; set; } = new(0f, 0.5f, AnimationCurve.Linear(0f, 0f, 1f, 1f));
        [field: BoxGroup("Advanced Y Movement"), SerializeField, ShowIf("UseAdvancedYMovement")]
        private AdvancedCurve MoveFallCurve { get; set; } = new(0.5f, 1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));

        [field: TitleGroup("Rotation"), SerializeField]
        public Vector3 InitialRotateOffset { get; private set; }
        [field: SerializeField]
        public AnimationCurve RotateCurve { get; private set; } = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [field: TitleGroup("Scale"), SerializeField]
        public bool UseAdvancedScaling { get; private set; }
        [field: BoxGroup("Advanced Scaling"), SerializeField, ShowIf("UseAdvancedScaling")]
        public float ScaleMultiplier { get; private set; } = 1f;
        [field: BoxGroup("Advanced Scaling"), SerializeField, ShowIf("UseAdvancedScaling")]
        private AdvancedCurve ScaleGrowCurve { get; set; } = new(0f, 0.5f, AnimationCurve.Linear(0f, 0f, 1f, 1f));
        [field: BoxGroup("Advanced Scaling"), SerializeField, ShowIf("UseAdvancedScaling")]
        private AdvancedCurve ScaleShrinkCurve { get; set; } = new(0.5f, 1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));

        // ------------------------------

        public Vector3 GetLerpedPosition(Vector3 startPosition, Vector3 endPosition, float t)
        {
            Vector3 targetPosition = Vector3.LerpUnclamped(startPosition, endPosition, MoveCurveXZ.Evaluate(t));
            if(UseAdvancedYMovement && DropHeight <= 0f)
            {
                targetPosition.y = Mathf.LerpUnclamped(startPosition.y, endPosition.y, MoveCurveY.Evaluate(t));
            }
            else
            {
                float peakHeight = Mathf.Max(startPosition.y, endPosition.y) + DropHeight;
                if(t < MoveRiseCurve.startTime)
                    targetPosition.y = startPosition.y;
                else if(t <= MoveRiseCurve.endTime)
                {
                    t = Mathf.InverseLerp(MoveRiseCurve.startTime, MoveRiseCurve.endTime, t);
                    targetPosition.y = Mathf.Lerp(startPosition.y, peakHeight, MoveRiseCurve.curve.Evaluate(t));
                }
                else if(t <= MoveFallCurve.endTime)
                {
                    t = Mathf.InverseLerp(MoveFallCurve.startTime, MoveFallCurve.endTime, t);
                    targetPosition.y = Mathf.Lerp(endPosition.y, peakHeight, MoveFallCurve.curve.Evaluate(t));
                }
                else
                    targetPosition.y = endPosition.y;
            }

            return targetPosition;
        }

        public Quaternion GetLerpedRotation(Quaternion startRotation, Quaternion endRotation, float t)
        {
            return Quaternion.LerpUnclamped(startRotation, endRotation, RotateCurve.Evaluate(t));
        }

        public float GetLerpedScale(float startScale, float endScale, float t)
        {
            if(!UseAdvancedScaling)
                return Mathf.Lerp(startScale, endScale, t);
            float peakScale = Mathf.Max(startScale, endScale) * ScaleMultiplier;

            if(t <= ScaleGrowCurve.startTime)
                return startScale;
            if(t <= ScaleGrowCurve.endTime)
            {
                t = Mathf.InverseLerp(ScaleGrowCurve.startTime, ScaleGrowCurve.endTime, t);
                return Mathf.Lerp(startScale, peakScale, ScaleGrowCurve.curve.Evaluate(t));
            }
            if(t <= ScaleShrinkCurve.endTime)
            {
                t = Mathf.InverseLerp(ScaleShrinkCurve.startTime, ScaleShrinkCurve.endTime, t);
                return Mathf.Lerp(endScale, peakScale, ScaleShrinkCurve.curve.Evaluate(t));
            }
            return endScale;
        }

        // ------------------------------

#if UNITY_EDITOR
        private void OnValidate()
        {
            MoveFallCurve.time.x = MoveRiseCurve.time.y;
            ScaleShrinkCurve.time.x = ScaleGrowCurve.time.y;
        }
#endif
    }
}
