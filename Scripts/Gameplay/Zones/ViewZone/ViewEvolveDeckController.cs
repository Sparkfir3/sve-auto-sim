using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;

namespace SVESimulator
{
    public class ViewEvolveDeckController : ViewZoneControllerBase
    {
        protected override bool CanViewZone()
        {
            return IsLocalPlayerZone ? zone.Runtime.numCards > 0 : zone.AllCards.Count > 0;
        }

        protected override int GetSlotCount()
        {
            return IsLocalPlayerZone ? zone.Runtime.numCards : zone.AllCards.Count;
        }

        protected override string GetDisplayText()
        {
            return $"Viewing{(!IsLocalPlayerZone ? " Opponent's" : "")} Evolve Deck";
        }

        protected override void AddCards()
        {
            if(IsLocalPlayerZone)
                FieldManager.PlayerZones.zoneViewingArea.AddEvolveDeck();
            else
                FieldManager.PlayerZones.zoneViewingArea.AddOpponentEvolveDeck();
        }
    }
}
