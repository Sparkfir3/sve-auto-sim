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
            // Card Movement
            { "Fanfare", new EffectTriggerInfo("SVESimulator.SveOnCardEnterFieldTrigger") },
            { "LastWords", new EffectTriggerInfo("SVESimulator.SveLastWordsTrigger") },
            { "OnReturnToHandFromField", new EffectTriggerInfo("SVESimulator.SveOnCardReturnToHandFromField") },
            { "OnLeaveField", new EffectTriggerInfo("SVESimulator.SveOnCardLeaveFieldTrigger") },
            { "OnOtherEnterField", new EffectTriggerInfo("SVESimulator.SveOnOtherCardEnterFieldTrigger", TriggerParameterType.Filter) },
            { "OnOtherLeaveField", new EffectTriggerInfo("SVESimulator.SveOnOtherCardLeaveFieldTrigger", TriggerParameterType.Filter) },

            // Card/Player Actions
            { "OnEvolve", new EffectTriggerInfo("SVESimulator.SveOnEvolveTrigger") },
            { "Strike", new EffectTriggerInfo("SVESimulator.SveOnAttackTrigger") },
            { "OnOtherAttack", new EffectTriggerInfo("SVESimulator.SveOnOtherCardAttackTrigger", TriggerParameterType.Filter) },
            { "OnPlaySpell", new EffectTriggerInfo("SVESimulator.SveOnPlaySpellTrigger", TriggerParameterType.Filter) },

            // Game Phases
            { "StartMainPhase", new EffectTriggerInfo("SVESimulator.SveStartMainPhaseTrigger") },
            { "StartOpponentMainPhase", new EffectTriggerInfo("SVESimulator.SveStartOpponentMainPhaseTrigger") },
            { "StartEndPhase", new EffectTriggerInfo("SVESimulator.SveStartEndPhaseTrigger") },

            // Other
            { "Spell", new EffectTriggerInfo("SVESimulator.SpellAbility") },
            { "Passive", new EffectTriggerInfo("SVESimulator.PassiveAbilityOnField", TriggerParameterType.CanTargetSelf, TriggerParameterType.Filter) },
            { "ModifiedCost", new EffectTriggerInfo("SVESimulator.ModifiedCostTrigger") },
        };
    }
}
