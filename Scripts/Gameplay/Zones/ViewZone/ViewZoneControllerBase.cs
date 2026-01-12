using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using SVESimulator.UI;

namespace SVESimulator
{
    public abstract class ViewZoneControllerBase : MonoBehaviour
    {
        [SerializeField]
        protected CardZone zone;
        [SerializeField]
        protected CardSelectionArea.SelectionMode selectionMode;

        protected bool IsLocalPlayerZone => zone && zone.IsLocalPlayerZone;
        protected bool isOverlayingSelectionArea;

        // ------------------------------

        [Button, HideInEditorMode]
        public void ViewZone()
        {
            if(!zone || !CanViewZone() || FieldManager.PlayerZones.zoneViewingArea.IsActive)
                return;

            if(FieldManager.PlayerZones.selectionArea.IsActive)
            {
                isOverlayingSelectionArea = true;
                FieldManager.PlayerZones.selectionArea.transform.position += Vector3.down * 10f;
            }
            FieldManager.PlayerZones.zoneViewingArea.Enable(selectionMode, GetSlotCount(), GetSlotCount(), slotBackgroundsActive: false);
            AddCards();
            GameUIManager.ViewingZone.Open(GetDisplayText(), Close);
        }

        public void Close()
        {
            if(isOverlayingSelectionArea)
            {
                isOverlayingSelectionArea = false;
                FieldManager.PlayerZones.selectionArea.transform.position += Vector3.up * 10f;
            }
            FieldManager.PlayerZones.zoneViewingArea.Disable();
            GameUIManager.ViewingZone.Close();
        }

        // ------------------------------

        protected virtual bool CanViewZone()
        {
            return zone.AllCards.Count > 0;
        }

        protected virtual int GetSlotCount()
        {
            return zone.AllCards.Count;
        }

        protected abstract string GetDisplayText();
        protected abstract void AddCards();

        // ------------------------------

        protected void Awake()
        {
            if(!zone)
                gameObject.SetActive(false);
        }

#if UNITY_EDITOR
        protected void OnValidate()
        {
            if(!zone)
                zone = GetComponentInParent<CardZone>();
        }
#endif
    }
}
