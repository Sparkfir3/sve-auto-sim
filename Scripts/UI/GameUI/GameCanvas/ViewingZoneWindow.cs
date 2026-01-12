using System;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using TMPro;

namespace SVESimulator.UI
{
    public class ViewingZoneWindow : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI headerText;
        [SerializeField]
        private MultipleChoiceButton button;

        // ------------------------------

        public void Open(string text, Action onClose)
        {
            headerText.text = text;
            button.OnClickEffect.AddListener(() =>
            {
                onClose?.Invoke();
                Close();
            });
            GameUIManager.Instance.SetPrimaryMenusVisible(false);
            gameObject.SetActive(true);
        }

        public void Close()
        {
            gameObject.SetActive(false);
            GameUIManager.Instance.SetPrimaryMenusVisible(true);
            button.ResetButton();
        }
    }
}
