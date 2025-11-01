using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Sparkfire.Utility
{
    public class MinMaxSlider : MonoBehaviour
    {
        [SerializeField]
        private Slider minSlider;
        [SerializeField]
        private Slider maxSlider;
        [SerializeField]
        private RectTransform fillRect;
        [SerializeField]
        private bool interactable = true;

        [Header("Events")]
        public UnityEvent<float> onMinValueChanged;
        public UnityEvent<float> onMaxValueChanged;
        public UnityEvent<float, float> onValueChanged;

        [NonSerialized]
        private float prevMin;
        [NonSerialized]
        private float prevMax;

        public bool Interactable
        {
            get => interactable;
            set => SetInteractable(value);
        }

        // ------------------------------

        private void Start()
        {
            prevMin = minSlider.value;
            prevMax = maxSlider.value;

            minSlider.onValueChanged.AddListener(x =>
            {
                float targetValue = Mathf.Clamp(x, minSlider.minValue, maxSlider.value);
                minSlider.SetValueWithoutNotify(targetValue);

                if(Mathf.Approximately(targetValue, prevMin))
                    return;
                prevMin = targetValue;
                onMinValueChanged?.Invoke(targetValue);
                onValueChanged?.Invoke(targetValue, maxSlider.value);
                UpdateFillRect();
            });
            maxSlider.onValueChanged.AddListener(x =>
            {
                float targetValue = Mathf.Clamp(x, minSlider.value, maxSlider.maxValue);
                maxSlider.SetValueWithoutNotify(targetValue);

                if(Mathf.Approximately(targetValue, prevMax))
                    return;
                prevMax = targetValue;
                onMaxValueChanged?.Invoke(targetValue);
                onValueChanged?.Invoke(minSlider.value, targetValue);
                UpdateFillRect();
            });
        }

        // ------------------------------

        private void SetInteractable(bool isInteractable)
        {
            interactable = isInteractable;
            minSlider.interactable = interactable;
            maxSlider.interactable = interactable;
        }

        private void UpdateFillRect()
        {
            if(!fillRect)
                return;
            fillRect.anchorMin = new Vector2(minSlider.normalizedValue, fillRect.anchorMin.y);
            fillRect.anchorMax = new Vector2(maxSlider.normalizedValue, fillRect.anchorMax.y);
        }
    }
}
