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

        public enum SlideType { WorldSpace, MoveDirectionSpace }
        public enum DropType { Max, Start, End }

        // -----

        [field: SerializeField]
        public float MoveDuration { get; private set; } = 0.25f;

        [field: TitleGroup("Movement"), SerializeField]
        public bool UseAdvancedXZMovement { get; private set; }
        [field: SerializeField]
        public AnimationCurve MoveCurveXZ { get; private set; } = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [field: BoxGroup("Advanced XZ Movement"), SerializeField, ShowIf("UseAdvancedXZMovement"), LabelText("Slide Type")]
        public SlideType MoveSlideType { get; private set; }
        [field: BoxGroup("Advanced XZ Movement"), SerializeField, ShowIf("@UseAdvancedXZMovement && MoveSlideType == SlideType.WorldSpace")]
        public Vector3 SidewaysSlideDistanceVector { get; private set; }
        [field: BoxGroup("Advanced XZ Movement"), SerializeField, ShowIf("@UseAdvancedXZMovement && MoveSlideType == SlideType.MoveDirectionSpace")]
        public float SidewaysSlideDistanceFloat { get; private set; }
        [field: BoxGroup("Advanced XZ Movement"), SerializeField, ShowIf("UseAdvancedXZMovement")]
        private AdvancedCurve MoveSlideOutCurve { get; set; } = new(0f, 0.5f, AnimationCurve.Linear(0f, 0f, 1f, 1f));
        [field: BoxGroup("Advanced XZ Movement"), SerializeField, ShowIf("UseAdvancedXZMovement")]
        private AdvancedCurve MoveSlideInCurve { get; set; } = new(0.5f, 1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));

        // -----

        [field: SerializeField]
        public bool UseAdvancedYMovement { get; private set; }
        [field: SerializeField, HideIf("UseAdvancedYMovement")]
        public AnimationCurve MoveCurveY { get; private set; } = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [field: BoxGroup("Advanced Y Movement"), SerializeField, ShowIf("UseAdvancedYMovement"), LabelText("Drop Relative To")]
        public DropType MoveDropType { get; private set; }
        [field: BoxGroup("Advanced Y Movement"), SerializeField, ShowIf("UseAdvancedYMovement")]
        public float DropHeight { get; private set; }
        [field: BoxGroup("Advanced Y Movement"), SerializeField, ShowIf("UseAdvancedYMovement")]
        private AdvancedCurve MoveRiseCurve { get; set; } = new(0f, 0.5f, AnimationCurve.Linear(0f, 0f, 1f, 1f));
        [field: BoxGroup("Advanced Y Movement"), SerializeField, ShowIf("UseAdvancedYMovement")]
        private AdvancedCurve MoveFallCurve { get; set; } = new(0.5f, 1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));

        // -----

        [field: TitleGroup("Rotation"), SerializeField]
        public Vector3 InitialRotateOffset { get; private set; }
        [field: SerializeField]
        public AnimationCurve RotateCurve { get; private set; } = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [field: SerializeField, Tooltip("Rotates in the direction of movement, simulating the natural curve of picking up and placing a physical card.")]
        public bool ApplyPickedUpRotation { get; private set; }
        [field: BoxGroup("Advanced Rotation"), SerializeField, ShowIf("ApplyPickedUpRotation")]
        public Vector3 PickUpRiseAngle { get; private set; }
        [field: BoxGroup("Advanced Rotation"), SerializeField, ShowIf("ApplyPickedUpRotation")]
        private AdvancedCurve PickUpRiseCurve { get; set; } = new(0f, 0.5f, AnimationCurve.Linear(0f, 0f, 1f, 1f));
        [field: BoxGroup("Advanced Rotation"), SerializeField, ShowIf("ApplyPickedUpRotation")]
        public Vector3 PickUpFallAngle { get; private set; }
        [field: BoxGroup("Advanced Rotation"), SerializeField, ShowIf("ApplyPickedUpRotation")]
        private AdvancedCurve PickUpFallCurve { get; set; } = new(0.5f, 1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));

        // -----

        [field: TitleGroup("Scale"), SerializeField]
        public bool UseAdvancedScaling { get; private set; }
        [field: BoxGroup("Advanced Scaling"), SerializeField, ShowIf("UseAdvancedScaling")]
        public float ScaleMultiplier { get; private set; } = 1f;
        [field: BoxGroup("Advanced Scaling"), SerializeField, ShowIf("UseAdvancedScaling")]
        private AdvancedCurve ScaleGrowCurve { get; set; } = new(0f, 0.5f, AnimationCurve.Linear(0f, 0f, 1f, 1f));
        [field: BoxGroup("Advanced Scaling"), SerializeField, ShowIf("UseAdvancedScaling")]
        private AdvancedCurve ScaleShrinkCurve { get; set; } = new(0.5f, 1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));

        // ------------------------------

        public void GetLerpedPositionAndRotation(Vector3 startPosition, Vector3 endPosition, out Vector3 outPosition,
            Quaternion startRotation, Quaternion endRotation, out Quaternion outRotation, float t)
        {
            outPosition = GetLerpedPosition(startPosition, endPosition, t);
            outRotation = GetLerpedRotation(startRotation, endRotation, t);
            if(ApplyPickedUpRotation && t >= PickUpRiseCurve.startTime && t <= PickUpFallCurve.endTime)
            {
                Vector3 moveDirection = endPosition - startPosition;
                moveDirection = new Vector3(moveDirection.x, 0f, moveDirection.z).normalized;
                float peakHeight = !UseAdvancedYMovement ? Mathf.Max(startPosition.y, endPosition.y) : DropHeight + MoveDropType switch
                {
                    DropType.Start  => startPosition.y,
                    DropType.End    => endPosition.y,
                    _               => Mathf.Max(startPosition.y, endPosition.y)
                };
                bool isRising = UseAdvancedYMovement
                    ? t <= MoveFallCurve.startTime
                    : outPosition.y >= endPosition.y;

                AdvancedCurve targetCurve = isRising ? PickUpRiseCurve : PickUpFallCurve;
                float t2 = Mathf.InverseLerp(targetCurve.startTime, targetCurve.endTime, t);
                Quaternion tilt = Quaternion.Slerp(Quaternion.identity, Quaternion.Euler(isRising ? PickUpRiseAngle : PickUpFallAngle), t2);
                outRotation *= tilt * Quaternion.LookRotation(moveDirection, Vector3.up);
            }
        }

        public Vector3 GetLerpedPosition(Vector3 startPosition, Vector3 endPosition, float t)
        {
            // XZ Movement
            Vector3 targetPosition = Vector3.LerpUnclamped(startPosition, endPosition, MoveCurveXZ.Evaluate(t));
            if(UseAdvancedXZMovement && t >= MoveSlideOutCurve.startTime && t <= MoveSlideInCurve.endTime)
            {
                Vector3 slideDistance = SidewaysSlideDistanceVector;
                if(MoveSlideType == SlideType.MoveDirectionSpace)
                {
                    Vector3 positionDifference = endPosition - startPosition;
                    positionDifference.y = 0f;
                    slideDistance = Vector3.Cross(positionDifference.normalized, Vector3.up) * SidewaysSlideDistanceFloat;
                }

                if(t <= MoveSlideOutCurve.endTime)
                {
                    float t2 = Mathf.InverseLerp(MoveSlideOutCurve.startTime, MoveSlideOutCurve.endTime, t);
                    targetPosition += slideDistance * MoveSlideOutCurve.curve.Evaluate(t2);
                }
                else if(t <= MoveSlideInCurve.endTime)
                {
                    float t2 = Mathf.InverseLerp(MoveSlideInCurve.startTime, MoveSlideInCurve.endTime, t);
                    targetPosition += slideDistance * MoveSlideInCurve.curve.Evaluate(t2);
                }
            }

            // Y Movement
            if(UseAdvancedYMovement && DropHeight <= 0f)
            {
                targetPosition.y = Mathf.LerpUnclamped(startPosition.y, endPosition.y, MoveCurveY.Evaluate(t));
            }
            else
            {
                float peakHeight = DropHeight + MoveDropType switch
                {
                    DropType.Start  => startPosition.y,
                    DropType.End    => endPosition.y,
                    _               => Mathf.Max(startPosition.y, endPosition.y)
                };
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
            return Quaternion.SlerpUnclamped(startRotation, endRotation, RotateCurve.Evaluate(t));
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
            MoveSlideInCurve.time.x = MoveSlideOutCurve.time.y;
            MoveFallCurve.time.x = MoveRiseCurve.time.y;
            PickUpFallCurve.time.x = PickUpRiseCurve.time.y;
            ScaleShrinkCurve.time.x = ScaleGrowCurve.time.y;
        }
#endif
    }
}
