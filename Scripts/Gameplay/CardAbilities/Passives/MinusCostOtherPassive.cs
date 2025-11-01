using System;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public class MinusCostOtherPassive : SvePassiveEffect
    {
        [StringField("Amount", width = 200), Order(1)]
        public string amount;

        // ------------------------------

        public override void ApplyPassive(RuntimeCard card, PlayerController player) { }
        public override void RemovePassive(RuntimeCard card, PlayerController player) { }

        public int GetReductionAmount(RuntimeCard sourceCard, PlayerController player)
        {
            return SVEFormulaParser.ParseValue(amount, player, sourceCard);
        }
    }
}
