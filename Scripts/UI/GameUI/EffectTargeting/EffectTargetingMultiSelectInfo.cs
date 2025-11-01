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

        // ------------------------------

        public void SetText(string text)
        {
            textBox.text = text;
        }

        public void SetAnchoredPosition(Vector2 position)
        {
            rectTransform.anchoredPosition = position;
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
