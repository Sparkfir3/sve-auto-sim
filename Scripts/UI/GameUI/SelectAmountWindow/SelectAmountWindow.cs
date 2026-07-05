using System;
using UnityEngine;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine.UI;
using Sparkfire.Utility;

namespace SVESimulator.UI
{
    public class SelectAmountWindow : MonoBehaviour
    {
        [Title("Runtime Data"), SerializeField, DisableInEditorMode]
        private int currentAmount;
        [SerializeField, DisableInEditorMode]
        private int currentMin;
        [SerializeField, DisableInEditorMode]
        private int currentMax;

        [Title("Settings"), SerializeField]
        private string defaultText = "Select a Number";
        [SerializeField]
        private string minTextTemplate = "Min {0}";
        [SerializeField]
        private string maxTextTemplate = "Max {0}";

        [Title("Object References"), SerializeField]
        private TextMeshProUGUI currentAmountTextBox;
        [SerializeField]
        private TextMeshProUGUI minTextBox;
        [SerializeField]
        private TextMeshProUGUI maxTextBox;
        [SerializeField]
        private Button decrementButton;
        [SerializeField]
        private Button incrementButton;
        [SerializeField]
        private TextMeshProUGUI mainTextBox;
        [SerializeField]
        private TextMeshProUGUI subTextBox;
        [SerializeField]
        private Button confirmButton;

        // ------------------------------

        private void Start()
        {
            decrementButton.onClick.AddListener(Decrement);
            incrementButton.onClick.AddListener(Increment);
        }

        // ------------------------------

        public void Open(int min, int max, string text, string subtext, Action<int> onConfirm)
        {
            currentAmount = max;
            currentMin = min;
            currentMax = max;

            mainTextBox.text = text.IsNullOrWhiteSpace() ? defaultText : text;
            if(!subtext.IsNullOrWhiteSpace())
            {
                subTextBox.text = subtext;
                subTextBox.gameObject.SetActive(true);
            }
            else
                subTextBox.gameObject.SetActive(false);
            minTextBox.text = string.Format(minTextTemplate, min);
            maxTextBox.text = string.Format(maxTextTemplate, max);

            OnChangeAmount();
            confirmButton.onClick.AddListener(() =>
            {
                onConfirm?.Invoke(currentAmount);
                Close();
            });
            gameObject.SetActive(true);
        }

        public void Close()
        {
            confirmButton.onClick.RemoveAllListeners();
            gameObject.SetActive(false);
        }

        // ------------------------------

        private void Increment()
        {
            if(currentAmount >= currentMax)
                return;
            currentAmount++;
            OnChangeAmount();
        }

        private void Decrement()
        {
            if(currentAmount <= currentMin)
                return;
            currentAmount--;
            OnChangeAmount();
        }

        private void OnChangeAmount()
        {
            currentAmountTextBox.text = currentAmount.ToString();
        }
    }
}
