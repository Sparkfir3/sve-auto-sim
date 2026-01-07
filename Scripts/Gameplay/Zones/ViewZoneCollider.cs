using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;

namespace SVESimulator
{
    public class ViewZoneCollider : MonoBehaviour
    {
        [SerializeField]
        private CardZone zone;

        // ------------------------------

        [Button]
        public void ViewZone()
        {
            if(!zone || zone.AllCards.Count == 0 || !zone.IsLocalPlayerZone)
                return;
            zone.ZoneController.selectionArea.Enable(CardSelectionArea.SelectionMode.ViewCardsCemetery, zone.AllCards.Count, zone.AllCards.Count, slotBackgroundsActive: false);
            zone.ZoneController.selectionArea.AddCemetery();
            zone.ZoneController.selectionArea.SetConfirmAction("", "Close", "Viewing Cemetery", 1, 0,
                _ =>
                {
                    zone.ZoneController.selectionArea.Disable();
                }, showTargetingToOpponent: false);
        }

        private void Awake()
        {
            if(!zone || !zone.IsLocalPlayerZone)
                gameObject.SetActive(false);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if(!zone)
                zone = GetComponentInParent<CardZone>();
        }
#endif
    }
}
