using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CCGKit;
using Mirror;
using SVESimulator.UI;
using Sparkfire.Utility;
using CardFilterSetting = SVESimulator.SVEFormulaParser.CardFilterSetting;

namespace SVESimulator
{
    /// <summary>
    /// Handles effect pools and confirmation timing
    /// This is a MonoBehaviour singleton and not attached to CCGKit GameState because we need the ability for the player to choose the order of effects,
    ///   and to hold the effects to active when we want instead of them all being automated
    /// </summary>
    public class SVEEffectPool : NetworkBehaviour
    {
        #region Variables

        public static SVEEffectPool Instance;

        private enum ConfirmationTimingState { Idle, ResolvingTurnPlayer, FinishedTurnPlayer,
            ResolvingNonTurnPlayer, FinishedNonTurnPlayer }

        // ---

        [Header("Runtime Data"), SerializeField]
        private PlayerController localPlayer;
        [SerializeField]
        private PlayerController opponentPlayer;
        [SerializeField]
        private List<RegisteredPassiveAbility> registeredPassives = new();
        [SerializeField]
        private List<SVEPendingEffect> pendingEffects = new();
        [SerializeField, SyncVar(hook = nameof(ConfirmationTimingSyncVarHook))]
        private ConfirmationTimingState confirmationTimingState = ConfirmationTimingState.Idle;
        [field: SerializeField, SyncVar]
        public bool IsResolvingEffect { get; private set; }

        public event Action OnNextConfirmationTimingStart;
        public event Action OnConfirmationTimingStartConstant;
        public event Action OnNextConfirmationTimingEnd;
        public event Action OnConfirmationTimingEndConstant;
        public event Action OnNextConfirmationTimingStartOrEnd;
        public event Action OnConfirmationTimingStartOrEndConstant;

        public List<RegisteredPassiveAbility> RegisteredPassives => new(registeredPassives);
        public bool IsActive => confirmationTimingState != ConfirmationTimingState.Idle;

        #endregion

        // ------------------------------

        #region Unity Functions + Initialize

        private void Awake()
        {
            // Singleton
            if(Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void LocalInitialize()
        {
            PlayerController[] controllers = FindObjectsOfType<PlayerController>();
            localPlayer = controllers.FirstOrDefault(x => x.isLocalPlayer);
            opponentPlayer = controllers.FirstOrDefault(x => !x.isLocalPlayer);
            Debug.Assert(localPlayer);
            Debug.Assert(opponentPlayer);

            OnConfirmationTimingStartOrEndConstant += UpdatePassivesByCondition;
        }

        #endregion

        // ------------------------------

        #region Add/Pool Effects

        public void TriggerPendingEffects<T>(GameState gameState, RuntimeCard sourceCard, PlayerInfo resolvingPlayer, Predicate<T> predicate, bool executeConfirmationTiming,
            RuntimeCard triggeringCard = null, string triggeringCardZone = null, List<Ability> abilityList = null) where T : SveTrigger
        {
            Card libraryCard = LibraryCardCache.GetCard(sourceCard.cardId, gameState.config);
            TriggerPendingEffects(libraryCard, sourceCard, resolvingPlayer, predicate, executeConfirmationTiming, triggeringCard, triggeringCardZone, abilityList);
        }

        // might need to add delays in here
        public void TriggerPendingEffects<T>(Card libraryCard, RuntimeCard sourceCard, PlayerInfo resolvingPlayer, Predicate<T> predicate, bool executeConfirmationTiming,
            RuntimeCard triggeringCard = null, string triggeringCardZone = null, List<Ability> abilityList = null) where T : SveTrigger
        {
            List<Ability> triggeredAbilities = abilityList ?? GetCardTriggeredAbilities(libraryCard, sourceCard);
            foreach(Ability ability in triggeredAbilities)
            {
                TriggeredAbility triggeredAbility = ability as TriggeredAbility;
                if(triggeredAbility?.trigger is T trigger && predicate(trigger) && triggeredAbility.effect is SveEffect sveEffect)
                {
                    bool isCardLocalPlayer = resolvingPlayer == localPlayer.GetPlayerInfo(); // should always be true but safety check never hurts
                    string sourceZone = (isCardLocalPlayer ? localPlayer : opponentPlayer).GetPlayerInfo().namedZones
                        .First(x => x.Value.cards.Any(y => y.instanceId == sourceCard.instanceId)).Key;

                    // Condition & cost checks
                    if((trigger.condition?.StartsWith("<<") ?? false) && !SVEFormulaParser.ParseValueAsCondition(trigger.condition[2..], localPlayer, null as RuntimeCard))
                        break;
                    if(trigger.Costs is { Count: > 0 } &&
                       trigger.Costs.All(x => x is SveCost { IsInternalCost: true } sveCost && !sveCost.CanPayCost(localPlayer, sourceCard, triggeredAbility.name)))
                        break;

                    // Add effect
                    SVEPendingEffect effect = new()
                    {
                        triggeringCardInstanceId = (triggeringCard ?? sourceCard).instanceId,
                        triggeringCardZone = triggeringCard != null && triggeringCardZone != null ? triggeringCardZone : sourceZone,
                        sourceCardInstanceId = sourceCard.instanceId,
                        sourceCardZone = sourceZone,
                        resolvingPlayerId = resolvingPlayer.netId.netId,
                        effect = sveEffect,
                        costs = trigger.Costs,
                        cardId = libraryCard.id,
                        abilityName = triggeredAbility.name,
                        condition = trigger.condition
                    };
                    pendingEffects.Add(effect);
                }
            }

            if(executeConfirmationTiming)
                CmdExecuteConfirmationTiming();
        }

        // -----

        public void TriggerPendingEffectsForOtherCardsInZone<T>(GameState gameState, RuntimeCard sourceCard, RuntimeZone targetZone,
            PlayerInfo resolvingPlayer, Predicate<T> predicate, bool executeConfirmationTiming) where T : SveTrigger
        {
            string sourceZoneName = sourceCard != null
                ? CardManager.Instance.GetCardByInstanceId(sourceCard.instanceId).CurrentZone.Runtime.name
                : null;
            TriggerPendingEffectsForOtherCardsInZone(gameState, sourceCard, sourceZoneName, targetZone, resolvingPlayer, predicate, executeConfirmationTiming);
        }

        public void TriggerPendingEffectsForOtherCardsInZone<T>(GameState gameState, RuntimeCard sourceCard, string sourceZoneName, RuntimeZone targetZone,
            PlayerInfo resolvingPlayer, Predicate<T> predicate, bool executeConfirmationTiming) where T : SveTrigger
        {
            // Trigger for cards in zone
            PlayerController player = localPlayer.GetPlayerInfo() == resolvingPlayer ? localPlayer : opponentPlayer;
            CardZone cardZone = player.ZoneController.AllZones[targetZone.name];
            foreach(CardObject card in (cardZone is CardPositionedZone positionedZone ? positionedZone.GetAllPrimaryCards() : cardZone.AllCards))
            {
                if(sourceCard != null && card.RuntimeCard.instanceId == sourceCard.instanceId)
                    continue;
                TriggerPendingEffects(gameState, card.RuntimeCard, resolvingPlayer, predicate, false, sourceCard, sourceZoneName);
            }

            // Trigger floating effects (handled internally as abilities given to the player's leader)
            List<RegisteredPassiveAbility> floatingAbilitiesPassives = GetFloatingAbilityPassives();
            if(floatingAbilitiesPassives is { Count: > 0 })
            {
                for(int i = 0; i < floatingAbilitiesPassives.Count; i++)
                {
                    // TODO - more efficient find (probably need a generic GetRuntimeCardFromInstanceId function at some point)
                    RuntimeCard floatingAbilitySourceCard = localPlayer.GetPlayerInfo().namedZones.FirstOrDefault(x => x.Value.cards.Any(y => y.instanceId == floatingAbilitiesPassives[i].sourceCardInstanceId))
                        .Value?.cards?.FirstOrDefault(x => x.instanceId == floatingAbilitiesPassives[i].sourceCardInstanceId);
                    Ability abilityToTrigger = (floatingAbilitiesPassives[i].effect as GiveAbilityPassive)?.GetAbility(floatingAbilitiesPassives[i].sourceCardInstanceId);
                    TriggerPendingEffects(gameState, floatingAbilitySourceCard, resolvingPlayer, predicate, false,
                        triggeringCard: sourceCard, triggeringCardZone: sourceZoneName, abilityList: new List<Ability>() { abilityToTrigger });
                }
            }

            // Confirmation timing
            if(executeConfirmationTiming)
                CmdExecuteConfirmationTiming();
        }

        #endregion

        // ------------------------------

        #region Register Passives

        public void RegisterPassiveAbilities(GameState gameState, RuntimeCard sourceCard)
        {
            Card libraryCard = LibraryCardCache.GetCard(sourceCard.cardId, gameState.config);
            RegisterPassiveAbilities(libraryCard, sourceCard);
        }

        public void RegisterPassiveAbilities(Card libraryCard, RuntimeCard sourceCard)
        {
            List<Ability> triggeredAbilities = libraryCard.abilities.FindAll(x => x is TriggeredAbility);
            foreach(Ability ability in triggeredAbilities)
            {
                TriggeredAbility triggeredAbility = ability as TriggeredAbility;
                if(triggeredAbility != null && triggeredAbility.trigger is PassiveAbilityOnField trigger && triggeredAbility.effect is SvePassiveEffect passiveEffect)
                {
                    string filterFormula = trigger.filter + (trigger.target == SVEProperties.SVEEffectTarget.Self ? $"i({sourceCard.instanceId})" : "");
                    RegisteredPassiveAbility newPassive = new()
                    {
                        sourceCardInstanceId = sourceCard.instanceId,
                        sourceCardId = sourceCard.cardId,
                        filters = SVEFormulaParser.ParseCardFilterFormula(filterFormula, sourceCard.instanceId),
                        effect = passiveEffect,
                        affectedCards = new List<RuntimeCard>(),
                        target = trigger.target,
                        duration = passiveEffect.duration,
                        condition = trigger.condition
                    };
                    RegisterPassiveAbility(newPassive);
                }
            }
        }

        public void RegisterPassiveAbility(RegisteredPassiveAbility passive)
        {
            registeredPassives.Add(passive);
            if(passive.effect.duration == SVEProperties.PassiveDuration.OpponentTurn && localPlayer.isActivePlayer)
                return;
            OnNextConfirmationTimingStartOrEnd += () => EnablePassive(passive, localPlayer);
            // See TODO in ApplyAllActivePassivesToCard
        }

        public void UnregisterPassiveAbilities(RuntimeCard sourceCard)
        {
            foreach(RegisteredPassiveAbility passive in registeredPassives.Where(x => x.sourceCardInstanceId == sourceCard.instanceId))
            {
                DisablePassive(passive);
            }
            registeredPassives.RemoveAll(x => x.sourceCardInstanceId == sourceCard.instanceId);
        }

        public void UnregisterPassiveAbility(RegisteredPassiveAbility passive)
        {
            DisablePassive(passive);
            registeredPassives.Remove(passive);
        }

        #endregion

        // ------------------------------

        #region Trigger Spell

        public void TriggerSpellImmediate(GameState gameState, RuntimeCard sourceCard, PlayerInfo resolvingPlayer, Action onComplete)
        {
            Card libraryCard = LibraryCardCache.GetCard(sourceCard.cardId, gameState.config);
            List<Ability> abilities = libraryCard.abilities.FindAll(x => x is TriggeredAbility { trigger: SpellAbility });
            if(abilities.Count == 0)
            {
                Debug.LogWarning($"Spell \"{libraryCard.name}\" with card ID {libraryCard.GetStringProperty(SVEProperties.CardStats.ID)} " +
                    $"(instance ID {sourceCard.instanceId}) was played but has no abilities.");
                onComplete?.Invoke();
                return;
            }
            TriggeredAbility abilityToTrigger = abilities[0] as TriggeredAbility;
            string condition = (abilityToTrigger?.trigger as SveTrigger)?.condition;

            if(!string.IsNullOrWhiteSpace(condition) && !SVEFormulaParser.ParseValueAsCondition(condition, resolvingPlayer.netId.isLocalPlayer ? localPlayer : opponentPlayer, sourceCard))
            {
                onComplete?.Invoke();
                return;
            }
            ResolveEffectImmediate(abilityToTrigger.effect as SveEffect, sourceCard, "Resolution", onComplete);
        }

        #endregion

        // ------------------------------

        #region Confirmation Timing & Resolve Effects

        [Command(requiresAuthority = false)]
        public void CmdExecuteConfirmationTiming()
        {
            if(confirmationTimingState != ConfirmationTimingState.Idle)
                return;
            StartCoroutine(ConfirmationTimingCoroutine());

            IEnumerator ConfirmationTimingCoroutine()
            {
                Debug.Log("Executing confirmation timing");
                confirmationTimingState = ConfirmationTimingState.ResolvingTurnPlayer;
                RpcResolveConfirmationTimingPlayer(GetTurnPlayer().netIdentity.connectionToClient, true);
                yield return new WaitForSeconds(0.1f); // test delay
                yield return new WaitUntil(() => confirmationTimingState == ConfirmationTimingState.FinishedTurnPlayer);

                confirmationTimingState = ConfirmationTimingState.ResolvingNonTurnPlayer;
                RpcResolveConfirmationTimingPlayer(GetNonTurnPlayer().netIdentity.connectionToClient, false);
                yield return new WaitForSeconds(0.1f); // test delay
                yield return new WaitUntil(() => confirmationTimingState == ConfirmationTimingState.FinishedNonTurnPlayer);

                yield return new WaitForSeconds(0.1f); // test delay
                confirmationTimingState = ConfirmationTimingState.Idle;
            }
        }

        [TargetRpc]
        public void RpcResolveConfirmationTimingPlayer(NetworkConnectionToClient networkConnection, bool isTurnPlayer)
        {
            Debug.Log($"Resolving local confirmation timing ({(isTurnPlayer ? "" : "non-")}turn player): {pendingEffects.Count} effects");
            if(pendingEffects.Any(x => x.effect == null))
            {
                Debug.LogError($"Effect pool had {pendingEffects.Count(x => x.effect == null)} effects that were null!");
                pendingEffects = pendingEffects.Where(x => x.effect != null).ToList();
            }
            StartCoroutine(ResolveOverTime());

            IEnumerator ResolveOverTime()
            {
                // Skip prompt if all effects fail condition
                if(pendingEffects.All(x =>
                   {
                       if(x.condition.IsNullOrWhiteSpace())
                           return false;
                       CardObject card = CardManager.Instance.GetCardByInstanceId(x.sourceCardInstanceId);
                       return !SVEFormulaParser.ParseValueAsCondition(x.condition, localPlayer, card);
                   }))
                {
                    goto exit;
                }

                // Resolve single effect
                while(pendingEffects.Count == 1)
                {
                    for(int i = 0; i < pendingEffects.Count; i++)
                    {
                        yield return ResolveEffectAtIndex(i);
                        break;
                    }
                }

                // Resolve multiple effects (choose from list)
                while(pendingEffects.Count > 0)
                {
                    yield return null;
                    bool effectDone = false;
                    List<MultipleChoiceWindow.MultipleChoiceEntryData> multipleChoiceEntries = new();
                    for(int i = 0; i < pendingEffects.Count; i++)
                    {
                        int index = i;
                        multipleChoiceEntries.Add(pendingEffects[i].AsMultipleChoiceEntry(() =>
                        {
                            GameUIManager.MultipleChoice.Close();
                            StartCoroutine(ResolveEffectAtIndex(index, () => { effectDone = true; }));
                        }));
                    }
                    GameUIManager.MultipleChoice.Open(localPlayer, "Confirmation Timing", multipleChoiceEntries, "Select effect order", showTargetingToOpponent: true);
                    yield return new WaitUntil(() => effectDone);
                }

                // Complete
                exit:
                yield return null;
                pendingEffects.Clear();
                CmdSetConfirmationTimingState(isTurnPlayer ? ConfirmationTimingState.FinishedTurnPlayer : ConfirmationTimingState.FinishedNonTurnPlayer);
            }

            IEnumerator ResolveEffectAtIndex(int index, Action onComplete = null)
            {
                bool effectDone = false;
                ResolvePendingEffect(pendingEffects[index], () =>
                {
                    effectDone = true;
                });
                yield return new WaitUntil(() => effectDone);
                pendingEffects.RemoveAt(index);
                onComplete?.Invoke();
            }
        }

        public void ResolvePendingEffect(SVEPendingEffect pendingEffect, Action onComplete = null)
        {
            CardObject cardObject = CardManager.Instance.GetCardByInstanceId(pendingEffect.sourceCardInstanceId);
            if(!pendingEffect.condition.IsNullOrWhiteSpace() && !SVEFormulaParser.ParseValueAsCondition(pendingEffect.condition, localPlayer, cardObject))
            {
                onComplete?.Invoke();
                return;
            }

            CmdSetIsResolvingEffect(true);
            if(pendingEffect.costs == null || pendingEffect.costs.Count == 0)
            {
                Resolve();
            }
            else
            {
                Debug.Assert(cardObject, $"Failed to find card with instance ID {pendingEffect.sourceCardInstanceId} in zone {pendingEffect.sourceCardZone} for ability {pendingEffect.abilityName}");
                bool canPayCost = localPlayer.LocalEvents.CanPayCosts(cardObject.RuntimeCard, pendingEffect.costs, pendingEffect.abilityName);
                bool isOptionalEffect = pendingEffect.costs.Any(x => x is OptionalEffectAsCost);

                // Skip prompt if all costs are internal
                if(!isOptionalEffect && pendingEffect.costs.All(x => x is SveCost { IsInternalCost: true }))
                {
                    if(canPayCost)
                        ResolveWithCost();
                    else
                        onComplete?.Invoke();
                    return;
                }

                // Prompt player to pay for cost or decline
                List<MultipleChoiceWindow.MultipleChoiceEntryData> costOptions = new()
                {
                    new MultipleChoiceWindow.MultipleChoiceEntryData
                    {
                        text = canPayCost ? (isOptionalEffect ? "Perform Effect" : "Pay Cost") : "Cannot Pay Cost",
                        onSelect = () =>
                        {
                            GameUIManager.NetworkedCalls.CmdCloseOpponentTargeting(localPlayer.GetOpponentInfo().netId);
                            ResolveWithCost();
                        },
                        disabled = !canPayCost
                    },
                    new MultipleChoiceWindow.MultipleChoiceEntryData
                    {
                        text = "Decline",
                        onSelect = () =>
                        {
                            CmdSetIsResolvingEffect(false);
                            onComplete?.Invoke();
                        }
                    },
                };
                GameUIManager.MultipleChoice.Open(localPlayer, cardObject.LibraryCard.name, costOptions, pendingEffect.effect.text);
            }

            // ---

            void Resolve()
            {
                localPlayer.AdditionalStats.AbilitiesUsedThisTurn.Add(new PlayedAbilityData(pendingEffect.sourceCardInstanceId, cardObject.LibraryCard.id, pendingEffect.abilityName));
                pendingEffect.effect.Resolve(localPlayer, pendingEffect.triggeringCardInstanceId, pendingEffect.triggeringCardZone,
                    pendingEffect.sourceCardInstanceId, pendingEffect.sourceCardZone, () =>
                    {
                        CmdSetIsResolvingEffect(false);
                        onComplete?.Invoke();
                    });
            }
            void ResolveWithCost()
            {
                localPlayer.AdditionalStats.AbilitiesUsedThisTurn.Add(new PlayedAbilityData(pendingEffect.sourceCardInstanceId, cardObject.LibraryCard.id, pendingEffect.abilityName));
                localPlayer.LocalEvents.PayAbilityCosts(cardObject, pendingEffect.costs, pendingEffect.abilityName, Resolve);
            }
        }

        /// <summary>
        /// Resolves an effect immediately without going through confirmation timing and the effect pool
        /// </summary>
        public void ResolveEffectImmediate(SveEffect effect, RuntimeCard card, string zoneName = "Resolution", Action onComplete = null, bool useLocalPlayer = true)
        {
            CmdSetIsResolvingEffect(true);
            effect.Resolve(useLocalPlayer ? localPlayer : opponentPlayer, card.instanceId, zoneName, card.instanceId, zoneName, () =>
            {
                CmdSetIsResolvingEffect(false);
                onComplete?.Invoke();
            });
        }

        // ---

        [Command(requiresAuthority = false)]
        private void CmdSetIsResolvingEffect(bool isResolving)
        {
            IsResolvingEffect = isResolving;
        }

        [Command(requiresAuthority = false)]
        private void CmdSetConfirmationTimingState(ConfirmationTimingState newState)
        {
            confirmationTimingState = newState;
        }

        private void ConfirmationTimingSyncVarHook(ConfirmationTimingState oldState, ConfirmationTimingState newState)
        {
            if(oldState == ConfirmationTimingState.Idle && newState == ConfirmationTimingState.ResolvingTurnPlayer)
            {
                OnConfirmationTimingStartConstant?.Invoke();
                OnNextConfirmationTimingStart?.Invoke();
                OnNextConfirmationTimingStart = null;

                OnConfirmationTimingStartOrEndConstant?.Invoke();
                OnNextConfirmationTimingStartOrEnd?.Invoke();
                OnNextConfirmationTimingStartOrEnd = null;

                if(localPlayer)
                {
                    localPlayer.InputController.allowedInputs = PlayerInputController.InputTypes.None;
                    if(!EffectTargetingUI.TargetCard.IsActive)
                    {
                        localPlayer.ZoneController.handZone.RemoveAllCardHighlights();
                        localPlayer.ZoneController.fieldZone.RemoveAllCardHighlights();
                        localPlayer.ZoneController.exAreaZone.RemoveAllCardHighlights();
                    }
                }
            }
            else if(oldState == ConfirmationTimingState.FinishedNonTurnPlayer && newState == ConfirmationTimingState.Idle)
            {
                OnConfirmationTimingEndConstant?.Invoke();
                OnNextConfirmationTimingEnd?.Invoke();
                OnNextConfirmationTimingEnd = null;

                OnConfirmationTimingStartOrEndConstant?.Invoke();
                OnNextConfirmationTimingStartOrEnd?.Invoke();
                OnNextConfirmationTimingStartOrEnd = null;
                CardManager.Instance.ReleaseAllDisabledCards();

                if(localPlayer && !SVEQuickTimingController.Instance.IsActive)
                {
                    localPlayer.InputController.allowedInputs = localPlayer.isActivePlayer ? PlayerInputController.InputTypes.All : PlayerInputController.InputTypes.None;
                    localPlayer.ZoneController.handZone.SetAllCardsInteractable(localPlayer.isActivePlayer);
                    localPlayer.ZoneController.fieldZone.SetAllCardsInteractable(localPlayer.isActivePlayer);
                    if(localPlayer.isActivePlayer)
                        foreach(CardObject card in localPlayer.ZoneController.fieldZone.GetAllPrimaryCards())
                            card.CalculateCanAttackStatus();
                }
            }
        }

        #endregion

        // ------------------------------

        #region Apply Passives

        public void ApplyAllActivePassivesToCard(RuntimeCard card)
        {
            if(card == null)
                return;

            // Apply during next confirmation timing to wait for other effects to resolve TODO - update any time field changes/apply in the middle of confirmation timing
            //   (i.e. don't apply the passive before we finish playing the card to the field)
            OnNextConfirmationTimingStartOrEnd += () =>
            {
                if(!localPlayer.ZoneController.fieldZone.ContainsCard(card) && !opponentPlayer.ZoneController.fieldZone.ContainsCard(card))
                    return;

                foreach(RegisteredPassiveAbility registeredPassive in registeredPassives)
                {
                    if((registeredPassive.duration == SVEProperties.PassiveDuration.OpponentTurn && (card.ownerPlayer == localPlayer.GetPlayerInfo()) == localPlayer.isActivePlayer)
                       || (registeredPassive.target == SVEProperties.SVEEffectTarget.Self && card.instanceId != registeredPassive.sourceCardInstanceId)
                       || registeredPassive.effect is MinusCostOtherPassive
                       || !registeredPassive.filters.MatchesCard(card)
                       || registeredPassive.affectedCards.Contains(card)
                       || !registeredPassive.MeetsCondition(localPlayer))
                        continue;

                    registeredPassive.effect.ApplyPassive(card, localPlayer);
                    registeredPassive.affectedCards.Add(card);
                }
            };
        }

        public void RemovePassivesFromCard(RuntimeCard card, PlayerController player)
        {
            IEnumerable<RegisteredPassiveAbility> appliedPassives = registeredPassives.Where(x => x.affectedCards.Contains(card));
            foreach(RegisteredPassiveAbility passive in appliedPassives)
            {
                passive.affectedCards.Remove(card);
                passive.effect.RemovePassive(card, player);
            }
        }

        public void UpdatePassivesByCondition()
        {
            foreach(RegisteredPassiveAbility passive in registeredPassives)
            {
                if(passive.condition.IsNullOrWhiteSpace())
                    continue;
                bool meetsCondition = passive.MeetsCondition(localPlayer)
                    && (passive.duration != SVEProperties.PassiveDuration.OpponentTurn || localPlayer.isActivePlayer);

                if(meetsCondition && passive.affectedCards.Count == 0)
                    EnablePassive(passive, localPlayer);
                else if(!meetsCondition && passive.affectedCards.Count > 0)
                    DisablePassive(passive);
            }
        }

        public void UpdatePassiveDurationsStartOfTurn(PlayerController player, bool isTurnPlayer)
        {
            List<RegisteredPassiveAbility> passiveList = new List<RegisteredPassiveAbility>(registeredPassives);
            foreach(RegisteredPassiveAbility passive in passiveList)
            {
                switch(passive.duration)
                {
                    case SVEProperties.PassiveDuration.WhileOnField:
                        continue;
                    case SVEProperties.PassiveDuration.OpponentTurn:
                        if(isTurnPlayer)
                            DisablePassive(passive);
                        else
                            EnablePassive(passive, player);
                        break;
                    case SVEProperties.PassiveDuration.EndOfTurn:
                        UnregisterPassiveAbility(passive);
                        break;
                    case SVEProperties.PassiveDuration.EndOfNextTurn:
                        passive.duration = SVEProperties.PassiveDuration.EndOfTurn;
                        break;
                }
            }
        }

        public int GetReducedCostFromActivePassives(RuntimeCard card, PlayerController player)
        {
            int reduction = 0;
            foreach(RegisteredPassiveAbility passive in registeredPassives)
            {
                if(passive.effect is not MinusCostOtherPassive minusCostEffect)
                    continue;
                if(!passive.filters.MatchesCard(card) || !passive.MeetsCondition(player))
                    continue;

                PlayerInfo playerInfo = player ? player.GetPlayerInfo() : card.ownerPlayer;
                RuntimeCard sourceCard = playerInfo.namedZones[SVEProperties.Zones.Field].cards.FirstOrDefault(x => x.instanceId == passive.sourceCardInstanceId);
                reduction += minusCostEffect.GetReductionAmount(sourceCard, player);
            }
            return reduction;
        }

        // -----

        private void EnablePassive(RegisteredPassiveAbility passive, PlayerController player)
        {
            if(!passive.MeetsCondition(player))
            {
                DisablePassive(passive);
                return;
            }
            if(passive.target == SVEProperties.SVEEffectTarget.Self)
            {
                CardObject card = CardManager.Instance.GetCardByInstanceId(passive.sourceCardInstanceId);
                if(card)
                {
                    passive.effect.ApplyPassive(card.RuntimeCard, player);
                    passive.affectedCards.Add(card.RuntimeCard);
                }
                return;
            }

            List<CardObject> potentialPassiveTargets = new();
            potentialPassiveTargets.AddRange(player.ZoneController.fieldZone.GetAllPrimaryCards());
            potentialPassiveTargets.AddRange(player.OppZoneController.fieldZone.GetAllPrimaryCards());
            foreach(CardObject card in potentialPassiveTargets)
            {
                if(!passive.filters.MatchesCard(card.RuntimeCard) || passive.affectedCards.Contains(card.RuntimeCard))
                    continue;

                passive.effect.ApplyPassive(card.RuntimeCard, player);
                passive.affectedCards.Add(card.RuntimeCard);
            }
        }

        private void DisablePassive(RegisteredPassiveAbility passive)
        {
            foreach(RuntimeCard card in passive.affectedCards)
            {
                passive.effect.RemovePassive(card, localPlayer);
            }
            passive.affectedCards.Clear();
        }

        #endregion

        // ------------------------------

        #region Get Info

        private PlayerController GetTurnPlayer()
        {
            return localPlayer.isActivePlayer ? localPlayer : opponentPlayer;
        }

        private PlayerController GetNonTurnPlayer()
        {
            return localPlayer.isActivePlayer ? opponentPlayer : localPlayer;
        }

        private List<Ability> GetCardTriggeredAbilities(Card libraryCard, RuntimeCard card)
        {
            List<Ability> abilityList = libraryCard?.abilities?.FindAll(x => x is TriggeredAbility) ?? new();
            foreach(RegisteredPassiveAbility passive in registeredPassives)
            {
                if(passive.target == SVEProperties.SVEEffectTarget.Leader ||
                   (passive.target == SVEProperties.SVEEffectTarget.Self && card.instanceId != passive.sourceCardInstanceId))
                    continue;
                if(passive.effect is not GiveAbilityPassive giveAbilityPassive || !passive.filters.MatchesCard(card) /*|| !passive.MeetsCondition(player) TODO*/)
                    continue;
                Ability ability = giveAbilityPassive.GetAbility(passive.sourceCardId);
                if(ability is TriggeredAbility)
                    abilityList.Add(ability);
            }
            return abilityList;
        }

        private List<RegisteredPassiveAbility> GetFloatingAbilityPassives()
        {
            // Floating effects are handled internally as abilities given to the player's leader
            // Floating abilities (generally) come from spells that have abilities that active after they are played (i.e. start end phase)
            return registeredPassives.Where(x => x.target == SVEProperties.SVEEffectTarget.Leader).ToList();
        }

        public bool TryGetAdditionalCardTraits(RuntimeCard card, out List<string> additionalTraits)
        {
            additionalTraits = null;
            for(int i = 0; i < registeredPassives.Count; i++)
            {
                if(registeredPassives[i].effect is not AddTraitPassive addTraitPassive || !registeredPassives[i].affectedCards.Any(x => x.instanceId == card.instanceId))
                    continue;
                additionalTraits ??= new List<string>();
                additionalTraits.Add(addTraitPassive.trait);
            }
            return additionalTraits != null;
        }

        #endregion
    }

    // ------------------------------

    [Serializable]
    public class SVEPendingEffect
    {
        public int triggeringCardInstanceId;
        public string triggeringCardZone;
        public int sourceCardInstanceId;
        public string sourceCardZone;
        public uint resolvingPlayerId;
        public SveEffect effect;
        public List<Cost> costs;
        public int cardId;
        public string abilityName;
        public string condition;

        public MultipleChoiceWindow.MultipleChoiceEntryData AsMultipleChoiceEntry(Action onSelect)
        {
            return new MultipleChoiceWindow.MultipleChoiceEntryData()
            {
                text = $"{LibraryCardCache.GetName(cardId)}{(effect.text.IsNullOrWhiteSpace() ? "" : $" - {effect.text}")}",
                onSelect = onSelect
            };
        }
    }

    [Serializable]
    public class RegisteredPassiveAbility : IEquatable<RegisteredPassiveAbility>
    {
        public int sourceCardInstanceId;
        public int sourceCardId;
        public Dictionary<CardFilterSetting, string> filters;
        public SvePassiveEffect effect;
        public List<RuntimeCard> affectedCards;
        public SVEProperties.SVEEffectTarget target;
        public SVEProperties.PassiveDuration duration;
        public string condition;

        // ------------------------------

        public bool MeetsCondition(PlayerController player, RuntimeCard sourceCard = null)
        {
            if(condition.IsNullOrWhiteSpace())
                return true;
            if(sourceCard == null)
            {
                CardObject cardObject = CardManager.Instance.GetCardByInstanceId(sourceCardInstanceId);
                sourceCard = cardObject ? cardObject.RuntimeCard : null;
            }
            Debug.Assert(sourceCard != null);
            return SVEFormulaParser.ParseValueAsCondition(condition, player, sourceCard);
        }

        public bool Equals(RegisteredPassiveAbility other)
        {
            return sourceCardInstanceId == other.sourceCardInstanceId
                && effect.GetType() == other.effect.GetType()
                && affectedCards.Count == other.affectedCards.Count
                && target == other.target
                && duration == other.duration
                && condition == null ? other.condition == null : condition.Equals(other.condition);
        }

        public override bool Equals(object obj) => obj is RegisteredPassiveAbility other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(sourceCardInstanceId, effect, (int)target, (int)duration);
    }
}
