using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sparkfire.AppStateSystem;
using Sparkfire.Utility;
using TMPro;

namespace SVESimulator.DeckBuilder
{
    public class DeckBuilderController : MonoBehaviour
    {
        #region Variables

        public static string DeckDataToLoad = null;
        public static string DeckNameToLoad = null;

        [SerializeField]
        private DeckBuilderModel model;
        [SerializeField]
        private DeckBuilderView view;

        [Title("Filters Handling"), SerializeField]
        private float filterUpdateTimerDuration = 0.5f;
        [SerializeField, ProgressBar(0f, "filterUpdateTimerDuration"), ReadOnly, HideInEditorMode]
        private float filterUpdateTimer;
        [SerializeField]
        private List<CardListSorting.SortMode> sortModes = new();

        [Title("Sub Menus"), SerializeField]
        private DeckBuilderCardList cardList;
        [SerializeField]
        private DeckBuilderDeckPreview deckPreview;
        [SerializeField]
        private DeckBuilderSaveMenu saveMenu;

        [Title("Components"), SerializeField]
        private TMP_Dropdown sortTypeDropdown;
        [SerializeField]
        private ApplicationStateTransition exitTransitioner;

        [Title("Asset References"), SerializeField, InlineEditor]
        private FilePathInfo pathInfo;

        #endregion

        // ------------------------------

        #region Unity Functions

        private void Awake()
        {
            cardList.AddCard += model.AddCard;
            deckPreview.AddCard += model.AddCard;
            cardList.RemoveCard += model.RemoveCard;
            deckPreview.RemoveCard += model.RemoveCard;
            
            saveMenu.OnSaveDeck += SaveDeck;
            model.OnUpdateFilters += HandleFiltersUpdated;
            cardList.OnListUpdated += ManageLibraryCache;

            if(sortModes.Count == 0)
                sortModes.Add(0);
            sortTypeDropdown.ClearOptions();
            sortTypeDropdown.AddOptions(sortModes.Select(x => x.ToString()).ToList());
            sortTypeDropdown.value = 0;
            sortTypeDropdown.onValueChanged.AddListener(HandleSortModeUpdated);
        }

        private void Start()
        {
            LibraryCardCache.ClearCache();
            if(!DeckDataToLoad.IsNullOrWhiteSpace())
            {
                model.ImportDeck(DeckDataToLoad);
            }
            DeckDataToLoad = null;
            DeckNameToLoad = null;

            model.SortMode = sortModes[sortTypeDropdown.value];
            model.UpdateFilteredCardList();
        }

        private void LateUpdate()
        {
            if(filterUpdateTimer > 0f)
            {
                filterUpdateTimer -= Time.deltaTime;
                if(filterUpdateTimer <= 0f)
                {
                    model.UpdateFilteredCardList();
                }
            }
        }

        private void OnDestroy()
        {
            DeckDataToLoad = null;
            DeckNameToLoad = null;
        }

        #endregion

        // ------------------------------

        #region Filter/Data Updating

        private void HandleFiltersUpdated()
        {
            filterUpdateTimer = filterUpdateTimerDuration;
        }

        private void HandleSortModeUpdated(int index)
        {
            model.SortMode = sortModes[index];
            model.UpdateFilteredCardList();
            filterUpdateTimer = 0f;
        }

        private void ManageLibraryCache()
        {
            if(LibraryCardCache.CacheSize > 100)
                LibraryCardCache.ClearCache();
        }

        #endregion

        // ------------------------------

        #region Save Handling

        private void SaveDeck(string deckName)
        {
            if(deckName.IsNullOrWhiteSpace())
                return;

            string filePath = Path.Combine(pathInfo.DeckFolderPath, $"{deckName}.txt");
            if(File.Exists(filePath))
                File.Delete(filePath);
            Directory.CreateDirectory(pathInfo.DeckFolderPath);
            using(FileStream _ = File.Create(filePath)) { }

            using(StreamWriter writer = new(filePath))
            {
                string deckCode = model.DeckAsString();
                writer.Write(deckCode);
            }
            model.IsDirty = false;
            view.CloseSaveDeckMenu();
        }

        #endregion

        // ------------------------------

        #region Other

        public void ExitDeckBuilder()
        {
            exitTransitioner.Transition();
        }

        #endregion
    }
}
