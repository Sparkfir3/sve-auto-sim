using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
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
            return SVEFormulaParser.ParseCardFilterFormula(filter, card.instanceId).MatchesCard(card);
        }
    }

    // ------------------------------

    #region Card Movement

    public class SveOnCardEnterFieldTrigger : SveTrigger { }

    public class SveLastWordsTrigger : SveTrigger { }

    public class SveOnCardReturnToHandFromField : SveTrigger { }

    public class SveOnCardLeaveFieldTrigger : SveTrigger { }

    public class SveOnOtherCardEnterFieldTrigger : SveTriggerWithFilter { }

    public class SveOnOtherCardLeaveFieldTrigger : SveTriggerWithFilter { }

    public class SveOnOpponentCardLeaveFieldTrigger : SveTriggerWithFilter { }

    // -----

    public class SveOnDiscardTrigger : SveTrigger { }

    #endregion

    // ------------------------------

    #region Card/Player Actions

    public class SveOnEvolveTrigger : SveTrigger { }

    public class SveOnOtherEvolveTrigger : SveTriggerWithFilter { }

    public class SveOnAttackTrigger : SveTrigger { }

    public class SveOnAttackFollowerTrigger : SveTrigger { }

    public class SveOnAttackLeaderTrigger : SveTrigger { }

    public class SveOnOtherCardAttackTrigger : SveTriggerWithFilter { }

    public class SveOnPlaySpellTrigger : SveTrigger { }

    #endregion

    // ------------------------------

    #region Game Phases

    public class SveStartMainPhaseTrigger : SveTrigger { }

    public class SveStartOpponentMainPhaseTrigger : SveTrigger { }

    public class SveStartEndPhaseTrigger : SveTrigger { }

    #endregion

    // ------------------------------

    #region Other

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
