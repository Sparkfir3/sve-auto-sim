using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sparkfire.Utility;
using UnityEngine;
using CCGKit;
using SVESimulator.UI;

namespace SVESimulator
{
    public partial class ComplexEffect : SveEffect
    {
        #region Perform Effect

        private IEnumerator PerformEffect(bool ignoreCosts = false, Action onComplete = null)
        {
            string effectName = function.NextWord(pointerL, out pointerR);
            ComplexLog(LogMode.Perform, $"Effect Name = {effectName}");

            string arguments = function[pointerR..].TextInsideBraces(out _, out int pointerEffectR);
            pointerR += pointerEffectR;
            ComplexLog(LogMode.Perform, $"Args: {arguments}");
            pointerL = pointerR;

            string overrideAmount = null;
            int? overrideTriggerInstanceId = null;
            string overrideTriggerZone = null;
            if(!arguments.IsNullOrWhiteSpace())
            {
                string token = arguments.NextWord(0, out int argPointer);
                ComplexLog(LogMode.Perform, $"Token: {token}");
                switch(token)
                {
                    case "amount":
                        arguments.NextWord(argPointer, out argPointer); // move past '='
                        overrideAmount = ReplaceWithVariableValues(arguments[argPointer..].Trim());
                        ComplexLog(LogMode.Perform, $"Override Amount: {arguments[argPointer..].Trim()} => {overrideAmount}");
                        break;
                    case "trigger":
                        arguments.NextWord(argPointer, out argPointer); // move past '='
                        RuntimeCard overrideTriggerCard = (variables.GetValueOrDefault(arguments[argPointer..].Trim(), null) as CE_Card)?.card;
                        if(overrideTriggerCard != null)
                        {
                            overrideTriggerInstanceId = overrideTriggerCard.instanceId;
                            // assuming if the card doesn't exist anywhere on the board, it is currently in the deck
                            CardObject cardObject = CardManager.Instance.GetCardByInstanceId(overrideTriggerCard.instanceId);
                            overrideTriggerZone = cardObject && cardObject.CurrentZone ? cardObject.CurrentZone.Runtime.name : SVEProperties.Zones.Deck;
                        }
                        ComplexLog(LogMode.Perform, $"Override Trigger: {arguments[argPointer..].Trim()} => " +
                            $"Instance ID {overrideTriggerInstanceId} in zone {overrideTriggerZone}");
                        break;
                    default:
                        break;
                }
            }

            yield return EffectSequence.ResolveEffectsAsSequence(new List<string>() { effectName }, player,
                overrideTriggerInstanceId ?? triggerInstanceId, overrideTriggerZone ?? triggerZone,
                sourceInstanceId, sourceZone,
                onComplete, overrideAmount: overrideAmount, ignoreCosts: ignoreCosts);
            yield return new WaitForEndOfFrame();
        }

        private IEnumerator PerformIfElse(bool ignoreCosts = false)
        {
            pointerR = function.IndexOf("\n", pointerL, StringComparison.Ordinal);
            if(pointerR == -1)
                pointerR = function.Length;
            string line = function[pointerL..pointerR];
            string[] splitA = line.Split(" then ");
            if(splitA.Length <= 1)
                yield break;
            string[] splitB = splitA[1].Split(" else ");

            string variable = ReplaceWithVariableValues(splitA[0]).Trim();
            string ifTrue = splitB[0].Trim();
            string ifFalse = splitB.Length > 1 ? splitB[1].Trim() : null;
            bool isTrue = SVEFormulaParser.ParseValueAsCondition(variable, player, null as RuntimeCard);
            ComplexLog(LogMode.Perform, $"Condition = {splitA[0]} => {variable} => {isTrue}\nTrue = {ifTrue} // False = {ifFalse}");

            if(isTrue)
                yield return EffectSequence.ResolveEffectsAsSequence(new List<string>() { ifTrue }, player, triggerInstanceId, triggerZone, sourceInstanceId, sourceZone, null);
            else if(!ifFalse.IsNullOrWhiteSpace())
                yield return EffectSequence.ResolveEffectsAsSequence(new List<string>() { ifFalse }, player, triggerInstanceId, triggerZone, sourceInstanceId, sourceZone, null);
            yield return new WaitForEndOfFrame();
        }

        #endregion

        // ------------------------------

        #region Effect Cost

        private async Task<CE_Object> PayEffectCost(string effectName)
        {
            // Init
            CardObject card = CardManager.Instance.GetCardByInstanceId(sourceInstanceId);
            if(!card)
                return null;
            Ability ability = card.LibraryCard.abilities.FirstOrDefault(x => x.name.Equals(effectName));
            SveTrigger trigger = (ability as TriggeredAbility)?.trigger as SveTrigger;
            List<Cost> costs = trigger?.Costs;
            List<MoveCardToZoneData> movedCardsData = null;
            List<RemoveCounterData> removedCountersData = null;

            // Select pay or decline
            bool waiting = false;
            if(costs != null && !costs.Any(x => x is IgnorePromptAsCost))
            {
                waiting = true;
                bool canPayCost = player.LocalEvents.CanPayCosts(card.RuntimeCard, costs, effectName);
                List<MultipleChoiceWindow.MultipleChoiceEntryData> costOptions = new()
                {
                    new MultipleChoiceWindow.MultipleChoiceEntryData
                    {
                        text = canPayCost ? "Pay Cost" : "Cannot Pay Cost",
                        onSelect = canPayCost ? () => waiting = false : null
                    },
                    new MultipleChoiceWindow.MultipleChoiceEntryData
                    {
                        text = "Decline",
                        onSelect = () =>
                        {
                            forceExit = true;
                            waiting = false;
                        }
                    },
                };
                GameUIManager.MultipleChoice.Open(player, card.LibraryCard.name, costOptions, LibraryCardCache.GetEffectText(card.RuntimeCard.cardId, effectName));
                GameUIManager.MultipleChoice.SetButtonActive(0, canPayCost);
                while(waiting && !BreakCondition)
                    await Task.Yield();
            }
            if(forceExit)
                return null;

            // Pay cost
            waiting = true;
            player.LocalEvents.PayAbilityCosts(card, costs, effectName, (movedCards, removedCounters) =>
            {
                movedCardsData = movedCards;
                removedCountersData = removedCounters;
                waiting = false;
            });

            // Wait
            while(waiting && !BreakCondition)
                await Task.Yield();
            await Task.Delay(200);
            ComplexLog(LogMode.Value, $"[Pay Effect Cost] Instance ID {(card != null ? card.RuntimeCard.instanceId : "null")} / Effect {effectName}");
            return new CE_EffectCost
            {
                movedCardsData = movedCardsData,
                removedCountersData = removedCountersData
            };
        }

        #endregion

        // ------------------------------

        #region Other

        private async Task<CE_Object> RevealTopDeck()
        {
            bool waiting = true;
            RuntimeCard card = null;
            player.LocalEvents.FlipTopDeckToFaceUp(async revealedCard =>
            {
                await Task.Delay(400);
                player.LocalEvents.FlipTopDeckToFaceDown(revealedCard);
                card = revealedCard.RuntimeCard;
                waiting = false;
            });

            // Wait
            while(waiting && !BreakCondition)
                await Task.Yield();
            await Task.Delay(200);
            ComplexLog(LogMode.Value, $"[Reveal Top Deck] Instance ID {(card != null ? card.instanceId : "null")}");
            return card != null ? new CE_Card
            {
                card = card
            } : null;
        }

        private async Task<CE_Object> MillDeck(string millCountRaw)
        {
            bool waiting = true;
            List<RuntimeCard> cardList = null;
            int millCount;
            try
            {
                millCount = int.Parse(millCountRaw);
            }
            catch
            {
                millCount = 0;
            }

            player.LocalEvents.MillDeck(true, millCount, async milledCards =>
            {
                await Task.Delay(400);
                cardList = milledCards;
                waiting = false;
            });

            // Wait
            while(waiting && !BreakCondition)
                await Task.Yield();
            await Task.Delay(200);
            ComplexLog(LogMode.Value, $"[Mill Deck] Milled {cardList.Count} cards, Instance IDs {string.Join(",", cardList.Select(x => x.instanceId))}");
            return cardList != null ? new CE_CardList
            {
                cardList = cardList
            } : null;
        }

        #endregion
    }
}
