using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;

namespace SVESimulator.UI
{
    public class LoadingBar : MonoBehaviour
    {
        [SerializeField, ProgressBar(0f, 1f), ReadOnly]
        private float progressPercent;
        [SerializeField]
        private RectTransform progressBar;

        // ------------------------------

        private void Awake()
        {
            MoveProgressBar(0f);
        }

        // ------------------------------

        [Title("Buttons"), Button]
        public void SetPercent(float percent)
        {
            progressPercent = percent;
            MoveProgressBar(progressPercent);
        }

        private void MoveProgressBar(float percent)
        {
            float width = progressBar.rect.width;
            progressBar.anchoredPosition = new Vector2(-width * (1f - percent), 0f);
        }
    }
}
