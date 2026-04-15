using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;

namespace SVESimulator.UI
{
    public class MainMenuInputController : MonoBehaviour
    {
        [field: TitleGroup("Runtime Data"), SerializeField]
        public bool AllowInputs { get; set; } = true;
        [SerializeField]
        private CardObject currentCard;

        [TitleGroup("Settings & References"), SerializeField, InlineEditor]
        private PlayerInputSettings inputSettings;

        private Camera cam;

        // ------------------------------

        private void Start()
        {
            cam = Camera.main;
        }

        private void Update()
        {
            if(!AllowInputs)
            {
                SetCurrentCard(null);
                return;
            }

            Vector3 mousePosition = cam.ScreenToWorldPoint(Input.mousePosition);
            if(Physics.Raycast(mousePosition, Vector3.down, out RaycastHit hit, inputSettings.RaycastDistance, inputSettings.CardRaycastLayers.value))
            {
                if(hit.transform.TryGetComponent(out MainMenuCardObject card))
                {
                    if(Input.GetKeyDown(KeyCode.Mouse0))
                    {
                        card.OnClick();
                        return;
                    }
                    SetCurrentCard(card);
                }
                else
                    SetCurrentCard(null);
            }
        }

        // ------------------------------

        private void SetCurrentCard(CardObject card)
        {
            if(currentCard == card)
                return;

            if(currentCard)
                currentCard.OnHoverExit();
            currentCard = card;
            if(currentCard)
                currentCard.OnHoverEnter();
        }
    }
}
