using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CCGKit;
using SVESimulator.UI;

namespace SVESimulator
{
    public class SvePerformAsEachTargetEffect : SveEffect
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
            CardObject cardObject = CardManager.Instance.GetCardByInstanceId(sourceCardInstanceId);
            Card libraryCard = LibraryCardCache.GetCard(cardObject.RuntimeCard.cardId, GameManager.Instance.config);
            Ability abilityToResolve = libraryCard?.abilities?.FirstOrDefault(x => x is TriggeredAbility { effect: SveEffect and not SveEffectSequence }
                && x.name.Trim().Equals(effectName.Trim()));
            if(abilityToResolve == null)
            {
                Debug.LogError($"Failed to find ability {effectName} on card {(libraryCard?.name ?? "null")} to resolve effect \"perform as each target\" effect");
                onComplete?.Invoke();
                return;
            }

            ResolveOnTarget(player, triggeringCardInstanceId, triggeringCardZone, sourceCardInstanceId, sourceCardZone, target, filter, onTargetFound: targets =>
            {
                SVEEffectPool.Instance.StartCoroutine(ResolveCoroutine(targets));

            });

            // ---

            IEnumerator ResolveCoroutine(List<CardObject> targets)
            {
                SveEffect effectToPerform = (abilityToResolve.effect as SveEffect).CopyWithOverrideTargetFilter(SVEProperties.SVEEffectTarget.Self, null);
                List<CardObject> targetsRemaining = new(targets);
                while(targetsRemaining.Count > 0)
                {
                    bool waitingOnSelect = true;
                    CardObject selectedTarget = null;
                    List<MultipleChoiceWindow.MultipleChoiceEntryData> choices = new();
                    foreach(CardObject targetCard in targetsRemaining)
                    {
                        choices.Add(new MultipleChoiceWindow.MultipleChoiceEntryData()
                        {
                            text = targetCard.LibraryCard.name,
                            onSelect = () =>
                            {
                                selectedTarget = targetCard;
                                waitingOnSelect = false;
                            }
                        });
                    }
                    GameUIManager.MultipleChoice.Open(player, libraryCard.name, choices, $"Select next target for effect: {text}");
                    yield return new WaitUntil(() => !waitingOnSelect);
                    Debug.Assert(selectedTarget);

                    bool effectDone = false;
                    effectToPerform.Resolve(player, triggeringCardInstanceId, triggeringCardZone, selectedTarget.RuntimeCard.instanceId, selectedTarget.CurrentZone.Runtime.name,
                        onComplete: () =>
                    {
                        effectDone = true;
                    });
                    yield return new WaitUntil(() => effectDone);
                    targetsRemaining.Remove(selectedTarget);

                    yield return new WaitForEndOfFrame();
                }
                onComplete?.Invoke();
            }
        }
    }
}
