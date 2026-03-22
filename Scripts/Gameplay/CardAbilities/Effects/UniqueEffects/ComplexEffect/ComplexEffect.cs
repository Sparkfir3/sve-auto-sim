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
        [StringField("Function", width = 200), Order(1)]
        public string function;

        private PlayerController player;
        private int pointerL = 0, pointerR = 0;

        // ------------------------------

        public override void Resolve(PlayerController player, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, Action onComplete = null)
        {
            this.player = player;
            SVEEffectPool.Instance.StartCoroutine(ResolveOverTime(triggeringCardInstanceId, triggeringCardZone, sourceCardInstanceId, sourceCardZone, onComplete));
        }

        private IEnumerator ResolveOverTime(int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, Action onComplete)
        {
            CardObject cardObject = CardManager.Instance.GetCardByInstanceId(sourceCardInstanceId);
            Dictionary<string, string> variables = new();

            pointerL = 0;
            pointerR = 0;
            while(pointerL < function.Length)
            {
                if(function[pointerL] == ' ' || function[pointerL] == '\n' || function[pointerL] == '\t' || function[pointerL] == ';')
                {
                    pointerL++;
                    continue;
                }

                string token = function.NextWord(pointerL, out pointerR).ToLower();
                pointerL = pointerR;
                switch(token)
                {
                    case "let":
                        yield return ParseNewVariable(variables);
                        break;

                    case "perform":
                        yield return PerformEffect(variables, triggeringCardInstanceId, triggeringCardZone, sourceCardInstanceId, sourceCardZone);
                        break;
                    case "performcostless":
                        // TODO
                        break;

                    default:
                        break;
                }
                pointerL = pointerR;
                yield return null;
            }

            yield return null;
            onComplete?.Invoke();
        }

        // ------------------------------

        private IEnumerator ParseNewVariable(Dictionary<string, string> variables)
        {
            string variableName = function.NextWord(pointerL, out pointerL);
            if(!function.NextWord(pointerL, out pointerL).Trim().Equals("="))
            {
                pointerR = pointerL;
                yield break;
            }

            pointerR = function.IndexOf(';', pointerL);
            if(pointerR < pointerL)
                pointerR = function.Length - 1;
            string line = function[pointerL..pointerR].Trim();

            Task<string> task = ParseValue(line);
            yield return new WaitUntil(() => task.IsCompleted);
            string value = task.Result;

            if(variables.ContainsKey(variableName))
                variables[variableName] = value;
            else
                variables.Add(variableName, value);
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
                        token = line[pointer..valuePointerL];
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

        private async Task<CE_Object> RevealTopDeck()
        {
            bool waiting = true;
            CardObject card = null;
            player.LocalEvents.RevealTopDeck(async revealedCard =>
            {
                await Task.Delay(400);
                player.LocalEvents.FlipTopDeckToFaceDown(revealedCard);
                card = revealedCard;
                waiting = false;
            });
            while(waiting || !player || !Application.isPlaying)
                await Task.Yield();
            return card && player ? new CE_Card()
            {
                card = card.RuntimeCard
            } : null;
        }

        // ------------------------------

        private IEnumerator PerformEffect(Dictionary<string, string> variables, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone,
            Action onComplete = null)
        {
            string effectName = function.NextWord(pointerL, out pointerL);
            pointerR = function.IndexOf(';', pointerL);
            if(pointerR < pointerL)
                pointerR = function.Length - 1;

            string arguments = function[pointerL..].TextInsideBraces(out pointerL, out pointerR);
            string overrideAmount = null;
            pointerL = pointerR;
            if(!arguments.IsNullOrWhiteSpace())
            {
                string token = arguments.NextWord(0, out int argPointer);
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
                        break;
                    default:
                        break;
                }
            }

            yield return EffectSequence.ResolveEffectsAsSequence(new List<string>() { effectName }, player, triggeringCardInstanceId, triggeringCardZone, sourceCardInstanceId, sourceCardZone,
                onComplete, overrideAmount: overrideAmount);
        }
    }
}
