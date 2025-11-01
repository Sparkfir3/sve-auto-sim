using System;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CCGKit;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.UI;

namespace SVESimulator.DeckBuilder
{
    public class DeckBuilderCardList : MonoBehaviour
    {
        #region Variables

        [field: Title("Page Info"), SerializeField]
        public int CurrentPage { get; set; } = 1;
        [field: SerializeField]
        public int MaxPages { get; set; } = 1;

        [Title("Object References"), SerializeField]
        private DeckBuilderModel model;
        [SerializeField]
        private List<DeckBuilderCard> cardImages;
        [SerializeField]
        private TextMeshProUGUI textCurrentPage;
        [SerializeField]
        private TextMeshProUGUI textMaxPage;
        [FoldoutGroup("Navigation Buttons"), SerializeField]
        private Button buttonFirst;
        [FoldoutGroup("Navigation Buttons"), SerializeField]
        private Button buttonPrevious;
        [FoldoutGroup("Navigation Buttons"), SerializeField]
        private Button buttonNext;
        [FoldoutGroup("Navigation Buttons"), SerializeField]
        private Button buttonLast;

        public int ImageCount => cardImages.Count(x => x.isActiveAndEnabled);

        public event Action<Card> OnMouseHoverOverCard;
        public event Action<Card> AddCard;
        public event Action<Card> RemoveCard;

        #endregion

        // ------------------------------

        #region Unity Functions

        private void Start()
        {
            foreach(DeckBuilderCard card in cardImages)
            {
                card.OnMouseEnter += () => OnMouseHoverOverCard?.Invoke(card.CurrentCard);
                card.OnLeftClick += () => AddCard?.Invoke(card.CurrentCard);
                card.OnRightClick += () => RemoveCard?.Invoke(card.CurrentCard);
            }
            model.OnUpdateDeck += UpdateCardCounts;
            buttonFirst.onClick.AddListener(FirstPage);
            buttonPrevious.onClick.AddListener(PreviousPage);
            buttonNext.onClick.AddListener(NextPage);
            buttonLast.onClick.AddListener(LastPage);
        }

        #endregion

        // ------------------------------

        #region List Controls

        [Title("Controls"), Button, DisableInEditorMode]
        public void UpdateCardList()
        {
            if(model.FilteredListCount == 0)
            {
                MaxPages = 1;
                CurrentPage = 1;
                textMaxPage.text = "1";
                textCurrentPage.text = "1";
                SetImages(null);
                return;
            }

            MaxPages = Mathf.CeilToInt((float)model.FilteredListCount / ImageCount);
            CurrentPage = Mathf.Clamp(CurrentPage, 1, MaxPages);
            textMaxPage.text = MaxPages.ToString();
            textCurrentPage.text = CurrentPage.ToString();
            buttonFirst.interactable = CurrentPage > 1;
            buttonPrevious.interactable = CurrentPage > 1;
            buttonNext.interactable = CurrentPage < MaxPages;
            buttonLast.interactable = CurrentPage < MaxPages;

            int index = ImageCount * (CurrentPage - 1);
            int listCount = Mathf.Min(model.FilteredCardList.Count - index, ImageCount);
            SetImages(model.FilteredCardList.GetRange(index, listCount));
        }

        [Button, HorizontalGroup("Page Prev"), DisableInEditorMode]
        public void FirstPage()
        {
            CurrentPage = 1;
            UpdateCardList();
        }

        [Button, HorizontalGroup("Page Prev"), DisableInEditorMode]
        public void PreviousPage()
        {
            if(CurrentPage <= 1)
                return;
            CurrentPage--;
            UpdateCardList();
        }

        [Button, HorizontalGroup("Page Next"), DisableInEditorMode]
        public void NextPage()
        {
            if(CurrentPage >= MaxPages)
                return;
            CurrentPage++;
            UpdateCardList();
        }

        [Button, HorizontalGroup("Page Next"), DisableInEditorMode]
        public void LastPage()
        {
            CurrentPage = MaxPages;
            UpdateCardList();
        }

        #endregion

        // ------------------------------

        #region Data/Component Handling

        private void SetImages(List<Card> cardList)
        {
            for(int i = 0; i < cardImages.Count; i++)
            {
                if(cardList == null || i >= cardList.Count)
                {
                    cardImages[i].Image.enabled = false;
                    cardImages[i].SetAmount(0);
                    continue;
                }

                cardImages[i].Image.enabled = true;
                cardImages[i].SetCard(cardList[i], model.GetCardAmount(cardList[i]));
            }
        }

        private void UpdateCardCounts()
        {
            // TODO - this is moderately inefficient and possibly computationally costly, keep an eye on it
            foreach(DeckBuilderCard cardDisplay in cardImages)
            {
                if(!cardDisplay.Image.isActiveAndEnabled || cardDisplay.CurrentCard == null)
                {
                    cardDisplay.SetAmount(0);
                    continue;
                }
                cardDisplay.SetAmount(model.GetCardAmount(cardDisplay.CurrentCard));
            }
        }

        #endregion
    }
}
