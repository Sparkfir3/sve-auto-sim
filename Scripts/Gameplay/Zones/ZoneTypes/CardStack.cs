using UnityEngine;
using Sirenix.OdinInspector;

namespace SVESimulator
{
    public class CardStack : CardZone
    {
        [SerializeField]
        private Transform faceDownBlock;
        [SerializeField]
        private Transform faceDownBlockTopFace;
        [SerializeField]
        private int currentStackHeight = -1;

        public override Quaternion CardRotation =>
            (visible ? SVEProperties.CardFaceUpRotation : SVEProperties.CardFaceDownRotation)
            * (!IsLocalPlayerZone ? SVEProperties.OpponentCardRotation : Quaternion.identity);

        // ------------------------------

        public Vector3 GetTopStackPosition()
        {
            return transform.position + Vector3.up * ((Mathf.Max(currentStackHeight, 0) + cards.Count) * SVEProperties.CardThickness);
        }

        public Vector3 GetBottomStackPosition()
        {
            return transform.position;
        }

        // ------------------------------

        public void SetStackHeight(int height)
        {
            if(!faceDownBlock || height == currentStackHeight)
                return;
            if(height <= 0)
            {
                currentStackHeight = 0;
                faceDownBlock.gameObject.SetActive(false);
                if(faceDownBlockTopFace)
                    faceDownBlockTopFace.gameObject.SetActive(false);
                return;
            }

            currentStackHeight = height;
            float heightUnits = height * SVEProperties.CardThickness;
            faceDownBlock.localScale = new Vector3(faceDownBlock.localScale.x, heightUnits, faceDownBlock.localScale.z);
            faceDownBlock.localPosition = new Vector3(0f, heightUnits / 2f, 0f);
            faceDownBlock.gameObject.SetActive(true);
            if(faceDownBlockTopFace)
            {
                faceDownBlockTopFace.localPosition = new Vector3(0f, heightUnits + 0.001f, 0f);
                faceDownBlockTopFace.gameObject.SetActive(true);
            }
        }

        public void IncrementStackHeight() => SetStackHeight(currentStackHeight + 1);
        public void DecrementStackHeight() => SetStackHeight(currentStackHeight - 1);
    }
}
