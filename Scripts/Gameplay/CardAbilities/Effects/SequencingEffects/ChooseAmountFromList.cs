using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CCGKit;

namespace SVESimulator
{
    public class ChooseAmountFromList : ChooseEffectFromList
    {
        [StringField("Amount", width = 100), Order(11)]
        public string amount;

        // ------------------------------

        protected override IEnumerator ResolveCoroutine(List<string> effectList, CardObject cardObject, Card libraryCard, List<Ability> sveAbilities,
            PlayerController player, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, Action onComplete, Action<string> onChooseOption = null)
        {
            int amountRemaining = Mathf.Min(SVEFormulaParser.ParseValue(amount, player, cardObject), effectList.Count(x => x != null));
            List<string> remainingEffectList = new(effectList);
            for(; amountRemaining > 0; amountRemaining--)
            {
                bool waiting = true;
                yield return SVEEffectPool.Instance.StartCoroutine(base.ResolveCoroutine(remainingEffectList, cardObject, libraryCard, sveAbilities,
                    player, triggeringCardInstanceId, triggeringCardZone, sourceCardInstanceId, sourceCardZone,
                    onComplete: () => { waiting = false; },
                    onChooseOption: x => { remainingEffectList.Remove(x); }));
                yield return new WaitUntil(() => !waiting);
                yield return new WaitForSeconds(0.2f);
            }
            onComplete?.Invoke();
        }
    }
}
