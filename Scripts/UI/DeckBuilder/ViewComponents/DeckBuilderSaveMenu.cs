using System;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using CCGKit;
using Sparkfire.Utility;
using SVESimulator.UI;
using TMPro;
using UnityEngine.UI;

namespace SVESimulator.DeckBuilder
{
    public class DeckBuilderSaveMenu : MonoBehaviour
    {
        [Title("Settings"), SerializeField]
        private SerializedDictionary<DeckConstructionErrors, string> constructionErrorStrings;

        [Title("Object References"), SerializeField]
        private DeckBuilderModel model;
        [SerializeField]
        private CardInfoDisplay cardInfoDisplay;
        [SerializeField]
        private Transform errorsContainer;
        [SerializeField]
        private GameObject errorWarningPrefab;

        [Title("Components"), SerializeField]
        private DeckBuilderDeckPreview deckPreview;
        [SerializeField]
        private TMP_InputField deckNameInputField;
        [SerializeField]
        private Button saveDeckButton;

        private DeckConstructionErrors currentErrors = DeckConstructionErrors.None;

        public event Action<string> OnSaveDeck;

        // ------------------------------

        public void Initialize()
        {
            if(!DeckBuilderController.DeckNameToLoad.IsNullOrWhiteSpace())
                deckNameInputField.SetTextWithoutNotify(DeckBuilderController.DeckNameToLoad);
            deckNameInputField.onValueChanged.AddListener(_ => UpdateSaveButton());
            saveDeckButton.onClick.AddListener(() => OnSaveDeck?.Invoke(deckNameInputField.text));

            deckPreview.Initialize();
            deckPreview.AddCard += model.AddCard;
            deckPreview.RemoveCard += model.RemoveCard;
            deckPreview.OnMouseHoverOverCard += SetInfoDisplay;
        }

        public void Show()
        {
            gameObject.SetActive(true);
            UpdateSaveButton();
            UpdateDeck();

            model.OnUpdateDeck += UpdateDeck;
        }

        public void Hide()
        {
            gameObject.SetActive(false);

            model.OnUpdateDeck -= UpdateDeck;
        }

        // ------------------------------

        private void UpdateDeck()
        {
            deckPreview.UpdateDeck(model.CurrentLeader, model.CurrentMainDeck, model.CurrentEvolveDeck);

            if(!model.IsDeckValid(out DeckConstructionErrors errors) || deckNameInputField.text.IsNullOrWhiteSpace())
            {
                errorsContainer.gameObject.SetActive(true);
                if(deckNameInputField.text.IsNullOrWhiteSpace())
                    errors |= DeckConstructionErrors.NoName;
                PopulateErrors(errors);
            }
            else
            {
                errorsContainer.gameObject.SetActive(false);
                currentErrors = DeckConstructionErrors.None;
            }
        }

        private void UpdateSaveButton()
        {
            saveDeckButton.interactable = !deckNameInputField.text.IsNullOrWhiteSpace() && ((model.CurrentLeader != null ? 1 : 0) + model.MainDeckCount + model.EvolveDeckCount > 0);
            if(((currentErrors & DeckConstructionErrors.NoName) != 0 && saveDeckButton.interactable)
               || ((currentErrors & DeckConstructionErrors.NoName) == 0 && !saveDeckButton.interactable))
                UpdateDeck(); // causes error list to update
        }

        private void PopulateErrors(DeckConstructionErrors errors)
        {
            if(errors == currentErrors)
                return;

            for(int i = errorsContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(errorsContainer.GetChild(i).gameObject);
            }

            foreach(var kvPair in constructionErrorStrings)
            {
                (DeckConstructionErrors error, string text) = (kvPair.Key, kvPair.Value);
                if((errors & error) != error)
                    continue;

                GameObject warningObj = Instantiate(errorWarningPrefab, errorsContainer);
                TextMeshProUGUI textObj = warningObj.GetComponentInChildren<TextMeshProUGUI>();
                if(textObj)
                    textObj.text = text;
            }
            currentErrors = errors;
        }

        private void SetInfoDisplay(Card card)
        {
            if(card == null)
                return;
            cardInfoDisplay.Display(card);
        }

        // ------------------------------

#if UNITY_EDITOR
        private void OnValidate()
        {
            if(!model)
                model = GetComponentInParent<DeckBuilderModel>();
        }
#endif
    }
}
