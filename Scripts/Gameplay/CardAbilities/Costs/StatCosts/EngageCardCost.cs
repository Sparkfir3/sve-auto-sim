using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public class EngageCardCost : SveCost
    {
        [EnumField("Target", width = 200), Order(4)]
        public SVEProperties.SVEEffectTarget target = SVEProperties.SVEEffectTarget.Self;

        [StringField("Target Filter", width = 100), Order(5)]
        public string filter;

        // ------------------------------

        public override string GetReadableString(GameConfiguration config)
        {
            return $"Engage {target} {filter}";
        }

        public override bool CanPayCost(PlayerController player, RuntimeCard card, string abilityName)
        {
            switch(target)
            {
                case SVEProperties.SVEEffectTarget.Self:
                    return IsCardValidTarget(card);
                case SVEProperties.SVEEffectTarget.TargetPlayerCard:
                    var cardFilter = SVEFormulaParser.ParseCardFilterFormula(filter, card.instanceId);
                    int minAmount = 1;
                    if(cardFilter.TryGetValue(SVEFormulaParser.CardFilterSetting.MinMaxCount, out string minMaxRaw))
                        SVEFormulaParser.ParseMinMaxCount(minMaxRaw, player, card, out minAmount, out _);
                    return player.ZoneController.fieldZone.GetAllPrimaryCards().Count(x => IsCardValidTarget(x.RuntimeCard) && cardFilter.MatchesCard(x)) >= minAmount;
                default:
                    return false;
            }

            bool IsCardValidTarget(RuntimeCard runtimeCard)
            {
                return runtimeCard != null && runtimeCard.namedStats.TryGetValue(SVEProperties.CardStats.Engaged, out Stat engageStat) && engageStat.effectiveValue == 0;
            }
        }

        public IEnumerator PayCost(PlayerController player, CardObject card, string abilityName, List<int> cardInstanceIdsToEngage)
        {
            bool waiting = true;
            TargetCardForCostEffect getTargetsEffect = new()
            {
                text = LibraryCardCache.GetEffectTextCost(card.LibraryCard.id, abilityName),
                target = target,
                filter = filter
            };

            getTargetsEffect.GetTargets(player, card.RuntimeCard.instanceId, card.CurrentZone.Runtime.name, targets =>
            {
                foreach(CardObject targetCard in targets)
                {
                    targetCard.SetEngaged();
                    cardInstanceIdsToEngage.Add(targetCard.RuntimeCard.instanceId);
                }
                waiting = false;
            });

            yield return new WaitUntil(() => !waiting);
            yield return null;
        }
    }
}
