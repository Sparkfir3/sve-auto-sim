using System;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using CCGKit;
using Sparkfire.Utility;
using TMPro;
using UnityEngine.UI;

namespace SVESimulator
{
    public class CardSlotStatsDisplay : MonoBehaviour
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
                counterName = CounterUtilities.FormatName(keywordId);
                this.textbox = textbox;
                this.count = count;
            }
        }

        // ---

        [Title("Object References"), SerializeField]
        private CardObject cardObject;
        [SerializeField]
        private GraphicRaycaster graphicRaycaster;

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

        // ------------------------------

        public void SetCard(RuntimeCard card)
        {
            Reset();
            if(card == null)
                return;

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

        // ------------------------------

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
