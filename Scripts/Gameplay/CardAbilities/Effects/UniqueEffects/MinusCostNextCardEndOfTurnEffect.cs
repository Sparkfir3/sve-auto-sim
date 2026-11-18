using System;
using System.Collections.Generic;
using CCGKit;

namespace SVESimulator
{
    public class MinusCostNextCardEndOfTurnEffect : SveEffect
    {
        [StringField("Amount", width = 100), Order(1)]
        public string amount;

        [StringField("Filter", width = 100), Order(2)]
        public string filter;

        // ------------------------------

        public override void Resolve(PlayerController player, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, Action onComplete = null)
        {
            var filterDict = SVEFormulaParser.ParseCardFilterFormula(filter, sourceCardInstanceId);
            RegisteredPassiveAbility passive = new()
            {
                sourceCardInstanceId = sourceCardInstanceId,
                filters = filterDict,
                effect = GetPassiveEffect(GetActiveCondition(player,filterDict)),
                affectedCards = new List<RuntimeCard>(),
                target = SVEProperties.SVEEffectTarget.Self,
                duration = SVEProperties.PassiveDuration.EndOfTurn
            };
            SVEEffectPool.Instance.RegisterPassiveAbility(passive);
            onComplete?.Invoke();
        }

        protected virtual MinusCostOtherPassive.ActiveConditionDelegate GetActiveCondition(PlayerController player, Dictionary<SVEFormulaParser.CardFilterSetting, string> filterDict)
        {
            int currentPlayedCount = player.AdditionalStats.CardsPlayedThisTurnWithFilter(filterDict);
            return (_, p) => p.AdditionalStats.CardsPlayedThisTurnWithFilter(filterDict) == currentPlayedCount;
        }

        protected virtual MinusCostOtherPassive GetPassiveEffect(MinusCostOtherPassive.ActiveConditionDelegate activeCondition)
        {
            return new MinusCostOtherPassive
            {
                duration = SVEProperties.PassiveDuration.EndOfTurn,
                amount = amount,
                ActiveCondition = activeCondition
            };
        }
    }
}
