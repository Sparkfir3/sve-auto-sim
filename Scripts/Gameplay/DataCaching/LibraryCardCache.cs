using UnityEngine;
using Newtonsoft.Json;
using Sparkfire.Utility;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SVESimulator.CardTextData;
using CCGKit;

namespace SVESimulator
{
    public static class LibraryCardCache
    {
        #region Variables

        private class CardData
        {
            public Card card;
            public string displayId;
            public string name;
            public string text;
            public string trait;
            public Dictionary<string, EffectText> effects = new();
        }

        private const string TextDataResourcesPath = "CardTextData";

        private static TextAsset[] textDataList = null;
        private static Dictionary<int, CardData> cards = new(); // [CardID, Data]

        public static int CacheSize => cards.Count;

        #endregion

        // ------------------------------

        #region Cache Management

        public static void CacheCard(Card card)
        {
            if(card == null || cards.ContainsKey(card.id))
                return;
            CardData data = new CardData()
            {
                card = card
            };
            cards.TryAdd(card.id, data);
            CacheCardTextData(ref data);
        }

        public static void ClearCache()
        {
            cards.Clear();
        }

        #endregion

        // ------------------------------

        #region Get Info

        public static Card GetCard(in int cardId, in GameConfiguration config = null)
        {
            if(cards.TryGetValue(cardId, out CardData cardData) && cardData != null)
                return cardData.card;

            Card newCard = (config ?? GameManager.Instance.config).GetCard(cardId);
            CacheCard(newCard);
            return newCard;
        }

        public static Card GetCardFromInstanceId(in int instanceId, in GameConfiguration config = null)
        {
            CardObject cardObject = CardManager.Instance.GetCardByInstanceId(instanceId);
            if(!cardObject || cardObject.RuntimeCard == null)
                return null;
            return GetCard(cardObject.RuntimeCard.cardId, config);
        }

        // -----

        public static string GetName(in int cardId) => cards.GetValueOrDefault(cardId, null)?.name;
        public static string GetDisplayId(in int cardId) => cards.GetValueOrDefault(cardId, null)?.displayId;
        public static string GetCardText(in int cardId) => cards.GetValueOrDefault(cardId, null)?.text;
        public static string GetCardTrait(in int cardId) => cards.GetValueOrDefault(cardId, null)?.trait;

        public static string GetEffectName(in int cardId, in string effectName)
        {
            if(!cards.TryGetValue(cardId, out CardData cardData) || !cardData.effects.TryGetValue(effectName, out EffectText textData))
                return null;
            return textData?.name ?? (textData?.key ?? "");
        }

        public static string GetEffectText(in int cardId, in string effectName)
        {
            if(!cards.TryGetValue(cardId, out CardData cardData) || !cardData.effects.TryGetValue(effectName, out EffectText textData))
                return null;
            if(textData == null)
                return "";
            return textData.text ?? (textData.cost.IsNullOrWhiteSpace()
                ? $"{textData.trigger} {textData.body}".TrimStart()
                : $"{textData.trigger}{textData.cost}: {textData.body}".TrimStart());
        }

        public static string GetEffectTextCost(in int cardId, in string effectName)
        {
            if(!cards.TryGetValue(cardId, out CardData cardData) || !cardData.effects.TryGetValue(effectName, out EffectText textData))
                return null;
            return textData?.cost ?? "";
        }

        public static string GetEffectTextBody(in int cardId, in string effectName)
        {
            if(!cards.TryGetValue(cardId, out CardData cardData) || !cardData.effects.TryGetValue(effectName, out EffectText textData))
                return null;
            return textData?.body ?? (textData?.text ?? "");
        }

        #endregion

        // ------------------------------

        #region Cache Processing

        private static void CacheCardTextData(ref CardData data)
        {
            if(textDataList == null)
            {
                textDataList = Resources.LoadAll<TextAsset>(TextDataResourcesPath);
            }

            string baseId = data.card.GetStringProperty(SVEProperties.CardStats.ID) + "EN";
            foreach(TextAsset textAsset in textDataList)
            {
                List<TextData> allTextData = JsonConvert.DeserializeObject<List<TextData>>(textAsset.text);
                TextData textData = allTextData.FirstOrDefault(x => x.id.ToString().Equals(baseId));
                if(textData == null)
                    continue;

                data.displayId = textData.id;
                data.name = textData.name;
                data.text = FormatCardText(textData.cardText, "");
                data.trait = textData.trait;
                data.effects = textData.effectText?.ToDictionary(x => x.key);
                if(data.effects != null)
                {
                    foreach(EffectText effect in data.effects.Values)
                    {
                        effect.text = FormatCardText(effect.text);
                        effect.trigger = FormatCardText(effect.trigger);
                        effect.cost = FormatCardText(effect.cost);
                        effect.body = FormatCardText(effect.body);
                    }
                }
                else
                {
                    data.effects = new Dictionary<string, EffectText>();
                }
                return;
            }
        }

        private static string FormatCardText(string text, in string defaultValue = null)
        {
            if(text.IsNullOrWhiteSpace())
                return defaultValue;
            return TextFormatting.FormatCardText(text);
        }

        #endregion
    }
}
