using System;
using System.Collections.Generic;
using CCGKit;

namespace SVESimulator
{
    public class SveDealDamageDividedEffect : SveEffect
    {
        [StringField("Target Filter", width = 100), Order(1)]
        public string filter;

        [StringField("Amount", width = 100), Order(2)]
        public string amount;

        // ------------------------------

        public override void Resolve(PlayerController player, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, Action onComplete = null)
        {
            SVEProperties.SVEEffectTarget target = SVEProperties.SVEEffectTarget.TargetOpponentCardsDivided;
            int totalDamage = SVEFormulaParser.ParseValue(amount, player, sourceCardInstanceId, sourceCardZone);

            // Target cards (on field)
            // " // " is a unique divider to allow additional parameter for effect targeting
            // TODO - better solution than arbitrary string split
            ResolveOnTarget(player, triggeringCardInstanceId, triggeringCardZone, sourceCardInstanceId, sourceCardZone, target, $"{filter} // {totalDamage}", onTargetFound: targets =>
            {
                Dictionary<CardObject, int> targetPoints = new();
                foreach(CardObject card in targets)
                {
                    if(!targetPoints.TryAdd(card, 1))
                        targetPoints[card]++;
                }

                foreach(KeyValuePair<CardObject, int> cardPair in targetPoints)
                {
                    (CardObject card, int damageAmount) = (cardPair.Key, -cardPair.Value);
                    player.LocalEvents.ApplyModifierToCard(card.RuntimeCard, card.RuntimeCard.namedStats[SVEProperties.CardStats.Defense].statId, damageAmount, true);
                }
                onComplete?.Invoke();
            });
        }
    }
}
