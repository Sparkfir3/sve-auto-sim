using System.Collections.Generic;
using CCGKit;
using Sparkfire.Utility;

namespace SVESimulator
{
    public class SveTrigger : Trigger
    {
        [StringField("Condition", width = 100), Order(10)]
        public string condition;
        [StringField("Cost", width = 300), Order(11)]
        public string cost;

        protected List<Cost> _costList = null;
        public List<Cost> Costs
        {
            get
            {
                if(_costList != null || cost.IsNullOrWhiteSpace())
                    return _costList;
                _costList = cost.ToCostList();
                return _costList;
            }
        }
    }

    public class SveTriggerWithFilter : SveTrigger
    {
        [StringField("Filter", width = 100)]
        public string filter;

        public bool MatchesFilter(RuntimeCard card)
        {
            return filter.IsNullOrWhiteSpace() || SVEFormulaParser.ParseCardFilterFormula(filter, card.instanceId).MatchesCard(card);
        }
    }

    // ------------------------------

    #region Card Movement - From Field (Self)

    // Fanfare
    public class SveOnCardEnterFieldTrigger : SveTrigger { }
    public class SveOnCardEnterFieldFromHandTrigger : SveTrigger { }
    public class SveOnCardEnterFieldFromNotHandTrigger : SveTrigger { }

    // Other
    public class SveLastWordsTrigger : SveTriggerWithFilter { }
    public class SveOnCardReturnToHandFromField : SveTrigger { }
    public class SveOnCardLeaveFieldTrigger : SveTrigger { }

    #endregion

    // -----

    #region Card Movement - From Field (Other Card)

    // Player cards
    public class SveOnOtherCardEnterFieldTrigger : SveTriggerWithFilter { }
    public class SveOnOtherCardLeaveFieldTrigger : SveTriggerWithFilter { }
    public class SveOnOtherCardReturnToHandFromField : SveTriggerWithFilter { }

    // Opponent cards
    public class SveOnOpponentCardLeaveFieldTrigger : SveTriggerWithFilter { }
    public class SveOnOpponentCardDestroyedTrigger : SveTriggerWithFilter { }

    #endregion

    // -----

    #region Card Movement - Other

    public class SveOnDiscardedTrigger : SveTrigger { }

    #endregion

    // ------------------------------

    #region Card/Player Actions

    public class SveOnEvolveTrigger : SveTrigger { }
    public class SveOnOtherEvolveTrigger : SveTriggerWithFilter { }

    public class SveOnRaceTrigger : SveTrigger { }
    public class SveOnOtherRaceTrigger : SveTriggerWithFilter { }

    public class SveOnPlaySpellTrigger : SveTriggerWithFilter { }

    #endregion

    // ------------------------------

    #region Combat

    public class SveOnAttackTrigger : SveTriggerWithFilter { }
    public class SveOnAttackFollowerTrigger : SveTriggerWithFilter { }
    public class SveOnAttackLeaderTrigger : SveTrigger { }

    public class SveOnOtherCardAttackTrigger : SveTriggerWithFilter { }

    public class SveOnDealCombatDamageTrigger : SveTrigger { }

    #endregion

    // ------------------------------

    #region Game Phases

    public class SveStartMainPhaseTrigger : SveTrigger { }

    public class SveStartOpponentMainPhaseTrigger : SveTrigger { }

    public class SveStartEndPhaseTrigger : SveTrigger { }

    #endregion

    // ------------------------------

    #region Other

    public class SveOnLeaderGainDefenseTrigger : SveTrigger { }
    public class SveOnSelectedForAbilityTrigger : SveTrigger { }

    public class SpellAbility : SveTrigger { }

    public class PassiveAbilityOnField : SveTrigger
    {
        [EnumField("Target", width = 200), Order(1)]
        public SVEProperties.SVEEffectTarget target = SVEProperties.SVEEffectTarget.Self;

        [StringField("Target Filter", width = 200), Order(2)]
        public string filter;
    }

    public class ModifiedCostTrigger : SveTrigger { }

    #endregion
}
