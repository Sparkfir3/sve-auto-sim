using UnityEngine;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SVESimulator
{
    public class MultipleChoiceButton : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI textbox;
        [SerializeField]
        private Button button;
        [HideInInspector]
        public UnityEvent OnClickEffect;

        public bool Interactable
        {
            get => button.interactable;
            set => button.interactable = value;
        }

        public string Text
        {
            set => textbox.text = value;
        }

        // ------------------------------

        private void Awake()
        {
            button.onClick.AddListener(() => OnClickEffect?.Invoke());
        }

        public void ResetButton()
        {
            OnClickEffect.RemoveAllListeners();
            button.interactable = true;
        }

        // ------------------------------

#if UNITY_EDITOR
        private void OnValidate()
        {
            if(!textbox)
                textbox = GetComponentInChildren<TextMeshProUGUI>();
            if(!button)
                button = GetComponentInChildren<Button>();
        }
#endif
    }
}
