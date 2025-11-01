using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using System.Collections;
using Sparkfire.Utility;
using CCGKit;
using SVESimulator.UI;
using TMPro;

namespace SVESimulator.DeckBuilder
{
    [DefaultExecutionOrder(1000)]
    public class DeckBuilderView : MonoBehaviour
    {
        #region Variables

        [SerializeField]
        private DeckBuilderModel model;
        [SerializeField]
        private DeckBuilderController controller;

        [Title("Sub Menus"), SerializeField]
        private DeckBuilderCardList cardList;
        [SerializeField]
        private DeckBuilderDeckPreview deckPreview;
        [SerializeField]
        private DeckBuilderDeckStats deckStats;
        [SerializeField]
        private DeckBuilderFilterOptionsMenu filterOptionsMenu;
        [SerializeField]
        private GameObject exitModal;
        [SerializeField]
        private CardInfoDisplay cardInfoDisplay;
        [SerializeField]
        private DeckBuilderSaveMenu saveMenu;

        [Title("Components"), SerializeField]
        private TMP_InputField deckNameInputField;
        [SerializeField]
        private GameObject exitDeckNotSavedWarning;

        #endregion

        // ------------------------------

        #region Unity Functions

        private void Awake()
        {
            filterOptionsMenu.Initialize();
            deckPreview.Initialize();
            saveMenu.Initialize();

            model.OnUpdateFilteredCardList += cardList.UpdateCardList;
            model.OnUpdateDeck += UpdateDeck;
            deckPreview.OnMouseHoverOverCard += SetInfoDisplay;
            cardList.OnMouseHoverOverCard += SetInfoDisplay;
        }

        private void Start()
        {
            CloseAdvancedFilters();
            CloseSaveDeckMenu();
            CloseExitModal();
            UpdateDeck();
            cardList.CurrentPage = 1;
            SetInfoDisplay(model.FilteredCardList?[0]);
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                if(filterOptionsMenu.isActiveAndEnabled)
                    CloseAdvancedFilters();
                if(saveMenu.isActiveAndEnabled)
                    CloseSaveDeckMenu();
                if(exitModal.activeInHierarchy)
                    CloseExitModal();
            }
        }

        #endregion

        // ------------------------------

        #region Controls

        private void UpdateDeck()
        {
            deckPreview.UpdateDeck(model.CurrentLeader, model.CurrentMainDeck, model.CurrentEvolveDeck);
            deckStats.UpdateDeckStats();
        }

        private void SetInfoDisplay(Card card)
        {
            if(card == null)
                return;
            cardInfoDisplay.Display(card);
        }

        #endregion

        // ------------------------------

        #region Open/Close Menus

        public void OpenAdvancedFilters()
        {
            filterOptionsMenu.gameObject.SetActive(true);
        }

        public void CloseAdvancedFilters()
        {
            filterOptionsMenu.gameObject.SetActive(false);
        }

        public void OpenSaveDeckMenu()
        {
            saveMenu.Show();
        }

        public void CloseSaveDeckMenu()
        {
            saveMenu.Hide();
        }

        public void OpenExitModal()
        {
            exitDeckNotSavedWarning.SetActive(model.IsDirty && (model.CurrentLeader != null || model.MainDeckCount > 0 || model.EvolveDeckCount > 0));
            exitModal.SetActive(true);
        }

        public void CloseExitModal()
        {
            exitModal.SetActive(false);
        }

        #endregion
    }
}
