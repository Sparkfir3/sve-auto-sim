using System;
using CCGKit;

namespace SVESimulator
{
    public class DrawThenDamageEffect : SveEffect
    {
        [EnumField("Target", width = 200), Order(1)]
        public SVEProperties.SVEEffectTarget target = SVEProperties.SVEEffectTarget.Self;

        [StringField("Target Filter", width = 100), Order(2)]
        public string filter;

        [StringField("Draw Amount", width = 100), Order(3)]
        public string amount;

        [StringField("Damage Amount", width = 100), Order(3)]
        public string amount2;

        // ------------------------------

        public override void Resolve(PlayerController player, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, Action onComplete = null)
        {
            // Target cards on field
            ResolveOnTarget(player, triggeringCardInstanceId, triggeringCardZone, sourceCardInstanceId, sourceCardZone, target, filter, onTargetFound: targets =>
            {
                // Draw
                int drawAmount = SVEFormulaParser.ParseValue(amount, player);
                for(int i = 0; i < drawAmount; i++)
                    player.LocalEvents.DrawCard();

                // Damage
                int damageAmount = SVEFormulaParser.ParseValue(amount2, player) * -1;
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
