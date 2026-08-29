using Sirenix.OdinInspector;
using UnityEngine;

namespace SVESimulator.UI
{
    public class CardInfoDisplayDynamicPosition : CardInfoDisplay
    {
        [TitleGroup("Runtime Data"), SerializeField]
        private bool pinnedToRight = true;

        [TitleGroup("Settings"), SerializeField, Range(0.5f, 1f)]
        private float repositionPoint = 0.7f;

        private Camera cam;

        // ------------------------------

        public override void Initialize()
        {
            if(initialized)
                return;
            base.Initialize();
            cam = Camera.main;

            // Pin to the correct side on launch, invert bool first so that pin function doesn't break
            pinnedToRight = !pinnedToRight;
            if(pinnedToRight)
                PinToLeft();
            else
                PinToRight();
        }

        public override void Display(CardObject card)
        {
            if(!card || !card.IsVisible)
            {
                Hide();
                return;
            }

            float screenXPos = cam.WorldToViewportPoint(card.transform.position).x;
            if(screenXPos <= repositionPoint)
                PinToRight();
            else
                PinToLeft();
            Display(card.LibraryCard);
        }

        // ------------------------------

        private void PinToRight()
        {
            if(pinnedToRight)
                return;

            Vector2 anchor = Vector2.one;
            displayContainer.anchorMin = anchor;
            displayContainer.anchorMax = anchor;
            displayContainer.pivot = anchor;
            displayContainer.anchoredPosition = new Vector2(-Mathf.Abs(displayContainer.anchoredPosition.x), displayContainer.anchoredPosition.y);
            pinnedToRight = true;
        }

        private void PinToLeft()
        {
            if(!pinnedToRight)
                return;

            Vector2 anchor = new Vector2(0f, 1f);
            displayContainer.anchorMin = anchor;
            displayContainer.anchorMax = anchor;
            displayContainer.pivot = anchor;
            displayContainer.anchoredPosition = new Vector2(Mathf.Abs(displayContainer.anchoredPosition.x), displayContainer.anchoredPosition.y);
            pinnedToRight = false;
        }
    }
}
