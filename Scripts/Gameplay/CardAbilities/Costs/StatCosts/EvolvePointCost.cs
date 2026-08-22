using System;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public class EvolvePointCost : SveCost
    {
        [StringField("Amount", width = 100)]
        public string amount;

        // ------------------------------

        public override string GetReadableString(GameConfiguration config)
        {
            return $"{amount} Evolve Points";
        }

        public override bool CanPayCost(PlayerController player, RuntimeCard card, string abilityName)
        {
            int value = SVEFormulaParser.ParseValue(amount, player);
            return player.GetPlayerInfo().namedStats[SVEProperties.PlayerStats.EvolutionPoints].effectiveValue >= value;
        }
    }
}
