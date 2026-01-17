using System;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using Sparkfire.Utility;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CCGKit;
using TMPro;

namespace SVESimulator
{
    public class CardObjectStatsDisplay : MonoBehaviour
    {
        [Serializable]
        private class CounterDisplay
        {
            public int keywordId;
            public string counterName;
            public TextMeshProUGUI textbox;
            public int count;

            public CounterDisplay(int keywordId, TextMeshProUGUI textbox, int count)
            {
                this.keywordId = keywordId;
                counterName = GameManager.Instance.config.keywords[keywordId].name.Replace("Counter", "");
                this.textbox = textbox;
                this.count = count;
            }
        }

        [field: Title("Containers"), SerializeField]
        public GameObject MainStatContainer { get; private set; }
        [field: SerializeField]
        public GameObject CostStatContainer { get; private set; }
        [SerializeField]
        private GraphicRaycaster graphicRaycaster;

        [Title("Stat Displays"), SerializeField]
        private GameObject attackContainer;
        [SerializeField]
        private TextMeshProUGUI attackText;
        [SerializeField]
        private GameObject defenseContainer;
        [SerializeField]
        private TextMeshProUGUI defenseText;
        [SerializeField]
        private GameObject costContainer;
        [SerializeField]
        private TextMeshProUGUI costText;

        [Title("Keywords"), SerializeField]
        private Transform keywordIconsContainer;
        [SerializeField, AssetsOnly]
        private KeywordIcon keywordIconPrefab;
        [ShowInInspector, HideInEditorMode]
        private SerializedDictionary<int, GameObject> currentKeywordIcons = new();

        [Title("Counters Display"), SerializeField, DisableInEditorMode]
        private List<CounterDisplay> counterLines;
        [SerializeField]
        private Transform countersContainer;
        [SerializeField, AssetsOnly]
        private TextMeshProUGUI counterTextPrefab;
        [SerializeField]
        private string counterTextTemplate = "{0} {1}";

        private RuntimeCard runtimeCard;
        private Stat attackStat;
        private Stat defenseStat;
        private Stat costStat;
        private int baseCost;

        // ------------------------------

        public void SetCard(RuntimeCard card)
        {
            Reset();
            if(card == null)
                return;

            // Attack
            if(card.namedStats.TryGetValue(SVEProperties.CardStats.Attack, out attackStat))
            {
                attackContainer.SetActive(true);
                SetAttackStat(attackStat.effectiveValue);
                attackStat.onValueChanged += SetAttackStat;
            }

            // Defense
            if(card.namedStats.TryGetValue(SVEProperties.CardStats.Defense, out defenseStat))
            {
                defenseContainer.SetActive(true);
                SetDefenseStat(defenseStat.effectiveValue);
                defenseStat.onValueChanged += SetDefenseStat;
            }

            // Cost
            if(card.namedStats.TryGetValue(SVEProperties.CardStats.Cost, out costStat))
            {
                baseCost = costStat.baseValue;
                int currentCost = card.PlayPointCost();
                SetCostStat(currentCost);
                SVEEffectPool.Instance.OnConfirmationTimingEndConstant += UpdateCostStat;
            }
            else
            {
                baseCost = -1;
                costContainer.SetActive(false);
            }

            // Keywords & Counters
            runtimeCard = card;
            runtimeCard.onKeywordAdded += OnKeywordAdded;
            runtimeCard.onKeywordRemoved += OnKeywordRemoved;
            foreach(RuntimeKeyword keyword in runtimeCard.keywords)
                OnKeywordAdded(keyword);
            if(currentKeywordIcons.Count == 0)
            {
                keywordIconsContainer.gameObject.SetActive(false);
                graphicRaycaster.enabled = false;
            }
        }

        public void Reset()
        {
            // Attack
            attackContainer.SetActive(false);
            if(attackStat != null)
            {
                attackStat.onValueChanged -= SetAttackStat;
                attackStat = null;
            }

            // Defense
            defenseContainer.SetActive(false);
            if(defenseStat != null)
            {
                defenseStat.onValueChanged -= SetDefenseStat;
                defenseStat = null;
            }

            // Cost
            costContainer.SetActive(false);
            if(costStat != null)
            {
                SVEEffectPool.Instance.OnConfirmationTimingEndConstant -= UpdateCostStat;
                costStat = null;
            }

            // Keywords & Counters
            if(runtimeCard != null)
            {
                runtimeCard.onKeywordAdded -= OnKeywordAdded;
                runtimeCard.onKeywordRemoved -= OnKeywordRemoved;
                runtimeCard = null;
            }
            foreach(GameObject icon in currentKeywordIcons.Select(x => x.Value))
                Destroy(icon);
            currentKeywordIcons.Clear();
            keywordIconsContainer.gameObject.SetActive(false);
            graphicRaycaster.enabled = false;
            foreach(TextMeshProUGUI textbox in counterLines.Select(x => x.textbox))
                Destroy(textbox.gameObject);
            counterLines.Clear();
        }

        private void OnDestroy()
        {
            SVEEffectPool.Instance.OnConfirmationTimingEndConstant -= UpdateCostStat;
        }

        // ------------------------------

        private void SetAttackStat(int oldAtk, int newAtk) => SetAttackStat(newAtk);
        private void SetAttackStat(int atk)
        {
            attackText.text = atk.ToString();
        }

        private void SetDefenseStat(int oldDef, int newDef) => SetDefenseStat(newDef);
        private void SetDefenseStat(int def)
        {
            defenseText.text = def.ToString();
        }

        public void UpdateCostStat()
        {
            if(costStat == null || !CostStatContainer.activeInHierarchy)
                return;
            SetCostStat(runtimeCard.PlayPointCost());
        }

        public void SetCostStat(int cost)
        {
            if(cost < 0 || cost == baseCost)
            {
                costContainer.SetActive(false);
                return;
            }
            costContainer.SetActive(true);
            costText.text = cost.ToString();
        }

        private void OnKeywordAdded(RuntimeKeyword keyword)
        {
            // Standard keyword
            if(keyword.keywordId == 0)
            {
                if(currentKeywordIcons.TryGetValue(keyword.valueId, out GameObject keywordIcon))
                {
                    keywordIcon.SetActive(true);
                }
                else if(CardManager.Instance.TryGetKeywordIconData(keyword.valueId, out KeywordIcon.KeywordIconData data))
                {
                    KeywordIcon newImage = Instantiate(keywordIconPrefab, keywordIconsContainer);
                    newImage.Initialize(data);
                    currentKeywordIcons.Add(keyword.valueId, newImage.gameObject);
                }
                keywordIconsContainer.gameObject.SetActive(true);
                graphicRaycaster.enabled = true;
                return;
            }

            // Counters
            if(keyword.keywordId < (int)SVEProperties.Counters.Stack)
                return;
            CounterDisplay display = counterLines.FirstOrDefault(x => x.keywordId == keyword.keywordId);
            if(display == null)
            {
                display = new CounterDisplay(keyword.keywordId, Instantiate(counterTextPrefab, countersContainer), 1);
                counterLines.Add(display);
            }
            display.count = runtimeCard.CountOfCounter((SVEProperties.Counters)keyword.keywordId);
            display.textbox.text = string.Format(counterTextTemplate, display.counterName, display.count > 1 ? display.count : "").Trim();
        }

        private void OnKeywordRemoved(RuntimeKeyword keyword)
        {
            // Standard keyword
            if(keyword.keywordId == 0)
            {
                if(currentKeywordIcons.TryGetValue(keyword.valueId, out GameObject keywordIcon))
                {
                    Destroy(keywordIcon);
                    currentKeywordIcons.Remove(keyword.valueId);
                    if(currentKeywordIcons.Count == 0)
                    {
                        keywordIconsContainer.gameObject.SetActive(false);
                        graphicRaycaster.enabled = false;
                    }
                }
                return;
            }

            // Counters
            if(keyword.keywordId < (int)SVEProperties.Counters.Stack)
                return;
            CounterDisplay display = counterLines.FirstOrDefault(x => x.keywordId == keyword.keywordId);
            if(display == null)
                return;
            display.count = runtimeCard.CountOfCounter((SVEProperties.Counters)keyword.keywordId);
            if(display.count > 0)
            {
                display.textbox.text = string.Format(counterTextTemplate, display.counterName, display.count > 1 ? display.count : "").Trim();
            }
            else
            {
                Destroy(display.textbox.gameObject); // TODO - object pooling
                counterLines.Remove(display);
            }
        }
    }
}
