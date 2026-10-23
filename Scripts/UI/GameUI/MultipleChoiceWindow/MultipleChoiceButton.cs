using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SVESimulator.UI
{
    public class MultipleChoiceButton : MonoBehaviour
    {
        #region Variables

        [SerializeField]
        private TextMeshProUGUI textbox;
        [SerializeField]
        private Button button;
        [SerializeField]
        private Image sideArrow;

        [HideInInspector]
        public UnityEvent OnClickEffect;

        public Dictionary<string, Action> AdditionalEffects { get; private set; } = new();
        public RectTransform RectTransform => GetComponent<RectTransform>();

        public event Action OnHoverEnter;
        public event Action OnHoverExit;

        public bool Interactable
        {
            get => button.interactable;
            set => button.interactable = value;
        }

        public string Text
        {
            set => textbox.text = value;
        }

        #endregion

        // ------------------------------

        #region Unity Events

        private void Awake()
        {
            button.onClick.AddListener(() => OnClickEffect?.Invoke());
        }

        public void HoverEnter()
        {
            OnHoverEnter?.Invoke();
        }

        public void HoverExit()
        {
            OnHoverExit?.Invoke();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if(!textbox)
                textbox = GetComponentInChildren<TextMeshProUGUI>();
            if(!button)
                button = GetComponentInChildren<Button>();
        }
#endif

        #endregion

        // ------------------------------

        #region Controls

        public void ResetButton()
        {
            OnClickEffect.RemoveAllListeners();
            AdditionalEffects.Clear();
            button.interactable = true;
            if(sideArrow)
                sideArrow.gameObject.SetActive(false);
        }

        public void SetArrowActive(bool active)
        {
            if(sideArrow)
                sideArrow.gameObject.SetActive(active);
        }

        public bool InvokeAdditionalEffect(string effectName)
        {
            if(AdditionalEffects.TryGetValue(effectName, out Action action))
            {
                action?.Invoke();
                return true;
            }
            return false;
        }

        #endregion
    }
}
