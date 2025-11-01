using System;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public class SveMillDeckEffect : SveEffect
    {
        [EnumField("Target", width = 200), Order(1)]
        public SVEProperties.SVEEffectTarget target = SVEProperties.SVEEffectTarget.Self;

        [StringField("Amount", width = 100), Order(2)]
        public string amount = "1";

        // ------------------------------

        public override void Resolve(PlayerController player, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, Action onComplete = null)
        {
            target.IsLeader(out bool selfMill, out bool oppMill);
            selfMill |= target == SVEProperties.SVEEffectTarget.Self;
            oppMill |= target == SVEProperties.SVEEffectTarget.Opponent;

            int millCount = SVEFormulaParser.ParseValue(amount, player);
            if(selfMill)
                player.LocalEvents.MillDeck(millCount);
            if(oppMill)
                player.LocalEvents.TellOpponentMillDeck(millCount);
            onComplete?.Invoke();
        }
    }
}
