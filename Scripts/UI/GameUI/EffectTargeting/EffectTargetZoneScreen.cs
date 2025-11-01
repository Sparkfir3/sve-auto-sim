using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SVESimulator.UI
{
    public class EffectTargetZoneScreen : MonoBehaviour
    {
        [TitleGroup("Buttons"), SerializeField]
        private Button closeButton;
        [SerializeField]
        private Button fieldButton;
        [SerializeField]
        private Button exAreaButton;

        [TitleGroup("Text"), SerializeField]
        private TextMeshProUGUI effectDescriptionTextBox;
        [SerializeField]
        private TextMeshProUGUI countRemainingTextBox;
        [SerializeField]
        private string templateCountRemaining = "{0} Remaining";

        private Dictionary<string, Button> localButtons = new();
        private PlayerController player;
        private PlayerInputController mainInputController;

        [HideInInspector] // Params: ZoneName, IsLocalZone
        public UnityEvent<string, bool> OnSelectZone;

        // ------------------------------

        public void Initialize()
        {
            closeButton.onClick.AddListener(Close);
            SetCloseButtonActive(true);

            fieldButton.onClick.AddListener(() => OnSelectZone?.Invoke(SVEProperties.Zones.Field, true));
            exAreaButton.onClick.AddListener(() => OnSelectZone?.Invoke(SVEProperties.Zones.ExArea, true));

            localButtons.Add(SVEProperties.Zones.Field, fieldButton);
            localButtons.Add(SVEProperties.Zones.ExArea, exAreaButton);
            mainInputController = FindObjectOfType<PlayerInputController>();
        }

        // ------------------------------

        public void Open(PlayerController player, List<string> validLocalZones, List<string> validOppZones = null)
        {
            this.player = player;
            mainInputController.allowedInputs = PlayerInputController.InputTypes.None;
            player.ZoneController.fieldZone.RemoveAllCardHighlights();
            player.ZoneController.exAreaZone.RemoveAllCardHighlights();
            player.OppZoneController.fieldZone.RemoveAllCardHighlights();
            player.OppZoneController.exAreaZone.RemoveAllCardHighlights();

            foreach(string zone in validLocalZones)
                localButtons[zone].gameObject.SetActive(true);
            // TODO - implement opponent zones selection

            gameObject.SetActive(true);
        }

        public void Close()
        {
            if(!gameObject.activeSelf)
                return;

            gameObject.SetActive(false);
            foreach(KeyValuePair<string, Button> kvPair in localButtons)
                kvPair.Value.gameObject.SetActive(false);
            SetDescriptionText("");
            SetCountRemainingText(null);
            GameUIManager.NetworkedCalls.CmdCloseOpponentTargeting(player.GetOpponentInfo().netId);
            OnSelectZone.RemoveAllListeners();

            mainInputController.allowedInputs = PlayerInputController.InputTypes.All;
            player.ZoneController.fieldZone.HighlightCardsCanAttack();
        }

        public void SetDescriptionText(string effectDescription)
        {
            effectDescriptionTextBox.text = effectDescription;
        }

        public void SetCountRemainingText(int? count)
        {
            countRemainingTextBox.text = count.HasValue ? string.Format(templateCountRemaining, count.Value) : "";
        }

        public void SetCloseButtonActive(bool active)
        {
            closeButton.gameObject.SetActive(active);
        }
    }
}
