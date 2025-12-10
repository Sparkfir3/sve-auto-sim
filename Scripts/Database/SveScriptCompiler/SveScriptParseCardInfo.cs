using System;
using System.Collections.Generic;
using System.Linq;
using Sparkfire.Utility;
using UnityEngine;
using static SVESimulator.SveScript.SveScriptData;

namespace SVESimulator.SveScript
{
    internal static class SveScriptParseCardInfo
    {
	    #region Parse Data

	    public static void ParseAndSaveCardType(in string text, ref CardInfo cardInfo)
	    {
		    if(text.ToLower().Contains("/ token"))
			    cardInfo.trait = cardInfo.trait.IsNullOrWhiteSpace() ? "Token" : cardInfo.trait + " / Token";
		    cardInfo.ccgCardTypeId = CardTypeIDs.GetValueOrDefault(text.ToLower().Replace("/ token", "").Trim(), 0);
	    }

	    public static string ParseClass(in string text)
	    {
		    return ClassList.GetValueOrDefault(text.ToLower().Replace("craft", ""), "Neutral");
	    }

	    public static string ParseUniverse(in string text)
	    {
		    return UniverseList.GetValueOrDefault(text.ToLower());
	    }

	    public static string ParseRarity(in string text)
	    {
		    return RarityList.GetValueOrDefault(text.ToLower());
	    }

	    public static void ParseAndSaveAtkDef(in string text, ref CardInfo cardInfo)
	    {
		    string[] stats = text.Split('/');
		    try
		    {
			    cardInfo.attack = int.Parse(stats[0].Trim());
			    cardInfo.defense = int.Parse(stats[1].Trim());
		    }
		    catch(Exception e)
		    {
			    Debug.LogError($"Error while parsing card stats (input of {text}):\n{e}");
		    }
	    }

	    public static void ParseAndSaveCardText(in string text, ref CardInfo cardInfo)
	    {
		    cardInfo.text = string.Join("\n", text.Split("\n").Select(x => x.Trim())).Trim();
		    if(cardInfo.text.StartsWith('\"') && cardInfo.text.EndsWith('\"'))
			    cardInfo.text = cardInfo.text[1..^1];

		    // foreach(var formattingInfo in TextFormatting)
		    // {
			   //  (string key, string value) = (formattingInfo.Key, formattingInfo.Value);
			   //  cardInfo.text = cardInfo.text.Replace(key, value);
		    // }
	    }

	    #endregion

        // ------------------------------

        #region Compile Properties/Stats

		public static void CompileCardProperties(ref CardInfo cardInfo)
		{
			cardInfo.properties.Add(new CardPropertyString(cardInfo.cardClass, "Class"));
			if(!string.IsNullOrWhiteSpace(cardInfo.universe)) // only if has universe, otherwise it's clutter
				cardInfo.properties.Add(new CardPropertyString(cardInfo.universe, "Universe"));
			if(cardInfo.ccgCardTypeId != 5) // not leader
			{
				cardInfo.properties.Add(new CardPropertyString(cardInfo.trait, "Trait"));
				cardInfo.properties.Add(new CardPropertyString(cardInfo.text, "Text"));
			}
			cardInfo.properties.Add(new CardPropertyString(cardInfo.cardID, "ID"));
			cardInfo.ccgID = CardIDConversion.CardIdToCCGKitId(cardInfo.cardID);

			if(cardInfo.ccgCardTypeId == CardTypeIDs["evolved follower"] && !cardInfo.name.EndsWith("(Evolved)"))
				cardInfo.name += " (Evolved)";
			else if(cardInfo.name.EndsWith("(Evolved)") && cardInfo.ccgCardTypeId == CardTypeIDs["follower"])
				cardInfo.ccgCardTypeId = CardTypeIDs["evolved follower"];
		}

		public static void CompileCardStats(ref CardInfo cardInfo)
		{
			if(cardInfo.ccgCardTypeId == 0 || cardInfo.ccgCardTypeId == 1) // follower or evolved follower
			{
				cardInfo.stats.Add(new CardStat(cardInfo.attack,     0, "Attack",      0, 99));
				cardInfo.stats.Add(new CardStat(cardInfo.defense,    1, "Defense",     0, 99));
			}
			if(cardInfo.ccgCardTypeId == 0 || cardInfo.ccgCardTypeId == 1 || cardInfo.ccgCardTypeId == 4) // follower, evolved follower, or amulet
				cardInfo.stats.Add(new CardStat(0,                   2, "Engaged",     0, 1));
			if(cardInfo.ccgCardTypeId == 0) // regular follower
				cardInfo.stats.Add(new CardStat(cardInfo.evolveCost, 3, "Evolve Cost", 0, 99));
			if(cardInfo.ccgCardTypeId != 1 && cardInfo.ccgCardTypeId != 5) // not evolved follower or leader
				cardInfo.stats.Add(new CardStat(cardInfo.cost,       4, "Cost",        0, 99));
			if(cardInfo.ccgCardTypeId == 0 || cardInfo.ccgCardTypeId == 1) // follower or evolved follower
				cardInfo.stats.Add(new CardStat(-1,                  5, "Attached Instance IDs", -1, 999999));
			if(cardInfo.ccgCardTypeId == 1) // evolved follower
				cardInfo.stats.Add(new CardStat(0,                   6, "Face Up",     0, 1));
		}

		#endregion
    }
}
