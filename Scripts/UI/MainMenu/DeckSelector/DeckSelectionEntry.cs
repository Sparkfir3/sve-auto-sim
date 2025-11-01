using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using Sparkfire.Utility;
using System.Collections.Generic;
using TMPro;

namespace SVESimulator
{
    public class DeckSelectionEntry : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI textDeckName;
        [SerializeField]
        private Image classIcon;
        [SerializeField]
        private Toggle selectToggle;
        [SerializeField]
        private Image editButtonIcon;
        [SerializeField]
        private Button editButton;
        [SerializeField]
        private Button deleteButton;
        [SerializeField]
        private Button confirmDeleteButton;
        [SerializeField]
        private Button cancelDeleteButton;

        [field: Title("Runtime Data"), SerializeField, DisableInEditorMode]
        public string AssignedDeck { get; private set; }
        [field: SerializeField, DisableInEditorMode]
        public string AssignedData { get; private set; }
        [NonSerialized]
        private bool canDelete = true;

        [Title("Asset References"), SerializeField, AssetsOnly]
        private Sprite editIcon;
        [SerializeField, AssetsOnly]
        private Sprite copyIcon;

        public event Action<string, string> OnSelect;
        public event Action<string, string> OnEdit;
        public event Action<string, string> OnDelete;

        // ------------------------------

        public void Initialize(ToggleGroup toggleGroup)
        {
			selectToggle.group = toggleGroup;
			selectToggle.onValueChanged.AddListener(x => {
				if(x)
					OnSelect?.Invoke(AssignedDeck, AssignedData);
			});
            editButton.onClick.AddListener(() => OnEdit?.Invoke(AssignedDeck, AssignedData));
            deleteButton.onClick.AddListener(SetConfirmDeleteButtonActive);
            confirmDeleteButton.onClick.AddListener(() => OnDelete?.Invoke(AssignedDeck, AssignedData));
            cancelDeleteButton.onClick.AddListener(SetDefaultButtonsActive);
        }

        public void SetInfo(string deckName, string data, Sprite icon, bool isStarterDeck)
        {
            AssignedDeck = deckName;
            textDeckName.text = deckName;
            AssignedData = data;
            classIcon.sprite = icon;

            editButtonIcon.sprite = isStarterDeck ? copyIcon : editIcon;
            canDelete = !isStarterDeck;
            SetDefaultButtonsActive();
        }

		public void SelectDeck()
		{
			selectToggle.isOn = true;
		}

		// ------------------------------

		private void SetDefaultButtonsActive()
		{
			editButton.gameObject.SetActive(true);
			deleteButton.gameObject.SetActive(canDelete);
			confirmDeleteButton.gameObject.SetActive(false);
			cancelDeleteButton.gameObject.SetActive(false);
		}

		private void SetConfirmDeleteButtonActive()
		{
			editButton.gameObject.SetActive(false);
			deleteButton.gameObject.SetActive(false);
			confirmDeleteButton.gameObject.SetActive(true);
			cancelDeleteButton.gameObject.SetActive(true);
		}
    }
}
