using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using Sparkfire.Utility;
using CCGKit;

namespace SVESimulator
{
    public class ActivateEffectWindow : MonoBehaviour
    {
        [Title("Runtime Data"), SerializeField, ReadOnly]
        public bool PointerInside { get; set; }

        [Title("Settings"), SerializeField]
        private string evolveTextTemplate = "Evolve ({0})";
        [SerializeField]
        private float verticalOffset = 50f;
        [SerializeField]
        private SerializedDictionary<int, string> evolveCostFormatting = new();
        [SerializeField]
        private string evolvePointFormatting = "EP";

        [Title("Object References"), SerializeField]
        private RectTransform rectTransform;
        [SerializeField]
        private RectTransform window;
        [SerializeField]
        private List<MultipleChoiceButton> buttons;
        [SerializeField, HideInPlayMode]
        private List<GameObject> testButtons;
        [SerializeField]
        private MultipleChoiceButton buttonPrefab;

        private Camera cam;

        // ------------------------------

        public void Initialize()
        {
            cam = Camera.main;
            if(!rectTransform)
                rectTransform = GetComponent<RectTransform>();
            foreach(GameObject testButton in testButtons)
            {
                Destroy(testButton);
            }
        }

        // ------------------------------

        #region Open/Close Controls

        public void Open(PlayerController player, CardObject card, List<ActivatedAbility> abilities, bool onlyQuicks = false)
        {
            // Effects
            int i = 0;
            if(!onlyQuicks && card.RuntimeCard.HasCounter(SVEProperties.Counters.Stack))
                abilities.Add(CounterUtilities.InnateStackAbility);
            for(; i < abilities.Count; i++)
            {
                MultipleChoiceButton button = i < buttons.Count ? buttons[i] : AddNewButton();
                ActivatedAbility ability = abilities[i]; // need to detach reference from var i for the button event

                button.gameObject.SetActive(true);
                button.Text = LibraryCardCache.GetEffectText(card.RuntimeCard.cardId, ability.name);
                button.Interactable = player.LocalEvents.CanPayCosts(card.RuntimeCard, ability.costs, ability.name)
                    && (ability.effect is not EvolveEffect evolveEffect || evolveEffect.CanEvolve(player, card.RuntimeCard));
                if(onlyQuicks)
                    button.Interactable &= ability.IsQuickAbility();
                button.OnClickEffect.AddListener(() =>
                {
                    player.LocalEvents.PayAbilityCosts(card, ability.costs, ability.effect as SveEffect, ability.name, () =>
                    {
                        player.AdditionalStats.AbilitiesUsedThisTurn.Add(new PlayedAbilityData(card.RuntimeCard.instanceId, card.LibraryCard.id, ability.name));
                        SVEEffectPool.Instance.ResolveEffectImmediate(ability.effect as SveEffect, card.RuntimeCard, SVEProperties.Zones.Field, onComplete: () =>
                        {
                            SVEEffectPool.Instance.CmdExecuteConfirmationTiming();
                        });
                    });
                    Close();
                });
            }

            // Evolve without evolve point
            int evolveCost = onlyQuicks ? -1 : card.GetEvolveCost();
            if(evolveCost >= 0)
            {
                MultipleChoiceButton button = i < buttons.Count ? buttons[i] : AddNewButton();
                button.gameObject.SetActive(true);
                button.Text = string.Format(evolveTextTemplate, GetFormattedEvolveCost(evolveCost));
                button.Interactable = !player.EvolvedThisTurn && player.LocalEvents.CanPayEvolveCost(evolveCost) && player.ZoneController.EvolveDeckHasEvolvedVersionOf(card.RuntimeCard);
                button.OnClickEffect.AddListener(() =>
                {
                    player.LocalEvents.EvolveCard(card, false);
                    Close();
                });
                i++;
            }

            // Evolve with evolve point
            if(evolveCost > 0 && player.LocalEvents.HasEvolvePoint())
            {
                MultipleChoiceButton button = i < buttons.Count ? buttons[i] : AddNewButton();
                button.gameObject.SetActive(true);
                button.Text = string.Format(evolveTextTemplate, GetFormattedEvolveCost(evolveCost - 1, true));
                button.Interactable = !player.EvolvedThisTurn && player.LocalEvents.CanPayEvolveCost(evolveCost - 1, true) && player.ZoneController.EvolveDeckHasEvolvedVersionOf(card.RuntimeCard);
                button.OnClickEffect.AddListener(() =>
                {
                    player.LocalEvents.EvolveCard(card, true);
                    Close();
                });
            }

            // Open window
            Vector2 viewportPos = cam.WorldToViewportPoint(card.transform.position);
            window.anchoredPosition = new Vector2(viewportPos.x * rectTransform.rect.width, viewportPos.y * rectTransform.rect.height + verticalOffset);
            gameObject.SetActive(true);
        }

        public void Close()
        {
            if(!gameObject.activeSelf)
                return;

            gameObject.SetActive(false);
            PointerInside = false;
            foreach(MultipleChoiceButton button in buttons)
            {
                button.gameObject.SetActive(false);
                button.ResetButton();
            }
        }

        #endregion

        // ------------------------------

        #region Other

        private void Update()
        {
            if(Input.GetButtonDown("Cancel"))
                Close();
            else if(Input.GetKeyDown(KeyCode.Mouse0) && !PointerInside)
                Close();
        }

        private MultipleChoiceButton AddNewButton()
        {
            MultipleChoiceButton button = Instantiate(buttonPrefab, window);
            buttons.Add(button);
            return button;
        }

        private string GetFormattedEvolveCost(int cost, bool withEvolvePoint = false)
        {
            return evolveCostFormatting.GetValueOrDefault(cost, cost.ToString()) + (withEvolvePoint ? $" + {evolvePointFormatting}" : "");
        }

        #endregion
    }
}
