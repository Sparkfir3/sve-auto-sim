using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;

namespace SVESimulator
{
    public class ViewBanishedController : ViewZoneControllerBase
    {
        protected override string GetDisplayText()
        {
            return $"Viewing{(!IsLocalPlayerZone ? " Opponent's" : "")} Banished Zone";
        }

        protected override void AddCards()
        {
            if(IsLocalPlayerZone)
                FieldManager.PlayerZones.zoneViewingArea.AddBanished();
            else
                FieldManager.PlayerZones.zoneViewingArea.AddOpponentBanished();
        }
    }
}
