using System;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public class GiveKeywordEffect : SveEffect
    {
        [EnumField("Target", width = 200), Order(1)]
        public SVEProperties.SVEEffectTarget target = SVEProperties.SVEEffectTarget.Self;
        [StringField("Target Filter", width = 100), Order(2)]
        public string filter;
        [KeywordTypeField("Type"), Order(2)]
        public int keywordType;
        [KeywordValueField("Value"), Order(3)]
        public int keywordValue;

        // ------------------------------

        public override void Resolve(PlayerController player, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, Action onComplete = null)
        {
            ResolveOnTarget(player, triggeringCardInstanceId, triggeringCardZone, sourceCardInstanceId, sourceCardZone, target, filter, onTargetFound: targets =>
            {
                foreach(CardObject card in targets)
                {
                    player.LocalEvents.ApplyKeywordToCard(card.RuntimeCard, keywordType, keywordValue, true);
                }
                onComplete?.Invoke();
            });
        }
    }
}
