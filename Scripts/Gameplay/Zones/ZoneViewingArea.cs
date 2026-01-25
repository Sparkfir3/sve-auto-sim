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
            if(SVEQuickTimingController.Instance.IsActive)
            {
                Player.InputController.allowedInputs = Player.isActivePlayer ? PlayerInputController.InputTypes.None : PlayerInputController.InputTypes.QuickTiming;
                if(IsLocalPlayerZone)
                {
                    ZoneController.handZone.HighlightValidQuicks();
                    ZoneController.fieldZone.HighlightInteractableCards();
                }
                return;
            }
            Player.InputController.allowedInputs = Player.isActivePlayer ? PlayerInputController.InputTypes.All : PlayerInputController.InputTypes.None;
            if(IsLocalPlayerZone)
                ZoneController.fieldZone.HighlightCardsCanAttack();
        }

        protected override void MoveCardToSelectionArea(CardObject card, bool rearrangeHand = false)
        {
            ZoneController.MoveCardToZoneViewingArea(card, rearrangeHand);
        }
    }
}
