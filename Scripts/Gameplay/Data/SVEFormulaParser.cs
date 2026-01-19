using System;
using System.Collections.Generic;
using System.Linq;
using CCGKit;
using Sparkfire.Utility;
using UnityEngine;

namespace SVESimulator
{
    public static class SVEFormulaParser
    {
        #region Enums

        public enum CardFilterSetting
        {
            // Card Type
            Follower,
            Evolved,
            Token,
            Spell,
            Amulet,

            // Card Properties
            Trait,
            Keyword,
            Counter,
            Name,
            Class,

            // Card Stats
            Attack,
            Defense,
            EvolveCost,
            PlayPointCost,
            Reserved,
            Engaged,

            // Other
            InstanceID,
            ExcludeSelf,
            MinMaxCount,
        }

        private enum FormulaType
        {
            None,

            // Player Info
            Combo,
            Spellchain,
            Overflow,
            Necrocharge,
            Sanguine,
            IsTurnPlayer,

            // Card Properties
            Trait,
            Keyword,
            Counter,
            Name,
            Class,

            // Card Stats
            Attack,
            Defense,
            PlayPointCost,
            EvolveCost,

            // Arithmetic
            Addition,
            Subtraction,
            GreaterThan,
            LessThan,
            BooleanOr,
            BooleanAnd,
            Conditional,

            // Other
            InstanceID,
            MinMaxCount,
        }

        #endregion

        // ------------------------------

        #region Value

        public static int ParseValue(in string formula, in PlayerController player, in CardObject card) => ParseValue(formula, player, card.RuntimeCard);
        public static int ParseValue(in string formula, in PlayerController player, int cardInstanceId, string cardZone)
            => ParseValue(formula, player, player ? player.GetPlayerInfo().namedZones[cardZone].cards.FirstOrDefault(x => x.instanceId == cardInstanceId) : null);
        public static int ParseValue(in string formula, in PlayerController player = null, in RuntimeCard card = null)
        {
            if(string.IsNullOrWhiteSpace(formula))
                return 0;

            // Parse left hand side
            int leftHandValue, nextIndex;
            if(formula[0] == '[')
            {
                leftHandValue = ParseValue(formula.TextInsideBrackets(out _, out nextIndex), player, card);
                nextIndex++;
            }
            else
            {
                leftHandValue = ParseFormulaInt(formula, 0, out nextIndex, player, card);
            }
            if(nextIndex >= formula.Length)
                return leftHandValue;

            // Operator
            while(formula[nextIndex] == ' ')
                nextIndex++;
            FormulaType formulaType = formula[nextIndex++] switch
            {
                'c' => FormulaType.Combo,
                's' => FormulaType.Spellchain,
                'o' => FormulaType.Overflow,
                'n' => FormulaType.Necrocharge,
                'g' => FormulaType.Sanguine,
                't' => FormulaType.IsTurnPlayer,
                '+' => FormulaType.Addition,
                '-' => FormulaType.Subtraction,
                '>' => FormulaType.GreaterThan,
                '<' => FormulaType.LessThan,
                '|' => FormulaType.BooleanOr,
                '&' => FormulaType.BooleanAnd,
                '?' => FormulaType.Conditional,
                _ => FormulaType.None
            };
            switch(formulaType)
            {
                case FormulaType.None:
                    return leftHandValue;

                case FormulaType.Combo:
                case FormulaType.Spellchain:
                case FormulaType.Necrocharge:
                    nextIndex++; // Move past open parentheses
                    int neededValue = ParseFormulaInt(formula, nextIndex, out nextIndex);
                    int comparedValue = player
                        ? formulaType switch
                        {
                            FormulaType.Combo => player.Combo,
                            FormulaType.Spellchain => player.Spellchain,
                            FormulaType.Necrocharge => player.Necrocharge,
                            _ => 0
                        }
                        : 0;
                    if(comparedValue < neededValue)
                        return leftHandValue;
                    nextIndex++; // Move past close parentheses
                    break;

                case FormulaType.Overflow:
                    if(!player || !player.Overflow)
                        return leftHandValue;
                    break;
                case FormulaType.Sanguine:
                    if(!player || !player.Sanguine)
                        return leftHandValue;
                    break;
                case FormulaType.IsTurnPlayer:
                    if(!player || !player.isActivePlayer)
                        return leftHandValue;
                    break;

                case FormulaType.Conditional:
                    int conditionalCheck = ParseValue(formula[nextIndex..].TextInsideParentheses(out _, out int conditionalCloseIndex), player, card);
                    if(conditionalCheck <= 0)
                        return leftHandValue;
                    nextIndex += conditionalCloseIndex + 1; // Move past parentheses content + close parentheses
                    break;
            }
            while(nextIndex < formula.Length && formula[nextIndex] == ' ')
                nextIndex++;

            // Parse right hand side
            int rightHandValue = nextIndex < formula.Length && formula[nextIndex] == '['
                ? ParseValue(formula.TextInsideBrackets(out _, out int lastIndex), player, card)
                : ParseFormulaInt(formula, nextIndex, out lastIndex, player, card);
            if(nextIndex == lastIndex && rightHandValue == 0)
                rightHandValue = 1; // default to 1 if right hand side is blank
            return formulaType switch
            {
                FormulaType.Addition =>     leftHandValue + rightHandValue,
                FormulaType.Subtraction =>  leftHandValue - rightHandValue,
                FormulaType.GreaterThan =>  leftHandValue > rightHandValue ? 1 : 0,
                FormulaType.LessThan =>     leftHandValue < rightHandValue ? 1 : 0,
                FormulaType.BooleanOr =>    leftHandValue > 0 || rightHandValue > 0 ? 1 : 0,
                FormulaType.BooleanAnd =>   leftHandValue > 0 && rightHandValue > 0 ? 1 : 0,
                _ =>                        rightHandValue
            };
        }

        public static bool ParseValueAsCondition(in string formula, PlayerController player = null, CardObject card = null) => ParseValueAsCondition(formula, player, card ? card.RuntimeCard : null);
        public static bool ParseValueAsCondition(in string formula, PlayerController player = null, RuntimeCard card = null)
        {
            if(string.IsNullOrWhiteSpace(formula))
                return true;
            return ParseValue(formula, player, card) > 0;
        }

        public static void ParseValueAsMinMax(in string formula, PlayerController player, out int min, out int max)
        {
            if(string.IsNullOrWhiteSpace(formula))
            {
                min = max = 0;
            }
            else if(formula.Trim().StartsWith("m("))
            {
                var minMaxFormula = ParseCardFilterFormula(formula);
                ParseMinMaxCount(minMaxFormula[CardFilterSetting.MinMaxCount], out min, out max);
            }
            else
            {
                min = max = ParseValue(formula, player);
            }
        }
        
        private static int ParseFormulaInt(in string formula, int startIndex, out int endIndex, in PlayerController player = null, in RuntimeCard card = null)
        {
            // Int value
            int value = 0;
            bool negative = false;
            endIndex = startIndex;
            int i;
            for(i = startIndex; i < formula.Length; i++)
            {
                endIndex = i;
                if(i == startIndex && formula[i] == '-')
                    negative = true;
                else if(CharIsInt(formula[i]))
                    value = value * 10 + CharToInt(formula[i]);
                else
                    break;
            }
            if(i == formula.Length)
                endIndex = i;
            if(endIndex > startIndex + (negative ? 1 : 0) || endIndex >= formula.Length)
                return value * (negative ? -1 : 1);

            // Dynamic game value
            bool usedPlayerReference = true;
            int indexDelta = 0;
            switch(formula[endIndex++])
            {
                // Player stats/info
                case 'C': // Combo
                    value = player ? player.Combo : 0;
                    break;
                case 'H': // Cards in hand
                    if(formula[endIndex] == '(')
                    {
                        Dictionary<CardFilterSetting, string> filterH = ParseCardFilterFormula(formula[endIndex..].TextInsideParentheses(out _, out indexDelta), card);
                        endIndex += indexDelta + 1; // Move past close parentheses
                        value = player ? player.ZoneController.handZone.Runtime.cards.Count(x => filterH.MatchesCard(x)) : 0;
                    }
                    else
                    {
                        value = player ? player.ZoneController.handZone.Runtime.numCards : 0;
                    }
                    break;
                case 'L': // Leader defense
                    value = player ? player.GetPlayerInfo().namedStats[SVEProperties.PlayerStats.Defense].effectiveValue : 0;
                    break;
                case '#': // Misc player stats
                    value = GetMiscPlayerStats(formula[endIndex..].TextInsideParentheses(out _, out indexDelta), player);
                    endIndex += indexDelta + 1; // Move past close parentheses
                    break;

                // Card stats
                case 'A': // Attack
                    value = card != null ? card.namedStats[SVEProperties.CardStats.Attack].effectiveValue : 0;
                    Debug.Assert(card != null, $"Attempted to parse card's Attack for value formula {formula}, but no card was provided");
                    usedPlayerReference = false;
                    break;
                case 'D': // Defense
                    value = card != null ? card.namedStats[SVEProperties.CardStats.Defense].effectiveValue : 0;
                    Debug.Assert(card != null, $"Attempted to parse card's Defense for value formula {formula}, but no card was provided");
                    usedPlayerReference = false;
                    break;

                // Player zone counts
                case 'f': // Count on field matching filter
                    Dictionary<CardFilterSetting, string> filterF = ParseCardFilterFormula(formula[endIndex..].TextInsideParentheses(out _, out indexDelta), card);
                    endIndex += indexDelta + 1; // Move past close parentheses
                    value = player ? player.ZoneController.fieldZone.GetAllPrimaryCards().Count(x => filterF.MatchesCard(x.RuntimeCard)) : 0;
                    break;
                case 'x': // Count in EX area matching filter
                    Dictionary<CardFilterSetting, string> filterEX = ParseCardFilterFormula(formula[endIndex..].TextInsideParentheses(out _, out indexDelta), card);
                    endIndex += indexDelta + 1; // Move past close parentheses
                    value = player ? player.ZoneController.exAreaZone.GetAllPrimaryCards().Count(x => filterEX.MatchesCard(x.RuntimeCard)) : 0;
                    break;
                case 'y': // Count in cemetery matching filter
                    Dictionary<CardFilterSetting, string> filterCemetery = ParseCardFilterFormula(formula[endIndex..].TextInsideParentheses(out _, out indexDelta), card);
                    endIndex += indexDelta + 1; // Move past close parentheses
                    value = player ? player.ZoneController.cemeteryZone.AllCards.Count(x => filterCemetery.MatchesCard(x.RuntimeCard)) : 0;
                    break;

                // Opponent zone counts
                case 'p': // Count on opponent's field matching filter
                    Dictionary<CardFilterSetting, string> filterP = ParseCardFilterFormula(formula[endIndex..].TextInsideParentheses(out _, out indexDelta), card);
                    endIndex += indexDelta + 1; // Move past close parentheses
                    value = player ? player.OppZoneController.fieldZone.GetAllPrimaryCards().Count(x => filterP.MatchesCard(x.RuntimeCard)) : 0;
                    break;

                default:
                    usedPlayerReference = false;
                    endIndex--; // offset the ++ we did at start of switch
                    break;
            }
            Debug.Assert(!usedPlayerReference || player, $"Attempted to parse formula {formula}, but no player reference was provided");

            return value * (negative ? -1 : 1);
        }

        private static int GetMiscPlayerStats(in string formula, in PlayerController player = null)
        {
            if(!player || formula.IsNullOrWhiteSpace())
                return 0;
            string[] args = formula.Split(',');
            if(args.Length == 0)
                return 0;

            args[0] = args[0].Trim().ToLower();
            Dictionary<CardFilterSetting, string> filter = args.Length >= 2 ? ParseCardFilterFormula(args[1]) : null;
            switch(args[0])
            {
                case "destroyed":
                    return filter == null || filter.Count == 0
                        ? player.AdditionalStats.CardsDestroyedThisTurn.Count
                        : player.AdditionalStats.CardsDestroyedThisTurn.Count(x =>
                        {
                            CardObject card = CardManager.Instance.GetCardByInstanceId(x.instanceId);
                            Debug.Assert(card);
                            return card && filter.MatchesCard(card);
                        });
                default:
                    return 0;
            }
        }

        #endregion

        // ------------------------------

        #region Filter

        public static Dictionary<CardFilterSetting, string> ParseCardFilterFormula(string formula)
        {
            int nextIndex = 0;
            Dictionary<CardFilterSetting, string> filters = new();
            if(string.IsNullOrWhiteSpace(formula))
                return filters;

            // Shortcut handling
            formula = formula
                .Replace("#F", "F!k(Aura)")
                .Replace("#a", "!k(Aura)")
                .Replace("#e", "e(m(0,99))");

            while(nextIndex < formula.Length)
            {
                string currentFilterData = null;
                if(formula[nextIndex] == '!') // Inverse
                {
                    currentFilterData = "!";
                    nextIndex++;
                }

                // ---

                // Card Type Check
                CardFilterSetting? filterSettingCardType = formula[nextIndex] switch
                {
                    'F' => CardFilterSetting.Follower,
                    'S' => CardFilterSetting.Spell,
                    'A' => CardFilterSetting.Amulet,
                    'E' => CardFilterSetting.Evolved,
                    'K' => CardFilterSetting.Token,
                    'X' => CardFilterSetting.ExcludeSelf,
                    'R' => CardFilterSetting.Reserved,
                    'N' => CardFilterSetting.Engaged,
                    _ => null
                };
                if(filterSettingCardType.HasValue)
                {
                    if(formula[nextIndex] == 'K') // Token
                        currentFilterData = $"{currentFilterData}{SVEProperties.CardTypes.Token}";

                    filters.Add(filterSettingCardType.Value, currentFilterData);
                    nextIndex++;
                    continue;
                }

                // ---

                // Card Property/Stat Check
                FormulaType formulaType = formula[nextIndex++] switch
                {
                    'n' => FormulaType.Name,
                    't' => FormulaType.Trait,
                    'k' => FormulaType.Keyword,
                    'r' => FormulaType.Counter,
                    'c' => FormulaType.Class,
                    'a' => FormulaType.Attack,
                    'd' => FormulaType.Defense,
                    'e' => FormulaType.EvolveCost,
                    'p' => FormulaType.PlayPointCost,
                    'm' => FormulaType.MinMaxCount,
                    'i' => FormulaType.InstanceID,
                    _ => FormulaType.None
                };

                if(formulaType == FormulaType.None)
                    continue;

                nextIndex++; // move past open parentheses TODO - actually check for a parentheses
                if(formulaType == FormulaType.Name)
                {
                    string name = ParseFilterFormulaSubstring(formula, nextIndex, out nextIndex).Trim();
                    if(name.StartsWith('\"') && name.EndsWith('\"'))
                        name = name[1..^1];
                    filters.Add(CardFilterSetting.Name, $"{currentFilterData}{name.Trim()}");
                }
                else
                {
                    CardFilterSetting filterSetting = formulaType switch
                    {
                        FormulaType.Trait => CardFilterSetting.Trait,
                        FormulaType.Keyword => CardFilterSetting.Keyword,
                        FormulaType.Counter => CardFilterSetting.Counter,
                        FormulaType.Class => CardFilterSetting.Class,
                        FormulaType.Attack => CardFilterSetting.Attack,
                        FormulaType.Defense => CardFilterSetting.Defense,
                        FormulaType.EvolveCost => CardFilterSetting.EvolveCost,
                        FormulaType.PlayPointCost => CardFilterSetting.PlayPointCost,
                        FormulaType.MinMaxCount => CardFilterSetting.MinMaxCount,
                        FormulaType.InstanceID => CardFilterSetting.InstanceID,
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    filters.Add(filterSetting, $"{currentFilterData}{ParseFilterFormulaSubstring(formula, nextIndex, out nextIndex)}");
                }
                nextIndex++; // move past close parentheses
            }
            return filters;
        }

        public static Dictionary<CardFilterSetting, string> ParseCardFilterFormula(in string formula, in int sourceCardInstanceId)
        {
            Dictionary<CardFilterSetting, string> filter = ParseCardFilterFormula(formula);
            if(filter.ContainsKey(CardFilterSetting.ExcludeSelf))
                filter[CardFilterSetting.ExcludeSelf] = sourceCardInstanceId.ToString();
            return filter;
        }

        public static Dictionary<CardFilterSetting, string> ParseCardFilterFormula(in string formula, RuntimeCard card)
        {
            return card != null ? ParseCardFilterFormula(formula, card.instanceId) : ParseCardFilterFormula(formula);
        }

        public static void ParseMinMaxCount(in string formula, out int min, out int max)
        {
            string[] substrings = formula.Split(',');
            Debug.Assert(substrings.Length == 2, $"Tried to parse invalid min max formula: {formula}");
            min = ParseFormulaInt(substrings[0], 0, out _);
            max = ParseFormulaInt(substrings[1], 0, out _);
        }
        
        private static string ParseFilterFormulaSubstring(in string formula, int startIndex, out int endIndex)
        {
            endIndex = startIndex;
            int parenthesesCount = 0;
            for(int i = startIndex; i < formula.Length; i++)
            {
                endIndex = i;
                if(formula[i] == '(')
                    parenthesesCount++;
                else if(formula[i] == ')' && --parenthesesCount < 0)
                    break;
            }
            return formula.Substring(startIndex, endIndex - startIndex);
        }

        #endregion

        // ------------------------------

        #region Parsing Utils
        
        private static bool CharIsInt(char ch) => ch >= 48 && ch <= 57;
        private static int CharToInt(char ch) => ch - 48;

        #endregion
    }
}
