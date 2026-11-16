using System.Collections.Generic;

namespace SVESimulator
{
    public class MinusCostNextCardEndOfTurnExEffect : MinusCostNextCardEndOfTurnEffect
    {
        protected override MinusCostOtherPassive.ActiveConditionDelegate GetActiveCondition(PlayerController player, Dictionary<SVEFormulaParser.CardFilterSetting, string> filterDict)
        {
            int currentPlayedCount = player.AdditionalStats.CardsPlayedThisTurnFromExAreaWithFilter(filterDict);
            return (_, p) => p.AdditionalStats.CardsPlayedThisTurnFromExAreaWithFilter(filterDict) == currentPlayedCount;
        }

        protected override MinusCostOtherPassive GetPassiveEffect(MinusCostOtherPassive.ActiveConditionDelegate activeCondition)
        {
            return new MinusCostOtherExPassive
            {
                duration = SVEProperties.PassiveDuration.EndOfTurn,
                amount = amount,
                ActiveCondition = activeCondition
            };
        }
    }
}
