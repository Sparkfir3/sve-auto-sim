using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;

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

        [TitleGroup("Movement Settings"), SerializeField]
        private float cardMoveSpeed = 50f;
        [SerializeField]
        private AnimationCurve cardMoveCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        [SerializeField]
        private float cardLerpedMoveSpeed = 10f;
        [FoldoutGroup("Attack Settings", true), SerializeField]
        private float attackAnimDuration = 1f;
        [FoldoutGroup("Attack Settings", true), SerializeField]
        private AnimationCurve attackAnimCurveX = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        [FoldoutGroup("Attack Settings", true), SerializeField]
        private AnimationCurve attackAnimCurveY = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [TitleGroup("Timing Settings"), SerializeField]
        private float afterAttackDelay;

        [TitleGroup("Object References"), SerializeField]
        private LineRenderer targetingLine;
        [SerializeField, AssetsOnly]
        private GameObject attackEffectPrefab;
        [SerializeField, ReadOnly]
        private List<GameObject> attackEffectObjectPool;

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

        public void MoveCardToPosition(CardObject card, Vector3 targetPos, Action onComplete = null) => MoveCardToPosition(card, targetPos, card.transform.rotation, out _);
        public void MoveCardToPosition(CardObject card, Vector3 targetPos, out float moveTime, Action onComplete = null) => MoveCardToPosition(card, targetPos, card.transform.rotation, out moveTime, onComplete);
        public void MoveCardToPosition(CardObject card, Vector3 targetPos, Quaternion targetRot, Action onComplete = null) => MoveCardToPosition(card, targetPos, targetRot, out _, onComplete);
        public void MoveCardToPosition(CardObject card, Vector3 targetPos, Quaternion targetRot, out float moveTime, Action onComplete = null)
        {
            CancelCardMovement(card);
            card.IsAnimating = true;

            Vector3 startPos = card.transform.position;
            Quaternion startRot = card.transform.rotation;
            float distance = (startPos - targetPos).magnitude;
            moveTime = distance / cardMoveSpeed;
            float duration = moveTime;
            currentMovingCardsData.Add(card, new CardMovementData(StartCoroutine(MoveCardCoroutine())));

            IEnumerator MoveCardCoroutine()
            {
                for(float i = 0f; i < duration; i += Time.deltaTime)
                {
                    float t = cardMoveCurve.Evaluate(i / duration);
                    Vector3 newPosition = Vector3.Lerp(startPos, targetPos, t);
                    Quaternion newRotation = Quaternion.Lerp(startRot, targetRot, t);
                    card.transform.SetPositionAndRotation(newPosition, newRotation);
                    yield return null;
                }
                card.transform.SetPositionAndRotation(targetPos, targetRot);
                card.IsAnimating = false;
                currentMovingCardsData.Remove(card);
                onComplete?.Invoke();
            }
        }

        public void MoveCardToLocalPosition(CardObject card, Vector3 targetPos, Action onComplete = null) => MoveCardToLocalPosition(card, targetPos, card.transform.rotation, out _);
        public void MoveCardToLocalPosition(CardObject card, Vector3 targetPos, out float moveTime, Action onComplete = null) => MoveCardToLocalPosition(card, targetPos, card.transform.rotation, out moveTime, onComplete);
        public void MoveCardToLocalPosition(CardObject card, Vector3 targetPos, Quaternion targetRot, Action onComplete = null) => MoveCardToLocalPosition(card, targetPos, targetRot, out _, onComplete);
        public void MoveCardToLocalPosition(CardObject card, Vector3 targetPos, Quaternion targetRot, out float moveTime, Action onComplete = null)
        {
            CancelCardMovement(card);
            card.IsAnimating = true;

            Vector3 startPos = card.transform.position;
            Quaternion startRot = card.transform.rotation;
            float distance = (startPos - targetPos).magnitude;
            moveTime = distance / cardMoveSpeed;
            float duration = moveTime;
            currentMovingCardsData.Add(card, new CardMovementData(StartCoroutine(MoveCardCoroutine())));

            IEnumerator MoveCardCoroutine()
            {
                for(float i = 0f; i < duration; i += Time.deltaTime)
                {
                    float t = cardMoveCurve.Evaluate(i / duration);
                    Vector3 newPosition = Vector3.Lerp(startPos, card.transform.parent.TransformPoint(targetPos), t);
                    Quaternion newRotation = Quaternion.Lerp(startRot, targetRot, t);
                    card.transform.SetPositionAndRotation(newPosition, newRotation);
                    yield return null;
                }
                card.transform.SetPositionAndRotation(card.transform.parent.TransformPoint(targetPos), targetRot);
                card.IsAnimating = false;
                currentMovingCardsData.Remove(card);
                onComplete?.Invoke();
            }
        }

        public void LerpCardToPosition(CardObject card, Vector3 targetPos)
        {
            card.transform.position = Vector3.Lerp(card.transform.position, targetPos, Time.deltaTime * cardLerpedMoveSpeed);
        }

        #endregion

        // ------------------------------

        #region Rotation

        public void RotateCard(CardObject card, Quaternion targetRot, Action onComplete = null)
        {
            CancelCardMovement(card);
            card.IsAnimating = true;

            float duration = 0.2f; // TODO - properly find what this should be or make it a setting
            Quaternion startRot = card.transform.rotation;
            currentMovingCardsData.Add(card, new CardMovementData(StartCoroutine(RotateCardCoroutine())));

            IEnumerator RotateCardCoroutine()
            {
                for(float i = 0f; i < duration; i += Time.deltaTime)
                {
                    float t = cardMoveCurve.Evaluate(i / duration);
                    Quaternion newRotation = Quaternion.Lerp(startRot, targetRot, t);
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

        #region Targeting

        public void SetTargetingLineActive(bool active)
        {
            targetingLine.gameObject.SetActive(active);
        }

        public void SetTargetingLine(CardObject card, Vector3 endPos) => SetTargetingLine(card.transform.position, endPos);
        public void SetTargetingLine(Vector3 startPos, Vector3 endPos)
        {
            targetingLine.SetPositions(new []{ startPos + Vector3.up, endPos + Vector3.up });
        }

        #endregion

        // ------------------------------

        #region Attack

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
