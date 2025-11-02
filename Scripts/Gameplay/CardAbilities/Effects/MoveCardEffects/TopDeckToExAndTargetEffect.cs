using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CCGKit;
using Sparkfire.Utility;

namespace SVESimulator
{
    public class SveTopDeckToExAndTargetEffect : SveTopDeckToExEffect
    {
        [StringField("Effect 1", width = 200), Order(11)]
        public string effectName1;
        [StringField("Effect 2", width = 200), Order(12)]
        public string effectName2;
        [StringField("Effect 3", width = 200), Order(13)]
        public string effectName3;
        [StringField("Effect 4", width = 200), Order(13)]
        public string effectName4;
        [StringField("Effect 5", width = 200), Order(14)]
        public string effectName5;

        public List<string> allEffects => new() { effectName1, effectName2, effectName3, effectName4, effectName5 };

        // ------------------------------

        public override void Resolve(PlayerController player, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, Action onComplete = null)
        {
            SVEEffectPool.Instance.StartCoroutine(ResolveCoroutine());

            IEnumerator ResolveCoroutine()
            {
                // Move cards
                int count = SVEFormulaParser.ParseValue(amount, player);
                List<CardObject> cards = new();
                for(int i = 0; i < count; i++)
                    cards.Add(player.LocalEvents.TopDeckToExArea());
                if(cards.Count == 0)
                    yield break;

                // Perform effects
                player.ZoneController.AllZones[sourceCardZone].TryGetCard(sourceCardInstanceId, out CardObject cardObject);
                Card libraryCard = LibraryCardCache.GetCard(cardObject.RuntimeCard.cardId, GameManager.Instance.config);
                List<Ability> sveAbilities = libraryCard.abilities.FindAll(x => x is TriggeredAbility { effect: SveEffect and not SveEffectSequence });

                foreach(string effectName in allEffects)
                {
                    if(effectName.IsNullOrWhiteSpace())
                        continue;

                    // Fetch ability
                    Ability baseAbility = sveAbilities.FirstOrDefault(x => x.name.Trim().Equals(effectName.Trim()));
                    if(baseAbility == null)
                    {
                        Debug.LogWarning($"Attempted to resolve sequenced ability {effectName} from card {libraryCard.name}, but failed to find the ability");
                        continue;
                    }
                    if(baseAbility is TriggeredAbility { trigger: SveTrigger trigger }
                       && !string.IsNullOrWhiteSpace(trigger.condition) && !SVEFormulaParser.ParseValueAsCondition(trigger.condition, player, cardObject))
                        continue;

                    // Perform effect
                    bool effectDone = false;
                    SveEffect effectToPerform = (baseAbility.effect as SveEffect).CopyWithAddFilters($"i({string.Join(",", cards.Select(x => x.RuntimeCard.instanceId))})");
                    effectToPerform.Resolve(player, triggeringCardInstanceId, triggeringCardZone, sourceCardInstanceId, sourceCardZone, onComplete: () =>
                    {
                        effectDone = true;
                    });
                    yield return new WaitUntil(() => effectDone);
                    yield return new WaitForEndOfFrame();
                }
                onComplete?.Invoke();
            }
        }
    }
}
