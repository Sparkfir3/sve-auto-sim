using System;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CCGKit;

namespace SVESimulator.DeckBuilder
{
    public class DeckBuilderDeckPreview : MonoBehaviour
    {
        [SerializeField]
        private DeckBuilderCard leaderCardImage;
        [SerializeField]
        private List<DeckBuilderCard> mainDeckCardImages;
        [SerializeField]
        private List<DeckBuilderCard> evolveDeckCardImages;
        [SerializeField]
        private GameObject missingLeader;
        [SerializeField]
        private GameObject emptyMainDeck;
        [SerializeField]
        private GameObject emptyEvolveDeck;

        public event Action<Card> OnMouseHoverOverCard;
        public event Action<Card> AddCard;
        public event Action<Card> RemoveCard;
        
        // ------------------------------
        
        public void Initialize()
        {
            leaderCardImage.OnMouseEnter += () => OnMouseHoverOverCard?.Invoke(leaderCardImage.CurrentCard);
            leaderCardImage.OnLeftClick += () => AddCard?.Invoke(leaderCardImage.CurrentCard);
            leaderCardImage.OnRightClick += () => RemoveCard?.Invoke(leaderCardImage.CurrentCard);
            foreach(DeckBuilderCard card in mainDeckCardImages)
            {
                card.OnMouseEnter += () => OnMouseHoverOverCard?.Invoke(card.CurrentCard);
                card.OnLeftClick += () => AddCard?.Invoke(card.CurrentCard);
                card.OnRightClick += () => RemoveCard?.Invoke(card.CurrentCard);
            }
            foreach(DeckBuilderCard card in evolveDeckCardImages)
            {
                card.OnMouseEnter += () => OnMouseHoverOverCard?.Invoke(card.CurrentCard);
                card.OnLeftClick += () => AddCard?.Invoke(card.CurrentCard);
                card.OnRightClick += () => RemoveCard?.Invoke(card.CurrentCard);
            }
        }
        
        // ------------------------------
        
        public void UpdateDeck(Card leader, Dictionary<Card, int> mainDeck, Dictionary<Card, int> evolveDeck)
        {
            missingLeader.SetActive(leader == null);
            leaderCardImage.gameObject.SetActive(leader != null);
            leaderCardImage.SetCard(leader);

            List<Card> mainDeckOrderedList = mainDeck.Select(x => x.Key).OrderBy(x =>
                {
                    Stat stat = x.stats.FirstOrDefault(y => y.name.Equals(SVEProperties.CardStats.Cost));
                    return stat?.baseValue ?? 99;
                })
                .ThenBy(x => x.cardTypeId)
                .ThenBy(x => x.id).ToList();
            ApplyCardImages(mainDeckOrderedList, mainDeck, mainDeckCardImages);
            emptyMainDeck.SetActive(mainDeckOrderedList.Count == 0);

            List<Card> evolveDeckOrderedList = evolveDeck.Select(x => x.Key)
                .OrderBy(x => x.cardTypeId)
                .ThenBy(x => x.id).ToList();
            ApplyCardImages(evolveDeckOrderedList, evolveDeck, evolveDeckCardImages);
            emptyEvolveDeck.SetActive(evolveDeckOrderedList.Count == 0);
        }

        private void ApplyCardImages(List<Card> orderedList, Dictionary<Card, int> deck, List<DeckBuilderCard> imageList)
        {
            int i = 0;
            foreach(Card card in orderedList)
            {
                int amount = deck[card];
                if(i >= imageList.Count)
                    AddNewImage(imageList);
                imageList[i].gameObject.SetActive(true);
                imageList[i].SetCard(card, amount);
                i++;
            }
            for(; i < imageList.Count; i++)
                imageList[i].gameObject.SetActive(false);
        }

        private void AddNewImage(List<DeckBuilderCard> imageList)
        {
            DeckBuilderCard newCard = Instantiate(imageList[0], imageList[0].transform.parent, true);
            newCard.OnMouseEnter += () => OnMouseHoverOverCard?.Invoke(newCard.CurrentCard);
            newCard.OnLeftClick += () => AddCard?.Invoke(newCard.CurrentCard);
            newCard.OnRightClick += () => RemoveCard?.Invoke(newCard.CurrentCard);
            imageList.Add(newCard);
        }
    }
}
