using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CCGKit;
using SVESimulator.UI;

namespace SVESimulator
{
    public class ChooseEffectFromList : SveEffect
    {
        [StringField("Effect 1", width = 200), Order(1)]
        public string effectName1;
        [StringField("Effect 2", width = 200), Order(2)]
        public string effectName2;
        [StringField("Effect 3", width = 200), Order(3)]
        public string effectName3;
        [StringField("Effect 4", width = 200), Order(3)]
        public string effectName4;
        [StringField("Effect 5", width = 200), Order(4)]
        public string effectName5;

        public List<string> allEffects => new() { effectName1, effectName2, effectName3, effectName4, effectName5 };

        // ------------------------------

        public override void Resolve(PlayerController player, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, Action onComplete = null)
        {
            CardObject cardObject = CardManager.Instance.GetCardByInstanceId(sourceCardInstanceId);
            Card libraryCard = LibraryCardCache.GetCard(cardObject.RuntimeCard.cardId, GameManager.Instance.config);
            List<Ability> sveAbilities = libraryCard.abilities.FindAll(x => x is TriggeredAbility { effect: SveEffect });
            SVEEffectPool.Instance.StartCoroutine(ResolveCoroutine(allEffects, cardObject, libraryCard, sveAbilities,
                player, triggeringCardInstanceId, triggeringCardZone, sourceCardInstanceId, sourceCardZone, onComplete));
        }

        protected virtual IEnumerator ResolveCoroutine(List<string> effectList, CardObject cardObject, Card libraryCard, List<Ability> sveAbilities,
            PlayerController player, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, Action onComplete, Action<string> onChooseOption = null)
        {
            bool effectDone = false;
            List<MultipleChoiceWindow.MultipleChoiceEntryData> choices = new();
            foreach(string effectName in effectList)
            {
                if(string.IsNullOrWhiteSpace(effectName))
                    continue;

                // Fetch ability
                Ability abilityToResolve = sveAbilities.FirstOrDefault(x => x.name.Trim().Equals(effectName.Trim()));
                if(abilityToResolve == null || abilityToResolve.effect is not SveEffect effect)
                {
                    Debug.LogWarning($"Failed to find effect \"{effectName}\" from card \"{libraryCard.name}\" for multiple choice options");
                    continue;
                }

                // Condition & cost check
                bool enabled = true;
                SveTrigger trigger = (abilityToResolve as TriggeredAbility)?.trigger as SveTrigger;
                if(trigger != null)
                {
                    if((!string.IsNullOrWhiteSpace(trigger.condition) && !SVEFormulaParser.ParseValueAsCondition(trigger.condition, player, cardObject))
                        || !player.LocalEvents.CanPayCosts(cardObject.RuntimeCard, trigger.Costs, effectName))
                        enabled = false;
                }

                // Populate multiple choice window
                MultipleChoiceWindow.MultipleChoiceEntryData entry = new()
                {
                    text = effect.text ?? effectName,
                    onSelect = () =>
                    {
                        // no need for CanPayCost check, this button should be disabled if cost check above failed
                        player.LocalEvents.PayAbilityCosts(cardObject, trigger?.Costs, effect, effectName, () =>
                        {
                            effect.Resolve(player, triggeringCardInstanceId, triggeringCardZone, sourceCardInstanceId, sourceCardZone, () =>
                            {
                                onChooseOption?.Invoke(effectName);
                                effectDone = true;
                            });
                        });
                    },
                    disabled = !enabled
                };
                choices.Add(entry);
            }
            GameUIManager.MultipleChoice.Open(player, libraryCard.name, choices, text);
            yield return new WaitUntil(() => effectDone);

            onComplete?.Invoke();
        }
    }
}
