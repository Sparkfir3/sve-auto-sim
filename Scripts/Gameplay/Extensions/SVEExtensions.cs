using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CCGKit;
using Newtonsoft.Json.Linq;
using Sparkfire.Utility;
using UnityEngine;
using CardFilterSetting = SVESimulator.SVEFormulaParser.CardFilterSetting;

namespace SVESimulator
{
    public static class SVEExtensions
    {
		#region Card Filters

        public static bool MatchesCard(this Dictionary<CardFilterSetting, string> filters, in CardObject card) => filters.MatchesCard(card ? card.RuntimeCard : null);
        public static bool MatchesCard(this Dictionary<CardFilterSetting, string> filters, in RuntimeCard card)
        {
            if(filters == null || filters.Count == 0)
                return true;
            if(card == null)
                return false;

            Card libraryCard = null;
            foreach(KeyValuePair<CardFilterSetting, string> filter in filters)
            {
                (CardFilterSetting setting, string value) = (filter.Key, filter.Value);
                bool inverse = false;
                if(!value.IsNullOrWhiteSpace() && value[0] == '!')
                {
                    inverse = true;
                    value = value[1..];
                }

                switch(setting)
                {
                    // Card Type (except Token)
                    case CardFilterSetting.Follower:
                        if(!card.IsFollowerOrEvolvedFollower() ^ inverse)
                            return false;
                        break;
                    case CardFilterSetting.Spell:
                        if(!card.IsSpell() ^ inverse)
                            return false;
                        break;
                    case CardFilterSetting.Amulet:
                        if(!card.IsAmulet() ^ inverse)
                            return false;
                        break;
                    case CardFilterSetting.Evolved:
                        if(!card.IsEvolvedType() ^ inverse)
                            return false;
                        break;

                    // Card Properties (+ Token)
                    case CardFilterSetting.Token:
                    case CardFilterSetting.Trait:
                        libraryCard ??= LibraryCardCache.GetCard(card.cardId);
                        if(libraryCard == null)
                            throw new Exception($"Card not found in cache: {card.cardId}");
                        if(!libraryCard.GetStringProperty(SVEProperties.CardStats.Trait).Split('/').Any(x => x.Trim().Equals(value)) ^ inverse)
                            return false;
                        break;
                    case CardFilterSetting.Keyword:
                        if(!card.HasKeyword(value) ^ inverse)
                            return false;
                        break;
                    case CardFilterSetting.Counter:
                        try
                        {
                            string[] counterParams = value.Split(',');
                            string counterName = counterParams.Length > 0 ? counterParams[0].Replace("Counter", "").Trim() : null;
                            SVEProperties.Counters counterType = (SVEProperties.Counters)Enum.Parse(typeof(SVEProperties.Counters), counterName);
                            int minCounters = SVEFormulaParser.ParseValue(counterParams.Length > 1 ? string.Join("", counterParams[1..]) : "1", null, card);
                            int currentCounterCount = card.CountOfCounter(counterType);
                            if(currentCounterCount < minCounters ^ inverse)
                                return false;
                        }
                        catch(System.ArgumentException e)
                        {
                            Debug.LogError($"Attempted to parse invalid counter name from {value}\n{e.ToString()}");
                        }
                        break;
                    case CardFilterSetting.Class:
                        libraryCard ??= LibraryCardCache.GetCard(card.cardId);
                        if(libraryCard == null)
                            throw new Exception($"Card not found in cache: {card.cardId}");
                        if(!libraryCard.GetStringProperty(SVEProperties.CardStats.Class).ToLower().Contains(value.ToLower()) ^ inverse)
                            return false;
                        break;
                    case CardFilterSetting.Name:
                        libraryCard ??= LibraryCardCache.GetCard(card.cardId);
                        if(libraryCard == null)
                            throw new Exception($"Card not found in cache: {card.cardId}");
                        if(!libraryCard.name.Replace("(Evolved)", "").Trim().Equals(value) ^ inverse)
                            return false;
                        break;

                    // Card Stats
                    case CardFilterSetting.Reserved:
                    case CardFilterSetting.Engaged:
                        if((card.namedStats.TryGetValue(SVEProperties.CardStats.Engaged, out Stat engagedStat) && engagedStat.effectiveValue != (setting == CardFilterSetting.Reserved ? 0 : 1)) ^ inverse)
                            return false;
                        break;
                    case CardFilterSetting.Attack:
                    case CardFilterSetting.Defense:
                    case CardFilterSetting.PlayPointCost:
                        SVEFormulaParser.ParseValueAsMinMax(value, null, out int min, out int max);
                        string targetStat = setting switch
                        {
                            CardFilterSetting.Attack => SVEProperties.CardStats.Attack,
                            CardFilterSetting.Defense => SVEProperties.CardStats.Defense,
                            _ => SVEProperties.CardStats.Cost
                        };
                        if((!card.namedStats.TryGetValue(targetStat, out Stat stat) || stat.effectiveValue < min || stat.effectiveValue > max) ^ inverse)
                            return false;
                        break;
                    case CardFilterSetting.EvolveCost:
                        SVEFormulaParser.ParseValueAsMinMax(value, null, out int minEvolveCost, out int maxEvolveCost);
                        int evolveCost = card.EvolveCost();
                        if((evolveCost < minEvolveCost || evolveCost > maxEvolveCost) ^ inverse)
                            return false;
                        break;

                    // Other
                    case CardFilterSetting.ExcludeSelf:
                        if(value.Equals(card.instanceId.ToString()) ^ inverse)
                            return false;
                        break;
                    case CardFilterSetting.InstanceID:
                        int[] instanceIds = value.Split(',').Select(x => int.Parse(x.Trim())).ToArray();
                        if(!instanceIds.Contains(card.instanceId) ^ inverse)
                            return false;
                        break;
                }
            }
            return true;
        }

        public static bool HasExcludeSelf(this Dictionary<CardFilterSetting, string> filters) => filters.ContainsKey(CardFilterSetting.ExcludeSelf);

		#endregion

        // ------------------------------

        #region Effect Targets

        public static bool IsLeader(this SVEProperties.SVEEffectTarget target) => target.IsLeader(out _, out _);
        public static bool IsLeader(this SVEProperties.SVEEffectTarget target, out bool localLeader, out bool opponentLeader)
        {
            localLeader = target is SVEProperties.SVEEffectTarget.Leader or SVEProperties.SVEEffectTarget.TargetPlayerCardOrLeader or SVEProperties.SVEEffectTarget.AllLeaders;
            opponentLeader = target is SVEProperties.SVEEffectTarget.OpponentLeader or SVEProperties.SVEEffectTarget.TargetOpponentCardOrLeader or SVEProperties.SVEEffectTarget.AllLeaders;
            return localLeader || opponentLeader;
        }

        public static bool IsFieldCard(this SVEProperties.SVEEffectTarget target)
        {
            return target is SVEProperties.SVEEffectTarget.AllPlayerCards or SVEProperties.SVEEffectTarget.TargetPlayerCard or SVEProperties.SVEEffectTarget.TargetPlayerCardOrLeader
                or SVEProperties.SVEEffectTarget.AllOpponentCards or SVEProperties.SVEEffectTarget.TargetOpponentCard
                or SVEProperties.SVEEffectTarget.TargetOpponentCardsDivided or SVEProperties.SVEEffectTarget.TargetOpponentCardOrLeader
                or SVEProperties.SVEEffectTarget.AllCards;
        }

        #endregion

        // ------------------------------

        #region Effect Parameters/Costs

        public static string[] AsNamedStatArray(this SVEProperties.StatBoostType boostType)
        {
            return boostType switch
            {
                SVEProperties.StatBoostType.Attack => new [] { SVEProperties.CardStats.Attack },
                SVEProperties.StatBoostType.Defense => new [] { SVEProperties.CardStats.Defense },
                SVEProperties.StatBoostType.AttackDefense => new [] { SVEProperties.CardStats.Attack, SVEProperties.CardStats.Defense },
                SVEProperties.StatBoostType.Cost => new [] { SVEProperties.CardStats.Cost },
                SVEProperties.StatBoostType.EvolveCost => new [] { SVEProperties.CardStats.EvolveCost },
                _ => null
            };
        }

        public static List<Cost> ToCostList(this string input)
        {
            if(input.IsNullOrWhiteSpace())
                return null;

            try
            {
                JArray jArray = JArray.Parse(input);
                List<Cost> costList = new();

                foreach(JObject jObject in jArray.Children<JObject>())
                {
                    string rawType = jObject.GetValue("$type")?.Value<string>();
                    Type type = Type.GetType(rawType);
                    SveCost cost = Activator.CreateInstance(type) as SveCost;

                    FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
                    for(int i = 0; i < fields.Length; i++)
                    {
                        if(fields[i].FieldType == typeof(string))
                        {
                            string stringValue = jObject.GetValue(fields[i].Name)?.Value<string>();
                            fields[i].SetValue(cost, stringValue);
                        }
                        else if(fields[i].FieldType == typeof(int))
                        {
                            int intValue = int.Parse(jObject.GetValue(fields[i].Name)?.Value<string>() ?? "0");
                            fields[i].SetValue(cost, intValue);
                        }
                        else if(fields[i].FieldType == typeof(SVEProperties.SVEEffectTarget))
                        {
                            Enum.TryParse(jObject.GetValue(fields[i].Name)?.Value<string>(), out SVEProperties.SVEEffectTarget target);
                            fields[i].SetValue(cost, target);
                        }
                    }
                    costList.Add(cost);
                }

                return costList;
            }
            catch(Exception e)
            {
                Debug.LogError($"{e}\nAn error occurred while parsing cost list {input}");
                return null;
            }
        }

        #endregion

        // ------------------------------

        #region Copy SVE Effects with Modifications

        public static SveEffect CopyWithAddFilters(this SveEffect baseEffect, string additionalFilters = null)
        {
            Type type = baseEffect.GetType();
            SveEffect newEffect = Activator.CreateInstance(type) as SveEffect;
            FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
            for(int i = 0; i < fields.Length; i++)
            {
                if(fields[i].Name.Equals("filter"))
                {
                    string filter = fields[i].GetValue(baseEffect) as string;
                    filter = filter.IsNullOrWhiteSpace() ? additionalFilters : filter + additionalFilters;
                    fields[i].SetValue(newEffect, filter);
                    continue;
                }
                fields[i].SetValue(newEffect, fields[i].GetValue(baseEffect));
            }
            return newEffect;
        }

        public static SveEffect CopyWithOverrideTargetFilter(this SveEffect baseEffect, SVEProperties.SVEEffectTarget newTarget, string newFilter)
        {
            Type type = baseEffect.GetType();
            SveEffect newEffect = Activator.CreateInstance(type) as SveEffect;
            FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
            for(int i = 0; i < fields.Length; i++)
            {
                switch(fields[i].Name)
                {
                    case "target":
                        fields[i].SetValue(newEffect, newTarget);
                        continue;
                    case "filter":
                        fields[i].SetValue(newEffect, newFilter);
                        continue;
                    default:
                        fields[i].SetValue(newEffect, fields[i].GetValue(baseEffect));
                        continue;
                }
            }
            return newEffect;
        }

        #endregion
    }
}
