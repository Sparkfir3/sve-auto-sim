using System;
using System.Collections.Generic;
using Sparkfire.Utility;
using UnityEngine;

namespace SVESimulator.Database
{
    public static class DeckSaveLoadUtils
    {
        public struct CardAmountPair
        {
            public int id;
            public int amount;

            public CardAmountPair(int id, int amount)
            {
                this.id = id;
                this.amount = amount;
            }
        }

        public static List<CardAmountPair> LoadDeck(string data, out string deckClass)
        {
            string version = null; // unused currently, but here for future-proofing
            deckClass = null;
            List<CardAmountPair> cards = new();

            foreach(string line in data.Split('\n'))
            {
                if(line.IsNullOrWhiteSpace() || line.TrimStart().StartsWith('#'))
                    continue;
                if(deckClass == null)
                {
                    string[] versionInfo = line.Trim().Split();
                    deckClass = versionInfo[0];
                    version = versionInfo.Length > 0 ? versionInfo[1] : null;
                    continue;
                }

                string[] cardInfo = line.Split(' ');
                if(cardInfo.Length < 2)
                {
                    Debug.LogError($"Invalid line found in deck: {line}");
                    continue;
                }
                int amount = int.Parse(cardInfo[0]);
                int ccgId = int.Parse(cardInfo[1]); // TODO - convert SVE ID to CCG Kit ID
                cards.Add(new CardAmountPair(ccgId, amount));
            }
            return cards;
        }
    }
}
