using System;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sparkfire.Utility;
using CCGKit;
using SVESimulator.CardTextData;

namespace SVESimulator.UI
{
    public class ActivateEffectWindow : MonoBehaviour
    {
        #region Variables

        [field: Title("Runtime Data"), ShowInInspector, ReadOnly]
        public bool PointerInsideMain { get; set; }
        [field: ShowInInspector, ReadOnly]
        public bool PointerInsideSub { get; set; }

        [Title("Settings"), SerializeField]
        private float verticalOffset = 50f;

        [BoxGroup("Evolve Text"), SerializeField]
        private string evolveTextTemplate = "{0}: Evolve this follower.";
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
        private ActivateEvolveOptionsWindow evolveOptionsWindow;

        [Title("Object References - Prefabs"), SerializeField]
        private MultipleChoiceButton buttonPrefab;

        private Camera cam;

        public bool PointerInside
        {
            get => PointerInsideMain || PointerInsideSub;
            set
            {
                PointerInsideMain = value;
                PointerInsideSub = value;
            }
        }

        #endregion

        // ------------------------------

        #region Initialization

        public void Initialize()
        {
            cam = Camera.main;
            if(!rectTransform)
                rectTransform = GetComponent<RectTransform>();
            foreach(GameObject testButton in testButtons)
            {
                Destroy(testButton);
            }
            evolveOptionsWindow.Initialize();
        }

        #endregion

        // ------------------------------

        #region Open/Close Controls

        public void Open(PlayerController player, CardObject card, List<ActivatedAbility> abilities, bool checkEvolveServe = true, bool checkStack = true, bool onlyQuicks = false)
        {
            int i = 0;
            bool canUseAct = !card.RuntimeCard.HasKeyword(SVEProperties.PassiveAbilities.CannotUseAct);

            // Effects
            for(; i < abilities.Count; i++)
            {
                MultipleChoiceButton button = i < buttons.Count ? buttons[i] : AddNewButton();
                ActivatedAbility ability = abilities[i]; // need to detach reference from var i for the button event

                button.gameObject.SetActive(true);
                button.Text = LibraryCardCache.GetEffectText(card.RuntimeCard.cardId, ability.name);
                button.Interactable = canUseAct && player.LocalEvents.CanPayCosts(card.RuntimeCard, ability.costs, ability.name)
                    && (ability.effect is not IEvolveEffect evolveEffect || evolveEffect.CanEvolve(player, card.RuntimeCard));
                if(onlyQuicks)
                    button.Interactable &= ability.IsQuickAbility();
                Action onClick = () =>
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
                };
                button.OnClickEffect.AddListener(() => onClick?.Invoke());

                if(!checkEvolveServe && !onlyQuicks && ability.effect is IEvolveEffect)
                    AddEvolveOptionsFromAbility(player, card, ability, button, onClick);
                else
                    button.OnHoverEnter += () => evolveOptionsWindow.Close();
            }

            // Stack
            if(checkStack && !onlyQuicks && card.RuntimeCard.HasCounter(SVEProperties.Counters.Stack))
            {
                MultipleChoiceButton button = i < buttons.Count ? buttons[i] : AddNewButton();
                ActivatedAbility ability = CounterUtilities.InnateStackAbility;

                button.gameObject.SetActive(true);
                button.Text = (ability.effect as SveEffect)?.text;
                button.Interactable = canUseAct && player.LocalEvents.CanPayCosts(card.RuntimeCard, ability.costs, ability.name);
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
                button.OnHoverEnter += () => evolveOptionsWindow.Close();
                abilities.Add(CounterUtilities.InnateStackAbility);
                i++;
            }

            // Evolve/Serve
            if(checkEvolveServe && !onlyQuicks)
            {
                AddEvolveOptionsFromCost(player, card, ref i);
                AddServeEffects(player, card, ref i);
            }

            // Open window
            evolveOptionsWindow.Close();
            Vector2 viewportPos = cam.WorldToViewportPoint(card.transform.position);
            SetAnchoredPosition(new Vector2(viewportPos.x * rectTransform.rect.width, viewportPos.y * rectTransform.rect.height + verticalOffset));
            gameObject.SetActive(true);
        }

        public void AddEffects(PlayerController player, CardObject card, List<ActivatedAbility> abilities)
        {
            int i = buttons.Count(x => x.gameObject.activeSelf);
            for(; i < abilities.Count; i++)
            {
                MultipleChoiceButton button = i < buttons.Count ? buttons[i] : AddNewButton();
                ActivatedAbility ability = abilities[i]; // need to detach reference from var i for the button event

                button.gameObject.SetActive(true);
                button.Text = LibraryCardCache.GetEffectText(card.RuntimeCard.cardId, ability.name);
                button.Interactable = player.LocalEvents.CanPayCosts(card.RuntimeCard, ability.costs, ability.name)
                    && (ability.effect is not IEvolveEffect evolveEffect || evolveEffect.CanEvolve(player, card.RuntimeCard));
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
        }

        public void SetAnchoredPosition(Vector2 viewportPos)
        {
            window.anchoredPosition = viewportPos;
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

        #region Evolve Options

        private void AddEvolveOptionsFromAbility(PlayerController player, CardObject card, ActivatedAbility ability, MultipleChoiceButton button, Action baseEvolveAction)
        {
            // Should add options check
            if(player.EvolvedThisTurn || !player.LocalEvents.HasEvolvePoint())
                return;
            int playPointCostAmount = SVEFormulaParser.ParseValue((ability.costs.FirstOrDefault(x => x is PlayPointCost) as PlayPointCost)?.amount, player, card);
            if(playPointCostAmount <= 0)
                return;

            // Cost check
            List<Cost> costs = new(ability.costs);
            for(int i = 0; i < costs.Count; i++)
            {
                if(costs[i] is not PlayPointCost playPointCost)
                    continue;
                costs[i] = playPointCost.CopyWithOverrideAmount(playPointCostAmount - 1);
                costs.Insert(++i, new EvolvePointCost() { amount = "1" });
            }
            if(!player.LocalEvents.CanPayCosts(card.RuntimeCard, costs, ability.name))
                return;

            // Init options
            button.AdditionalEffects.Add(ActivateEvolveOptionsWindow.KEY_STANDARD_EVOLVE, baseEvolveAction);
            button.AdditionalEffects.Add(ActivateEvolveOptionsWindow.KEY_EVOLVE_POINT, () =>
            {
                player.AdditionalStats.AbilitiesUsedThisTurn.Add(new PlayedAbilityData(card.RuntimeCard.instanceId, card.LibraryCard.id, ability.name));
                player.LocalEvents.PayAbilityCosts(card, costs, ability.name, () =>
                {
                    SVEEffectPool.Instance.ResolveEffectImmediate(ability.effect as SveEffect, card.RuntimeCard, SVEProperties.Zones.Field, onComplete: () =>
                    {
                        SVEEffectPool.Instance.CmdExecuteConfirmationTiming();
                    });
                });
                Close();
            });
            button.SetArrowActive(true);
            button.OnHoverEnter += () =>
            {
                evolveOptionsWindow.Open(button, isEvolve: true, canUseBase: button.Interactable, hasEvolvePoint: true, hasSuperEvolve: false);
            };

            // Enable base button if necessary (can't use base evolve, but can use evolve point)
            if(!button.Interactable)
            {
                button.Interactable = true;
                button.OnClickEffect.RemoveAllListeners();
            }
        }

        private void AddEvolveOptionsFromCost(PlayerController player, CardObject card, ref int buttonsIndex)
        {
            int evolveCost = card.GetEvolveCost();
            if(evolveCost < 0)
                return;
            bool canEvolveThisTurn = !player.EvolvedThisTurn && player.ZoneController.EvolveDeckHasEvolvedVersionOf(card.RuntimeCard);
            bool canPayBaseCost = player.LocalEvents.CanPayEvolveCost(evolveCost);

            // Evolve without evolve point
            MultipleChoiceButton button = buttonsIndex < buttons.Count ? buttons[buttonsIndex] : AddNewButton();
            button.gameObject.SetActive(true);
            button.Text = string.Format(evolveTextTemplate, GetFormattedEvolveCost(evolveCost));
            button.Interactable = canEvolveThisTurn && canPayBaseCost;
            Action evolveAction = () =>
            {
                player.LocalEvents.EvolveCard(card, false);
                Close();
            };
            button.OnClickEffect.AddListener(() => evolveAction?.Invoke());
            buttonsIndex++;

            if(!canEvolveThisTurn)
                return;
            button.AdditionalEffects.Add(ActivateEvolveOptionsWindow.KEY_STANDARD_EVOLVE, evolveAction);

            // Evolve with evolve point
            bool canUseEvolvePoint = evolveCost > 0 && player.LocalEvents.HasEvolvePoint() && player.LocalEvents.CanPayEvolveCost(evolveCost - 1, true);
            if(canUseEvolvePoint)
            {
                button.AdditionalEffects.Add(ActivateEvolveOptionsWindow.KEY_EVOLVE_POINT, () =>
                {
                    player.LocalEvents.EvolveCard(card, true);
                    Close();
                });
            }

            // Super evolve
            bool canSuperEvolve = (canPayBaseCost || canUseEvolvePoint) && player.LocalEvents.CanSuperEvolve();
            if(canSuperEvolve)
            {
                button.AdditionalEffects.Add(ActivateEvolveOptionsWindow.KEY_SUPER_EVOLVE, () =>
                {
                    player.LocalEvents.EvolveCard(card, useEvolvePoint: false, isSuperEvolve: true);
                    Close();
                });
                if(canUseEvolvePoint)
                {
                    button.AdditionalEffects.Add(ActivateEvolveOptionsWindow.KEY_SUPER_EVOLVE_WITH_EP, () =>
                    {
                        player.LocalEvents.EvolveCard(card, useEvolvePoint: true, isSuperEvolve: true);
                        Close();
                    });
                }
            }

            // Options menu
            if(button.AdditionalEffects.Count <= 1)
                return;
            button.SetArrowActive(true);
            button.OnHoverEnter += () =>
            {
                evolveOptionsWindow.Open(button, isEvolve: true, canUseBase: canPayBaseCost, hasEvolvePoint: canUseEvolvePoint, hasSuperEvolve: canSuperEvolve);
            };
            // Enable base button if necessary (can't use base evolve, but can use evolve point)
            if(!button.Interactable)
            {
                button.Interactable = true;
                button.OnClickEffect.RemoveAllListeners();
            }
        }

        private void AddServeEffects(PlayerController player, CardObject card, ref int buttonsIndex)
        {
            if(card.RuntimeCard.HasKeyword(SVEProperties.PassiveAbilities.IsRacing))
                return;

            if(card.RuntimeCard.HasKeyword(SVEProperties.PassiveAbilities.Serve1))
                AddServeButton(ref buttonsIndex, 1);
            if(card.RuntimeCard.HasKeyword(SVEProperties.PassiveAbilities.Serve2))
                AddServeButton(ref buttonsIndex, 2);
            if(card.RuntimeCard.HasKeyword(SVEProperties.PassiveAbilities.Serve3))
                AddServeButton(ref buttonsIndex, 3);

            // -----

            void AddServeButton(ref int buttonsIndex, int serveCount)
            {
                bool canServe = !player.EvolvedThisTurn && serveCount <= player.ZoneController.EvolveDeckCarrotCount();
                bool canPayBaseCost = player.LocalEvents.CanPayEvolveCost(serveCount);

                // Create button
                MultipleChoiceButton button = buttonsIndex < buttons.Count ? buttons[buttonsIndex] : AddNewButton();
                button.gameObject.SetActive(true);
                button.Text = GetFormattedServeText(serveCount);
                button.Interactable = canServe && canPayBaseCost;
                Action baseServe = () =>
                {
                    player.LocalEvents.ServeAndRaceCard(card, false, serveCount);
                    Close();
                };
                button.OnClickEffect.AddListener(() => baseServe?.Invoke());
                buttonsIndex++;

                // Check serve with EP
                if(!canServe || !player.LocalEvents.HasEvolvePoint() || !player.LocalEvents.CanPayEvolveCost(serveCount, true))
                    return;
                button.AdditionalEffects.Add(ActivateEvolveOptionsWindow.KEY_RACE, baseServe);
                button.AdditionalEffects.Add(ActivateEvolveOptionsWindow.KEY_EVOLVE_POINT, () =>
                {
                    player.LocalEvents.ServeAndRaceCard(card, true, serveCount);
                    Close();
                });
                button.SetArrowActive(true);
                button.OnHoverEnter += () =>
                {
                    evolveOptionsWindow.Open(button, isEvolve: false, canUseBase: canPayBaseCost, hasEvolvePoint: true, hasSuperEvolve: false);
                };
                if(!button.Interactable) // Enable base button if necessary (can't use base cost, but can use evolve point)
                {
                    button.Interactable = true;
                    button.OnClickEffect.RemoveAllListeners();
                }
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

        private string GetFormattedServeText(int serveCount, bool withEvolvePoint = false)
        {
            return string.Format(serveTextTemplate, string.Concat(Enumerable.Repeat(carrotIconFormatting, serveCount)),
                (serveCount == 1 && withEvolvePoint) ? evolvePointFormatting : GetFormattedEvolveCost(serveCount - (withEvolvePoint ? 1 : 0), withEvolvePoint),
                serveCount > 1 ? $" {serveCount} times" : "");
        }

        #endregion
    }
}
