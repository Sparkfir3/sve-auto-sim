using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public class ChooseAmountFromList : ChooseEffectFromList
    {
        [StringField("Amount", width = 100), Order(11)]
        public string amount;

        // ------------------------------

        protected override void GetMinMax(PlayerController player, RuntimeCard card, in int effectListSize, out int min, out int max)
        {
            SVEFormulaParser.ParseValueAsMinMax(amount, player, card, out min, out max);
            max = player.AdditionalStats.CanChooseAnyNumberOfOptions ? effectListSize : Mathf.Min(max, effectListSize);
            min = player.AdditionalStats.CanChooseAnyNumberOfOptions ? 0 : Mathf.Min(min, max);
        }
    }
}
