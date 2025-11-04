using System;
using System.Linq;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public class SveDestroyAndControllerPerformEffect : SveEffect
    {
        [EnumField("Target", width = 200), Order(1)]
        public SVEProperties.SVEEffectTarget target = SVEProperties.SVEEffectTarget.TargetPlayerCardEx;

        [StringField("Target Filter", width = 100), Order(2)]
        public string filter;

        [StringField("Effect", width = 200), Order(11)]
        public string effectName;

        // ------------------------------

        public override void Resolve(PlayerController player, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, Action onComplete = null)
        {
            ResolveOnTarget(player, triggeringCardInstanceId, triggeringCardZone, sourceCardInstanceId, sourceCardZone, target, filter, onTargetFound: targets =>
            {
                if(targets.Count == 0)
                {
                    onComplete?.Invoke();
                    return;
                }

                foreach(CardObject card in targets)
                {
                    player.LocalEvents.DestroyCard(card);
                }

                if(targets[0].RuntimeCard.ownerPlayer == player.GetPlayerInfo())
                {
                    Ability abilityToResolve = LibraryCardCache.GetCard(sourceCardInstanceId, GameManager.Instance.config).abilities.FirstOrDefault(x => x.name.Trim().Equals(effectName.Trim()));
                    if(abilityToResolve == null)
                    {
                        Debug.LogError($"Failed to find effect \"{effectName}\" from card with instance ID {sourceCardInstanceId}");
                        onComplete?.Invoke();
                        return;
                    }
                    (abilityToResolve.effect as SveEffect)?.Resolve(player, triggeringCardInstanceId, triggeringCardZone, sourceCardInstanceId, sourceCardZone, onComplete);
                }
                else
                {
                    CardObject cardObject = CardManager.Instance.GetCardByInstanceId(sourceCardInstanceId);
                    player.LocalEvents.TellOpponentToPerformEffect(cardObject, effectName);
                    onComplete?.Invoke();
                }
            });
        }
    }
}
