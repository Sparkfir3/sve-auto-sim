using System;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CCGKit;
using SVESimulator.UI;
using TMPro;
using Sparkfire.Utility;

namespace SVESimulator.UI
{
    public class MultipleChoiceWindow : MonoBehaviour
    {
        public struct MultipleChoiceEntryData
        {
            public string text;
            public Action onSelect;
            public bool disabled;
        }

        [TitleGroup("Runtime Data"), ShowInInspector, ReadOnly]
        private ButtonDisplayPosition currentDisplayPosition;

        [Title("Settings"), SerializeField]
        private string defaultHeaderString = "Select an effect";
        [SerializeField]
        private string templateResolving = "Resolving {0}";
        [SerializeField]
        private SerializedDictionary<ButtonDisplayPosition, ButtonPositionData> displayPositions = new();

        [Title("Object References"), SerializeField]
        private Transform buttonContainer;
        [SerializeField]
        private List<MultipleChoiceButton> buttons;
        [SerializeField, HideInPlayMode]
        private List<GameObject> testButtons;
        [SerializeField]
        private MultipleChoiceButton buttonPrefab;
        [SerializeField]
        private RectTransform textContainer;
        [SerializeField]
        private TextMeshProUGUI resolvingTextBox;
        [SerializeField]
        private TextMeshProUGUI effectTextbox;
        [SerializeField]
        private GameObject backgroundTint;

        private PlayerController player;
        private PlayerInputController mainInputController;

        // ------------------------------

        public void Initialize(ButtonDisplayPosition displayPosition = ButtonDisplayPosition.Center)
        {
            mainInputController = FindObjectOfType<PlayerInputController>();
            foreach(GameObject testButton in testButtons)
            {
                Destroy(testButton);
            }
            SetDisplayPosition(displayPosition);
        }

        // ------------------------------

        #region Controls

        public void Open(PlayerController player, string cardName, List<MultipleChoiceEntryData> entries, string effectText, bool showBackgroundTint = true,
            bool showTargetingToOpponent = true, bool disablePlayerInputs = true, ButtonDisplayPosition position = ButtonDisplayPosition.Center)
        {
            this.player = player;
            if(disablePlayerInputs)
                mainInputController.allowedInputs = PlayerInputController.InputTypes.None;
            player.ZoneController.fieldZone.RemoveAllCardHighlights();
            if(currentDisplayPosition != position)
                SetDisplayPosition(position);

            // Entries
            for(int i = 0; i < entries.Count; i++)
            {
                MultipleChoiceButton button = i < buttons.Count ? buttons[i] : AddNewButton();
                MultipleChoiceEntryData entry = entries[i]; // need to detach reference from var i for the button event

                button.gameObject.SetActive(true);
                button.Text = entry.text;
                button.OnClickEffect.AddListener(() =>
                {
                    if(showTargetingToOpponent)
                        GameUIManager.NetworkedCalls.CmdCloseOpponentTargeting(player.GetOpponentInfo().netId);
                    Close();
                    entry.onSelect?.Invoke();
                });
                button.Interactable = !entry.disabled;
            }

            // Open window
            backgroundTint.SetActive(showBackgroundTint);
            resolvingTextBox.text = string.Format(templateResolving, !cardName.IsNullOrWhiteSpace() ? $"- {cardName}" : "").Trim();
            effectTextbox.text = !string.IsNullOrWhiteSpace(effectText) ? effectText : defaultHeaderString;
            gameObject.SetActive(true);
            if(showTargetingToOpponent)
                GameUIManager.NetworkedCalls.CmdShowOpponentTargeting(player.GetOpponentInfo().netId, cardName, effectText);
        }

        public void OpenEngageWardCardOptions(PlayerController player, CardObject card, bool executeConfirmationTiming = true, Action onComplete = null)
        {
            List<MultipleChoiceEntryData> multipleChoiceEntries = new()
            {
                new MultipleChoiceEntryData()
                {
                    text = "Yes",
                    onSelect = () =>
                    {
                        player.LocalEvents.EngageCard(card.RuntimeCard);
                        if(executeConfirmationTiming)
                            SVEEffectPool.Instance.CmdExecuteConfirmationTiming();
                        onComplete?.Invoke();
                    }
                },
                new MultipleChoiceEntryData()
                {
                    text = "No",
                    onSelect = () =>
                    {
                        if(executeConfirmationTiming)
                            SVEEffectPool.Instance.CmdExecuteConfirmationTiming();
                        onComplete?.Invoke();
                    }
                }
            };
            GameUIManager.MultipleChoice.Open(player, card.LibraryCard.name, multipleChoiceEntries, "Engage played card with Ward?", showTargetingToOpponent: false);
        }

        public void AddSingleEntry(MultipleChoiceEntryData entry) => AddSingleEntry(entry.text, entry.onSelect);
        public void AddSingleEntry(string text, Action onSelect)
        {
            MultipleChoiceButton button = buttons.FirstOrDefault(x => !x.isActiveAndEnabled);
            if(!button)
                button = AddNewButton();

            button.gameObject.SetActive(true);
            button.Text = text;
            button.OnClickEffect.AddListener(() =>
            {
                onSelect?.Invoke();
                Close();
            });
        }

        public void Close()
        {
            if(!gameObject.activeSelf)
                return;

            if(player.isActivePlayer && !SVEEffectPool.Instance.IsResolvingEffect)
            {
                mainInputController.allowedInputs = PlayerInputController.InputTypes.All;
                player.ZoneController.fieldZone.HighlightCardsCanAttack();
            }
            gameObject.SetActive(false);
            foreach(MultipleChoiceButton button in buttons)
            {
                button.gameObject.SetActive(false);
                button.ResetButton();
            }
        }

        public void SetButtonActive(int index, bool active)
        {
            buttons[index].Interactable = active;
        }

        #endregion

        // ------------------------------

        #region Internal Controls

        private MultipleChoiceButton AddNewButton()
        {
            MultipleChoiceButton button = Instantiate(buttonPrefab, buttonContainer);
            buttons.Add(button);
            return button;
        }

        public void SetDisplayPosition(ButtonDisplayPosition position)
        {
            currentDisplayPosition = position;
            ButtonPositionData positionData = displayPositions[position];
            RectTransform buttonContainerRectTransform = buttonContainer.GetComponent<RectTransform>();

            textContainer.anchorMin = positionData.textAnchor;
            textContainer.anchorMax = positionData.textAnchor;
            textContainer.pivot = positionData.textPivot;
            textContainer.anchoredPosition = new Vector3(0f, positionData.textPosition, 0f);
            buttonContainerRectTransform.anchorMin = positionData.buttonAnchor;
            buttonContainerRectTransform.anchorMax = positionData.buttonAnchor;
            buttonContainerRectTransform.pivot = positionData.buttonPivot;
            buttonContainerRectTransform.anchoredPosition = new Vector3(0f, positionData.buttonPosition, 0f);
        }

        #endregion

    }
}
