using System;
using UnityEngine;
using CCGKit;
using Sparkfire.Utility;

namespace SVESimulator
{
    // Used to store condition of act abilities and calculate if it can be used during the "can pay cost" check
    public class ConditionAsCost : SveCost
    {
        [StringField("Condition", width = 100), Order(1)]
        public string condition;

        // ------------------------------

        public override string GetReadableString(GameConfiguration config)
        {
            return $"Condition {condition}";
        }

        public override bool CanPayCost(PlayerController player, RuntimeCard card, string abilityName)
        {
            return condition.IsNullOrWhiteSpace() || SVEFormulaParser.ParseValueAsCondition(condition, player, card);
        }
    }
}
