using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public class DynamicPlusAbilityDamagePassive : SvePassiveEffect
    {
        [StringField("Amount", width = 200), Order(1)]
        public string amount;

        // ------------------------------

        public override void ApplyPassive(RuntimeCard card, PlayerController player) { }
        public override void RemovePassive(RuntimeCard card, PlayerController player) { }

        public int GetPlusDamageAmount(RuntimeCard sourceCard, PlayerController player)
        {
            Debug.Log(amount);
            return SVEFormulaParser.ParseValue(amount, player, sourceCard);
        }
    }
}
