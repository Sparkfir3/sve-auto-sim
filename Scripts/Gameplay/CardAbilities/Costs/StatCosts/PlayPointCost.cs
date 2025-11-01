using System;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public class PlayPointCost : SveCost
    {
        [StringField("Amount", width = 100)]
        public string amount;

        // ------------------------------

        public override string GetReadableString(GameConfiguration config)
        {
            return $"{amount} Play Points";
        }

        public override bool CanPayCost(PlayerController player, RuntimeCard card, string abilityName)
        {
            int value = SVEFormulaParser.ParseValue(amount, player);
            return player.GetPlayerInfo().namedStats[SVEProperties.PlayerStats.PlayPoints].effectiveValue >= value;
        }
    }
}
