using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CCGKit;
using Sparkfire.Utility;

namespace SVESimulator
{
    public abstract class SendTargetToZoneCost : SveCost
    {
        [EnumField("Target", width = 200), Order(1)]
        public SVEProperties.SVEEffectTarget target = SVEProperties.SVEEffectTarget.Self;

        [StringField("Target Filter", width = 100), Order(2)]
        public string filter;

        // ------------------------------

        protected abstract string TargetZoneName { get; }
        protected abstract string CostName { get; }

        // ------------------------------

        public override string GetReadableString(GameConfiguration config)
        {
            return $"Send {target.ToString()}{(!filter.IsNullOrWhiteSpace() ? " " + filter : "")} to {TargetZoneName}";
        }

        public override bool CanPayCost(PlayerController player, RuntimeCard card, string abilityName)
        {
            var cardFilter = SVEFormulaParser.ParseCardFilterFormula(filter, card.instanceId);
            int minTargets = 1;
            if(cardFilter.TryGetValue(SVEFormulaParser.CardFilterSetting.MinMaxCount, out string minMaxFormula))
                SVEFormulaParser.ParseMinMaxCount(minMaxFormula, out minTargets, out _);

            switch(target)
            {
                case SVEProperties.SVEEffectTarget.Self:
                    return player.ZoneController.fieldZone.GetAllPrimaryCards().Any(x => x.RuntimeCard.instanceId == card.instanceId);
                case SVEProperties.SVEEffectTarget.AllPlayerCards:
                case SVEProperties.SVEEffectTarget.AllPlayerCardsEx:
                case SVEProperties.SVEEffectTarget.AllPlayerCardsFieldAndEx:
                    return true;
                case SVEProperties.SVEEffectTarget.TargetPlayerCard:
                    return player.ZoneController.fieldZone.GetAllPrimaryCards().Count(x => cardFilter.MatchesCard(x)) >= minTargets;
                case SVEProperties.SVEEffectTarget.TargetPlayerCardEx:
                    return player.ZoneController.exAreaZone.GetAllPrimaryCards().Count(x => cardFilter.MatchesCard(x)) >= minTargets;

                default:
                    Debug.LogError($"Invalid target {target} provided for {CostName}");
                    return false;
            }
        }

        public override IEnumerator PayCost(PlayerController player, CardObject card, SveEffect effect, List<MoveCardToZoneData> cardsToMove)
        {
            bool waiting = true;
            TargetCardForCostEffect getTargetsEffect = CostTargetingEffect(effect?.text);
            List<CardObject> targetCards = null;

            getTargetsEffect.GetTargets(player, card.RuntimeCard.instanceId, card.CurrentZone.Runtime.name, targets =>
            {
                targetCards = targets;
                foreach(CardObject targetCard in targetCards)
                    cardsToMove.Add(new MoveCardToZoneData(targetCard.RuntimeCard.instanceId, targetCard.CurrentZone.Runtime.name, TargetZoneName));
                waiting = false;
            });

            yield return new WaitUntil(() => !waiting);
            MoveCardObjectsToTargetZone(player, targetCards);
            yield return null;
        }

        protected virtual TargetCardForCostEffect CostTargetingEffect(string text = null)
        {
            return new TargetCardForCostEffect()
            {
                text = text.IsNullOrWhiteSpace() ? "" : $"Pay Cost for: {text}",
                target = target,
                filter = filter
            };
        }

        protected abstract void MoveCardObjectsToTargetZone(PlayerController player, List<CardObject> cards);
    }
}
