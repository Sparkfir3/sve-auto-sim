using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CCGKit;
using SVESimulator.UI;

namespace SVESimulator
{
    public class SveEffectSequence : SveEffect
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
            SVEEffectPool.Instance.StartCoroutine(ResolveCoroutine());
            IEnumerator ResolveCoroutine()
            {
                CardObject cardObject = player.GetCardInZoneFromBothPlayers(sourceCardInstanceId, sourceCardZone);
                Card libraryCard = LibraryCardCache.GetCard(cardObject.RuntimeCard.cardId, GameManager.Instance.config);
                List<Ability> sveAbilities = libraryCard.abilities.FindAll(x => x is TriggeredAbility { effect: SveEffect and not SveEffectSequence });
                foreach(string effectName in allEffects)
                {
                    if(string.IsNullOrWhiteSpace(effectName))
                        continue;

                    // Fetch ability
                    Ability abilityToResolve = sveAbilities.FirstOrDefault(x => x.name.Trim().Equals(effectName.Trim()));
                    if(abilityToResolve == null)
                    {
                        Debug.LogWarning($"Attempted to resolve sequenced ability {effectName} from card {libraryCard.name} which doesn't exist");
                        continue;
                    }

                    // Condition check
                    SveTrigger trigger = (abilityToResolve as TriggeredAbility)?.trigger as SveTrigger;
                    if(trigger != null && !string.IsNullOrWhiteSpace(trigger.condition) && !SVEFormulaParser.ParseValueAsCondition(trigger.condition, player, cardObject))
                        continue;

                    // Resolve with cost check
                    bool effectDone = false;
                    SveEffect effect = abilityToResolve.effect as SveEffect;
                    Action resolveAction = () =>
                    {
                        effect?.Resolve(player, triggeringCardInstanceId, triggeringCardZone, sourceCardInstanceId, sourceCardZone, () => { effectDone = true; });
                    };
                    if(trigger?.Costs is { Count: > 0 })
                        SelectPayCostOrDecline(player, cardObject, trigger, effect, effectName, resolveAction);
                    else
                        resolveAction.Invoke();
                    yield return new WaitUntil(() => effectDone);
                }

                onComplete?.Invoke();
            }
        }

        private void SelectPayCostOrDecline(PlayerController player, CardObject card, SveTrigger trigger, SveEffect effect, string effectName, Action onComplete)
        {
            if(trigger.Costs == null || trigger.Costs.Count == 0)
            {
                onComplete?.Invoke();
                return;
            }

            bool canPayCost = player.LocalEvents.CanPayCosts(card.RuntimeCard, trigger.Costs, effectName);
            List<MultipleChoiceWindow.MultipleChoiceEntryData> costOptions = new()
            {
                new MultipleChoiceWindow.MultipleChoiceEntryData
                {
                    text = canPayCost ? "Pay Cost" : "Cannot Pay Cost",
                    onSelect = () =>
                    {
                        player.LocalEvents.PayAbilityCosts(card, trigger.Costs, effect, effectName, onComplete);
                    }
                },
                new MultipleChoiceWindow.MultipleChoiceEntryData
                {
                    text = "Decline",
                    onSelect = onComplete
                },
            };
            GameUIManager.MultipleChoice.Open(player, card.LibraryCard.name, costOptions, effect.text);
            GameUIManager.MultipleChoice.SetButtonActive(0, canPayCost);
        }
    }
}
