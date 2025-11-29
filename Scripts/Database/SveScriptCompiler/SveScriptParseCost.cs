using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Sparkfire.Utility;
using UnityEngine;
using static SVESimulator.SveScript.SveScriptKeywordCompiler;

namespace SVESimulator.SveScript
{
    internal static partial class SveScriptAbilityCompiler
    {
        private static List<JObject> ParseAbilityCosts(in string text)
        {
            List<JObject> costs = new();
            int leftPointer = text.IndexOf('(');
            if(leftPointer == -1)
                return costs;

            while(leftPointer < text.Length)
            {
                if(text[leftPointer] == ' ' || text[leftPointer] == ',')
                {
                    leftPointer++;
                    continue;
                }

                string substring = text[leftPointer..].TextInsideParentheses(out _, out int rightPointer);
                if(rightPointer == -1)
                    throw new ArgumentException();
                string[] args = SveScriptEffectCompiler.SplitArgsArray(substring);

                JObject newCost = new();
                switch(args[0].Trim())
                {
                    case "PP":
                        string pointCost = string.Join("", args[1..]).Trim();
                        newCost.Add("amount", pointCost);
                        newCost.Add("$type", "SVESimulator.PlayPointCost");
                        break;
                    case "EngageSelf":
                        newCost.Add("$type", "SVESimulator.EngageSelfCost");
                        break;
                    case "Discard":
                        newCost.Add("amount", args.Length > 1 ? args[1].Trim() : "1");
                        newCost.Add("filter", args.Length > 2 ? args[2].Trim() : "");
                        newCost.Add("$type", "SVESimulator.DiscardCardCost");
                        break;
                    case "BanishFromCemetery":
                        newCost.Add("amount", args.Length > 1 ? args[1].Trim() : "1");
                        newCost.Add("filter", args.Length > 2 ? args[2].Trim() : "");
                        newCost.Add("$type", "SVESimulator.BanishFromCemeteryCost");
                        break;
                    case "LeaderDefense":
                    case "LeaderDef":
                        string defCost = string.Join("", args[1..]).Trim();
                        newCost.Add("amount", defCost);
                        newCost.Add("$type", "SVESimulator.LeaderDefenseCost");
                        break;
                    case "SendToCemetery":
                    case "ReturnToHand":
                    case "Banish":
                        if(args.Length < 2)
                            throw new ArgumentException();
                        newCost.Add("target", args[1].Trim());
                        newCost.Add("filter", args.Length > 2 ? args[2] : "");
                        newCost.Add("$type", $"SVESimulator.{args[0].Trim()}Cost");
                        break;
                    case "RemoveCounters":
                        if(args.Length < 2)
                            throw new ArgumentException();
                        Keyword counterToRemove = GetKeyword(args[1]);
                        newCost.Add("keywordType",   counterToRemove.keywordId);
                        newCost.Add("keywordValue",  counterToRemove.valueId);
                        newCost.Add("amount",        args.Length >= 3 ? args[2].Trim() : "");
                        newCost.Add("target",        args.Length >= 4 ? args[3] : "Self");
                        newCost.Add("filter",        args.Length >= 5 ? args[4] : "");
                        newCost.Add("$type",         "SVESimulator.RemoveCountersCost");
                        break;
                    case "EarthRite":
                        Keyword stackKeyword = GetKeyword("Stack");
                        newCost.Add("keywordType",   stackKeyword.keywordId);
                        newCost.Add("keywordValue",  stackKeyword.valueId);
                        newCost.Add("amount",        args.Length >= 2 ? args[1].Trim() : "1");
                        newCost.Add("target",        "TargetPlayerCard");
                        newCost.Add("filter",        "A");
                        newCost.Add("$type",         "SVESimulator.RemoveCountersCost");
                        break;
                    case "HasEvolveTarget":
                    case "OncePerTurn":
                        newCost.Add("$type", $"SVESimulator.{args[0].Trim()}Cost");
                        break;
                    default:
                        Debug.LogError($"Invalid cost arg {args[0]} found in line: {text}");
                        break;
                }
                costs.Add(newCost);

                leftPointer += rightPointer + 1;
            }
            return costs;
        }
    }
}
