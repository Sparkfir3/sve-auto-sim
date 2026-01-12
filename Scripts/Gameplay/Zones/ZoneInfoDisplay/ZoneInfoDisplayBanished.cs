using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;

namespace SVESimulator
{
    public class ZoneInfoDisplayBanished : ZoneInfoDisplayBase
    {
        protected override void Initialize()
        {
            base.Initialize();
            zone.Player.OnCardsInBanishedZoneChanged += UpdateCardCount;
        }
    }
}
