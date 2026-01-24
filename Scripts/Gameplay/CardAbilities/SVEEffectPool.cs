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

        public enum EffectTriggerState { Immediate, StartEndPhase }
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

        public List<RegisteredPassiveAbility> RegisteredPassives => new(registeredPassives);

        private Dictionary<System.Type, EffectTriggerState> TriggerStateTypeMap = new()
        {
            { typeof(SveStartEndPhaseTrigger), EffectTriggerState.StartEndPhase }
        };

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
        }

        #endregion

        // ------------------------------

        #region Add/Pool Effects

        public void TriggerPendingEffects<T>(GameState gameState, RuntimeCard sourceCard, PlayerInfo resolvingPlayer, Predicate<T> predicate, bool executeConfirmationTiming,
            RuntimeCard triggeringCard = null, string triggeringCardZone = null, EffectTriggerState triggerState = EffectTriggerState.Immediate) where T : SveTrigger
        {
            Card libraryCard = LibraryCardCache.GetCard(sourceCard.cardId, gameState.config);
            TriggerPendingEffects(libraryCard, sourceCard, resolvingPlayer, predicate, executeConfirmationTiming, triggeringCard, triggeringCardZone, triggerState);
        }

        // might need to add delays in here
        public void TriggerPendingEffects<T>(Card libraryCard, RuntimeCard sourceCard, PlayerInfo resolvingPlayer, Predicate<T> predicate, bool executeConfirmationTiming,
            RuntimeCard triggeringCard = null, string triggeringCardZone = null, EffectTriggerState triggerState = EffectTriggerState.Immediate) where T : SveTrigger
        {
            List<Ability> triggeredAbilities = libraryCard.abilities.FindAll(x => x is TriggeredAbility);
            foreach(Ability ability in triggeredAbilities)
            {
                TriggeredAbility triggeredAbility = ability as TriggeredAbility;
                if(triggeredAbility?.trigger is T trigger && predicate(trigger) && triggeredAbility.effect is SveEffect sveEffect)
                {
                    bool isCardLocalPlayer = resolvingPlayer == localPlayer.GetPlayerInfo(); // should always be true but safety check never hurts
                    string sourceZone = (isCardLocalPlayer ? localPlayer : opponentPlayer).GetPlayerInfo().namedZones
                        .First(x => x.Value.cards.Any(y => y.instanceId == sourceCard.instanceId)).Key;

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
                        condition = trigger.condition,
                        triggerState = triggerState
                    };
                    pendingEffects.Add(effect);
                }
            }

            if(executeConfirmationTiming)
                CmdExecuteConfirmationTiming();
        }

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

            // Trigger floating effects
            EffectTriggerState stateToUpdate = TriggerStateTypeMap.GetValueOrDefault(typeof(T), EffectTriggerState.Immediate);
            if(stateToUpdate != EffectTriggerState.Immediate)
            {
                foreach(SVEPendingEffect pendingEffect in pendingEffects.Where(x => x.triggerState == stateToUpdate))
                    pendingEffect.triggerState = EffectTriggerState.Immediate;
            }

            // Confirmation timing
            if(executeConfirmationTiming)
                CmdExecuteConfirmationTiming();
        }

        // ---

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
                        targetsFormula = filterFormula,
                        filters = SVEFormulaParser.ParseCardFilterFormula(filterFormula, sourceCard.instanceId),
                        effect = passiveEffect,
                        affectedCards = new List<RuntimeCard>(),
                        target = trigger.target,
                        duration = passiveEffect.duration
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
            EnablePassive(passive, localPlayer);
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

        // ---

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
                // Resolve single effect
                if(pendingEffects.Count(x => x.triggerState == EffectTriggerState.Immediate) == 1)
                {
                    for(int i = 0; i < pendingEffects.Count; i++)
                    {
                        if(pendingEffects[i].triggerState != EffectTriggerState.Immediate)
                            continue;
                        yield return ResolveEffectAtIndex(i);
                    }
                }

                // Resolve multiple effects (choose from list)
                while(pendingEffects.Count(x => x.triggerState == EffectTriggerState.Immediate) > 0)
                {
                    yield return null;
                    bool effectDone = false;
                    List<MultipleChoiceWindow.MultipleChoiceEntryData> multipleChoiceEntries = new();
                    for(int i = 0; i < pendingEffects.Count; i++)
                    {
                        if(pendingEffects[i].triggerState != EffectTriggerState.Immediate)
                            continue;
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
                yield return null;
                pendingEffects = pendingEffects.Where(x => x.triggerState != EffectTriggerState.Immediate).ToList();
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
            if(!string.IsNullOrWhiteSpace(pendingEffect.condition) && !SVEFormulaParser.ParseValueAsCondition(pendingEffect.condition, localPlayer, cardObject))
            {
                onComplete?.Invoke();
                return;
            }

            IsResolvingEffect = true;
            if(pendingEffect.costs == null || pendingEffect.costs.Count == 0)
            {
                Resolve();
            }
            else
            {
                Debug.Assert(cardObject, $"Failed to find card with instance ID {pendingEffect.sourceCardInstanceId} in zone {pendingEffect.sourceCardZone} for ability {pendingEffect.abilityName}");
                bool canPayCost = localPlayer.LocalEvents.CanPayCosts(cardObject.RuntimeCard, pendingEffect.costs, pendingEffect.abilityName);

                // Skip prompt if all costs are internal
                if(pendingEffect.costs.All(x => x is SveCost { IsInternalCost: true }))
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
                        text = canPayCost ? "Pay Cost" : "Cannot Pay Cost",
                        onSelect = () =>
                        {
                            GameUIManager.NetworkedCalls.CmdCloseOpponentTargeting(localPlayer.GetOpponentInfo().netId);
                            ResolveWithCost();
                        }
                    },
                    new MultipleChoiceWindow.MultipleChoiceEntryData
                    {
                        text = "Decline",
                        onSelect = () =>
                        {
                            IsResolvingEffect = false;
                            onComplete?.Invoke();
                        }
                    },
                };
                GameUIManager.MultipleChoice.Open(localPlayer, cardObject.LibraryCard.name, costOptions, pendingEffect.effect.text);
                GameUIManager.MultipleChoice.SetButtonActive(0, canPayCost);
            }

            // ---

            void Resolve()
            {
                localPlayer.AdditionalStats.AbilitiesUsedThisTurn.Add(new PlayedAbilityData(pendingEffect.sourceCardInstanceId, cardObject.LibraryCard.id, pendingEffect.abilityName));
                pendingEffect.effect.Resolve(localPlayer, pendingEffect.triggeringCardInstanceId, pendingEffect.triggeringCardZone,
                    pendingEffect.sourceCardInstanceId, pendingEffect.sourceCardZone, () =>
                {
                    IsResolvingEffect = false;
                    onComplete?.Invoke();
                });
            }
            void ResolveWithCost()
            {
                localPlayer.AdditionalStats.AbilitiesUsedThisTurn.Add(new PlayedAbilityData(pendingEffect.sourceCardInstanceId, cardObject.LibraryCard.id, pendingEffect.abilityName));
                localPlayer.LocalEvents.PayAbilityCosts(cardObject, pendingEffect.costs, pendingEffect.effect, pendingEffect.abilityName, Resolve);
            }
        }

        /// <summary>
        /// Resolves an effect immediately without going through confirmation timing and the effect pool
        /// </summary>
        public void ResolveEffectImmediate(SveEffect effect, RuntimeCard card, string zoneName = "Resolution", Action onComplete = null, bool useLocalPlayer = true)
        {
            IsResolvingEffect = true;
            effect.Resolve(useLocalPlayer ? localPlayer : opponentPlayer, card.instanceId, zoneName, card.instanceId, zoneName, () =>
            {
                IsResolvingEffect = false;
                onComplete?.Invoke();
            });
        }

        // ---

        [Command(requiresAuthority = false)]
        private void CmdSetConfirmationTimingState(ConfirmationTimingState newState)
        {
            confirmationTimingState = newState;
        }

        private void ConfirmationTimingSyncVarHook(ConfirmationTimingState oldState, ConfirmationTimingState newState)
        {
            if(oldState == ConfirmationTimingState.Idle && newState == ConfirmationTimingState.ResolvingTurnPlayer)
            {
                OnNextConfirmationTimingStart?.Invoke();
                OnConfirmationTimingStartConstant?.Invoke();
                OnNextConfirmationTimingStart = null;
            }
            else if(oldState == ConfirmationTimingState.FinishedNonTurnPlayer && newState == ConfirmationTimingState.Idle)
            {
                OnNextConfirmationTimingEnd?.Invoke();
                OnConfirmationTimingEndConstant?.Invoke();
                OnNextConfirmationTimingEnd = null;
                localPlayer.InputController.allowedInputs = localPlayer.isActivePlayer ? PlayerInputController.InputTypes.All : PlayerInputController.InputTypes.None;
                if(localPlayer && !SVEQuickTimingController.Instance.IsActive)
                {
                    localPlayer.ZoneController.handZone.SetAllCardsInteractable(localPlayer.isActivePlayer);
                    localPlayer.ZoneController.fieldZone.SetAllCardsInteractable(localPlayer.isActivePlayer);
                    if(localPlayer.isActivePlayer)
                        foreach(CardObject card in localPlayer.ZoneController.fieldZone.GetAllPrimaryCards())
                            card.CalculateCanAttackStatus(updateHighlightMode: false);
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

            // Apply during next confirmation timing to wait for other effects to resolve
            //   (i.e. don't apply the passive before we finish playing the card to the field)
            OnNextConfirmationTimingStart += () =>
            {
                if(!localPlayer.ZoneController.fieldZone.ContainsCard(card) && !opponentPlayer.ZoneController.fieldZone.ContainsCard(card))
                    return;

                foreach(RegisteredPassiveAbility registeredPassive in registeredPassives)
                {
                    if((registeredPassive.duration == SVEProperties.PassiveDuration.OpponentTurn && (card.ownerPlayer == localPlayer.GetPlayerInfo()) == localPlayer.isActivePlayer)
						|| registeredPassive.effect is MinusCostOtherPassive
                        || !registeredPassive.filters.MatchesCard(card)
                        || registeredPassive.affectedCards.Contains(card))
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
                if(!passive.filters.MatchesCard(card))
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
        public SVEEffectPool.EffectTriggerState triggerState;

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
    public struct RegisteredPassiveAbility : IEquatable<RegisteredPassiveAbility>
    {
        public int sourceCardInstanceId;
        public string targetsFormula;
        public Dictionary<CardFilterSetting, string> filters;
        public SvePassiveEffect effect;
        public List<RuntimeCard> affectedCards;
        public SVEProperties.SVEEffectTarget target;
        public SVEProperties.PassiveDuration duration;

        // ------------------------------

        public bool Equals(RegisteredPassiveAbility other)
        {
            return sourceCardInstanceId == other.sourceCardInstanceId
                && targetsFormula == null ? other.targetsFormula == null : targetsFormula.Equals(other.targetsFormula)
                && effect.GetType() == other.effect.GetType()
                && affectedCards.Count == other.affectedCards.Count
                && target == other.target
                && duration == other.duration;
        }

        public override bool Equals(object obj) => obj is RegisteredPassiveAbility other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(sourceCardInstanceId, targetsFormula, effect, (int)target, (int)duration);
    }
}
