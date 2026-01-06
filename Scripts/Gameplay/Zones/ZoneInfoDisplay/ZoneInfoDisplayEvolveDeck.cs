using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;

namespace SVESimulator
{
    public class ZoneInfoDisplayEvolveDeck : ZoneInfoDisplayBase
    {
        protected override void Initialize()
        {
            base.Initialize();
            player.OnCardsInEvolveDeckChanged += UpdateCardCount;
        }
    }
}
