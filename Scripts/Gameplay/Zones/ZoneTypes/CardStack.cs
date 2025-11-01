using UnityEngine;
using Sirenix.OdinInspector;

namespace SVESimulator
{
    public class CardStack : CardZone
    {
        public override Quaternion CardRotation =>
            (visible ? SVEProperties.CardFaceUpRotation : SVEProperties.CardFaceDownRotation)
            * (!IsLocalPlayerZone ? SVEProperties.OpponentCardRotation : Quaternion.identity);

        // ------------------------------

        public Vector3 GetTopStackPosition()
        {
            return transform.position + Vector3.up * (cards.Count * SVEProperties.CardThickness);
        }

        public Vector3 GetBottomStackPosition()
        {
            return transform.position;
        }

        // ------------------------------

        public void SetStackHeight(int height)
        {
            // TODO
        }
    }
}
