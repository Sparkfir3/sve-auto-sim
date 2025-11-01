using System;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public class SveReducedCostEffect : SveModifiedCostEffect
    {
        [StringField("Amount", width = 200), Order(1)]
        public string amount;

        // ------------------------------

        public int ReduceCostAmount(PlayerController player, RuntimeCard card)
        {
            return SVEFormulaParser.ParseValue(amount, player, card);
        }
    }
}
