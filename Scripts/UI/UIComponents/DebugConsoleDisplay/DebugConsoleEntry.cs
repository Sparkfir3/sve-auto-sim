using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using TMPro;
using UnityEngine.UI;

namespace SVESimulator.UI
{
    public class DebugConsoleEntry : MonoBehaviour
    {
        [SerializeField]
        private Image icon;
        [SerializeField]
        private TextMeshProUGUI text;

        public void SetInfo(Sprite sprite, string message)
        {
            icon.sprite = sprite;
            text.text = message;
        }
    }
}
