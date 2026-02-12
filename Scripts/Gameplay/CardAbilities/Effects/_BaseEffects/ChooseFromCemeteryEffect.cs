using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public abstract class ChooseFromCemeteryEffect : ChooseFromCardStackEffect
    {
        protected override void InitializeSelectionArea(PlayerController player, CardSelectionArea selectionArea)
        {
            int cardCount = player.ZoneController.cemeteryZone.AllCards.Count;
            selectionArea.Enable(CardSelectionArea.SelectionMode.SelectCardsFromCemetery, cardCount, cardCount, slotBackgroundsActive: false);
            selectionArea.SetFilter(filter);
            selectionArea.AddCemetery();
        }
    }
}
