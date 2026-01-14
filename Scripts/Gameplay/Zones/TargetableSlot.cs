using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Serialization;

namespace SVESimulator
{
    public class TargetableSlot : MonoBehaviour
    {
        public enum InteractionType { None = -1, PlayCard = 0, AttackCard = 1, AttackLeader = 2, PlaySpell = 3, MoveCard = 4 }

        [field: TitleGroup("Settings"), SerializeField]
        public CardZone ParentZone { get; private set; }
        [field: SerializeField, ShowIf("@ParentZone is CardPositionedZone")]
        public int SlotNumber { get; private set; } = -1;
        [field: SerializeField, LabelText("Interaction Type")]
        public InteractionType CurrentInteractionType { get; set; }
        [SerializeField]
        public GameObject background;

        [ShowInInspector, ReadOnly]
        public CardObject Card => (ParentZone is CardPositionedZone zone) ? zone.GetCard(SlotNumber) : null;

        // ------------------------------

        #region Unity Messages

        private void Start()
        {
            if(!ParentZone)
            {
                ParentZone = GetComponentInParent<CardZone>();
                if(ParentZone && ParentZone is CardPositionedZone zone)
                    SlotNumber = zone.GetSlotNumber(this);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if(!ParentZone)
                ParentZone = GetComponentInParent<CardZone>();
            if(!Application.isPlaying && ParentZone is CardPositionedZone zone)
                SlotNumber = zone.GetSlotNumber(this);
        }
#endif

        #endregion

        // ------------------------------

        #region Other

        public void SetBackgroundActive(bool active)
        {
            background.SetActive(active);
        }

        public void OnHoverBegin()
        {

        }

        public void OnHoverEnd()
        {

        }

        #endregion
    }
}
