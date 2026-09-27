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

        private PlayerController player;

        // ------------------------------

        public void Initialize(PlayerController player)
        {
            this.player = player;
            player.ZoneController.handZone.OnUpdated += () => SetButtonStatus(isButtonActive);
            button.onClick.AddListener(OnClick);
            SetButtonStatus(false);
        }

        private void LateUpdate()
        {
            if(!player || !player.InputController)
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
            button.gameObject.SetActive(active && player.ZoneController.handZone.HasActFromHand());
            toggleTimer = 0f;
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
            GameUIManager.ActivateEffect.SetAnchoredPosition(buttonRectTransform.anchoredPosition);
        }
    }
}
