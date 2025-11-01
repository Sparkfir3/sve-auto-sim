using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sparkfire.Utility;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SVESimulator
{
    public class EffectTargetCardScreen : MonoBehaviour
    {
        #region Variables

        public enum SelectMode { Single, MultiSelect }

        [TitleGroup("Runtime Data"), ShowInInspector, ReadOnly]
        private SelectMode currentMode;
        [ShowInInspector, ReadOnly]
        private List<CardObject> availableTargets = new();
        [ShowInInspector, ReadOnly]
        private List<CardObject> currentSelectedCards = new();
        [ShowInInspector, ReadOnly]
        private Dictionary<SVEFormulaParser.CardFilterSetting, string> filter = new();
        [ShowInInspector, ReadOnly]
        private SerializedDictionary<CardObject, EffectTargetingMultiSelectInfo> multiSelectBoxes = new();
        [ShowInInspector, ReadOnly]
        private List<EffectTargetingMultiSelectInfo> unusedMultiSelectBoxes = new();

        [TitleGroup("Object References"), SerializeField]
        private TextMeshProUGUI resolvingTextBox;
        [SerializeField]
        private TextMeshProUGUI effectDescriptionTextBox;
        [SerializeField]
        private TextMeshProUGUI countRemainingTextBox;
        [SerializeField]
        private Button confirmButton;
        [SerializeField, AssetsOnly]
        private EffectTargetingMultiSelectInfo multiSelectInfoPrefab;

        [TitleGroup("Settings"), SerializeField]
        private PlayerInputSettings inputSettings;
        [SerializeField]
        private string templateResolving = "Resolving {0}";
        [SerializeField]
        private string templateCountRemaining = "{0} Remaining";

        // ---

        private Camera cam;
        private PlayerController player;
        private PlayerInputController mainInputController;
        private List<CardZone> currentZones = new();
        private int minTargetAmount;
        private int maxTargetAmount;
        private int maxMultiSelectTargets;

        [HideInInspector]
        public UnityEvent<List<CardObject>> OnSelectionComplete;

        #endregion

        // ------------------------------

        #region Initialize

        public void Initialize()
        {
            mainInputController = FindObjectOfType<PlayerInputController>();
            cam = Camera.main;
            confirmButton.onClick.AddListener(ConfirmSelection);
        }

        #endregion

        // ------------------------------

        #region Open/Close

        public void Open(PlayerController player, string filterFormula, List<string> validLocalZones, List<string> validOppZones, SelectMode mode = SelectMode.Single)
            => Open(player, -1, filterFormula, validLocalZones, validOppZones, mode);
        public void Open(PlayerController player, int sourceCardInstanceId, string filterFormula, List<string> validLocalZones, List<string> validOppZones, SelectMode mode = SelectMode.Single)
        {
            // Init
            this.player = player;
            currentMode = mode;
            mainInputController.allowedInputs = PlayerInputController.InputTypes.None;
            player.ZoneController.fieldZone.RemoveAllCardHighlights();
            player.ZoneController.exAreaZone.RemoveAllCardHighlights();
            player.OppZoneController.fieldZone.RemoveAllCardHighlights();
            player.OppZoneController.exAreaZone.RemoveAllCardHighlights();
            availableTargets.Clear();
            currentSelectedCards.Clear();
            currentZones.Clear();

            // Calculate zones
            if(validLocalZones != null)
                currentZones.AddRange(player.ZoneController.AllZones.Where(x => validLocalZones.Contains(x.Key)).Select(x => x.Value));
            if(validOppZones != null)
                currentZones.AddRange(player.OppZoneController.AllZones.Where(x => validOppZones.Contains(x.Key)).Select(x => x.Value));

            // Calculate filter and target count
            string[] filterParams = filterFormula?.Split(" // "); // TODO - better solution than arbitrary string split
            filter = SVEFormulaParser.ParseCardFilterFormula(filterParams?[0], sourceCardInstanceId);
            if(filter.TryGetValue(SVEFormulaParser.CardFilterSetting.MinMaxCount, out string minMaxFormula))
                SVEFormulaParser.ParseMinMaxCount(minMaxFormula, out minTargetAmount, out maxTargetAmount);
            else
                (minTargetAmount, maxTargetAmount) = (1, 1);
            maxMultiSelectTargets = filterParams != null && filterParams.Length > 1 ? SVEFormulaParser.ParseValue(filterParams[1], player) : 1;

            // Calculate targets
            foreach(CardZone zone in currentZones)
            {
                if(zone == player.ZoneController.leaderZone || zone == player.OppZoneController.leaderZone) // leader target ignores filter
                {
                    CardObject leaderCard = zone.AllCards?[0];
                    Debug.Assert(leaderCard);

                    leaderCard.SetHighlightMode(CardObject.HighlightMode.ValidTarget);
                    availableTargets.Add(leaderCard);
                    continue;
                }

                List<CardObject> cards = (zone is CardPositionedZone posZone) ? posZone.GetAllPrimaryCards() : zone.AllCards;
                bool checkForAura = zone == player.OppZoneController.fieldZone;
                foreach(CardObject card in cards)
                {
                    if(checkForAura && card.RuntimeCard.HasKeyword(SVEProperties.Keywords.Aura))
                        continue;
                    if(filter.MatchesCard(card.RuntimeCard))
                    {
                        card.SetHighlightMode(CardObject.HighlightMode.ValidTarget);
                        availableTargets.Add(card);
                    }
                }
            }

            confirmButton.gameObject.SetActive(minTargetAmount == 0 || availableTargets.Count == 0);
            gameObject.SetActive(true);
        }

        public void Close()
        {
            if(!gameObject.activeSelf)
                return;

            mainInputController.allowedInputs = PlayerInputController.InputTypes.All;
            foreach(CardObject card in availableTargets)
                card.SetHighlightMode(CardObject.HighlightMode.None);
            foreach(EffectTargetingMultiSelectInfo box in multiSelectBoxes.Values)
            {
                unusedMultiSelectBoxes.Add(box);
                box.gameObject.SetActive(false);
            }
            multiSelectBoxes.Clear();
            player.ZoneController.fieldZone.HighlightCardsCanAttack();
            OnSelectionComplete.RemoveAllListeners();
            gameObject.SetActive(false);
        }

        private void ConfirmSelection()
        {
            OnSelectionComplete?.Invoke(currentSelectedCards);
            Close();
        }

        #endregion

        // ------------------------------

        #region Set Display

        public void SetText(in string text) => SetText(null, text);
        public void SetText(in string cardName, in string text)
        {
            resolvingTextBox.text = string.Format(templateResolving, !cardName.IsNullOrWhiteSpace() ? $"- {cardName}" : "").Trim();
            effectDescriptionTextBox.text = text ?? "";
        }

        #endregion
        
        // ------------------------------

        #region Unity Functions/Core Logic

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Mouse0) && Physics.Raycast(cam.ScreenToWorldPoint(Input.mousePosition), Vector3.down, out RaycastHit hit, inputSettings.RaycastDistance, inputSettings.CardRaycastLayers.value))
            {
                if(hit.transform.TryGetComponent(out CardObject card) && availableTargets.Contains(card))
                {
                    if(currentMode == SelectMode.Single)
                        ToggleCardSelection(card);
                    else if(currentMode == SelectMode.MultiSelect)
                        AddCardToMultiSelect(card);
                }
            }

            else if(currentMode == SelectMode.MultiSelect && Input.GetKeyDown(KeyCode.Mouse1) &&
                    Physics.Raycast(cam.ScreenToWorldPoint(Input.mousePosition), Vector3.down, out hit, inputSettings.RaycastDistance, inputSettings.CardRaycastLayers.value))
            {
                if(hit.transform.TryGetComponent(out CardObject card) && availableTargets.Contains(card))
                    RemoveCardFromMultiSelect(card);
            }
        }

        #endregion

        // ------------------------------

        #region Selection Handling

        private void ToggleCardSelection(CardObject card)
        {
            if(!currentSelectedCards.Contains(card))
            {
                if(currentSelectedCards.Count >= maxTargetAmount)
                    return;
                currentSelectedCards.Add(card);
                card.SetHighlightMode(CardObject.HighlightMode.Selected);
            }
            else
            {
                currentSelectedCards.Remove(card);
                card.SetHighlightMode(CardObject.HighlightMode.ValidTarget);
            }
            confirmButton.gameObject.SetActive(currentSelectedCards.Count >= minTargetAmount || currentSelectedCards.Count == availableTargets.Count);
        }

        private void AddCardToMultiSelect(CardObject card)
        {
            if(currentSelectedCards.Count >= maxMultiSelectTargets)
                return;
            if(!currentSelectedCards.Contains(card))
            {
                if(currentSelectedCards.Distinct().Count() >= maxTargetAmount)
                    return;
                currentSelectedCards.Add(card);
                card.SetHighlightMode(CardObject.HighlightMode.Selected);
                GetMultiSelectBox(card).SetText("1");
            }
            else
            {
                currentSelectedCards.Add(card);
                GetMultiSelectBox(card).SetText(currentSelectedCards.Count(x => x == card).ToString());
            }
            confirmButton.gameObject.SetActive(currentSelectedCards.Count >= maxMultiSelectTargets);
        }

        private void RemoveCardFromMultiSelect(CardObject card)
        {
            if(!currentSelectedCards.Contains(card))
                return;

            currentSelectedCards.Remove(card);
            if(!currentSelectedCards.Contains(card))
            {
                card.SetHighlightMode(CardObject.HighlightMode.ValidTarget);
                ReleaseMultiSelectBox(card);
            }
            else
            {
                GetMultiSelectBox(card).SetText(currentSelectedCards.Count(x => x == card).ToString());
            }

            confirmButton.gameObject.SetActive(currentSelectedCards.Count >= maxMultiSelectTargets);
        }

        private EffectTargetingMultiSelectInfo GetMultiSelectBox(CardObject card)
        {
            if(multiSelectBoxes.TryGetValue(card, out EffectTargetingMultiSelectInfo box))
                return box;
            if(unusedMultiSelectBoxes.Count > 0)
            {
                box = unusedMultiSelectBoxes[0];
                unusedMultiSelectBoxes.Remove(box);
            }
            else
            {
                box = Instantiate(multiSelectInfoPrefab, transform);
            }

            multiSelectBoxes.Add(card, box);
            box.gameObject.SetActive(true);
            box.SetAnchoredPosition(Camera.main.WorldToScreenPoint(card.transform.position));
            return box;
        }

        private void ReleaseMultiSelectBox(CardObject card)
        {
            if(multiSelectBoxes.Remove(card, out EffectTargetingMultiSelectInfo box))
            {
                unusedMultiSelectBoxes.Add(box);
                box.gameObject.SetActive(false);
            }
        }

        #endregion
    }
}
