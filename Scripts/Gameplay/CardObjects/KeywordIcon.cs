using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;
using System.Collections;
using SVESimulator.UI;

namespace SVESimulator
{
    public class KeywordIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [System.Serializable]
        public class KeywordIconData
        {
            public Sprite sprite;
            public string text;
        }

        [SerializeField]
        private Image image;
        [SerializeField]
        private string tooltipText;

        // ------------------------------

        public void Initialize(KeywordIconData data)
        {
            image.sprite = data.sprite;
            tooltipText = data.text;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Debug.Log("Enter");
            GameUIManager.MouseTooltip.Enable(tooltipText);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Debug.Log("Exit");
            GameUIManager.MouseTooltip.Disable();
        }
    }
}
