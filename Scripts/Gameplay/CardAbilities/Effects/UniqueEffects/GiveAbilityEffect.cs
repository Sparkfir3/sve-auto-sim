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

        // ------------------------------

        public override void Resolve(PlayerController player, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, Action onComplete = null)
        {
            ResolveOnTarget(player, triggeringCardInstanceId, triggeringCardZone, sourceCardInstanceId, sourceCardZone, target, filter, onTargetFound: targets =>
            {
                foreach(CardObject card in targets)
                {
                    GiveAbilityPassive effect = new()
                    {
                        duration = SVEProperties.PassiveDuration.WhileOnField,
                        effectName = effectName
                    };
                    effect.GetAbility(LibraryCardCache.GetCardFromInstanceId(sourceCardInstanceId).id); // cache ability data by fetching

                    RegisteredPassiveAbility passive = new()
                    {
                        sourceCardInstanceId = card.RuntimeCard.instanceId,
                        targetsFormula = null,
                        filters = new Dictionary<SVEFormulaParser.CardFilterSetting, string>(),
                        effect = effect,
                        affectedCards = new List<RuntimeCard>(),
                        target = SVEProperties.SVEEffectTarget.Self,
                        duration = SVEProperties.PassiveDuration.WhileOnField
                    };
                    SVEEffectPool.Instance.RegisterPassiveAbility(passive);
                }
                onComplete?.Invoke();
            });
        }
    }
}
