using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public class SveOpponentPerformEffect : SveEffect
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
            SVEEffectPool.Instance.StartCoroutine(ResolveCoroutine());
            IEnumerator ResolveCoroutine()
            {
                // delay makes sure "PlaySpell" messages send before this, allowing for the spell to hit resolution zone first (fix race condition error)
                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();
                Debug.Assert(SVEEffectPool.Instance.IsResolvingEffect);

                CardObject cardObject = CardManager.Instance.GetCardByInstanceId(sourceCardInstanceId);
                ResolveOnTarget(player, triggeringCardInstanceId, triggeringCardZone, sourceCardInstanceId, sourceCardZone, target, filter, onTargetFound: targets =>
                {
                    int[] targetInstanceIds = target != SVEProperties.SVEEffectTarget.Self ? targets.Select(x => x.RuntimeCard.instanceId).ToArray() : null;
                    player.LocalEvents.TellOpponentToPerformEffect(cardObject, effectName, targetInstanceIds);
                    onComplete?.Invoke();
                });
                // Wait for PlayerEventControllerOpponent.TellPerformEffect to set IsResolvingEffect to false
                yield return new WaitUntil(() => !SVEEffectPool.Instance.IsResolvingEffect);
            }
        }
    }
}
