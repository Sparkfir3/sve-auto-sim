using CCGKit;
using UnityEngine;

namespace SVESimulator
{
    public class ChooseAmountFromListWithRepeats : ChooseAmountFromList
    {
        protected override bool RemoveEffectFromRemaining => false;

        protected override void GetMinMax(PlayerController player, RuntimeCard card, in int effectListSize, out int min, out int max)
        {
            SVEFormulaParser.ParseValueAsMinMax(amount, player, card, out min, out max);
            min = Mathf.Min(min, max);
        }
    }
}
