using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Sirenix.OdinInspector;
using Sparkfire.Utility;

namespace SVESimulator
{
    public class CardAnimationController : MonoBehaviour
    {
        #region Variables

        [Serializable]
        private class CardMovementData
        {
            public CardObject card;
            public bool isLocalSpace;
            public Vector3 startPos;
            public Vector3 targetPos;
            public Quaternion startRot;
            public Quaternion targetRot;
            public float startScale;
            public float? targetScale;
            public CardAnimationSettings settings;
            public float delay;
            public Action onComplete;

            public float time;

            public CardMovementData(CardObject card, bool isLocalSpace, Vector3 targetPosition, Quaternion targetRotation, float? targetScale, CardAnimationSettings animSettings, float delay, Action onComplete)
            {
                this.card = card;
                this.isLocalSpace = isLocalSpace;
                startPos = card.transform.position;
                targetPos = targetPosition;
                startRot = card.transform.rotation;
                targetRot = targetRotation;
                startScale = card.transform.localScale.x;
                this.targetScale = targetScale;
                settings = animSettings;
                this.delay = delay;
                this.onComplete = onComplete;

                time = delay * -1f;
            }
        }

        // -----

        [field: TitleGroup("Runtime Data"), SerializeField]
        private List<CardMovementData> currentMovingCards = new();

        [field: TitleGroup("Movement Settings"), SerializeField]
        public CardAnimationSettings DefaultMoveSettings { get; private set; }
        [SerializeField]
        private SerializedDictionary<CardMovementType, CardAnimationSettings> movementSettings;
        [SerializeField]
        private float cardDragSpeed = 10f;

        [FoldoutGroup("Attack Settings", true), SerializeField]
        private float attackAnimDuration = 1f;
        [FoldoutGroup("Attack Settings", true), SerializeField]
        private AnimationCurve attackAnimCurveX = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        [FoldoutGroup("Attack Settings", true), SerializeField]
        private AnimationCurve attackAnimCurveY = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        [FoldoutGroup("Attack Settings", true), SerializeField]
        private float afterAttackDelay;

        [FoldoutGroup("Stat Change Settings", true), SerializeField]
        private float statChangeAnimDuration = 1f;
        [FoldoutGroup("Stat Change Settings", true), SerializeField]
        private Vector3 statChangeAnimStartOffset;
        [FoldoutGroup("Stat Change Settings", true), SerializeField]
        private float statChangeAnimMoveDistance;
        [FoldoutGroup("Stat Change Settings", true), SerializeField]
        private AnimationCurve statChangeAnimMoveCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        [FoldoutGroup("Stat Change Settings", true), SerializeField]
        private AnimationCurve statChangeAnimFadeCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [TitleGroup("Object References"), SerializeField]
        private LineRenderer targetingLine;
        [SerializeField, AssetsOnly]
        private GameObject attackEffectPrefab;
        [SerializeField, ReadOnly, HideInEditorMode]
        private List<GameObject> attackEffectObjectPool;
        [SerializeField]
        private Transform statChangeEffectContainer;
        [SerializeField, AssetsOnly]
        private GameObject statChangeEffectPrefab;
        [SerializeField, ReadOnly, HideInEditorMode]
        private List<GameObject> statChangeEffectObjectPool;

        public bool IsAnimating => currentMovingCards.Count > 0;

        #endregion

        // ------------------------------

        #region Unity Functions

        private void Start()
        {
            SetTargetingLineActive(false);
        }

        private void Update()
        {
            for(int i = 0; i < currentMovingCards.Count; i++)
            {
                CardMovementData data = currentMovingCards[i];
                CardObject card = data.card;

                // Delay
                if(data.time < 0f)
                {
                    data.time += Time.deltaTime;
                    continue;
                }

                // End animation
                if(data.time + Time.deltaTime >= data.settings.MoveDuration)
                {
                    card.transform.SetPositionAndRotation(data.isLocalSpace ? card.transform.parent.TransformPoint(data.targetPos) : data.targetPos, data.targetRot);
                    if(data.targetScale.HasValue && !Mathf.Approximately(data.startScale, data.targetScale.Value))
                        card.transform.localScale = Vector3.one * data.targetScale.Value;
                    currentMovingCards.RemoveAt(i--);
                    card.IsAnimating = false;
                    data.onComplete?.Invoke();
                    continue;
                }

                // Move
                float t = data.time / data.settings.MoveDuration;
                Vector3 targetPos = data.isLocalSpace ? card.transform.parent.TransformPoint(data.targetPos) : data.targetPos;
                data.settings.GetLerpedPositionAndRotation(data.startPos, targetPos, out Vector3 newPos, data.startRot, data.targetRot, out Quaternion newRot, t);
                card.transform.SetPositionAndRotation(newPos, newRot);
                if(data.settings.UseAdvancedScaling || (data.targetScale.HasValue && !Mathf.Approximately(data.startScale, data.targetScale.Value)))
                    card.transform.localScale = Vector3.one * data.settings.GetLerpedScale(data.startScale, data.targetScale ?? 1f, t);

                // Increment
                data.time += Time.deltaTime;
            }
        }

        #endregion

        // ------------------------------

        #region Movement

        public void MoveCardToPosition(CardMovementType movementType, CardObject card, Vector3 targetPos, Quaternion targetRot, float? scale = null, float delay = 0f, Action onComplete = null)
            => MoveCardToPosition(movementType, card, targetPos, targetRot, out _, scale, delay, onComplete);
        public void MoveCardToPosition(CardMovementType movementType, CardObject card, Vector3 targetPos, Quaternion targetRot, out float moveTime, float? scale = null, float delay = 0f, Action onComplete = null)
        {
            CancelCardMovement(card);
            CardAnimationSettings settings = movementSettings.GetValueOrDefault(movementType, DefaultMoveSettings);
            card.IsAnimating = true;
            card.transform.Rotate(settings.InitialRotateOffset, Space.Self);
            moveTime = settings.MoveDuration;

            CardMovementData moveData = new(card, false, targetPos, targetRot, scale, settings, delay, onComplete);
            currentMovingCards.Add(moveData);
        }

        public void MoveCardToLocalPosition(CardMovementType movementType, CardObject card, Vector3 targetPos, Quaternion targetRot, float? scale = null, float delay = 0f, Action onComplete = null)
            => MoveCardToLocalPosition(movementType, card, targetPos, targetRot, out _, scale, delay, onComplete);
        public void MoveCardToLocalPosition(CardMovementType movementType, CardObject card, Vector3 targetPos, Quaternion targetRot, out float moveTime, float? scale = null, float delay = 0f, Action onComplete = null)
        {
            CancelCardMovement(card);
            CardAnimationSettings settings = movementSettings.GetValueOrDefault(movementType, DefaultMoveSettings);
            card.IsAnimating = true;
            card.transform.Rotate(settings.InitialRotateOffset, Space.Self);
            moveTime = settings.MoveDuration;

            CardMovementData moveData = new(card, true, targetPos, targetRot, scale, settings, delay, onComplete);
            currentMovingCards.Add(moveData);
        }

        public void LerpCardToPosition(CardObject card, Vector3 targetPos)
        {
            card.transform.position = Vector3.Lerp(card.transform.position, targetPos, Time.deltaTime * cardDragSpeed);
        }

        #endregion

        // ------------------------------

        #region Rotation

        public void RotateCard(CardObject card, Quaternion targetRot, Action onComplete = null)
        {
            CancelCardMovement(card);
            CardAnimationSettings settings = DefaultMoveSettings;
            card.IsAnimating = true;
            card.transform.Rotate(settings.InitialRotateOffset, Space.Self);

            CardMovementData moveData = new(card, false, card.transform.position, targetRot, null, settings, 0f, onComplete);
            currentMovingCards.Add(moveData);
        }

        #endregion

        // ------------------------------

        #region Targeting & Attack

        public void SetTargetingLineActive(bool active)
        {
            if(targetingLine)
                targetingLine.gameObject.SetActive(active);
        }

        public void SetTargetingLine(CardObject card, Vector3 endPos) => SetTargetingLine(card.transform.position, endPos);
        public void SetTargetingLine(Vector3 startPos, Vector3 endPos)
        {
            targetingLine.SetPositions(new []{ startPos + Vector3.up, endPos + Vector3.up });
        }

        // ---

        public void PlayAttackPreview(CardObject attackingCard, CardObject defendingCard)
        {
            SetTargetingLineActive(true);
            SetTargetingLine(attackingCard.transform.position, defendingCard.transform.position);
        }

        public void EndAttackPreview()
        {
            SetTargetingLineActive(false);
        }

        public void PlayAttackAnimation(CardObject attackingCard, CardObject defendingCard, Action onComplete = null)
            => PlayAttackAnimation(attackingCard.transform.position, defendingCard.transform.position, onComplete);
        public void PlayAttackAnimation(Vector3 startPos, Vector3 endPos, Action onComplete = null)
        {
            GameObject effect = GetAttackEffectObject();
            effect.transform.position = startPos;
            effect.SetActive(true);
            StartCoroutine(PerformAttackEffect());

            IEnumerator PerformAttackEffect()
            {
                Vector3 xzDelta = endPos - startPos;
                xzDelta.y = 0f;
                for(float i = 0f; i < attackAnimDuration; i += Time.deltaTime)
                {
                    float t = i / attackAnimDuration;
                    effect.transform.position = startPos + (xzDelta * attackAnimCurveX.Evaluate(t)) + new Vector3(0f, attackAnimCurveY.Evaluate(t), 0f);
                    yield return null;
                }
                effect.SetActive(false);
                yield return new WaitForSeconds(afterAttackDelay);
                onComplete?.Invoke();
            }
        }

        private GameObject GetAttackEffectObject()
        {
            GameObject obj = attackEffectObjectPool.FirstOrDefault(x => !x.activeSelf);
            if(obj)
                return obj;

            obj = Instantiate(attackEffectPrefab, Vector3.zero, Quaternion.identity, transform);
            attackEffectObjectPool.Add(obj);
            return obj;
        }

        #endregion

        // ------------------------------

        #region Stat Change

        public void PlayStatChangeAnimation(Vector3 startPosition, int amount)
        {
            if(amount == 0)
                return;

            GameObject effect = GetStatChangeEffectObject();
            effect.transform.position = startPosition + statChangeAnimStartOffset;
            effect.SetActive(true);
            TextMeshProUGUI text = effect.GetComponentInChildren<TextMeshProUGUI>();
            text.text = amount < 0 ? amount.ToString() : $"+{amount}";
            text.alpha = 1f;
            StartCoroutine(PerformStatChangeEffect());

            IEnumerator PerformStatChangeEffect()
            {
                Vector3 moveStartPos = effect.transform.position;
                for(float i = 0f; i < statChangeAnimDuration; i += Time.deltaTime)
                {
                    float t = i / statChangeAnimDuration;
                    effect.transform.position = moveStartPos + (Vector3.forward * (statChangeAnimMoveDistance * statChangeAnimMoveCurve.Evaluate(t)));
                    text.alpha = statChangeAnimFadeCurve.Evaluate(t);
                    yield return null;
                }
                effect.SetActive(false);
            }
        }

        private GameObject GetStatChangeEffectObject()
        {
            GameObject obj = statChangeEffectObjectPool.FirstOrDefault(x => !x.activeSelf);
            if(obj)
                return obj;

            obj = Instantiate(statChangeEffectPrefab, Vector3.zero, statChangeEffectContainer.rotation, statChangeEffectContainer);
            statChangeEffectObjectPool.Add(obj);
            return obj;
        }

        #endregion

        // ------------------------------

        #region Internal Controls

        private void CancelCardMovement(CardObject card)
        {
            CardMovementData moveData = currentMovingCards.FirstOrDefault(x => x.card == card);
            if(moveData != null)
            {
                card.IsAnimating = false;
                currentMovingCards.Remove(moveData);
            }
        }

        #endregion
    }
}
