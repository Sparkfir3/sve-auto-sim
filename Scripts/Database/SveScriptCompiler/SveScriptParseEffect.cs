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
        #region Parse Effect

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

            // Unique Case - Complex Effect (Function Parameter Type)
            if(effectParams.parameters.Length > 0 && effectParams.parameters[0] == EffectParameterType.Function)
            {
                ParseMainArgsArray(new [] { mainEffectArgs }, effectParams, ref effectData);
                return effectData;
            }

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

        #endregion

        // -----

        #region Parse Args Array

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
            output.Add(currentArg);
            return output.ToArray();
        }

        private static void ParseMainArgsArray(in string[] argsArray, in EffectParams effectParams, ref JObject effectData)
        {
            for(int i = 0; i < effectParams.parameters.Length; i++)
            {
                string argument = i < argsArray.Length ? argsArray[i] : null;
                if(argument.IsNullOrWhiteSpace() && effectParams.parameters[i] is EffectParameterType.Amount or EffectParameterType.Amount2)
                    argument = "1";
                if(argument == null && (effectParams.parameters[i] is not EffectParameterType.AmountDefaultNull and not EffectParameterType.FilterOptional))
                {
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
                    case EffectParameterType.Trait:
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
                    case EffectParameterType.Function:
                        effectData.Add("function", argument?.Trim());
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
        
        private enum EffectParameterType { Amount, Amount2, AmountDefaultNull, Keyword, StatType, SearchDeckAction, CheckCardActions, SingleEffect, ListOfEffects, CreateTokenOption, TokenName, Trait, Filter, FilterOptional, Function, PassiveDuration }
        private readonly struct EffectParams
        {
            public readonly string ccgType;
            public readonly EffectParameterType[] parameters;
            public readonly bool hasTarget, hasFilter;
            
            public EffectParams(string ccgType, params EffectParameterType[] parameters) : this(ccgType, true, true, parameters) { }
            public EffectParams(string ccgType, bool hasTarget, bool hasFilter, params EffectParameterType[] parameters)
            {
                this.ccgType = $"SVESimulator.{ccgType}";
                this.parameters = parameters;
                this.hasTarget = hasTarget;
                this.hasFilter = hasFilter;
            }
        }
        
        private static Dictionary<string, EffectParams> StandardEffectInfoDictionary = new()
        {
            // Movement - Deck to Zone
            { "DrawThenDamage", new EffectParams("DrawThenDamageEffect",                    EffectParameterType.Amount, EffectParameterType.Amount2) },
            { "Draw", new EffectParams("DrawCardEffect",                                    true, false, EffectParameterType.Amount) },
            { "DrawCard", new EffectParams("DrawCardEffect",                                true, false, EffectParameterType.Amount) },
            { "Mill", new EffectParams("MillDeckEffect",                                    false, false, EffectParameterType.Amount) },
            { "RedrawHand", new EffectParams("RedrawHandEffect",                            false, false, EffectParameterType.AmountDefaultNull) },
            { "Search", new EffectParams("SearchDeckEffect",                                false, false, EffectParameterType.Amount, EffectParameterType.Filter, EffectParameterType.SearchDeckAction) },
            { "SearchAndTarget", new EffectParams("SearchDeckAndTargetEffect",              false, false, EffectParameterType.Amount, EffectParameterType.Filter, EffectParameterType.SearchDeckAction, EffectParameterType.ListOfEffects) },
            { "TopDeckToEx", new EffectParams("TopDeckToExEffect",                          false, false, EffectParameterType.Amount) },
            { "TopDeckToExArea", new EffectParams("TopDeckToExEffect",                      false, false, EffectParameterType.Amount) },
            { "TopDeckToExAndTarget", new EffectParams("TopDeckToExAndTargetEffect",        false, false, EffectParameterType.Amount, EffectParameterType.ListOfEffects) },

            // Movement - Field/EX to Zone
            { "Banish", new EffectParams("BanishCardEffect")                                },
            { "BanishCard", new EffectParams("BanishCardEffect")                            },
            { "BottomDeck", new EffectParams("SendToBottomDeckEffect")                      },
            { "Destroy", new EffectParams("DestroyCardEffect")                              },
            { "DestroyCard", new EffectParams("DestroyCardEffect")                          },
            { "PutExToFieldAndTarget", new EffectParams("PutExToFieldAndTargetEffect",      EffectParameterType.ListOfEffects) },
            { "ReturnToHand", new EffectParams("ReturnToHandEffect")                        },
            { "SendToCemetery", new EffectParams("SendToCemeteryEffect")                    },
            { "SendToEx", new EffectParams("SendToExAreaEffect")                            },
            { "SendToExArea", new EffectParams("SendToExAreaEffect")                        },
            { "TopDeck", new EffectParams("SendToTopDeckEffect")                            },
            { "TopOrBottomDeck", new EffectParams("SendToTopOrBottomDeckEffect")            },

            // Movement - Hand to Zone
            { "DiscardHand", new EffectParams("DiscardHandEffect",                                      true, false) },
            { "DiscardRandomCard", new EffectParams("DiscardRandomCardEffect",                          true, false, EffectParameterType.Amount) },
            { "DiscardRandomCards", new EffectParams("DiscardRandomCardEffect",                         true, false, EffectParameterType.Amount) },
            { "Discard", new EffectParams("DiscardEffect",                                              false, false, EffectParameterType.Amount, EffectParameterType.FilterOptional) },
            { "DiscardFromOpponentHand", new EffectParams("DiscardFromOpponentHandEffect",              false, false, EffectParameterType.Amount, EffectParameterType.FilterOptional) },
            { "DiscardToBottomDeck", new EffectParams("DiscardToBottomDeckEffect",                      false, false, EffectParameterType.Amount, EffectParameterType.FilterOptional) },
            { "HandToField", new EffectParams("HandToFieldEffect",                                      false, false, EffectParameterType.Amount, EffectParameterType.FilterOptional) },

            // Movement - Cemetery to Zone
            { "CemeteryToField", new EffectParams("CemeteryToFieldEffect",                              true,  false, EffectParameterType.Amount, EffectParameterType.FilterOptional) },
            { "CemeteryToFieldAndTarget", new EffectParams("CemeteryToFieldAndTargetEffect",            true,  false, EffectParameterType.Amount, EffectParameterType.Filter, EffectParameterType.ListOfEffects) },
            { "PlaySpellFromCemetery", new EffectParams("PlaySpellFromCemeteryEffect",                  false, false, EffectParameterType.Filter) },
            { "PlaySpellFromCemeterySetCost", new EffectParams("PlaySpellFromCemeterySetCostEffect",    false, false, EffectParameterType.Filter, EffectParameterType.Amount2) },
            { "Salvage", new EffectParams("SalvageCardEffect",                                          false, false, EffectParameterType.Amount, EffectParameterType.Filter) },

            // ------------------------------

            // Stat Effects
            { "DealDamage", new EffectParams("DealDamageEffect",                            EffectParameterType.Amount) },
            { "Engage", new EffectParams("EngageCardEffect")                                },
            { "EngageCard", new EffectParams("EngageCardEffect")                            },
            { "GiveStat", new EffectParams("GiveStatBoostEffect",                           EffectParameterType.StatType, EffectParameterType.Amount) },
            { "GiveStatEndOfTurn", new EffectParams("GiveStatEndOfTurnEffect",              EffectParameterType.StatType, EffectParameterType.Amount) },
            { "Refresh", new EffectParams("ReserveCardEffect")                              },
            { "Reserve", new EffectParams("ReserveCardEffect")                              },
            { "ReserveCard", new EffectParams("ReserveCardEffect")                          },
            { "SetStat", new EffectParams("SetStatEffect",                                  EffectParameterType.StatType, EffectParameterType.Amount) },
            { "DealDamageDivided", new EffectParams("DealDamageDividedEffect",              false, true, EffectParameterType.Amount) },

            // Keyword Effects
            { "GiveKeyword", new EffectParams("GiveKeywordEffect",                                  EffectParameterType.Keyword) },
            { "GiveKeywordEndOfTurn", new EffectParams("GiveKeywordEndOfTurnEffect",                EffectParameterType.Keyword) },
            { "GiveKeywordEndOfNextTurn", new EffectParams("GiveKeywordEndOfNextTurnEffect",        EffectParameterType.Keyword) },

            // Counter Effects
            { "GiveCounter", new EffectParams("GiveCounterEffect",                          EffectParameterType.Keyword, EffectParameterType.Amount) },
            { "MoveCounter", new EffectParams("MoveCountersEffect",                         EffectParameterType.Keyword, EffectParameterType.AmountDefaultNull) },
            { "RemoveCounter", new EffectParams("RemoveCounterEffect",                      EffectParameterType.Keyword, EffectParameterType.AmountDefaultNull) },

            // ------------------------------

            // Token Effects
            { "Transform", new EffectParams("TransformCardEffect",                          EffectParameterType.TokenName) },
            { "SummonToken", new EffectParams("SummonTokenEffect",                          false, false, EffectParameterType.TokenName, EffectParameterType.CreateTokenOption, EffectParameterType.Amount) },
            { "SummonTokenAndTarget", new EffectParams("SummonTokenAndTargetEffect",        false, false, EffectParameterType.TokenName, EffectParameterType.CreateTokenOption, EffectParameterType.Amount, EffectParameterType.ListOfEffects) },

            // ------------------------------

            // Effect Sequencing - Ordered Sequence
            { "DestroyAndControllerPerformEffect", new EffectParams("DestroyAndControllerPerformEffect",        EffectParameterType.SingleEffect) },
            { "OpponentPerformEffect", new EffectParams("OpponentPerformEffect",                                EffectParameterType.SingleEffect) },
            { "PerformAsEachTarget", new EffectParams("PerformAsEachTargetEffect",                              EffectParameterType.SingleEffect) },
            { "PerformWithTargetAmount", new EffectParams("PerformWithTargetAmountEffect",                      EffectParameterType.Amount, EffectParameterType.ListOfEffects) },
            { "TargetForEffectSequence", new EffectParams("TargetForEffectSequence",                            EffectParameterType.ListOfEffects) },
            { "TargetForSequence", new EffectParams("TargetForEffectSequence",                                  EffectParameterType.ListOfEffects) },
            { "TargetForSequenceWithTargetAmount", new EffectParams("TargetForEffectSequenceWithTargetAmount",  EffectParameterType.Amount, EffectParameterType.ListOfEffects) },
            { "Sequence", new EffectParams("EffectSequence",                                                    false, false, EffectParameterType.ListOfEffects) },

            // Effect Sequencing - Choose From List
            { "ChooseAmountFromList", new EffectParams("ChooseAmountFromList",                              false, false, EffectParameterType.Amount, EffectParameterType.ListOfEffects) },
            { "ChooseFromList", new EffectParams("ChooseEffectFromList",                                    false, false, EffectParameterType.ListOfEffects) },

            // ------------------------------

            // Other Effects
            { "Evolve", new EffectParams("EvolveEffect")                                    },
            { "GiveTrait", new EffectParams("GiveTraitEffect",                              EffectParameterType.Trait) },
            { "CheckTop", new EffectParams("CheckTopDeckEffect",                            false, false, EffectParameterType.CheckCardActions) },
            { "Complex", new EffectParams("ComplexEffect",                                  false, false, EffectParameterType.Function) },
            { "ComplexEffect", new EffectParams("ComplexEffect",                            false, false, EffectParameterType.Function) },
            { "ExtraTurn", new EffectParams("ExtraTurnEffect",                              false, false) },
            { "FlipEvolveDeckFaceDown", new EffectParams("FlipEvolveDeckFaceDownEffect",    false, false, EffectParameterType.FilterOptional) },

            // ------------------------------

            // Passive - Modified Cost
            { "AlternateCost", new EffectParams("AlternateCostEffect",                      false, false) },
            { "ReducedCost", new EffectParams("ReducedCostEffect",                          false, false, EffectParameterType.Amount) },
        };

        private static Dictionary<string, EffectParams> PassiveEffectInfoDictionary = new()
        {
            { "GiveKeyword", new EffectParams("GiveKeywordPassive",                         false, true, EffectParameterType.Keyword, EffectParameterType.PassiveDuration) },
            { "GiveStat", new EffectParams("GiveStatBoostPassive",                          false, true, EffectParameterType.StatType, EffectParameterType.Amount, EffectParameterType.PassiveDuration) },
            { "MinusCostOther", new EffectParams("MinusCostOtherPassive",                   false, true, EffectParameterType.Amount, EffectParameterType.PassiveDuration) },
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
            { "EvolvePoint", "EvolvePoint"},
            { "EP", "EvolvePoint"},
        };

        #endregion
    }
}
