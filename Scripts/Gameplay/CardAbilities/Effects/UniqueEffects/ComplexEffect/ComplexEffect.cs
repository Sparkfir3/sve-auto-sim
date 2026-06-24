using System;
using System.Collections;
using System.Collections.Generic;
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
        [NonSerialized]
        private bool forceExit;

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
                ComplexLog(LogMode.Main, $"Token: {token}");
                pointerL = pointerR;
                switch(token)
                {
                    // Variables
                    case "let":
                        yield return ParseNewVariable(variables);
                        break;

                    // Perform effect
                    case "perform":
                        yield return PerformEffect(variables);
                        break;
                    case "performcostless":
                        yield return PerformEffect(variables, ignoreCosts: true);
                        break;
                    case "performif":
                        yield return PerformIfElse(variables);
                        break;

                    // Other
                    default:
                        break;
                }

                if(forceExit)
                {
                    onComplete?.Invoke();
                    yield break;
                }

                pointerL = pointerR + 1;
                yield return new WaitForEndOfFrame();
            }

            yield return new WaitForEndOfFrame();
            onComplete?.Invoke();
        }

        #endregion

        // ------------------------------

        #region Variable Parsing

        private IEnumerator ParseNewVariable(Dictionary<string, string> variables)
        {
            string variableName = function.NextWord(pointerL, out pointerL);
            ComplexLog(LogMode.Value, $"Variable Name = {variableName}");
            if(!function.NextWord(pointerL, out pointerL).Trim().Equals("="))
            {
                pointerR = pointerL;
                yield break;
            }

            pointerR = function.IndexOf('\n', pointerL);
            if(pointerR < pointerL)
                pointerR = function.Length - 1;
            string line = function[pointerL..pointerR].Trim();
            ComplexLog(LogMode.Value, $"Line = {line}");

            Task<string> task = ParseValue(line, variables);
            yield return new WaitUntil(() => task.IsCompleted);
            string value = task.Result;

            variables[variableName] = value;
            ComplexLog(LogMode.Value, $"var {variableName} = {value}");
        }

        private async Task<string> ParseValue(string line, Dictionary<string, string> variables)
        {
            // Init
            int pointer = line.IndexOf('.');
            if(pointer < 0)
                pointer = line.Length - 1;
            string token = line[..pointer].Trim();
            if(line[pointer] == '.')
                pointer++;

            string args = null;
            if(token.Contains('('))
            {
                args = token.TextInsideParentheses(out int left, out _);
                token = token[..left];
            }

            // Get root object
            CE_Object obj = token switch
            {
                "revealTopDeck" => await RevealTopDeck(),
                "payCost" => await PayEffectCost(args),
                _ => null
            };
            if(obj == null)
                return ReplaceWithVariableValues(line, variables);

            // Handle object properties/functions
            while(obj != null && obj is not CE_Value && !forceExit)
            {
                switch(obj)
                {
                    case CE_Card:
                    case CE_EffectCost:
                        string[] parameters = line[pointer..].TextInsideParentheses(out int paramsPointerL, out int paramsPointerR).Split();
                        if(paramsPointerL == -1 && paramsPointerR == -1)
                        {
                            paramsPointerL = line.Length - pointer;
                            paramsPointerR = paramsPointerL;
                        }
                        token = line[pointer..(pointer + paramsPointerL)];
                        pointer += paramsPointerR;
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
    }
}
