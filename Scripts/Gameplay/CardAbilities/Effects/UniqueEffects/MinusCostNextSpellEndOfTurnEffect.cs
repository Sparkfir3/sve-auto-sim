using System;
using System.Collections.Generic;
using CCGKit;

namespace SVESimulator
{
    public class MinusCostNextSpellEndOfTurnEffect : SveEffect
    {
        [StringField("Amount", width = 100), Order(1)]
        public string amount;

        // ------------------------------

        public override void Resolve(PlayerController player, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, Action onComplete = null)
        {
            int currentPlayedSpellCount = player.AdditionalStats.SpellsPlayedThisTurn.Count;
            RegisteredPassiveAbility passive = new()
            {
                sourceCardInstanceId = sourceCardInstanceId,
                filters = new Dictionary<SVEFormulaParser.CardFilterSetting, string> { { SVEFormulaParser.CardFilterSetting.Spell, null } },
                effect = new MinusCostOtherPassive
                {
                    duration = SVEProperties.PassiveDuration.EndOfTurn,
                    amount = amount,
                    ActiveCondition = (_, p) => p.AdditionalStats.SpellsPlayedThisTurn.Count == currentPlayedSpellCount
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
