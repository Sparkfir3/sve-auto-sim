using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CCGKit;
using Sparkfire.Utility;
using SVESimulator.UI;

namespace SVESimulator
{
    public class RemoveCountersCost : SveCost
    {
        [KeywordTypeField("Type"), Order(1)]
        public int keywordType;

        [KeywordValueField("Value"), Order(2)]
        public int keywordValue; // unused, but required for parsing

        [StringField("Amount", width = 100), Order(3)]
        public string amount;

        [EnumField("Target", width = 200), Order(4)]
        public SVEProperties.SVEEffectTarget target = SVEProperties.SVEEffectTarget.Self;

        [StringField("Target Filter", width = 100), Order(5)]
        public string filter;

        // ------------------------------

        public override string GetReadableString(GameConfiguration config)
        {
            return $"Remove {amount} counters ({keywordType}) from {target} {filter}";
        }

        public override bool CanPayCost(PlayerController player, RuntimeCard card, string abilityName)
        {
            var cardFilter = SVEFormulaParser.ParseCardFilterFormula(filter, card.instanceId);
            int requiredAmount = 1;
            if(!amount.IsNullOrWhiteSpace())
                SVEFormulaParser.ParseValueAsMinMax(amount, player, card, out requiredAmount, out _);

            switch(target)
            {
                case SVEProperties.SVEEffectTarget.Self:
                    return card.CountOfCounter(keywordType) >= requiredAmount;
                case SVEProperties.SVEEffectTarget.TargetPlayerCard:
                    return player.ZoneController.fieldZone.GetAllPrimaryCards()
                        .Any(x => cardFilter.MatchesCard(x) && x.RuntimeCard.CountOfCounter(keywordType) >= requiredAmount);
                default:
                    return false;
            }
        }

        public IEnumerator PayCost(PlayerController player, CardObject card, string abilityName, List<RemoveCounterData> countersToRemove)
        {
            bool waiting = true;
            TargetCardForCostEffect getTargetsEffect = CostTargetingEffect(LibraryCardCache.GetEffectTextCost(card.LibraryCard.id, abilityName));

            getTargetsEffect.GetTargets(player, card.RuntimeCard.instanceId, card.CurrentZone.Runtime.name, targets =>
            {
                foreach(CardObject targetCard in targets)
                {
                    // Get amounts
                    int removeAmountMin, removeAmountMax;
                    if(amount.IsNullOrWhiteSpace())
                    {
                        removeAmountMin = targetCard.CountOfCounter(keywordType);
                        removeAmountMax = removeAmountMin;
                    }
                    else
                        SVEFormulaParser.ParseValueAsMinMax(amount, player, card.RuntimeCard, out removeAmountMin, out removeAmountMax);

                    // Dynamic remove count
                    if(removeAmountMax > removeAmountMin)
                    {
                        if(targets.Count > 1)
                            Debug.LogError($"{nameof(RemoveCountersCost)} does not support removing a dynamic number of counters {amount} with multiple targeted cards!");
                        GameUIManager.SelectAmount.Open(removeAmountMin, removeAmountMax, "Select number of counters to remove", null, selectedAmount =>
                        {
                            countersToRemove.Add(new RemoveCounterData(targetCard.RuntimeCard.instanceId, targetCard.CurrentZone.Runtime.name,
                                keywordType, targetCard.CountOfCounter(keywordType), selectedAmount));
                            waiting = false;
                        });
                        return;
                    }

                    // Standard remove
                    countersToRemove.Add(new RemoveCounterData(targetCard.RuntimeCard.instanceId, targetCard.CurrentZone.Runtime.name,
                        keywordType, targetCard.CountOfCounter(keywordType), removeAmountMin));
                }
                waiting = false;
            });

            yield return new WaitUntil(() => !waiting);
            yield return null;
        }

        private TargetCardForCostEffect CostTargetingEffect(string text = null)
        {
            // Test Display Text Logic - leaving it here in case I decide to use it later
            // string text = ((SVEProperties.Counters)keywordType == SVEProperties.Counters.Stack && target == SVEProperties.SVEEffectTarget.TargetPlayerCard)
            //     ? $"Earth Rite{(removeAmount > 1 ? $" ({removeAmount})" : "")}"
            //     : $"Remove{(removeAmount.HasValue ? $" {removeAmount.Value}" : "")} {(SVEProperties.Counters)keywordType} Counters";
            return new TargetCardForCostEffect()
            {
                text = text ?? "Remove Counters",
                target = target,
                filter = filter + $"r({GameManager.Instance.config.keywords[keywordType].name ?? ""},{amount})"
            };
        }
    }
}
