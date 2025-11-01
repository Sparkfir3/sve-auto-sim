using System;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using CCGKit;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace SVESimulator
{
    public class DeckBuilderCard : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
    {
        [field: SerializeField]
        public RawImage Image { get; private set; }
        [SerializeField]
        private GameObject containerAmountText;
        [SerializeField]
        private TextMeshProUGUI textCurrentCount;
        [SerializeField]
        private TextMeshProUGUI textMaxCount;

        [ShowInInspector, LabelText("Name"), HideInEditorMode]
        public string CardName => CurrentCard != null ? CurrentCard.name : "";
        [ShowInInspector, LabelText("ID"), HideInEditorMode]
        public string ID => CurrentCard != null ? CurrentCard.GetStringProperty(SVEProperties.CardStats.ID) : "";

        public Card CurrentCard { get; set; }

        public event Action OnMouseEnter;
        public event Action OnLeftClick;
        public event Action OnRightClick;

        // ------------------------------

        public void SetCard(Card card, int amount = 0)
        {
            CurrentCard = card;
            Image.texture = CurrentCard != null ? CardTextureManager.GetCardTexture(CurrentCard.GetStringProperty(SVEProperties.CardStats.ID)) : CardTextureManager.GetCardBackTexture();
            SetAmount(amount);
        }

        public void SetAmount(int amount, int max = 3)
        {
            // Leader or none
            if(CurrentCard == null || CurrentCard.cardTypeId == 5 || amount == 0)
            {
                containerAmountText.SetActive(false);
                return;
            }

            containerAmountText.SetActive(true);
            textCurrentCount.text = $"{amount}";
            textMaxCount.text = $"{max}";
        }

        // ------------------------------

        public void OnPointerClick(PointerEventData eventData)
        {
            if(eventData.button == PointerEventData.InputButton.Left)
                OnLeftClick?.Invoke();
            else if(eventData.button == PointerEventData.InputButton.Right)
                OnRightClick?.Invoke();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnMouseEnter?.Invoke();
        }
    }
}
