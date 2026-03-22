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
            Debug.Log($"[CE] Function Length: {function.Length}\n{function}");
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
                if(function[pointerL] == ' ' || function[pointerL] == '\n' || function[pointerL] == '\t' || function[pointerL] == '\r' || function[pointerL] == ';')
                {
                    pointerL++;
                    continue;
                }

                string token = function.NextWord(pointerL, out pointerR).ToLower();
                Debug.Log($"[CE] [Main Parser] Token: {token}\nPointers: {pointerL}, {pointerR}");
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
                pointerL = pointerR + 1;
                yield return null;
            }

            yield return null;
            onComplete?.Invoke();
        }

        // ------------------------------

        private IEnumerator ParseNewVariable(Dictionary<string, string> variables)
        {
            string variableName = function.NextWord(pointerL, out pointerL);
            Debug.Log($"[CE] [Parse Variable] var = {variableName}\nPointers: {pointerL}, {pointerR}");
            if(!function.NextWord(pointerL, out pointerL).Trim().Equals("="))
            {
                pointerR = pointerL;
                yield break;
            }

            pointerR = function.IndexOf('\n', pointerL);
            if(pointerR < pointerL)
                pointerR = function.Length - 1;
            string line = function[pointerL..pointerR].Trim();
            Debug.Log($"[CE] [Parse Variable] Line = {line}\nPointers: {pointerL}, {pointerR}");

            Task<string> task = ParseValue(line);
            yield return new WaitUntil(() => task.IsCompleted);
            string value = task.Result;

            variables[variableName] = value;
            Debug.Log($"[CE] [Parse Variable] var {variableName} = {value}\nPointers: {pointerL}, {pointerR}");
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
            Debug.Log($"[CE] [Reveal Top Deck] Instance ID {(card != null ? card.instanceId : "null")}");
            return card != null ? new CE_Card
            {
                card = card
            } : null;
        }

        // ------------------------------

        private IEnumerator PerformEffect(Dictionary<string, string> variables, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone,
            Action onComplete = null)
        {
            string effectName = function.NextWord(pointerL, out pointerR);
            Debug.Log($"[CE] [Perform Effect] Name = {effectName}\nPointers: {pointerL}, {pointerR}");

            string arguments = function[pointerR..].TextInsideBraces(out _, out int pointerEffectR);
            pointerR += pointerEffectR;
            Debug.Log($"[CE] [Perform Effect] Args = {arguments}\nPointers: {pointerL}, {pointerR}");
            pointerL = pointerR;

            string overrideAmount = null;
            if(!arguments.IsNullOrWhiteSpace())
            {
                string token = arguments.NextWord(0, out int argPointer);
                Debug.Log($"[CE] [Perform Effect] Token: {token}\nPointers: {pointerL}, {pointerR}");
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
                        Debug.Log($"[CE] [Perform Effect] Override Amount: {arguments[argPointer..].Trim()} => {overrideAmount}\nPointers: {pointerL}, {pointerR}");
                        break;
                    default:
                        break;
                }
            }

            yield return EffectSequence.ResolveEffectsAsSequence(new List<string>() { effectName }, player, triggeringCardInstanceId, triggeringCardZone, sourceCardInstanceId, sourceCardZone,
                onComplete, overrideAmount: overrideAmount);
            yield return new WaitForEndOfFrame();
        }
    }
}
