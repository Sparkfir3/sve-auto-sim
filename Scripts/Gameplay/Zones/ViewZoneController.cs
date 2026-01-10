using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using SVESimulator.UI;

namespace SVESimulator
{
    public class ViewZoneController : MonoBehaviour
    {
        [SerializeField]
        private CardZone zone;

        private bool isOverlayingSelectionArea;

        // ------------------------------

        [Button]
        public void ViewZone()
        {
            if(!zone || zone.AllCards.Count == 0 || !zone.IsLocalPlayerZone)
                return;
            if(zone.ZoneController.selectionArea.IsActive)
            {
                isOverlayingSelectionArea = true;
                zone.ZoneController.selectionArea.transform.position += Vector3.down * 10f;
            }
            zone.ZoneController.zoneViewingArea.Enable(CardSelectionArea.SelectionMode.ViewCardsCemetery, zone.AllCards.Count, zone.AllCards.Count, slotBackgroundsActive: false);
            zone.ZoneController.zoneViewingArea.AddCemetery();
            GameUIManager.ViewingZone.Open("Viewing Cemetery", Close);
        }

        public void Close()
        {
            if(isOverlayingSelectionArea)
            {
                isOverlayingSelectionArea = false;
                zone.ZoneController.selectionArea.transform.position += Vector3.up * 10f;
            }
            zone.ZoneController.zoneViewingArea.Disable();
            GameUIManager.ViewingZone.Close();
        }

        // ------------------------------

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
