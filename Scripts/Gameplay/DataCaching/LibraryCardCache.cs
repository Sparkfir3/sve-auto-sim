using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using CCGKit;

namespace SVESimulator
{
    public static class LibraryCardCache
    {
        private static Dictionary<int, Card> cards = new(); // [CardID, LibraryCard]

        public static void ClearCache()
        {
            cards.Clear();
        }

        public static Card GetCard(in int cardId, in GameConfiguration config = null)
        {
            if(cards.TryGetValue(cardId, out Card card) && card != null)
                return card;

            Card newCard = (config ?? GameManager.Instance.config).GetCard(cardId);
            if(newCard != null)
                cards.TryAdd(cardId, newCard);
            return newCard;
        }

        public static Card GetCardFromInstanceId(in int instanceId, in GameConfiguration config = null)
        {
            CardObject cardObject = CardManager.Instance.GetCardByInstanceId(instanceId);
            if(!cardObject || cardObject.RuntimeCard == null)
                return null;
            return GetCard(cardObject.RuntimeCard.instanceId, config);
        }

        public static void CacheCard(Card card)
        {
            if(card == null)
                return;
            cards.TryAdd(card.id, card);
        }
    }
}
