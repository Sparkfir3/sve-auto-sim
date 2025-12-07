using System;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using Sparkfire.Utility;
using Sparkfire.AppStateSystem;
using SVESimulator.DeckBuilder;
using TMPro;
using CCGKit;
using SVESimulator.Database;

namespace SVESimulator
{
    public class DeckSelectionController : MonoBehaviour
    {
        [Serializable]
        public class DeckInfo
        {
            public string name;
            public string data;
            public string deckClass;
            public DateTime editTime;

            public DeckInfo(string name, string data, string deckClass, DateTime editTime = default)
            {
                this.name = name;
                this.data = data;
                this.deckClass = deckClass;
                this.editTime = editTime;
            }
        }

        [Serializable]
        public class StarterDeck
        {
            public string name;
            public TextAsset data;
            public string deckClass;
        }

        // ------------------------------

        #region Variables

        [field: Title("Runtime Data"), SerializeField, DisableInEditorMode]
        public string CurrentDeckName { get; private set; }
        [field: SerializeField, DisableInEditorMode]
        public string CurrentDeckData { get; private set; }

        [Title("Decks"), SerializeField, TableList]
        private List<StarterDeck> starterDecks;
        public List<DeckInfo> LoadedStarterDecks { get; private set; } = new();
        [field: SerializeField, DisableInEditorMode, TableList]
        public List<DeckInfo> LoadedDecks { get; private set; }

        [Title("Object References"), SerializeField]
        private List<DeckSelectionEntry> deckEntries;
        [SerializeField]
        private TextMeshProUGUI currentDeckText;
        [SerializeField]
        private Transform entriesContainer;
        [SerializeField]
        private ToggleGroup toggleGroup;
        [SerializeField]
        private GameObject noDecksFoundWarning;
        [SerializeField]
        private Button newDeckButton;
        [SerializeField]
        private ApplicationStateTransition deckBuilderTransitioner;
        [SerializeField]
        private GameObject starterDecksTabBackground;
        [SerializeField]
        private GameObject customDecksTabBackground;

        [Title("Asset References"), SerializeField, InlineEditor]
        private FilePathInfo pathInfo;
        [SerializeField, AssetsOnly]
        private DeckSelectionEntry deckEntryPrefab;
        [SerializeField]
        private SerializedDictionary<string, Sprite> classIcons;
        [SerializeField]
        private Sprite defaultClassIcon;

        public bool HasSelectedDeck => !CurrentDeckData.IsNullOrWhiteSpace();
        public event Action OnSelectDeck;

        #endregion

        // ------------------------------

        #region Initialize & Controls

        public void Initialize()
        {
            // Clear existing
            foreach(DeckSelectionEntry entry in deckEntries)
                Destroy(entry.gameObject);
            deckEntries.Clear();
            CurrentDeckName = "";
            CurrentDeckData = "";
            UpdateCurrentDeckText();

            // Init
            LoadStarterDecks();
            LoadDecksListFromFiles();
            if(LoadedDecks.Count > 0)
                OpenLoadedDecksTab();
            else
                OpenStarterDecksTab();
        }

        public void OpenStarterDecksTab()
        {
            starterDecksTabBackground.SetActive(true);
            customDecksTabBackground.SetActive(false);
            SetListOfDecks(LoadedStarterDecks, true);
        }

        public void OpenLoadedDecksTab()
        {
            starterDecksTabBackground.SetActive(false);
            customDecksTabBackground.SetActive(true);
            SetListOfDecks(LoadedDecks, false);
        }

        public void LoadCurrentDeck()
        {
            Debug.Log($"Loading deck: {CurrentDeckName}");
            GameManager.Instance.defaultDeck = JsonUtility.FromJson<Deck>(DeckSaveLoadUtils.LoadAsRuntimeJson(CurrentDeckName, CurrentDeckData));
        }

        public void LoadDeckBuilder() => LoadDeckBuilder(null, null);
        public void LoadDeckBuilder(string deckData, string deckName)
        {
            DeckBuilderController.DeckDataToLoad = deckData;
            DeckBuilderController.DeckNameToLoad = deckName;
            deckBuilderTransitioner.Transition();
        }

        #endregion

        // ------------------------------

        #region Data/UI Handling

        private void LoadStarterDecks()
        {
            LoadedStarterDecks.Clear();
            foreach(StarterDeck sd in starterDecks)
                LoadedStarterDecks.Add(new DeckInfo(sd.name, sd.data.text, sd.deckClass));
        }

        private void LoadDecksListFromFiles()
        {
            LoadedDecks.Clear();
            if(!Directory.Exists(pathInfo.DeckFolderPath))
            {
                Directory.CreateDirectory(pathInfo.DeckFolderPath);
                return;
            }
            string[] deckFiles = Directory.GetFiles(pathInfo.DeckFolderPath, "*.txt");
            foreach(string fileName in deckFiles)
            {
                string deckName = fileName.Replace("/", "\\").Split("\\")[^1].Replace(".txt", "");
                string deckData = File.ReadAllText(fileName);
                string deckClass = deckData[..deckData.IndexOf(" ", StringComparison.Ordinal)];
                DateTime editTime = File.GetLastWriteTime(fileName);
                LoadedDecks.Add(new DeckInfo(deckName, deckData, deckClass, editTime));
            }
            LoadedDecks = LoadedDecks.OrderBy(x => x.editTime).ToList();
        }

        private void SetListOfDecks(List<DeckInfo> deckInfo, bool isStarterDeck)
        {
            foreach(DeckSelectionEntry entry in deckEntries)
                entry.gameObject.SetActive(false);
            int i = 0;
            foreach(DeckInfo info in deckInfo)
            {
                DeckSelectionEntry entry = GetNextAvailableEntry();
                entry.SetInfo(info.name, info.data, classIcons.GetValueOrDefault(info.deckClass, defaultClassIcon), isStarterDeck);
                entry.gameObject.SetActive(true);
                i++;
            }
            for(; i < deckEntries.Count; i++)
                deckEntries[i].gameObject.SetActive(false);

            if(deckInfo.Count > 0)
            {
                noDecksFoundWarning.SetActive(false);
                deckEntries[0].SelectDeck();
                CurrentDeckName = deckInfo[0].name;
                CurrentDeckData = deckInfo[0].data;
            }
            else
            {
                noDecksFoundWarning.SetActive(true);
                CurrentDeckName = "";
                CurrentDeckData = "";
            }
            UpdateCurrentDeckText();
        }

        private DeckSelectionEntry GetNextAvailableEntry()
        {
            DeckSelectionEntry entry = deckEntries.FirstOrDefault(x => !x.isActiveAndEnabled);
            if(entry)
            {
                entry.gameObject.SetActive(true);
                return entry;
            }

            entry = Instantiate(deckEntryPrefab, entriesContainer);
            entry.Initialize(toggleGroup);
            entry.OnSelect += SelectDeck;
            entry.OnEdit += EditDeck;
            entry.OnDelete += DeleteDeck;
            deckEntries.Add(entry);
            return entry;
        }

        private void UpdateCurrentDeckText()
        {
            currentDeckText.text = !CurrentDeckName.IsNullOrWhiteSpace() ? CurrentDeckName : "None";
        }

        #endregion

        // ------------------------------

        #region Event Handling

        private void SelectDeck(string deckName, string deckData)
        {
            CurrentDeckName = deckName;
            CurrentDeckData = deckData;
            UpdateCurrentDeckText();
            OnSelectDeck?.Invoke();
        }

        private void EditDeck(string deckName, string deckData)
        {
            // TODO - confirm modal
            LoadDeckBuilder(deckData, deckName);
        }

        private void DeleteDeck(string deckName, string deckData)
        {
            string filePath = Path.Join(pathInfo.DeckFolderPath, $"{deckName}.txt");
            if(!File.Exists(filePath))
                return;
            File.Delete(filePath);

            DeckSelectionEntry entry = deckEntries.FirstOrDefault(x => x.AssignedDeck.Equals(deckName));
            if(entry)
                entry.gameObject.SetActive(false);
            DeckInfo info = LoadedDecks.FirstOrDefault(x => x.name.Equals(deckName));
            if(info != null)
                LoadedDecks.Remove(info);
            if(deckName.Equals(CurrentDeckName))
                SelectDeck("", "");
            noDecksFoundWarning.SetActive(!deckEntries.Any(x => x.isActiveAndEnabled));
        }

        #endregion
    }
}
