using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sparkfire.Utility;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public partial class ComplexEffect : SveEffect
    {
        #region Variables

        [StringField("Function", width = 200), Order(1)]
        public string function;

        [NonSerialized]
        private PlayerController player;
        [NonSerialized]
        private int pointerL = 0, pointerR = 0;
        [NonSerialized]
        private int triggerInstanceId, sourceInstanceId;
        [NonSerialized]
        private string triggerZone, sourceZone;

        #endregion

        // ------------------------------

        #region Main Resolve

        public override void Resolve(PlayerController player, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, Action onComplete = null)
        {
            ComplexLog(LogMode.Main, $"{function}\nLength = {function.Length}");

            this.player = player;
            triggerInstanceId = triggeringCardInstanceId;
            triggerZone = triggeringCardZone;
            sourceInstanceId = sourceCardInstanceId;
            sourceZone = sourceCardZone;
            SVEEffectPool.Instance.StartCoroutine(ResolveOverTime(onComplete));
        }

        private IEnumerator ResolveOverTime(Action onComplete)
        {
            Dictionary<string, string> variables = new();
            pointerL = 0;
            pointerR = 0;

            while(pointerL < function.Length)
            {
                if(function[pointerL] == ' ' || function[pointerL] == '\n' || function[pointerL] == '\t' || function[pointerL] == '\r' || function[pointerL] == ';')
                {
                    pointerL++;
                    continue;
                }

                string token = function.NextWord(pointerL, out pointerR).ToLower();
                ComplexLog(LogMode.Main, $"Token: {token}\nPointers: {pointerL}, {pointerR}");
                pointerL = pointerR;
                switch(token)
                {
                    case "let":
                        yield return ParseNewVariable(variables);
                        break;

                    case "perform":
                        yield return PerformEffect(variables);
                        break;
                    case "performcostless":
                        // TODO
                        break;

                    default:
                        break;
                }
                pointerL = pointerR + 1;
                yield return null;
            }

            yield return null;
            onComplete?.Invoke();
        }

        #endregion

        // ------------------------------

        #region Variable Parsing

        private IEnumerator ParseNewVariable(Dictionary<string, string> variables)
        {
            string variableName = function.NextWord(pointerL, out pointerL);
            ComplexLog(LogMode.Value, $"var = {variableName}\nPointers: {pointerL}, {pointerR}");
            if(!function.NextWord(pointerL, out pointerL).Trim().Equals("="))
            {
                pointerR = pointerL;
                yield break;
            }

            pointerR = function.IndexOf('\n', pointerL);
            if(pointerR < pointerL)
                pointerR = function.Length - 1;
            string line = function[pointerL..pointerR].Trim();
            ComplexLog(LogMode.Value, $"Line = {line}\nPointers: {pointerL}, {pointerR}");

            Task<string> task = ParseValue(line);
            yield return new WaitUntil(() => task.IsCompleted);
            string value = task.Result;

            variables[variableName] = value;
            ComplexLog(LogMode.Value, $"var {variableName} = {value}\nPointers: {pointerL}, {pointerR}");
        }

        private async Task<string> ParseValue(string line)
        {
            int pointer = line.IndexOf('.');
            if(pointer < 0)
                pointer = line.Length - 1;
            string token = line[..pointer].Trim();
            if(line[pointer] == '.')
                pointer++;

            CE_Object obj = token switch
            {
                "revealTopDeck" => await RevealTopDeck(),
                _ => null
            };
            while(obj != null && obj is not CE_Value)
            {
                switch(obj)
                {
                    case CE_Card:
                        string[] parameters = line[pointer..].TextInsideParentheses(out int valuePointerL, out int valuePointerR).Split();
                        token = line[pointer..(pointer + valuePointerL)];
                        pointer = valuePointerR;
                        obj = await obj.GetValue(player, token, parameters);
                        break;

                    case CE_Value:
                        break;
                    default:
                        return "";
                }
            }
            return (obj as CE_Value)?.value ?? "";
        }

        #endregion

        // ------------------------------

        #region Get CE Object

        private async Task<CE_Object> RevealTopDeck()
        {
            bool waiting = true;
            RuntimeCard card = null;
            player.LocalEvents.RevealTopDeck(async revealedCard =>
            {
                await Task.Delay(400);
                player.LocalEvents.FlipTopDeckToFaceDown(revealedCard);
                card = revealedCard.RuntimeCard;
                waiting = false;
            });
            while(waiting || !player || !Application.isPlaying)
                await Task.Yield();
            await Task.Delay(200);
            ComplexLog(LogMode.Value, $"[Reveal Top Deck] Instance ID {(card != null ? card.instanceId : "null")}");
            return card != null ? new CE_Card
            {
                card = card
            } : null;
        }

        #endregion

        // ------------------------------

        #region Perform Effect

        private IEnumerator PerformEffect(Dictionary<string, string> variables, Action onComplete = null)
        {
            string effectName = function.NextWord(pointerL, out pointerR);
            ComplexLog(LogMode.Perform, $"Effect Name = {effectName}\nPointers: {pointerL}, {pointerR}");

            string arguments = function[pointerR..].TextInsideBraces(out _, out int pointerEffectR);
            pointerR += pointerEffectR;
            ComplexLog(LogMode.Perform, $"Args: {arguments}\nPointers: {pointerL}, {pointerR}");
            pointerL = pointerR;

            string overrideAmount = null;
            if(!arguments.IsNullOrWhiteSpace())
            {
                string token = arguments.NextWord(0, out int argPointer);
                ComplexLog(LogMode.Perform, $"Token: {token}\nPointers: {pointerL}, {pointerR}");
                switch(token)
                {
                    case "amount":
                        arguments.NextWord(argPointer, out argPointer); // move past '='
                        overrideAmount = arguments[argPointer..].Trim();
                        foreach(var kvPair in variables)
                        {
                            (string variable, string value) = (kvPair.Key, kvPair.Value);
                            overrideAmount = overrideAmount.Replace(variable, value);
                        }
                        ComplexLog(LogMode.Perform, $"Override Amount: {arguments[argPointer..].Trim()} => {overrideAmount}\nPointers: {pointerL}, {pointerR}");
                        break;
                    default:
                        break;
                }
            }

            yield return EffectSequence.ResolveEffectsAsSequence(new List<string>() { effectName }, player, triggerInstanceId, triggerZone, sourceInstanceId, sourceZone,
                onComplete, overrideAmount: overrideAmount);
            yield return new WaitForEndOfFrame();
        }

        #endregion
    }
}
