using DG.Tweening;
using UnityEngine;
using Sirenix.OdinInspector;

namespace SVESimulator
{
    [RequireComponent(typeof(Collider))]
    public class HandPreviewController : MonoBehaviour
    {
        [TitleGroup("Settings"), SerializeField]
        private float volumeMoveDistance;
        [SerializeField]
        private float handMoveDistance;
        [SerializeField]
        private float moveTime;
        [SerializeField]
        private Vector3 moveDirection = Vector3.forward;

        [TitleGroup("Object References"), SerializeField]
        private Transform handZone;

        private Vector3 volumeStartPosition;
        private Vector3 handStartPosition;

        // ------------------------------

        private void Awake()
        {
            volumeStartPosition = transform.position;
            handStartPosition = handZone.position;
            moveDirection.Normalize();
        }

        // ------------------------------

        public void OnMouseEnter()
        {
            MoveObject(transform, volumeStartPosition + moveDirection * volumeMoveDistance);
            MoveObject(handZone, handStartPosition + moveDirection * handMoveDistance);
        }

        public void OnMouseExit()
        {
            MoveObject(transform, volumeStartPosition);
            MoveObject(handZone, handStartPosition);
        }

        // ------------------------------

        private void MoveObject(Transform obj, Vector3 targetPos)
        {
            if(obj.DOKill() > 1)
            {
                Debug.LogWarning($"HandPreviewController killed more than 1 tween on object {obj.gameObject.name}, this shouldn't happen!");
            }
            obj.DOMove(targetPos, moveTime);
        }

    }
}
