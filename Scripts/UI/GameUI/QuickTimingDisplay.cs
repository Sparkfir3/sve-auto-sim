using System;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

namespace SVESimulator
{
    public class QuickTimingDisplay : MonoBehaviour
    {
        [field: Title("Runtime Data"), SerializeField, ReadOnly]
        public bool WasCanceled { get; private set; }

        [Title("Object References"), SerializeField]
        private GameObject waitingOnQuickContainer;
        [SerializeField]
        private GameObject performQuickContainer;
        [SerializeField]
        private List<Image> timers;
        [SerializeField]
        private TextMeshProUGUI subtitle;
        [SerializeField]
        private CanvasGroup canvasGroup;

        // ------------------------------

        public void OpenWaitingOnQuickUI()
        {
            WasCanceled = false;
            SetTimer(1f);
            SetAlpha(1f);
            gameObject.SetActive(true);

            waitingOnQuickContainer.SetActive(true);
            performQuickContainer.SetActive(false);
        }

        public void OpenPerformQuickUI()
        {
            WasCanceled = false;
            SetTimer(1f);
            SetAlpha(1f);
            gameObject.SetActive(true);

            waitingOnQuickContainer.SetActive(false);
            performQuickContainer.SetActive(true);
        }

        public void Cancel()
        {
            WasCanceled = true;
            CloseAll();
        }

        public void CloseAll()
        {
            SetTimer(0f);
            gameObject.SetActive(false);
            waitingOnQuickContainer.SetActive(false);
            performQuickContainer.SetActive(false);
        }

        public void SetAlpha(float alpha)
        {
            canvasGroup.alpha = alpha;
        }

        // ------------------------------

        public void SetTimer(float amount)
        {
            foreach(Image timer in timers)
                if(timer.isActiveAndEnabled)
                    timer.fillAmount = amount;
        }

        public void SetSubtitle(string text)
        {
            subtitle.text = text;
        }
    }
}
