using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;

namespace SVESimulator
{
    public class ViewCemeteryController : ViewZoneControllerBase
    {
        protected override string GetDisplayText()
        {
            return $"Viewing{(!IsLocalPlayerZone ? " Opponent's" : "")} Cemetery";
        }

        protected override void AddCards()
        {
            if(IsLocalPlayerZone)
                FieldManager.PlayerZones.zoneViewingArea.AddCemetery();
            else
                FieldManager.PlayerZones.zoneViewingArea.AddOpponentCemetery();
        }
    }
}
