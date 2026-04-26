using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sparkfire.Utility;
using CCGKit;

namespace SVESimulator
{
    public class ActivateEffectWindow : MonoBehaviour
    {
        [Title("Runtime Data"), SerializeField, ReadOnly]
        public bool PointerInside { get; set; }

        [Title("Settings"), SerializeField]
        private float verticalOffset = 50f;

        [BoxGroup("Evolve Text"), SerializeField]
        private string evolveTextTemplate = "Evolve ({0})";
        [BoxGroup("Evolve Text"), SerializeField]
        private SerializedDictionary<int, string> evolveCostFormatting = new();
        [BoxGroup("Evolve Text"), SerializeField]
        private string evolvePointFormatting = "EP";

        [FoldoutGroup("Serve Text"), SerializeField]
        private string serveTextTemplate = "{0} ({1}) Race this follower{2}.";
        [FoldoutGroup("Serve Text"), SerializeField]
        private string carrotIconFormatting = "Carrot";

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
            int i = 0;

            // Effects
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
                    player.AdditionalStats.AbilitiesUsedThisTurn.Add(new PlayedAbilityData(card.RuntimeCard.instanceId, card.LibraryCard.id, ability.name));
                    player.LocalEvents.PayAbilityCosts(card, ability.costs, ability.name, () =>
                    {
                        SVEEffectPool.Instance.ResolveEffectImmediate(ability.effect as SveEffect, card.RuntimeCard, SVEProperties.Zones.Field, onComplete: () =>
                        {
                            SVEEffectPool.Instance.CmdExecuteConfirmationTiming();
                        });
                    });
                    Close();
                });
            }

            // Stack
            if(!onlyQuicks && card.RuntimeCard.HasCounter(SVEProperties.Counters.Stack))
            {
                MultipleChoiceButton button = i < buttons.Count ? buttons[i] : AddNewButton();
                ActivatedAbility ability = CounterUtilities.InnateStackAbility;

                button.gameObject.SetActive(true);
                button.Text = (ability.effect as SveEffect)?.text;
                button.Interactable = player.LocalEvents.CanPayCosts(card.RuntimeCard, ability.costs, ability.name);
                button.OnClickEffect.AddListener(() =>
                {
                    player.AdditionalStats.AbilitiesUsedThisTurn.Add(new PlayedAbilityData(card.RuntimeCard.instanceId, card.LibraryCard.id, ability.name));
                    player.LocalEvents.PayAbilityCosts(card, ability.costs, ability.name, () =>
                    {
                        SVEEffectPool.Instance.ResolveEffectImmediate(ability.effect as SveEffect, card.RuntimeCard, SVEProperties.Zones.Field, onComplete: () =>
                        {
                            SVEEffectPool.Instance.CmdExecuteConfirmationTiming();
                        });
                    });
                    Close();
                });
                abilities.Add(CounterUtilities.InnateStackAbility);
                i++;
            }

            // Evolve/Serve
            if(!onlyQuicks)
            {
                AddEvolveEffects(player, card, ref i);
                AddServeEffects(player, card, ref i);
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

        #region Generic Acts

        private void AddEvolveEffects(PlayerController player, CardObject card, ref int buttonsIndex)
        {
            // Evolve without evolve point
            int evolveCost = card.GetEvolveCost();
            if(evolveCost >= 0)
            {
                MultipleChoiceButton button = buttonsIndex < buttons.Count ? buttons[buttonsIndex] : AddNewButton();
                button.gameObject.SetActive(true);
                button.Text = string.Format(evolveTextTemplate, GetFormattedEvolveCost(evolveCost));
                button.Interactable = !player.EvolvedThisTurn && player.LocalEvents.CanPayEvolveCost(evolveCost) && player.ZoneController.EvolveDeckHasEvolvedVersionOf(card.RuntimeCard);
                button.OnClickEffect.AddListener(() =>
                {
                    player.LocalEvents.EvolveCard(card, false);
                    Close();
                });
                buttonsIndex++;
            }

            // Evolve with evolve point
            if(!player.EvolvedThisTurn && evolveCost > 0 && player.LocalEvents.HasEvolvePoint())
            {
                MultipleChoiceButton button = buttonsIndex < buttons.Count ? buttons[buttonsIndex] : AddNewButton();
                button.gameObject.SetActive(true);
                button.Text = string.Format(evolveTextTemplate, GetFormattedEvolveCost(evolveCost - 1, true));
                button.Interactable = player.LocalEvents.CanPayEvolveCost(evolveCost, true) && player.ZoneController.EvolveDeckHasEvolvedVersionOf(card.RuntimeCard);
                button.OnClickEffect.AddListener(() =>
                {
                    player.LocalEvents.EvolveCard(card, true);
                    Close();
                });
                buttonsIndex++;
            }
        }

        private void AddServeEffects(PlayerController player, CardObject card, ref int buttonsIndex)
        {
            if(card.RuntimeCard.HasKeyword(SVEProperties.PassiveAbilities.IsRacing))
                return;

            bool allowEvolvePoint = !player.EvolvedThisTurn && player.LocalEvents.HasEvolvePoint();
            if(card.RuntimeCard.HasKeyword(SVEProperties.PassiveAbilities.Serve1))
            {
                AddServeButton(player, card, ref buttonsIndex, 1, false);
                if(allowEvolvePoint)
                    AddServeButton(player, card, ref buttonsIndex, 1, true);
            }
            if(card.RuntimeCard.HasKeyword(SVEProperties.PassiveAbilities.Serve2))
            {
                AddServeButton(player, card, ref buttonsIndex, 2, false);
                if(allowEvolvePoint)
                    AddServeButton(player, card, ref buttonsIndex, 2, true);
            }
            if(card.RuntimeCard.HasKeyword(SVEProperties.PassiveAbilities.Serve3))
            {
                AddServeButton(player, card, ref buttonsIndex, 3, false);
                if(allowEvolvePoint)
                    AddServeButton(player, card, ref buttonsIndex, 3, true);
            }
        }

        private void AddServeButton(PlayerController player, CardObject card, ref int buttonsIndex, int serveCount, bool withEvolvePoint)
        {
            MultipleChoiceButton button = buttonsIndex < buttons.Count ? buttons[buttonsIndex] : AddNewButton();
            button.gameObject.SetActive(true);
            button.Text = GetFormattedServeText(serveCount, withEvolvePoint);
            button.Interactable = !player.EvolvedThisTurn && player.LocalEvents.CanPayEvolveCost(serveCount, withEvolvePoint) && serveCount <= player.ZoneController.EvolveDeckCarrotCount();
            button.OnClickEffect.AddListener(() =>
            {
                player.LocalEvents.ServeAndRaceCard(card, withEvolvePoint, serveCount);
                Close();
            });
            buttonsIndex++;
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

        private string GetFormattedServeText(int serveCount, bool withEvolvePoint = false)
        {
            return string.Format(serveTextTemplate, string.Concat(Enumerable.Repeat(carrotIconFormatting, serveCount)), GetFormattedEvolveCost(serveCount, withEvolvePoint),
                serveCount > 1 ? $" {serveCount} times" : "");
        }

        #endregion
    }
}
