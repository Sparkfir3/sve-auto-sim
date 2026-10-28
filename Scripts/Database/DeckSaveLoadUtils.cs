using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Sparkfire.Utility;
using SVESimulator.SveScript;
using UnityEngine;

namespace SVESimulator.Database
{
    public static class DeckSaveLoadUtils
    {
        [Serializable]
        public struct CardAmountPair
        {
            public string id;
            public int ccgId;
            public int amount;

            public CardAmountPair(string id, int ccgId, int amount)
            {
                this.id = id;
                this.ccgId = ccgId;
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
                string cardId = cardInfo[1].Trim();
                int ccgId = CardIDConversion.CardIdToCCGKitId(cardId);
                cards.Add(new CardAmountPair(cardId, ccgId, amount));
            }
            return cards;
        }

        public static string LoadAsRuntimeJson(string name, string data)
        {
            List<CardAmountPair> cards = LoadDeck(data, out _);
            JObject deck = new() { { "name", name } };

            JArray cardList = new();
            foreach(CardAmountPair amountInfo in cards)
            {
                JObject cardCount = new()
                {
                    { "id", amountInfo.ccgId },
                    { "amount", amountInfo.amount }
                };
                cardList.Add(cardCount);
            }
            deck.Add("cards", cardList);

            return deck.ToString();
        }
    }
}
