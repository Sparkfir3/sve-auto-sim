using System;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public class GiveCounterEffect : SveEffect
    {
        [EnumField("Target", width = 200), Order(1)]
        public SVEProperties.SVEEffectTarget target = SVEProperties.SVEEffectTarget.Self;

        [StringField("Target Filter", width = 100), Order(2)]
        public string filter;

        [KeywordTypeField("Type"), Order(3)]
        public int keywordType;

        [KeywordValueField("Value"), Order(4)]
        public int keywordValue;

        [StringField("Amount", width = 100), Order(5)]
        public string amount;

        // ------------------------------

        public override void Resolve(PlayerController player, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, Action onComplete = null)
        {
            int counterAmount = SVEFormulaParser.ParseValue(amount, player, sourceCardInstanceId, sourceCardZone);

            ResolveOnTarget(player, triggeringCardInstanceId, triggeringCardZone, sourceCardInstanceId, sourceCardZone, target, filter, onTargetFound: targets =>
            {
                foreach(CardObject card in targets)
                    for(int i = 0; i < counterAmount; i++)
                        player.LocalEvents.AddCountersToCard(card.RuntimeCard, (SVEProperties.Counters)keywordType, counterAmount);
                onComplete?.Invoke();
            });
        }
    }
}
