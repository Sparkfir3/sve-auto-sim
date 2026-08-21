using System;
using System.Collections.Generic;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public class GiveAbilityEffect : SveEffect
    {
        [EnumField("Target", width = 200), Order(1)]
        public SVEProperties.SVEEffectTarget target = SVEProperties.SVEEffectTarget.Self;

        [StringField("Target Filter", width = 100), Order(2)]
        public string filter;

        [StringField("Effect Name", width = 200), Order(3)]
        public string effectName;

        protected virtual SVEProperties.PassiveDuration duration => SVEProperties.PassiveDuration.WhileOnField;

        // ------------------------------

        public override void Resolve(PlayerController player, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, Action onComplete = null)
        {
            // Resolve on leader (floating ability)
            if(target == SVEProperties.SVEEffectTarget.Leader)
            {
                RegisteredPassiveAbility passive = new()
                {
                    sourceCardInstanceId = sourceCardInstanceId,
                    targetsFormula = null,
                    filters = new Dictionary<SVEFormulaParser.CardFilterSetting, string>(),
                    effect = GetPassive(sourceCardInstanceId),
                    affectedCards = new List<RuntimeCard>(),
                    target = SVEProperties.SVEEffectTarget.Leader,
                    duration = duration
                };
                SVEEffectPool.Instance.RegisterPassiveAbility(passive);
                onComplete?.Invoke();
                return;
            }

            // Normal resolve
            ResolveOnTarget(player, triggeringCardInstanceId, triggeringCardZone, sourceCardInstanceId, sourceCardZone, target, filter, onTargetFound: targets =>
            {
                foreach(CardObject card in targets)
                {
                    RegisteredPassiveAbility passive = new()
                    {
                        sourceCardInstanceId = card.RuntimeCard.instanceId,
                        targetsFormula = null,
                        filters = new Dictionary<SVEFormulaParser.CardFilterSetting, string>(),
                        effect = GetPassive(sourceCardInstanceId),
                        affectedCards = new List<RuntimeCard>(),
                        target = SVEProperties.SVEEffectTarget.Self,
                        duration = duration
                    };
                    SVEEffectPool.Instance.RegisterPassiveAbility(passive);
                }
                onComplete?.Invoke();
            });
        }

        protected GiveAbilityPassive GetPassive(int sourceCardInstanceId)
        {
            GiveAbilityPassive passive = new()
            {
                duration = duration,
                effectName = effectName
            };
            passive.GetAbility(LibraryCardCache.GetCardFromInstanceId(sourceCardInstanceId).id); // cache ability data by fetching
            return passive;
        }
    }
}
