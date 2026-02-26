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

        private struct CardMovementData
        {
            public Coroutine coroutine;

            public CardMovementData(Coroutine coroutine)
            {
                this.coroutine = coroutine;
            }
        }

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

        private Dictionary<CardObject, CardMovementData> currentMovingCardsData = new();

        #endregion

        // ------------------------------

        #region Unity Functions

        private void Start()
        {
            SetTargetingLineActive(false);
        }

        #endregion

        // ------------------------------

        #region Movement

        public void MoveCardToPosition(CardMovementType movementType, CardObject card, Vector3 targetPos, Quaternion targetRot, float? scale = null, Action onComplete = null)
            => MoveCardToPosition(movementType, card, targetPos, targetRot, out _, scale, onComplete);
        public void MoveCardToPosition(CardMovementType movementType, CardObject card, Vector3 targetPos, Quaternion targetRot, out float moveTime, float? scale = null, Action onComplete = null)
        {
            CancelCardMovement(card);
            card.IsAnimating = true;

            CardAnimationSettings settings = movementSettings.GetValueOrDefault(movementType, DefaultMoveSettings);
            card.transform.Rotate(settings.InitialRotateOffset, Space.Self);

            Vector3 startPos = card.transform.position;
            Quaternion startRot = card.transform.rotation;
            float startScale = card.transform.localScale.x;
            moveTime = settings.MoveDuration;
            float duration = moveTime; // re-assign to keep value from out var in coroutine
            currentMovingCardsData.Add(card, new CardMovementData(StartCoroutine(MoveCardCoroutine())));

            IEnumerator MoveCardCoroutine()
            {
                for(float i = 0f; i < duration; i += Time.deltaTime)
                {
                    float t = i / duration;
                    settings.GetLerpedPositionAndRotation(startPos, targetPos, out Vector3 newPosition, startRot, targetRot, out Quaternion newRotation, t);
                    card.transform.SetPositionAndRotation(newPosition, newRotation);
                    if(settings.UseAdvancedScaling || (scale.HasValue && !Mathf.Approximately(startScale, scale.Value)))
                        card.transform.localScale = Vector3.one * settings.GetLerpedScale(startScale, scale ?? 1f, t);
                    yield return null;
                }
                card.transform.SetPositionAndRotation(targetPos, targetRot);
                if(scale.HasValue && !Mathf.Approximately(startScale, scale.Value))
                    card.transform.localScale = Vector3.one * scale.Value;
                card.IsAnimating = false;
                currentMovingCardsData.Remove(card);
                onComplete?.Invoke();
            }
        }

        public void MoveCardToLocalPosition(CardMovementType movementType, CardObject card, Vector3 targetPos, Quaternion targetRot, float? scale = null, Action onComplete = null)
            => MoveCardToLocalPosition(movementType, card, targetPos, targetRot, out _, scale, onComplete);
        public void MoveCardToLocalPosition(CardMovementType movementType, CardObject card, Vector3 targetPos, Quaternion targetRot, out float moveTime, float? scale = null, Action onComplete = null)
        {
            CancelCardMovement(card);
            card.IsAnimating = true;

            CardAnimationSettings settings = movementSettings.GetValueOrDefault(movementType, DefaultMoveSettings);
            card.transform.Rotate(settings.InitialRotateOffset, Space.Self);

            Vector3 startPos = card.transform.position;
            Quaternion startRot = card.transform.rotation;
            float startScale = card.transform.localScale.x;
            moveTime = settings.MoveDuration;
            float duration = moveTime; // re-assign to keep value from out var in coroutine
            currentMovingCardsData.Add(card, new CardMovementData(StartCoroutine(MoveCardCoroutine())));

            IEnumerator MoveCardCoroutine()
            {
                for(float i = 0f; i < duration; i += Time.deltaTime)
                {
                    float t = i / duration;
                    settings.GetLerpedPositionAndRotation(startPos, card.transform.parent.TransformPoint(targetPos), out Vector3 newPosition, startRot, targetRot, out Quaternion newRotation, t);
                    card.transform.SetPositionAndRotation(newPosition, newRotation);
                    if(settings.UseAdvancedScaling || (scale.HasValue && !Mathf.Approximately(startScale, scale.Value)))
                        card.transform.localScale = Vector3.one * settings.GetLerpedScale(startScale, scale ?? 1f, t);
                    yield return null;
                }
                card.transform.SetPositionAndRotation(card.transform.parent.TransformPoint(targetPos), targetRot);
                if(scale.HasValue && !Mathf.Approximately(startScale, scale.Value))
                    card.transform.localScale = Vector3.one * scale.Value;
                card.IsAnimating = false;
                currentMovingCardsData.Remove(card);
                onComplete?.Invoke();
            }
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
            card.IsAnimating = true;

            CardAnimationSettings settings = DefaultMoveSettings;
            float duration = settings.MoveDuration;
            Quaternion startRot = card.transform.rotation;
            currentMovingCardsData.Add(card, new CardMovementData(StartCoroutine(RotateCardCoroutine())));

            IEnumerator RotateCardCoroutine()
            {
                for(float i = 0f; i < duration; i += Time.deltaTime)
                {
                    float t = i / duration;
                    Quaternion newRotation = settings.GetLerpedRotation(startRot, targetRot, t);
                    card.transform.rotation = newRotation;
                    yield return null;
                }
                card.transform.rotation = targetRot;
                card.IsAnimating = false;
                currentMovingCardsData.Remove(card);
                onComplete?.Invoke();
            }
        }

        #endregion

        // ------------------------------

        #region Targeting & Attack

        public void SetTargetingLineActive(bool active)
        {
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
            if(currentMovingCardsData.TryGetValue(card, out CardMovementData movementData))
            {
                if(movementData.coroutine != null)
                    StopCoroutine(movementData.coroutine);
                card.IsAnimating = false;
                currentMovingCardsData.Remove(card);
            }
        }

        #endregion
    }
}
