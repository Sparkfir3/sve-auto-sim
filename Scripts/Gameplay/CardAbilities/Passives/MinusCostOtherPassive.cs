using System;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public class MinusCostOtherPassive : SvePassiveEffect
    {
        public delegate bool ActiveConditionDelegate(RuntimeCard sourceCard, PlayerController player);

        [StringField("Amount", width = 200), Order(1)]
        public string amount;

        [NonSerialized]
        public ActiveConditionDelegate ActiveCondition;

        // ------------------------------

        public override void ApplyPassive(RuntimeCard card, PlayerController player) { }
        public override void RemovePassive(RuntimeCard card, PlayerController player) { }

        public int GetReductionAmount(RuntimeCard sourceCard, PlayerController player)
        {
            return ActiveCondition != null && !ActiveCondition(sourceCard, player) ? 0
                : SVEFormulaParser.ParseValue(amount, player, sourceCard);
        }
    }
}
