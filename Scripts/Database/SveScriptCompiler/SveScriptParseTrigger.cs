using System.Collections.Generic;

namespace SVESimulator.SveScript
{
    internal static partial class SveScriptAbilityCompiler
    {
        private enum TriggerParameterType { CanTargetSelf, Filter }

        private struct EffectTriggerInfo
        {
            public string ccgType;
            public TriggerParameterType[] parameters;

            public EffectTriggerInfo(string ccgType, params TriggerParameterType[] parameters)
            {
                this.ccgType = ccgType;
                this.parameters = parameters;
            }
        }

        private static Dictionary<string, EffectTriggerInfo> EffectTriggerDictionary = new()
        {
            // Card Movement - From Field (Self)
            { "Fanfare", new EffectTriggerInfo("SVESimulator.SveOnCardEnterFieldTrigger") },
            { "FanfareHand", new EffectTriggerInfo("SVESimulator.SveOnCardEnterFieldFromHandTrigger") },
            { "FanfareNotHand", new EffectTriggerInfo("SVESimulator.SveOnCardEnterFieldFromNotHandTrigger") },

            { "LastWords", new EffectTriggerInfo("SVESimulator.SveLastWordsTrigger", TriggerParameterType.Filter) },
            { "OnReturnToHandFromField", new EffectTriggerInfo("SVESimulator.SveOnCardReturnToHandFromField") },
            { "OnLeaveField", new EffectTriggerInfo("SVESimulator.SveOnCardLeaveFieldTrigger") },

            // Card Movement - From Field (Other, Player Cards)
            { "OnOtherEnterField", new EffectTriggerInfo("SVESimulator.SveOnOtherCardEnterFieldTrigger", TriggerParameterType.Filter) },
            { "OnOtherLeaveField", new EffectTriggerInfo("SVESimulator.SveOnOtherCardLeaveFieldTrigger", TriggerParameterType.Filter) },
            { "OnOtherReturnToHandFromField", new EffectTriggerInfo("SVESimulator.SveOnOtherCardReturnToHandFromField", TriggerParameterType.Filter) },

            // Card Movement - From Field (Other, Opponent Cards)
            { "OnOpponentCardLeaveField", new EffectTriggerInfo("SVESimulator.SveOnOpponentCardLeaveFieldTrigger", TriggerParameterType.Filter) },
            { "OnOpponentCardDestroyed", new EffectTriggerInfo("SVESimulator.SveOnOpponentCardDestroyedTrigger", TriggerParameterType.Filter) },

            // Card Movement - Other
            { "OnDiscarded", new EffectTriggerInfo("SVESimulator.SveOnDiscardedTrigger") },

            // Card/Player Actions
            { "OnEvolve", new EffectTriggerInfo("SVESimulator.SveOnEvolveTrigger") },
            { "OnOtherEvolve", new EffectTriggerInfo("SVESimulator.SveOnOtherEvolveTrigger", TriggerParameterType.Filter) },
            { "OnRace", new EffectTriggerInfo("SVESimulator.SveOnRaceTrigger") },
            { "OnOtherRace", new EffectTriggerInfo("SVESimulator.SveOnOtherRaceTrigger", TriggerParameterType.Filter) },
            { "OnPlaySpell", new EffectTriggerInfo("SVESimulator.SveOnPlaySpellTrigger", TriggerParameterType.Filter) },

            // Combat
            { "Strike", new EffectTriggerInfo("SVESimulator.SveOnAttackTrigger", TriggerParameterType.Filter) },
            { "FollowerStrike", new EffectTriggerInfo("SVESimulator.SveOnAttackFollowerTrigger", TriggerParameterType.Filter) },
            { "LeaderStrike", new EffectTriggerInfo("SVESimulator.SveOnAttackLeaderTrigger") },
            { "OnOtherAttack", new EffectTriggerInfo("SVESimulator.SveOnOtherCardAttackTrigger", TriggerParameterType.Filter) },
            { "OnDealCombatDamage", new EffectTriggerInfo("SVESimulator.SveOnDealCombatDamageTrigger") },

            // Game Phases
            { "StartMainPhase", new EffectTriggerInfo("SVESimulator.SveStartMainPhaseTrigger") },
            { "StartOpponentMainPhase", new EffectTriggerInfo("SVESimulator.SveStartOpponentMainPhaseTrigger") },
            { "StartEndPhase", new EffectTriggerInfo("SVESimulator.SveStartEndPhaseTrigger") },

            // Other
            { "OnLeaderGainDefense", new EffectTriggerInfo("SVESimulator.SveOnLeaderGainDefenseTrigger") },
            { "OnSelectedForAbility", new EffectTriggerInfo("SVESimulator.SveOnSelectedForAbilityTrigger") },

            { "Spell", new EffectTriggerInfo("SVESimulator.SpellAbility") },
            { "Passive", new EffectTriggerInfo("SVESimulator.PassiveAbilityOnField", TriggerParameterType.CanTargetSelf, TriggerParameterType.Filter) },
            { "ModifiedCost", new EffectTriggerInfo("SVESimulator.ModifiedCostTrigger") },
        };
    }
}
