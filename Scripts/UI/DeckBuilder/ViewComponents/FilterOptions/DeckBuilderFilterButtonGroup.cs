using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using Sparkfire.Utility;
using System.Linq;

namespace SVESimulator.DeckBuilder
{
    public abstract class DeckBuilderFilterButtonGroup<T> : MonoBehaviour
    {
        [SerializeField]
        protected DeckBuilderModel model;
        [SerializeField]
        protected Toggle allButton;
        [SerializeField]
        protected SerializedDictionary<Toggle, T> filterButtons = new();

        public UnityEvent onValueChanged;

        // ------------------------------

        public virtual void Initialize(T initialValue = default)
        {
            allButton.onValueChanged.AddListener(x =>
            {
                if(x)
                {
                    foreach(var kvPair in filterButtons)
                        kvPair.Key.SetIsOnWithoutNotify(false);
                    ClearFilter();
                    onValueChanged?.Invoke();
                }
                allButton.interactable = !x;
            });
            foreach(var kvPair in filterButtons)
            {
                (Toggle toggle, T filterValue) = (kvPair.Key, kvPair.Value);
                toggle.onValueChanged.AddListener(x =>
                {
                    if(x)
                    {
                        EnableFilter(filterValue);
                        onValueChanged?.Invoke();
                        allButton.isOn = false;
                    }
                    else
                    {
                        if(!filterButtons.Any(x => x.Key.isOn))
                            allButton.isOn = true;
                        else
                            DisableFilter(filterValue);
                    }
                });
            }

            allButton.isOn = true;
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if(!model)
                model = GetComponentInParent<DeckBuilderModel>();
        }
#endif

        // ------------------------------

        protected abstract void ClearFilter();
        protected abstract void EnableFilter(T filterValue);
        protected abstract void DisableFilter(T filterValue);
    }
}
