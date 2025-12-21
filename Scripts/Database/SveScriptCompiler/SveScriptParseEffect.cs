using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Sparkfire.Utility;
using UnityEngine;
using static SVESimulator.SveScript.SveScriptKeywordCompiler;

namespace SVESimulator.SveScript
{
    internal static class SveScriptEffectCompiler
    {
        #region Parsing

        public static JObject ParseAbilityEffect(in string text, out string effectCcgType)
        {
            JObject effectData = new();

            // Get args
            int rightPointer = text.IndexOf('(');
            string effectType = text[..rightPointer].Trim()
                .Replace("Counters", "Counter");
            string mainEffectArgs = text.TextInsideParentheses(out _, out int pointer);
            pointer++; // move past close parentheses
            if(!StandardEffectInfoDictionary.TryGetValue(effectType, out EffectParams effectParams))
                throw new ArgumentException($"Invalid effect type provided: {effectType}\nParsed from {effectType} effect. Raw text: {text}");
            effectCcgType = effectParams.ccgType;

            // Parse additional args
            string[] additionalArgs = text[pointer..].Trim().Split();
            for(int i = 0; i < additionalArgs.Length; i++)
            {
                switch(additionalArgs[i])
                {
                    case "":
                        break;
                    case "to":
                        i++;
                        if(effectParams.hasTarget)
                            effectData.Add("target", additionalArgs[i++]);
                        if(effectParams.hasFilter)
                        {
                            effectData.Add("filter", additionalArgs[i..].Length > 0 ? string.Join(' ', additionalArgs[i..]) : null);
                            i = additionalArgs.Length - 1;
                        }
                        break;
                    default:
                        throw new ArgumentException($"Invalid effect argument: {additionalArgs[i]}");
                }
            }
            if(effectParams.hasTarget)
                effectData.TryAdd("target", "Self");
            if(effectParams.hasFilter)
                effectData.TryAdd("filter", null);

            // Init parse main args
            string[] argsArray = SplitArgsArray(mainEffectArgs);

            // Parse main args - Standard
            ParseMainArgsArray(argsArray, effectParams, ref effectData);
            return effectData;
        }

        public static JObject ParsePassiveAbilityEffect(in string text, out string effectCcgType, JObject triggerData)
        {
            JObject effectData = new();

            // Get args
            int rightPointer = text.IndexOf('(');
            string effectType = text[..rightPointer].Trim();
            string mainEffectArgs = text.TextInsideParentheses(out _, out int pointer);
            pointer++; // move past close parentheses
            if(!PassiveEffectInfoDictionary.TryGetValue(effectType, out EffectParams effectParams))
                throw new ArgumentException($"Invalid effect type provided: {effectType}\nParsed from {effectType} effect. Raw text: {text}");
            effectCcgType = effectParams.ccgType;

            // Parse additional args
            string[] additionalArgs = text[pointer..].Trim().Split();
            for(int i = 0; i < additionalArgs.Length; i++)
            {
                switch(additionalArgs[i])
                {
                    case "":
                        break;
                    case "to":
                        i++;
                        triggerData.Add("target", additionalArgs[i++]);
                        triggerData.Add("filter", additionalArgs[i..].Length > 0 ? string.Join(' ', additionalArgs[i..]) : null);
                        i = additionalArgs.Length - 1;
                        break;
                    default:
                        throw new ArgumentException($"Invalid effect argument: {additionalArgs[i]}");
                }
            }
            triggerData.TryAdd("target", "Self");
            triggerData.TryAdd("filter", null);

            // Parse main args
            string[] argsArray = SplitArgsArray(mainEffectArgs);
            ParseMainArgsArray(argsArray, effectParams, ref effectData);
            return effectData;
        }

        public static string[] SplitArgsArray(in string args)
        {
            List<string> output = new();
            string currentArg = "";
            int openParenCount = 0, closeParenCount = 0, quoteCount = 0;
            for(int i = 0; i < args.Length; i++)
            {
                switch(args[i])
                {
                    case '(':
                        openParenCount++;
                        break;
                    case ')':
                        closeParenCount++;
                        break;
                    case '\"':
                        quoteCount++;
                        break;
                    case ',':
                        if(openParenCount == closeParenCount && quoteCount % 2 == 0)
                        {
                            currentArg = currentArg.Trim();
                            if(currentArg.StartsWith('\"') && currentArg.EndsWith('\"'))
                                currentArg = currentArg[1..^1];
                            if(!string.IsNullOrWhiteSpace(currentArg))
                                output.Add(currentArg);
                            currentArg = "";
                            continue;
                        }
                        break;
                }
                currentArg += args[i];
            }

            currentArg = currentArg.Trim();
            if(currentArg.StartsWith('\"') && currentArg.EndsWith('\"'))
                currentArg = currentArg[1..^1];
            if(!string.IsNullOrWhiteSpace(currentArg))
                output.Add(currentArg);
            return output.ToArray();
        }

        // -----

        private static void ParseMainArgsArray(in string[] argsArray, in EffectParams effectParams, ref JObject effectData)
        {
            for(int i = 0; i < effectParams.parameters.Length; i++)
            {
                string argument = i < argsArray.Length ? argsArray[i] : null;
                if(argument == null)
                {
                    if(effectParams.parameters[i] is EffectParameterType.Amount or EffectParameterType.Amount2)
                        argument = "1";
                    else if(effectParams.parameters[i] is not EffectParameterType.AmountDefaultNull and not EffectParameterType.FilterOptional)
                        Debug.LogError($"Invalid argument: did not find an argument at index {i} (of expected type {effectParams.parameters[i].ToString()}) for effect of type {effectParams.ccgType}" +
                            $"{(effectParams.parameters.Length > 0 ? $"\nExpected parameters of type(s): {string.Join(", ", effectParams.parameters)}" : "")}" +
                            $"\nReceived: ({string.Join(", ", argsArray)})");
                }

                switch(effectParams.parameters[i])
                {
                    case EffectParameterType.Amount:
                    case EffectParameterType.Amount2:
                    case EffectParameterType.SearchDeckAction:
                    case EffectParameterType.CreateTokenOption:
                    case EffectParameterType.TokenName:
                    case EffectParameterType.Filter:
                        string keyName = effectParams.parameters[i].ToString();
                        keyName = char.ToLowerInvariant(keyName[0]) + keyName[1..];
                        effectData.Add(keyName, argument);
                        break;
                    case EffectParameterType.AmountDefaultNull:
                        effectData.Add("amount", argument);
                        break;
                    case EffectParameterType.FilterOptional:
                        effectData.Add("filter", argument);
                        break;
                    case EffectParameterType.Keyword:
                        Keyword keyword = GetKeyword(argument);
                        effectData.Add("keywordType", keyword.keywordId);
                        effectData.Add("keywordValue", keyword.valueId);
                        break;
                    case EffectParameterType.StatType:
                        effectData.Add("targetStats", StatTypeDictionary.GetValueOrDefault(argument, ""));
                        break;
                    case EffectParameterType.PassiveDuration:
                        effectData.Add("duration", argument);
                        break;
                    case EffectParameterType.SingleEffect:
                        effectData.Add("effectName", argument);
                        break;

                    // Dynamic length arguments - must always be the last argument in the list
                    case EffectParameterType.CheckCardActions:
                        ParseCheckTopArgsArray(argsArray[i..], ref effectData);
                        return;
                    case EffectParameterType.ListOfEffects:
                        for(int j = 0; j < 5; j++)
                            effectData.Add($"effectName{j + 1}", (i + j) < argsArray.Length ? argsArray[i + j] : null);
                        return;
                }
            }
        }

        private static void ParseCheckTopArgsArray(in string[] argsArray, ref JObject effectData)
        {
            if(argsArray.Length > 4)
                throw new ArgumentException($"Invalid argument array: received ({string.Join(",", argsArray)}) with {argsArray.Length} parameters, but no more than 4 (1+3) are supported for CheckTop effect");

            // Amount is a generic parameter, but we don't generically parse it since we have to uniquely handle the rest anyways
            effectData.Add("amount", argsArray[0]);
            for(int i = 1; i <= 3; i++)
            {
                string action, filter, amount;
                if(i >= argsArray.Length)
                {
                    action = "None";
                    filter = null;
                    amount = null;
                }
                else
                {
                    string arg = argsArray[i].Trim();
                    if(arg[0] == '(' && arg[^1] == ')')
                        arg = arg[1..^1];
                    string[] subArgsArray = SplitArgsArray(arg);
                    action = subArgsArray[0];
                    filter = subArgsArray.Length >= 3 ? subArgsArray[1] : null;
                    amount = subArgsArray.Length >= 3 ? subArgsArray[2]
                        : (subArgsArray.Length == 2 ? subArgsArray[1] : null);
                }

                effectData.Add($"checkAction{i}", action);
                effectData.Add($"checkFilter{i}", filter);
                effectData.Add($"checkAmount{i}", amount);
            }
        }

        #endregion
        
        // ------------------------------
        
        #region Effect Parameters
        
        private enum EffectParameterType { Amount, Amount2, AmountDefaultNull, Keyword, StatType, SearchDeckAction, CheckCardActions, SingleEffect, ListOfEffects, CreateTokenOption, TokenName, Filter, FilterOptional, PassiveDuration }
        private readonly struct EffectParams
        {
            public readonly string ccgType;
            public readonly EffectParameterType[] parameters;
            public readonly bool hasTarget, hasFilter;
            
            public EffectParams(string ccgType, params EffectParameterType[] parameters) : this(ccgType, true, true, parameters) { }
            public EffectParams(string ccgType, bool hasTarget, bool hasFilter, params EffectParameterType[] parameters)
            {
                this.ccgType = ccgType;
                this.parameters = parameters;
                this.hasTarget = hasTarget;
                this.hasFilter = hasFilter;
            }
        }
        
        private static Dictionary<string, EffectParams> StandardEffectInfoDictionary = new()
        {
            // Movement - Deck to Zone
            { "DrawCard", new EffectParams("SVESimulator.DrawCardEffect",                               true, false, EffectParameterType.Amount) },
            { "DrawThenDamage", new EffectParams("SVESimulator.DrawThenDamageEffect",                   EffectParameterType.Amount, EffectParameterType.Amount2) },
            { "TopDeckToEx", new EffectParams("SVESimulator.TopDeckToExEffect",                         false, false, EffectParameterType.Amount) },
            { "TopDeckToExArea", new EffectParams("SVESimulator.TopDeckToExEffect",                     false, false, EffectParameterType.Amount) },
            { "TopDeckToExAndTarget", new EffectParams("SVESimulator.TopDeckToExAndTargetEffect",       false, false, EffectParameterType.Amount, EffectParameterType.ListOfEffects) },
            { "Search", new EffectParams("SVESimulator.SearchDeckEffect",                               false, false, EffectParameterType.Amount, EffectParameterType.Filter, EffectParameterType.SearchDeckAction) },
            { "Mill", new EffectParams("SVESimulator.MillDeckEffect",                                   false, false, EffectParameterType.Amount) },

            // Movement - Field/EX to Zone
            { "ReturnToHand", new EffectParams("SVESimulator.ReturnToHandEffect")                       },
            { "TopDeck", new EffectParams("SVESimulator.SendToTopDeckEffect")                           },
            { "BottomDeck", new EffectParams("SVESimulator.SendToBottomDeckEffect")                     },
            { "TopOrBottomDeck", new EffectParams("SVESimulator.SendToTopOrBottomDeckEffect")           },
            { "Destroy", new EffectParams("SVESimulator.DestroyCardEffect")                             },
            { "DestroyCard", new EffectParams("SVESimulator.DestroyCardEffect")                         },
            { "Banish", new EffectParams("SVESimulator.BanishCardEffect")                               },
            { "BanishCard", new EffectParams("SVESimulator.BanishCardEffect")                           },
            { "SendToEx", new EffectParams("SVESimulator.SendToExAreaEffect")                           },
            { "SendToExArea", new EffectParams("SVESimulator.SendToExAreaEffect")                       },
            { "PutExToFieldAndTarget", new EffectParams("SVESimulator.PutExToFieldAndTargetEffect",     EffectParameterType.ListOfEffects) },

            // Movement - Other
            { "Discard", new EffectParams("SVESimulator.DiscardEffect",                                             false, false, EffectParameterType.Amount, EffectParameterType.FilterOptional) },
            { "DiscardToBottomDeck", new EffectParams("SVESimulator.DiscardToBottomDeckEffect",                     false, false, EffectParameterType.Amount, EffectParameterType.FilterOptional) },
            { "DiscardFromOpponentHand", new EffectParams("SVESimulator.DiscardFromOpponentHandEffect",             false, false, EffectParameterType.Amount, EffectParameterType.FilterOptional) },
            { "Salvage", new EffectParams("SVESimulator.SalvageCardEffect",                                         false, false, EffectParameterType.Amount, EffectParameterType.Filter) },
            { "CemeteryToField", new EffectParams("SVESimulator.CemeteryToFieldEffect",                             true,  false, EffectParameterType.Amount, EffectParameterType.FilterOptional) },
            { "CemeteryToFieldAndTarget", new EffectParams("SVESimulator.CemeteryToFieldAndTargetEffect",           true,  false, EffectParameterType.Amount, EffectParameterType.Filter, EffectParameterType.ListOfEffects) },
            { "PlaySpellFromCemetery", new EffectParams("SVESimulator.PlaySpellFromCemeteryEffect",                 false, false, EffectParameterType.Filter) },
            { "PlaySpellFromCemeterySetCost", new EffectParams("SVESimulator.PlaySpellFromCemeterySetCostEffect",   false, false, EffectParameterType.Filter, EffectParameterType.Amount2) },

            // ------------------------------

            // Stat Effects
            { "DealDamage", new EffectParams("SVESimulator.DealDamageEffect",                           EffectParameterType.Amount) },
            { "DealDamageDivided", new EffectParams("SVESimulator.DealDamageDividedEffect",             false, true, EffectParameterType.Amount) },
            { "GiveStat", new EffectParams("SVESimulator.GiveStatBoostEffect",                          EffectParameterType.StatType, EffectParameterType.Amount) },
            { "SetStat", new EffectParams("SVESimulator.SetStatEffect",                                 EffectParameterType.StatType, EffectParameterType.Amount) },
            { "EngageCard", new EffectParams("SVESimulator.EngageCardEffect")                           },
            { "ReserveCard", new EffectParams("SVESimulator.ReserveCardEffect")                         },

            // Keyword Effects
            { "GiveKeyword", new EffectParams("SVESimulator.GiveKeywordEffect",                         EffectParameterType.Keyword) },
            { "GiveKeywordEndOfTurn", new EffectParams("SVESimulator.GiveKeywordEndOfTurnEffect",       EffectParameterType.Keyword) },

            // Counter Effects
            { "GiveCounter", new EffectParams("SVESimulator.GiveCounterEffect",                         EffectParameterType.Keyword, EffectParameterType.Amount) },
            { "RemoveCounter", new EffectParams("SVESimulator.RemoveCounterEffect",                     EffectParameterType.Keyword, EffectParameterType.AmountDefaultNull) },
            { "MoveCounter", new EffectParams("SVESimulator.MoveCountersEffect",                        EffectParameterType.Keyword, EffectParameterType.AmountDefaultNull) },

            // ------------------------------

            // Token Effects
            { "SummonToken", new EffectParams("SVESimulator.SummonTokenEffect",                         false, false, EffectParameterType.TokenName, EffectParameterType.CreateTokenOption, EffectParameterType.Amount) },
            { "SummonTokenAndTarget", new EffectParams("SVESimulator.SummonTokenAndTargetEffect",       false, false, EffectParameterType.TokenName, EffectParameterType.CreateTokenOption, EffectParameterType.Amount, EffectParameterType.ListOfEffects) },
            { "Transform", new EffectParams("SVESimulator.TransformCardEffect",                         EffectParameterType.TokenName) },

            // Effect Execution
            { "DestroyAndControllerPerformEffect", new EffectParams("SVESimulator.DestroyAndControllerPerformEffect",   EffectParameterType.SingleEffect) },
            { "Sequence", new EffectParams("SVESimulator.EffectSequence",                                               false, false, EffectParameterType.ListOfEffects) },
            { "ChooseFromList", new EffectParams("SVESimulator.ChooseEffectFromList",                                   false, false, EffectParameterType.ListOfEffects) },
            { "OpponentPerformEffect", new EffectParams("SVESimulator.OpponentPerformEffect",                           EffectParameterType.SingleEffect) },
            { "PerformAsEachTarget", new EffectParams("SVESimulator.PerformAsEachTargetEffect",                         EffectParameterType.SingleEffect) },

            // Other Effects
            { "CheckTop", new EffectParams("SVESimulator.CheckTopDeckEffect",                           false, false, EffectParameterType.CheckCardActions) },
            { "ExtraTurn", new EffectParams("SVESimulator.ExtraTurnEffect",                             false, false) },
            { "Evolve", new EffectParams("SVESimulator.EvolveEffect")                                   },

            // ------------------------------

            // Passive - Modified Cost
            { "ReducedCost", new EffectParams("SVESimulator.ReducedCostEffect",                         false, false, EffectParameterType.Amount) },
            { "AlternateCost", new EffectParams("SVESimulator.AlternateCostEffect",                     false, false) },
        };
        
        private static Dictionary<string, EffectParams> PassiveEffectInfoDictionary = new()
        {
            { "GiveKeyword", new EffectParams("SVESimulator.GiveKeywordPassive",                        false, true, EffectParameterType.Keyword, EffectParameterType.PassiveDuration) },
            { "GiveStat", new EffectParams("SVESimulator.GiveStatBoostPassive",                         false, true, EffectParameterType.StatType, EffectParameterType.Amount, EffectParameterType.PassiveDuration) },
            { "MinusCostOther", new EffectParams("SVESimulator.MinusCostOtherPassive",                  false, true, EffectParameterType.Amount, EffectParameterType.PassiveDuration) },
        };

        // -----

        private static Dictionary<string, string> StatTypeDictionary = new()
        {
            { "Atk", "Attack" },
            { "Attack", "Attack" },
            { "Def", "Defense" },
            { "Defense", "Defense" },
            { "AtkDef", "AttackDefense" },
            { "AttackDefense", "AttackDefense" },
            { "Cost", "Cost" },
            { "EvolveCost", "EvolveCost" },

            { "MaxPlayPoint", "MaxPlayPoint"},
            { "MaxPlayPoints", "MaxPlayPoint"},
            { "MaxPP", "MaxPlayPoint"},
            { "PlayPoint", "PlayPoint"},
            { "PlayPoints", "PlayPoint"},
            { "PP", "PlayPoint"},
        };

        #endregion
    }
}
