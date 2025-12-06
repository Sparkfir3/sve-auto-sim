using System;
using CCGKit;

namespace SVESimulator
{
    public class DealDamageEffect : SveEffect
    {
        [EnumField("Target", width = 200), Order(1)]
        public SVEProperties.SVEEffectTarget target = SVEProperties.SVEEffectTarget.Self;

        [StringField("Target Filter", width = 100), Order(2)]
        public string filter;

        [StringField("Amount", width = 100), Order(3)]
        public string amount;

        // ------------------------------

        public override void Resolve(PlayerController player, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, Action onComplete = null)
        {
            int damageAmount = SVEFormulaParser.ParseValue(amount, player, sourceCardInstanceId, sourceCardZone) * -1;

            // Target leader
            if(target.IsLeader(out bool local, out bool opponent) && !target.IsFieldCard())
            {
                if(local)
                    player.LocalEvents.AddLeaderDefense(player.GetPlayerInfo(), damageAmount);
                if(opponent)
                    player.LocalEvents.AddLeaderDefense(player.GetOpponentInfo(), damageAmount);
                onComplete?.Invoke();
                return;
            }

            // Target cards on field
            ResolveOnTarget(player, triggeringCardInstanceId, triggeringCardZone, sourceCardInstanceId, sourceCardZone, target, filter, onTargetFound: targets =>
            {
                foreach(CardObject card in targets)
                {
                    // Follower
                    if(!card.IsCardType(SVEProperties.CardTypes.Leader))
                        player.LocalEvents.ApplyModifierToCard(card.RuntimeCard, card.RuntimeCard.namedStats[SVEProperties.CardStats.Defense].statId, damageAmount, true);

                    // Leader
                    else if(card.CurrentZone.IsLocalPlayerZone)
                        player.LocalEvents.AddLeaderDefense(player.GetPlayerInfo(), damageAmount);
                    else
                        player.LocalEvents.AddLeaderDefense(player.GetOpponentInfo(), damageAmount);
                }
                onComplete?.Invoke();
            });
        }
    }
}
