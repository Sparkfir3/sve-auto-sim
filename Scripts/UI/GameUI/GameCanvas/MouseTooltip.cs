using UnityEngine;
using Sirenix.OdinInspector;
using Sparkfire.Utility;
using System.Collections;
using TMPro;

namespace SVESimulator.UI
{
    public class MouseTooltip : MonoBehaviour
    {
        [SerializeField]
        private RectTransform rectTransform;
        [SerializeField]
        private Canvas canvas;
        [SerializeField]
        private TextMeshProUGUI textbox;

        // ------------------------------

        private void LateUpdate()
        {
            rectTransform.anchoredPosition = Input.mousePosition * (1f / canvas.scaleFactor);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if(!rectTransform)
                rectTransform = GetComponent<RectTransform>();
            if(!canvas)
                canvas = GetComponentInParent<Canvas>();
        }
#endif

        // ------------------------------

        public void Enable(string text)
        {
            if(text.IsNullOrWhiteSpace())
            {
                Disable();
                return;
            }
            textbox.text = text;
            gameObject.SetActive(true);
        }

        public void Disable()
        {
            gameObject.SetActive(false);
        }
    }
}
