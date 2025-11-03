using System;
using UnityEngine;
using CCGKit;
using Sparkfire.Utility;

namespace SVESimulator
{
    public class SveMoveCountersEffect : SveEffect
    {
        [KeywordTypeField("Type"), Order(3)]
        public int keywordType;

        [KeywordValueField("Value"), Order(4)]
        public int keywordValue;

        [StringField("Amount", width = 100), Order(5)]
        public string amount;

        [EnumField("Target", width = 200), Order(1)]
        public SVEProperties.SVEEffectTarget target = SVEProperties.SVEEffectTarget.Self;

        [StringField("Target Filter", width = 100), Order(2)]
        public string filter;

        // ------------------------------

        public override void Resolve(PlayerController player, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, Action onComplete = null)
        {
            int baseCounterAmount = SVEFormulaParser.ParseValue(amount, player, sourceCardInstanceId, sourceCardZone);

            ResolveOnTarget(player, triggeringCardInstanceId, triggeringCardZone, sourceCardInstanceId, sourceCardZone, target, filter, onTargetFound: targets =>
            {
                if(targets == null || targets.Count == 0)
                {
                    onComplete?.Invoke();
                    return;
                }
                if(targets.Count > 1)
                    Debug.LogError($"More than one target ({targets.Count}) was selected was selected for effect MoveCounterEffect. All cards other than the first one will be ignored." +
                        $"\nKeyword ID {keywordType}-{keywordValue} with amount {amount} and target {target} {filter}");

                CardObject sourceCard = CardManager.Instance.GetCardByInstanceId(sourceCardInstanceId);
                CardObject targetCard = targets[0];
                SVEProperties.Counters counterType = (SVEProperties.Counters)keywordType;
                int counterAmount = amount.IsNullOrWhiteSpace() ? sourceCard.RuntimeCard.CountOfCounter(counterType) : baseCounterAmount;

                player.LocalEvents.RemoveCountersFromCard(sourceCard.RuntimeCard, counterType, counterAmount);
                player.LocalEvents.AddCountersToCard(targetCard.RuntimeCard, counterType, counterAmount);

                onComplete?.Invoke();
            });
        }
    }
}
