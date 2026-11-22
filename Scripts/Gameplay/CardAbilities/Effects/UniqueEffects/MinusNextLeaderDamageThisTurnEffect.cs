using System;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public class MinusNextLeaderDamageThisTurnEffect : SveEffect
    {
        [StringField("Amount", width = 100), Order(1)]
        public string amount;

        // ------------------------------

        public override void Resolve(PlayerController player, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, Action onComplete = null)
        {
            int reductionAmount = SVEFormulaParser.ParseValue(amount, player, sourceCardInstanceId, sourceCardZone);
            player.AdditionalStats.LeaderDamageReductionInstances.Add(reductionAmount);
            onComplete?.Invoke();
        }
    }
}
