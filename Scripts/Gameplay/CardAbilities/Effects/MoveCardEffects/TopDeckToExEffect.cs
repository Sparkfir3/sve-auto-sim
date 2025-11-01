using System;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public class SveTopDeckToExEffect : SveEffect
    {
        [StringField("Amount", width = 100), Order(1)]
        public string amount = "1";

        // ------------------------------

        public override void Resolve(PlayerController player, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, Action onComplete = null)
        {
            int count = SVEFormulaParser.ParseValue(amount, player);
            for(int i = 0; i < count; i++)
                player.LocalEvents.TopDeckToExArea();
            onComplete?.Invoke();
        }
    }
}
