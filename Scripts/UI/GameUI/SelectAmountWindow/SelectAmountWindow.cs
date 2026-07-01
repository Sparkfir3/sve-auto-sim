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
        [Title("Runtime Data"), SerializeField]
        private int currentAmount;
        [SerializeField]
        private int currentMin;
        [SerializeField]
        private int currentMax;

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

        // ------------------------------

        private void Start()
        {
            decrementButton.onClick.AddListener(Decrement);
            incrementButton.onClick.AddListener(Increment);
        }

        // ------------------------------

        public void Open(int min, int max, string text, string subtext, Action<int> onConfirm)
        {
            currentAmount = min;
            currentMin = min;
            currentMax = max;

            mainTextBox.text = text;
            if(!subtext.IsNullOrWhiteSpace())
            {
                subTextBox.text = subtext;
                subTextBox.gameObject.SetActive(true);
            }
            else
                subTextBox.gameObject.SetActive(false);

            OnChangeAmount();
            gameObject.SetActive(true);
        }

        public void Close()
        {
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
