using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;

namespace SVESimulator
{
    // For viewing cards in a zone (ex. cemetery) outside performing effects
    // Inherits from CardSelectionArea to use its card movement & positioning logic
    public class ZoneViewingArea : CardSelectionArea
    {
        protected override void OnDisableZone()
        {
            if(ZoneController.selectionArea.IsActive)
            {
                ZoneController.selectionArea.SwitchMode(ZoneController.selectionArea.CurrentMode); // reset stuff like allowed inputs
                return;
            }
            Player.InputController.allowedInputs = Player.isActivePlayer ? PlayerInputController.InputTypes.All : PlayerInputController.InputTypes.None;
        }

        protected override void MoveCardToSelectionArea(CardObject card, bool rearrangeHand = false)
        {
            ZoneController.MoveCardToZoneViewingArea(card, rearrangeHand);
        }
    }
}
