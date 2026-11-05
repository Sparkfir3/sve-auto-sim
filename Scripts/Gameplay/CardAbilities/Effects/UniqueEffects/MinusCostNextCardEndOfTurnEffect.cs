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
            var filterDict = SVEFormulaParser.ParseCardFilterFormula(this.filter, sourceCardInstanceId);
            int currentPlayedCount = player.AdditionalStats.CardsPlayedThisTurnWithFilter(filterDict);
            RegisteredPassiveAbility passive = new()
            {
                sourceCardInstanceId = sourceCardInstanceId,
                filters = filterDict,
                effect = new MinusCostOtherPassive
                {
                    duration = SVEProperties.PassiveDuration.EndOfTurn,
                    amount = amount,
                    ActiveCondition = (_, p) => p.AdditionalStats.CardsPlayedThisTurnWithFilter(filterDict) == currentPlayedCount
                },
                affectedCards = new List<RuntimeCard>(),
                target = SVEProperties.SVEEffectTarget.Self,
                duration = SVEProperties.PassiveDuration.EndOfTurn
            };
            SVEEffectPool.Instance.RegisterPassiveAbility(passive);
            onComplete?.Invoke();
        }
    }
}
