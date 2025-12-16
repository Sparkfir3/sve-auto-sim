using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using static SVESimulator.SveScript.SveScriptData;
using static SVESimulator.SveScript.SveScriptEffectCompiler;

namespace SVESimulator.SveScript
{
    internal static partial class SveScriptAbilityCompiler
    {
        #region Parsing

        public static void ParseAndAddAbility(in string text, ref CardInfo cardInfo)
        {
            int splitIndex = text.IndexOf('{');
            if(splitIndex == -1)
                return;

            // Parse general info
            string[] generalInfo = text[0..splitIndex].Trim().Split();
            string trigger = generalInfo[0];
            string name = "";
            for(int i = 1; i < generalInfo.Length; i++)
            {
                switch(generalInfo[i])
                {
                    case "name":
                        name = generalInfo[++i];
                        i++;
                        while(i < generalInfo.Length)
                        {
                            name += $" {generalInfo[i]}";
                            i++;
                        }
                        name = name.Replace("\"", "");
                        break;
                    default:
                        throw new ArgumentException($"Invalid arg {generalInfo[i]} found in ability:\n{text}");
                }
            }
            if(string.IsNullOrWhiteSpace(name))
                name = trigger;

            // Specific ability data
            splitIndex++; // Move past '{'
            switch(trigger)
            {
                case "Act":
                case "Activate":
                case "Activated":
                    ParseAndAddActivatedAbility(text[splitIndex..], ref cardInfo, name);
                    break;
                case "Passive":
                    ParseAndAddPassiveAbility(text[splitIndex..], ref cardInfo, name, trigger);
                    break;
                default:
                    ParseAndAddTriggeredAbility(text[splitIndex..], ref cardInfo, name, trigger);
                    break;
            }
        }

        #endregion

        // ------------------------------

        #region Ability Types

        private static void ParseAndAddActivatedAbility(in string text, ref CardInfo cardInfo, in string name)
        {
            ActivatedAbility newAbility = new()
            {
                name = name
            };

            string[] args = text.Replace('\n', ' ').Split(';').Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToArray();
            ParseAbilityArgs(args, newAbility, null);
            cardInfo.abilities.Add(newAbility);
        }

        private static void ParseAndAddPassiveAbility(in string text, ref CardInfo cardInfo, in string name, in string triggerType)
        {
            PassiveAbility newAbility = new()
            {
                name = name
            };

            string[] args = text.Replace('\n', ' ').Split(';').Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToArray();
            ParseAbilityArgs(args, newAbility, triggerType);
            cardInfo.abilities.Add(newAbility);
        }

        private static void ParseAndAddTriggeredAbility(in string text, ref CardInfo cardInfo, in string name, in string triggerType)
        {
            TriggeredAbility newAbility = new()
            {
                name = name
            };

            string[] args = text.Replace('\n', ' ').Split(';').Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToArray();
            ParseAbilityArgs(args, newAbility, triggerType);
            cardInfo.abilities.Add(newAbility);
        }

        #endregion

        // ------------------------------

        #region Args

        private static void ParseAbilityArgs(in string[] args, Ability ability, in string triggerType)
        {
            string effectCcgType = "";
            JObject triggerData = ability is TriggeredAbility ? new JObject() : null;

            for(int i = 0; i < args.Length; i++)
            {
                if(string.IsNullOrWhiteSpace(args[i]))
                    continue;

                int splitIndex = args[i].Trim().IndexOf(' ');
                if(splitIndex == -1)
                    throw new Exception();
                switch(args[i].Trim().Split()[0])
                {
                    case "cost":
                    case "costs":
                        List<JObject> costs = ParseAbilityCosts(args[i][args[i].IndexOf(' ')..].Trim());
                        if(ability is ActivatedAbility activatedAbilityCost)
                        {
                            activatedAbilityCost.costs ??= new List<JObject>();
                            activatedAbilityCost.costs.AddRange(costs);
                        }
                        else if(ability is TriggeredAbility)
                            triggerData?.Add("cost", (new JArray(costs)).ToString());
                        break;
                    case "effect":
                        ability.effect = ability is not PassiveAbility
                            ? ParseAbilityEffect(args[i][6..].Trim(), out effectCcgType) // 6 = length of "effect"
                            : ParsePassiveAbilityEffect(args[i][6..].Trim(), out effectCcgType, triggerData);
                        break;
                    case "condition":
                        if(ability is ActivatedAbility activatedAbilityCond)
                        {
                            activatedAbilityCond.costs ??= new List<JObject>();
                            JObject conditionAsCost = new()
                            {
                                { "condition", args[i][9..].Trim() }, // 9 = length of "condition"
                                { "$type", "SVESimulator.ConditionAsCost" }
                            };
                            activatedAbilityCond.costs.Add(conditionAsCost);
                        }
                        else if(ability is TriggeredAbility)
                            triggerData?.Add("condition", args[i][9..].Trim()); // 9 = length of "condition"
                        break;

                    // currently only supported by triggered abilities, not activated ones
                    case "filter":
                        triggerData?.TryAdd("filter", args[i][6..].Trim()); // 6 = length of "filter"
                        break;

                    default:
                        throw new ArgumentException($"Invalid argument: {args[i]}");
                }
            }

            if(ability.effect != null)
            {
                ability.effect.Add("$type", effectCcgType);
            }
            if(ability is TriggeredAbility triggeredAbility)
            {
                if(!triggerData.ContainsKey("condition"))
                    triggerData.Add("condition", null);
                if(!triggerData.ContainsKey("cost"))
                    triggerData.Add("cost", null);
                triggerData.Add("$type", EffectTriggerDictionary[triggerType].ccgType);
                triggeredAbility.trigger = triggerData;
            }
        }

        #endregion
    }
}
