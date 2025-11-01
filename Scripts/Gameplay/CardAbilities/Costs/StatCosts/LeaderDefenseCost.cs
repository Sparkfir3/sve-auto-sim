using System;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public class LeaderDefenseCost : SveCost
    {
        [StringField("Amount", width = 100)]
        public string amount;

        // ------------------------------

        public override string GetReadableString(GameConfiguration config)
        {
            return $"{amount} Leader Defense";
        }

        public override bool CanPayCost(PlayerController player, RuntimeCard card, string abilityName)
        {
            int value = SVEFormulaParser.ParseValue(amount, player);
            return player.GetPlayerInfo().namedStats[SVEProperties.PlayerStats.Defense].effectiveValue > value;
        }
    }
}
