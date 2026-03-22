using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CCGKit;
using Sparkfire.Utility;

namespace SVESimulator
{
    public class ChooseAmountFromList : ChooseEffectFromList
    {
        [StringField("Amount", width = 100), Order(11)]
        public string amount;

        private bool _useSkipButton = false;
        protected override bool ShouldAddSkipButton => _useSkipButton;

        // ------------------------------

        protected override IEnumerator ResolveCoroutine(List<string> effectList, CardObject cardObject, Card libraryCard, List<Ability> sveAbilities,
            PlayerController player, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, Action onComplete, Action<string> onChooseOption = null)
        {
            SVEFormulaParser.ParseValueAsMinMax(amount, player, cardObject.RuntimeCard, out int minRemaining, out int maxRemaining);
            maxRemaining = Mathf.Min(maxRemaining, effectList.Count(x => x != null));
            minRemaining = Mathf.Min(minRemaining, maxRemaining);

            List<string> remainingEffectList = new(effectList);
            while(maxRemaining > 0)
            {
                bool waiting = true;
                _useSkipButton = minRemaining <= 0;
                yield return SVEEffectPool.Instance.StartCoroutine(base.ResolveCoroutine(remainingEffectList, cardObject, libraryCard, sveAbilities,
                    player, triggeringCardInstanceId, triggeringCardZone, sourceCardInstanceId, sourceCardZone,
                    onComplete: () => { waiting = false; },
                    onChooseOption: x =>
                    {
                        if(x.IsNullOrWhiteSpace())
                            maxRemaining = 0;
                        else
                            remainingEffectList.Remove(x);
                    }));
                yield return new WaitUntil(() => !waiting);
                yield return new WaitForSeconds(0.2f);

                minRemaining--;
                maxRemaining--;
            }
            onComplete?.Invoke();
        }
    }
}
