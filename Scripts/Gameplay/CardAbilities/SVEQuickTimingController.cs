using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using SVESimulator.UI;

namespace SVESimulator
{
    public class SVEQuickTimingController : NetworkBehaviour
    {
        #region Variables

        public static SVEQuickTimingController Instance;

        private enum QuickTimingState { Inactive, WaitingForEffect, PerformingEffect, Complete }

        // ---

        [Header("Runtime Data"), SerializeField]
        private PlayerController localPlayer;
        [SerializeField]
        private PlayerController opponentPlayer;
        [SerializeField]
        private PlayerInputController localInputController;
        [SerializeField, SyncVar]
        private QuickTimingState quickTimingState = QuickTimingState.Inactive;

        [Header("Settings"), SerializeField]
        private float quickDuration = 15f;

        public bool IsActive => quickTimingState == QuickTimingState.Inactive;

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
            localInputController = FindObjectOfType<PlayerInputController>();

            Debug.Assert(localPlayer);
            Debug.Assert(opponentPlayer);
            Debug.Assert(localInputController);
        }

        #endregion

        // ------------------------------

        #region Start Quick Timing

        public void CallQuickTimingCombat(CardObject attackingCard, CardObject defendingCard, Action onComplete = null)
        {
            if(quickTimingState != QuickTimingState.Inactive)
            {
                Debug.LogError($"Attempted to execute quick timing (combat) while quick timing is already running in state {quickTimingState}");
                return;
            }
            if(localPlayer.isActivePlayer)
            {
                localPlayer.ZoneController.fieldZone.RemoveAllCardHighlights();
                localPlayer.ZoneController.fieldZone.SetAllCardsInteractable(false);
            }

            CmdExecuteQuickTiming(true);
            CmdPlayAttackPreview(attackingCard.RuntimeCard.instanceId, defendingCard.RuntimeCard.instanceId);
            StartCoroutine(WaitForQuickTimingComplete(() =>
            {
                if(localPlayer.isActivePlayer)
                {
                    localPlayer.ZoneController.fieldZone.HighlightCardsCanAttack();
                    localPlayer.ZoneController.fieldZone.SetAllCardsInteractable(true);
                }
                onComplete?.Invoke();
            }));
        }

        public void CallQuickTimingEndOfTurn(Action onComplete = null)
        {
            if(quickTimingState != QuickTimingState.Inactive)
            {
                Debug.LogError($"Attempted to execute quick timing (end of turn) while quick timing is already running in state {quickTimingState}");
                return;
            }
            if(localPlayer.isActivePlayer)
            {
                localPlayer.ZoneController.fieldZone.RemoveAllCardHighlights();
                localPlayer.ZoneController.fieldZone.SetAllCardsInteractable(false);
            }

            CmdExecuteQuickTiming(false);
            StartCoroutine(WaitForQuickTimingComplete(onComplete));
        }

        #endregion

        // ------------------------------

        #region Run Quick Timing Logic

        private IEnumerator WaitForQuickTimingComplete(Action onComplete)
        {
            yield return new WaitUntil(() => quickTimingState != QuickTimingState.Inactive);
            yield return new WaitUntil(() => quickTimingState == QuickTimingState.Inactive);
            yield return new WaitForSeconds(0.1f); // test delay
            onComplete?.Invoke();
        }

        [Command(requiresAuthority = false)]
        private void CmdExecuteQuickTiming(bool isCombat)
        {
            // TODO - if opponent can never respond (no quick on field, no cards in hand), skip this whole thing
            StartCoroutine(QuickTimingNetworkCoroutine());

            IEnumerator QuickTimingNetworkCoroutine()
            {
                NetworkConnectionToClient turnPlayer = GetTurnPlayer().netIdentity.connectionToClient;
                NetworkConnectionToClient nonTurnPlayer = GetNonTurnPlayer().netIdentity.connectionToClient;

                quickTimingState = QuickTimingState.WaitingForEffect;
                TargetQuickTimingPlayer(nonTurnPlayer, isCombat);
                TargetOpenWaitForQuickTimingUI(turnPlayer); // TODO - timer on turn-player's client
                yield return new WaitForSeconds(0.1f); // test delay

                yield return new WaitUntil(() => quickTimingState == QuickTimingState.Complete);
                yield return new WaitForSeconds(0.1f); // test delay

                quickTimingState = QuickTimingState.Inactive;
                TargetCloseQuickTimingUI(turnPlayer);
                CmdEndAttackPreview();
            }
        }

        [TargetRpc]
        private void TargetQuickTimingPlayer(NetworkConnectionToClient networkConnection, bool isCombat)
        {
            StartCoroutine(QuickTimingPlayerCoroutine());

            IEnumerator QuickTimingPlayerCoroutine()
            {
                // Init/open UI
                startQuickTiming:
                GameUIManager.QuickTiming.OpenPerformQuickUI();
                GameUIManager.QuickTiming.SetSubtitle(isCombat ? "Combat" : "End Phase");
                localPlayer.ZoneController.handZone.SetValidQuicksInteractable();
                localPlayer.ZoneController.handZone.HighlightValidQuicks();
                localInputController.allowedInputs = PlayerInputController.InputTypes.PlayCards | PlayerInputController.InputTypes.Quick;

                // Run timer
                int currentPlayedCardCount = localPlayer.Combo;
                for(float i = 0f; i <= quickDuration; i += Time.deltaTime)
                {
                    // TODO - quick abilities
                    if(GameUIManager.QuickTiming.WasCanceled || localPlayer.Combo > currentPlayedCardCount)
                        break;
                    GameUIManager.QuickTiming.SetTimer(1f - (i / quickDuration));
                    yield return null;
                }

                // Close UI
                localPlayer.ZoneController.handZone.SetAllCardsInteractable(false);
                localPlayer.ZoneController.handZone.RemoveAllCardHighlights();
                localInputController.allowedInputs = PlayerInputController.InputTypes.None;
                GameUIManager.QuickTiming.CloseAll();

                // Loop if we played a card
                if(localPlayer.Combo > currentPlayedCardCount)
                {
                    yield return new WaitForSeconds(0.25f); // test delay
                    yield return new WaitUntil(() => !SVEEffectPool.Instance.IsResolvingEffect);
                    yield return new WaitForSeconds(0.25f); // test delay
                    if(isCombat)
                        CardManager.Animator.SetTargetingLineActive(true);
                    goto startQuickTiming;
                }

                // End
                yield return new WaitForSeconds(0.1f); // test delay
                CmdSetQuickTimingState(QuickTimingState.Complete);
            }
        }

        [Command(requiresAuthority = false)]
        private void CmdSetQuickTimingState(QuickTimingState newState)
        {
            quickTimingState = newState;
        }

        #endregion

        // ------------------------------

        #region UI/Animation RPCs

        [TargetRpc]
        private void TargetOpenWaitForQuickTimingUI(NetworkConnectionToClient networkConnection)
        {
            localPlayer.ZoneController.handZone.SetAllCardsInteractable(false);
            localPlayer.ZoneController.handZone.RemoveAllCardHighlights();
            GameUIManager.QuickTiming.OpenWaitingOnQuickUI();
        }

        [TargetRpc]
        private void TargetCloseQuickTimingUI(NetworkConnectionToClient networkConnection)
        {
            GameUIManager.QuickTiming.CloseAll();
            if(localPlayer.isActivePlayer && localPlayer.CurrentPhase == SVEProperties.GamePhase.Main)
            {
                localInputController.allowedInputs = PlayerInputController.InputTypes.All;
                localPlayer.ZoneController.fieldZone.HighlightCardsCanAttack();
                localPlayer.ZoneController.handZone.SetAllCardsInteractable(true);
            }
        }

        [TargetRpc]
        private void TargetStartTimerUI(NetworkConnectionToClient networkConnection)
        {
            StartCoroutine(UITimer());
            IEnumerator UITimer()
            {
                for(float i = 0f; i <= quickDuration; i += Time.deltaTime)
                {
                    if(!GameUIManager.QuickTiming.gameObject.activeInHierarchy)
                        yield break;
                    GameUIManager.QuickTiming.SetTimer(1f - (i / quickDuration));
                    yield return null;
                }
            }
        }

        // -----

        [Command(requiresAuthority = false)]
        private void CmdPlayAttackPreview(int attackerId, int defenderId) => TargetPlayAttackPreview(GetNonTurnPlayer().netIdentity.connectionToClient, attackerId, defenderId);
        [TargetRpc]
        private void TargetPlayAttackPreview(NetworkConnectionToClient networkConnection, int attackerId, int defenderId)
        {
            PlayerCardZoneController attackerZoneController = localPlayer.OppZoneController;
            CardObject attacker = attackerZoneController.fieldZone.GetAllPrimaryCards().FirstOrDefault(x => x.RuntimeCard.instanceId == attackerId);
            Debug.Assert(attacker, $"Attack Preview: Failed to finder attacker with ID {attackerId}");

            PlayerCardZoneController defenderZoneController = localPlayer.ZoneController;
            CardObject defender = defenderZoneController.fieldZone.GetAllPrimaryCards().FirstOrDefault(x => x.RuntimeCard.instanceId == defenderId);
            if(!defender)
                defender = defenderZoneController.leaderZone.AllCards.FirstOrDefault(x => x.RuntimeCard.instanceId == defenderId);
            Debug.Assert(defender, $"Attack Preview: Failed to finder defender with ID {defenderId}");

            CardManager.Animator.PlayAttackPreview(attacker, defender);
        }

        [Command(requiresAuthority = false)]
        private void CmdEndAttackPreview() => TargetEndAttackPreview(GetNonTurnPlayer().netIdentity.connectionToClient);
        [TargetRpc]
        private void TargetEndAttackPreview(NetworkConnectionToClient networkConnection)
        {
            CardManager.Animator.EndAttackPreview();
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
}
