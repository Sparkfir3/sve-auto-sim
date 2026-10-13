using CCGKit;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using InputTypes = SVESimulator.PlayerInputController.InputTypes;

namespace SVESimulator.UI
{
    public class ActFromHandButton : MonoBehaviour
    {
        [Title("Runtime Data"), ShowInInspector, ProgressBar(0f, "toggleTime")]
        private float toggleTimer;
        [ShowInInspector]
        private bool isButtonActive;
        [ShowInInspector]
        private bool targetButtonActive;

        [Title("Settings"), SerializeField, Tooltip("Time required before button toggles active, to prevent flickering")]
        private float toggleTime = 0.2f;

        [Title("Object References"), SerializeField]
        private Button button;
        [SerializeField]
        private RectTransform buttonRectTransform;
        [SerializeField]
        private Transform positionAnchor;

        private RectTransform rectTransform;
        private PlayerController player;
        private Camera cam;

        // ------------------------------

        public void Initialize(PlayerController player)
        {
            rectTransform = GetComponent<RectTransform>();
            cam = Camera.main;
            this.player = player;
            if(!positionAnchor)
                positionAnchor = player.ZoneController.ActFromHandButtonAnchor;

            player.ZoneController.handZone.OnUpdated += () => SetButtonStatus(isButtonActive);
            button.onClick.AddListener(OnClick);

            UpdateButtonPosition();
            SetButtonStatus(false);
            gameObject.SetActive(true);
        }

        private void LateUpdate()
        {
            if(!player || !player.InputController || !positionAnchor)
                return;
            if(!player.ZoneController.handZone.HasActFromHand())
            {
                SetButtonStatus(false);
                return;
            }

            // TODO - check quicks instead of assuming can't be quick
            targetButtonActive = player.InputController.allowedInputs.HasFlag(InputTypes.ActivateAbilities) && !player.InputController.allowedInputs.HasFlag(InputTypes.OnlyQuicks);
            if(!targetButtonActive)
                SetButtonStatus(false);
            else if(!isButtonActive)
            {
                toggleTimer += Time.deltaTime; // timer to prevent flickering
                if(toggleTimer > toggleTime)
                    SetButtonStatus(true);
            }
            else
                toggleTimer = 0f;

            // TODO - if button is active, check for if screen size changed & re-update position
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if(!buttonRectTransform && button)
                buttonRectTransform = button.GetComponent<RectTransform>();
        }
#endif

        // ------------------------------

        public void SetButtonStatus(bool active)
        {
            isButtonActive = active;
            bool setObjectActive = active && player.ZoneController.handZone.HasActFromHand();
            if(button.gameObject.activeSelf != setObjectActive)
            {
                if(setObjectActive)
                {
                    UpdateButtonPosition();
                }
                button.gameObject.SetActive(setObjectActive);
            }
            toggleTimer = 0f;
        }

        private void UpdateButtonPosition()
        {
            Vector2 viewportPos = cam.WorldToViewportPoint(positionAnchor.transform.position);
            buttonRectTransform.anchoredPosition = new Vector2(viewportPos.x * rectTransform.rect.width, viewportPos.y * rectTransform.rect.height);
        }

        private void OnClick()
        {
            if(!player.ZoneController.handZone.TryGetActFromHandAbilityData(out Dictionary<CardObject, List<ActivatedAbility>> abilityList))
                return;

            bool firstCard = true;
            foreach(var kvPair in abilityList)
            {
                (CardObject card, List<ActivatedAbility> abilities) = (kvPair.Key, kvPair.Value);
                if(firstCard)
                {
                    GameUIManager.ActivateEffect.Open(player, card, abilities, checkEvolveServe: false, checkStack: false);
                    firstCard = false;
                }
                else
                {
                    GameUIManager.ActivateEffect.AddEffects(player, card, abilities);
                }
            }
            GameUIManager.ActivateEffect.SetAnchoredPosition(new Vector2(buttonRectTransform.anchoredPosition.x,
                buttonRectTransform.anchoredPosition.y + (buttonRectTransform.rect.height / 2f) + 5f));
        }
    }
}
