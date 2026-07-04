using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using TMPro;

namespace SVESimulator
{
    public class EffectTargetingMultiSelectInfo : MonoBehaviour
    {
        [SerializeField]
        private RectTransform rectTransform;
        [SerializeField]
        private TextMeshProUGUI textBox;

        private RectTransform canvasRectTransform;

        // ------------------------------

        public void Initialize()
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.zero;
            rectTransform.pivot = Vector2.one * 0.5f;
            canvasRectTransform = transform.root.GetComponent<RectTransform>();
        }

        public void SetText(string text)
        {
            textBox.text = text;
        }

        public void SetAnchoredViewportPosition(Vector2 position)
        {
            rectTransform.anchoredPosition = new Vector2(canvasRectTransform.rect.width * position.x, canvasRectTransform.rect.height * position.y);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if(!rectTransform)
                rectTransform = GetComponent<RectTransform>();
            if(!textBox)
                textBox = GetComponentInChildren<TextMeshProUGUI>();
        }
#endif
    }
}
