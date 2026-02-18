using System;
using CCGKit;
using UnityEngine;

namespace SVESimulator
{
    public class DiscardRandomCardEffect : SveEffect
    {
        [StringField("Amount", width = 100), Order(1)]
        public string amount;

        [EnumField("Target", width = 200), Order(2)]
        public SVEProperties.SVEEffectTarget target = SVEProperties.SVEEffectTarget.Self;

        // ------------------------------

        public override void Resolve(PlayerController player, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, Action onComplete = null)
        {
            int discardAmount = SVEFormulaParser.ParseValue(amount, player);
            target.IsLeader(out bool local, out bool opponent);
            local |= target is SVEProperties.SVEEffectTarget.Self;
            opponent |= target is SVEProperties.SVEEffectTarget.Opponent;

            if(local)
                player.LocalEvents.DiscardRandomCards(player.GetPlayerInfo(), discardAmount);
            if(opponent)
                player.LocalEvents.DiscardRandomCards(player.GetOpponentInfo(), discardAmount);

            if(!local && !opponent)
                Debug.LogError($"Attempted to discard random card with invalid target mode {target}\nDiscard amount: {discardAmount} (raw: {amount})");
            onComplete?.Invoke();
        }
    }
}
