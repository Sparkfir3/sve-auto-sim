using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CCGKit;
using Mirror;
using UnityEngine;
using SVESimulator.UI;
using Random = UnityEngine.Random;

namespace SVESimulator
{
    /// <summary>
    /// Class representing each player
    /// Handles receiving of messages
    /// </summary>
    public class PlayerController : Player
    {
        #region Variables

        [Header("Runtime Data"), SyncVar]
        public int CurrentTurnNumber;
        [SyncVar]
        public bool EvolvedThisTurn;
        [SyncVar, SerializeField]
        private int damageTakenThisTurn;
        [SyncVar(hook = nameof(SyncHook_OnDeckCountChanged)), SerializeField]
        private int cardsInDeck;
        [SyncVar(hook = nameof(SyncHook_OnCemeteryCountChanged)), SerializeField]
        private int cardsInCemetery;
        [SyncVar(hook = nameof(SyncHook_OnSpellchainChanged)), SerializeField]
        private int spellchain;
        [SyncVar(hook = nameof(SyncHook_OnEvolveDeckFaceDownCountChanged)), SerializeField]
        private int cardsInEvolveDeckFaceDown;
        [SyncVar(hook = nameof(SyncHook_OnEvolveDeckFaceUpCountChanged)), SerializeField]
        private int cardsInEvolveDeckFaceUp;
        [SyncVar(hook = nameof(SyncHook_OnBanishedZoneCountChanged)), SerializeField]
        private int cardsInBanishedZone;

        public int Combo => AdditionalStats.CardsPlayedThisTurn.Count;
        public int Spellchain => spellchain;
        public bool Overflow => localMaxPlayPointStat != null && localMaxPlayPointStat.effectiveValue >= 7;
        public int Necrocharge => cardsInCemetery;
        public bool Sanguine => damageTakenThisTurn > 0;

        [Header("Runtime References"), SerializeField, ReadOnly]
        private PlayerCardZoneController localPlayerZoneController;
        [SerializeField, ReadOnly]
        private PlayerCardZoneController opponentPlayerZoneController;

        [field: Header("Object References"), SerializeField]
        public PlayerEventControllerLocal LocalEvents { get; private set; }
        [field: SerializeField]
        public PlayerEventControllerOpponent OpponentEvents { get; private set; }
        [field: SerializeField]
        public AdditionalPlayerStats AdditionalStats { get; private set; }
        [SerializeField]
        private PlayerInputController inputController;

        public PlayerCardZoneController ZoneController => localPlayerZoneController;
        public PlayerCardZoneController OppZoneController => opponentPlayerZoneController;
        public PlayerInputController InputController => inputController;
        protected SVEEffectSolver sveEffectSolver => effectSolver as SVEEffectSolver;

        public SVEProperties.GamePhase CurrentPhase => gameState.currentPhase;

        protected Stat localPlayPointStat;
        protected Stat localMaxPlayPointStat;
        protected Stat opponentPlayPointStat;

        // Events
        public event Action<int> OnCardsInDeckChanged;
        public event Action<int> OnCardsInCemeteryChanged;
        public event Action<int> OnSpellchainChanged;
        public event Action<int> OnCardsInEvolveDeckFaceDownChanged;
        public event Action<int> OnCardsInEvolveDeckFaceUpChanged;
        public event Action<int> OnCardsInBanishedZoneChanged;

        public event Action<bool> OnStartLocalTurn; // bool = increment turn number
        public event Action<bool> OnStartOpponentTurn;
        public event Action onEndGameEvent;

        #endregion

        // ------------------------------

        #region Unity Functions

        protected override void Awake()
        {
            base.Awake();
            isHuman = true;
            localPlayerZoneController = FieldManager.PlayerZones;
            opponentPlayerZoneController = FieldManager.OpponentZones;
        }

        #endregion

        // ------------------------------

        #region General Game Functions

        public override void StopTurn()
        {
            if(!isActivePlayer)
                return;
            base.StopTurn();
        }

        public IEnumerator StopTurnOnDelay(float delay = 0.5f)
        {
            if(!isActivePlayer)
                yield break;
            yield return new WaitForSecondsRealtime(delay);
            if(isActivePlayer)
                StopTurn(); // this is needed because Invoke() doesn't work because of the override wtf
        }

        #endregion

        // ------------------------------

        #region Networking Events - Game Flow

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();

            localPlayerZoneController.InitializeZones(this, playerInfo, !netIdentity.isClientOnly);
            playerInfo.namedStats[SVEProperties.PlayerStats.Defense].onValueChanged += (oldValue, newValue) =>
            {
                int difference = newValue - oldValue;
                if(difference < 0)
                    damageTakenThisTurn += -difference;
            };
            FieldManager.PlayerLeaderHealth.Initialize(playerInfo.namedStats[SVEProperties.PlayerStats.Defense]);

            if(isLocalPlayer && !inputController)
                inputController = FindObjectOfType<PlayerInputController>();
            if(inputController)
                inputController.allowedInputs = PlayerInputController.InputTypes.None;
        }

        public override void OnStartGame(StartGameMessage msg)
        {
            // base.OnStartGame(), modified to use SVEEffectSolver
            gameStarted = true;
            playerIndex = msg.playerIndex;
            turnDuration = msg.turnDuration;

            effectSolver = new SVEEffectSolver(gameState, msg.rngSeed);
            effectSolver.SetTriggers(playerInfo);
            effectSolver.SetTriggers(opponentInfo);
            sveEffectSolver.isPlayerEffectSolver = true;

            LoadPlayerStates(msg.player, msg.opponent);

            // Custom logic
            SVEEffectPool.Instance.LocalInitialize();
            SVEQuickTimingController.Instance.LocalInitialize();

            PlayerController[] controllers = FindObjectsOfType<PlayerController>();
            opponentPlayerZoneController.InitializeZones(controllers.FirstOrDefault(x => !x.isLocalPlayer), opponentInfo, !opponentInfo.netId.isClientOnly);
            FieldManager.OpponentLeaderHealth.Initialize(opponentInfo.namedStats[SVEProperties.PlayerStats.Defense]);

            AdditionalStats.Initialize(this);
            CurrentTurnNumber = 0;
            EvolvedThisTurn = false;
            damageTakenThisTurn = 0;
            localPlayPointStat = playerInfo.namedStats[SVEProperties.PlayerStats.PlayPoints];
            localMaxPlayPointStat = playerInfo.namedStats[SVEProperties.PlayerStats.MaxPlayPoints];
            opponentPlayPointStat = opponentInfo.namedStats[SVEProperties.PlayerStats.PlayPoints];

            LocalEvents.Initialize(playerInfo, opponentInfo, localPlayPointStat, sveEffectSolver, netIdentity);
            OpponentEvents.Initialize(playerInfo, opponentInfo, opponentPlayPointStat, sveEffectSolver, opponentInfo.netId);

            LocalEvents.InitializeDeckAndLeader();
            InitializePlayPointMeters();
            GameUIManager.Instance.Initialize(this);
            GameUIManager.GameControlsUI.SetPhase(SVEProperties.GamePhase.Setup);
        }

        public override void OnStartTurn(StartTurnMessage msg)
        {
            base.OnStartTurn(msg);

            gameState.currentPlayer.numTurn++;
            if(HandleGameSetupTurn(msg, out bool endTurn))
            {
                if(endTurn && msg.isRecipientTheActivePlayer)
                    StopTurn();
                return;
            }

            // -----

            // Set up turn
            if(msg.isRecipientTheActivePlayer)
                OnStartLocalTurn?.Invoke(playerInfo.isGoingFirst);
            else
                OnStartOpponentTurn?.Invoke(!playerInfo.isGoingFirst);
            inputController.allowedInputs = msg.isRecipientTheActivePlayer ? PlayerInputController.InputTypes.All : PlayerInputController.InputTypes.None;
            AdditionalStats.Reset();
            EvolvedThisTurn = false;
            damageTakenThisTurn = 0;
            SVEEffectPool.Instance.UpdatePassiveDurationsStartOfTurn(this, msg.isRecipientTheActivePlayer);
            LocalEvents.SetGamePhase(SVEProperties.GamePhase.Main); // TODO - start phase

            // Failsafe Calls
            GameUIManager.EffectTargeting.CloseOpponentIsTargeting();
            LocalEvents.OnFinishSpell = null;

            // Start turn
            if(msg.isRecipientTheActivePlayer)
            {
                Debug.Log("Turn starting");

                CurrentTurnNumber++;
                LocalEvents.IncrementMaxPlayPoints(updateCurrentPoints: true);
                ReserveAllCardsOnField();
                foreach(CardObject card in localPlayerZoneController.fieldZone.AllCards)
                    card.OnStartTurn();
                localPlayerZoneController.fieldZone.HighlightCardsCanAttack();
                localPlayerZoneController.fieldZone.SetAllCardsInteractable(true);
                localPlayerZoneController.exAreaZone.SetAllCardsInteractable(true);
                localPlayerZoneController.handZone.SetAllCardsInteractable(true);
                if(CurrentTurnNumber > 1 || !playerInfo.isGoingFirst)
                    LocalEvents.DrawCard();
            }
            else
            {
                Debug.Log("Opponent turn starting");

                foreach(CardObject card in opponentPlayerZoneController.fieldZone.AllCards)
                    card.OnStartTurn();
            }

            // Start of main phase
            if(msg.isRecipientTheActivePlayer)
                SVEEffectPool.Instance.TriggerPendingEffectsForOtherCardsInZone<SveStartMainPhaseTrigger >(gameState, null, localPlayerZoneController.fieldZone.Runtime,
                    playerInfo, _ => true, true);
            else
                SVEEffectPool.Instance.TriggerPendingEffectsForOtherCardsInZone<SveStartOpponentMainPhaseTrigger>(gameState, null, localPlayerZoneController.fieldZone.Runtime,
                    playerInfo, _ => true, true);
        }

        public override void OnEndTurn(EndTurnMessage msg)
        {
            base.OnEndTurn(msg);
            localPlayerZoneController.fieldZone.RemoveAllCardHighlights();
        }

        public override void OnEndGame(EndGameMessage msg)
        {
            base.OnEndGame(msg);
            if(msg.winnerPlayerIndex == netIdentity)
                GameUIManager.WinLoseDisplay.WinGame();
            else
                GameUIManager.WinLoseDisplay.LoseGame();

            onEndGameEvent?.Invoke();
        }

        #endregion

        // ------------------------------

        #region Non-Network Game Setup

        private bool HandleGameSetupTurn(StartTurnMessage msg, out bool endTurn)
        {
            endTurn = false;
            switch(gameState.currentPlayer.numTurn)
            {
                case 1: // pass turn - need to do this for init evolve deck/leader to work (for some reason...)
                    endTurn = msg.isRecipientTheActivePlayer;
                    return true;

                case 2: // determine first/second
                    if(msg.isRecipientTheActivePlayer && playerInfo.isGoingFirstDecided == false)
                    {
                        if(IsHostTurn()) // host's turn
                        {
                            // 50-50 chance this player chooses who goes first, otherwise other player chooses
                            if(Random.Range(0f, 1f) < 0.5f)
                            {
                                GameUIManager.GoingFirstScreen.SetDisplayMode(SelectGoingFirstScreen.Mode.LocalSelectGoingFirst);
                                GameUIManager.NetworkedCalls.TargetRpcShowOpponentChoosingFirst(opponentInfo.netId.connectionToClient);
                            }
                            else
                            {
                                GameUIManager.GoingFirstScreen.SetDisplayMode(SelectGoingFirstScreen.Mode.OpponentSelectGoingFirst);
                                endTurn = true;
                            }
                        }
                        else // non-host's turn
                        {
                            GameUIManager.GoingFirstScreen.SetDisplayMode(SelectGoingFirstScreen.Mode.LocalSelectGoingFirst);
                        }
                    }
                    else
                        endTurn = true;
                    return true;

                case 3: // draw 4
                    endTurn = msg.isRecipientTheActivePlayer;
                    if(msg.isRecipientTheActivePlayer)
                    {
                        DrawStartingHand();
                    }
                    return true;

                case 4: // mulligan - first player
                    if(msg.isRecipientTheActivePlayer && playerInfo.isGoingFirst)
                    {
                        GameUIManager.MulliganScreen.ShowLocalMulligan();
                        GameUIManager.NetworkedCalls.CmdShowOpponentMulligan(opponentInfo.netId);
                    }
                    else
                        endTurn = true;
                    return true;
                case 5: // mulligan - second player
                    if(msg.isRecipientTheActivePlayer && !playerInfo.isGoingFirst)
                    {
                        GameUIManager.MulliganScreen.ShowLocalMulligan();
                        GameUIManager.NetworkedCalls.CmdShowOpponentMulligan(opponentInfo.netId);
                    }
                    else
                        endTurn = true;
                    return true;

                case 6: // actual game - skip host if they are not first (opponent takes first turn)
                    GameUIManager.MulliganScreen.Close();
                    if(msg.isRecipientTheActivePlayer && IsHostTurn() && !playerInfo.isGoingFirst)
                    {
                        endTurn = true;
                        return true;
                    }
                    return false;
                default:
                    return false;
            }

            bool IsHostTurn() => gameState.currentPlayer.numTurn != gameState.currentOpponent.numTurn;
        }

        public void DrawStartingHand(float delayBeforeDrawing = 0f, float delayBetweenCards = 0.1f)
        {
            StartCoroutine(DrawStartingHandCoroutine());
            IEnumerator DrawStartingHandCoroutine()
            {
                yield return new WaitForSeconds(delayBeforeDrawing);
                for(int i = 0; i < SVEProperties.StartingHandSize; i++)
                {
                    LocalEvents.DrawCard();
                    yield return new WaitForSeconds(delayBetweenCards);
                }
            }
        }

        #endregion

        // ------------------------------

        #region Non-Network Game Controls

        public void ReserveAllCardsOnField()
        {
            List<CardObject> cardsToReserve = localPlayerZoneController.fieldZone.GetAllPrimaryCards();
            foreach(CardObject card in cardsToReserve)
            {
                if(!card || card.RuntimeCard == null)
                    continue;
                LocalEvents.ReserveCard(card.RuntimeCard);
            }
        }

        public void EnterEndPhase()
        {
            if(gameState.currentPhase != SVEProperties.GamePhase.Main)
                return;
            StartCoroutine(RunEndPhase());

            IEnumerator RunEndPhase()
            {
                LocalEvents.SetGamePhase(SVEProperties.GamePhase.End);
                ZoneController.fieldZone.RemoveAllCardHighlights();

                bool waiting = true;
                SVEEffectPool.Instance.OnNextConfirmationTimingEnd += () =>
                {
                    waiting = false;
                };
                SVEEffectPool.Instance.TriggerPendingEffectsForOtherCardsInZone<SveStartEndPhaseTrigger>(gameState, null, localPlayerZoneController.fieldZone.Runtime, playerInfo,
                    _ => true, true);
                yield return new WaitUntil(() => !waiting);

                SelectWardCardsToEngage(() =>
                {
                    inputController.allowedInputs = PlayerInputController.InputTypes.None;
                    localPlayerZoneController.fieldZone.RemoveAllCardHighlights();

                    SVEQuickTimingController.Instance.CallQuickTimingEndOfTurn(() =>
                    {
                        localPlayerZoneController.fieldZone.RemoveAllCardHighlights();
                        DiscardForHandSize(() =>
                        {
                            StartCoroutine(StopTurnOnDelay(0.1f));
                        });
                    });
                });
            }
        }

        public void SelectWardCardsToEngage(Action onComplete = null)
        {
            if(!ZoneController.fieldZone.GetAllPrimaryCards().Any(x => x.RuntimeCard.HasKeyword(SVEProperties.Keywords.Ward) && !x.Engaged))
            {
                onComplete?.Invoke();
                return;
            }

            EffectTargetingUI.TargetCard.SetText("End Phase: Select followers with ward to engage");
            EffectTargetingUI.TargetCard.Open(this, "k(Ward)m(0,5)R", new List<string> { SVEProperties.Zones.Field }, null);
            EffectTargetingUI.TargetCard.OnSelectionComplete.AddListener(cards =>
            {
                foreach(CardObject card in cards)
                    LocalEvents.EngageCard(card.RuntimeCard);
                onComplete?.Invoke();
            });
        }

        public void DiscardForHandSize(Action onComplete = null)
        {
            if(ZoneController.handZone.AllCards.Count <= SVEProperties.MaxHandSize)
            {
                onComplete?.Invoke();
                return;
            }
            StartCoroutine(HandSizeCoroutine());
            IEnumerator HandSizeCoroutine()
            {
                do
                {
                    // Discard
                    bool waiting = true;
                    int handSize = ZoneController.handZone.AllCards.Count;
                    ZoneController.selectionArea.Enable(CardSelectionArea.SelectionMode.PlaceCardsFromHand, handSize - SVEProperties.MaxHandSize, handSize - SVEProperties.MaxHandSize);
                    ZoneController.selectionArea.SetFilter(null);
                    ZoneController.selectionArea.SetConfirmAction(null,
                        "Discard",
                        "Discard for Hand Size",
                        1, handSize - SVEProperties.MaxHandSize,
                        targets =>
                        {
                            foreach(CardObject target in targets)
                                LocalEvents.SendToCemetery(target, SVEProperties.Zones.Hand);
                            waiting = false;
                        });
                    yield return new WaitUntil(() => !waiting);
                    ZoneController.selectionArea.Disable();
                    yield return new WaitForSeconds(0.1f);

                    // Confirmation Timing
                    waiting = true;
                    SVEEffectPool.Instance.OnNextConfirmationTimingEnd += () => { waiting = false; };
                    SVEEffectPool.Instance.CmdExecuteConfirmationTiming();
                    yield return new WaitUntil(() => !waiting);
                    yield return new WaitForSeconds(0.1f);

                } while(ZoneController.handZone.AllCards.Count > SVEProperties.MaxHandSize);
                onComplete?.Invoke();
            }
        }

        #endregion

        // ------------------------------

        #region Zone Card Counts Network Syncing

        public void SetDeckCount(int count)
        {
            if(isServer)
                cardsInDeck = count;
            else
            {
                int oldCount = cardsInDeck;
                cardsInDeck = count;
                SyncHook_OnDeckCountChanged(oldCount, count); // idk why SyncVar isn't syncing from Server->Client but this manual client-side set fixes/gets around it so fuck it ig
            }
        }

        private void SyncHook_OnDeckCountChanged(int oldCount, int newCount) { OnCardsInDeckChanged?.Invoke(newCount); }

        // -----

        public void SetCemeteryCount(int count)
        {
            if(isServer)
                cardsInCemetery = count;
            else
            {
                int oldCount = cardsInCemetery;
                cardsInCemetery = count;
                SyncHook_OnCemeteryCountChanged(oldCount, count); // See complaint in: SetDeckCount()
            }
        }

        private void SyncHook_OnCemeteryCountChanged(int oldCount, int newCount)
        {
            if(isOwned)
            {
                spellchain = localPlayerZoneController.cemeteryZone.CountOfCardType(SVEProperties.CardTypes.Spell) +
                    (AdditionalStats.UseRuneFollowersForSpellchain ? localPlayerZoneController.cemeteryZone.CountOfCardByFilter("Fc(rune)") : 0);
                if(!isServer)
                    SyncHook_OnSpellchainChanged(spellchain, spellchain); // See complaint in: SetDeckCount()
            }
            OnCardsInCemeteryChanged?.Invoke(newCount);
        }

        private void SyncHook_OnSpellchainChanged(int oldCount, int newCount) { OnSpellchainChanged?.Invoke(newCount); }

        // -----

        public void SetEvolveDeckCount(int faceDownCount, int faceUpCount)
        {
            if(isServer)
            {
                if(cardsInEvolveDeckFaceDown != faceDownCount)
                    cardsInEvolveDeckFaceDown = faceDownCount;
                if(cardsInEvolveDeckFaceUp != faceUpCount)
                    cardsInEvolveDeckFaceUp = faceUpCount;
            }
            else
            {
                if(cardsInEvolveDeckFaceDown != faceDownCount)
                {
                    int oldCountFaceDown = cardsInEvolveDeckFaceDown;
                    cardsInEvolveDeckFaceDown = faceDownCount;
                    SyncHook_OnEvolveDeckFaceDownCountChanged(oldCountFaceDown, faceDownCount);
                }
                if(cardsInEvolveDeckFaceUp != faceUpCount)
                {
                    int oldCountFaceUp = cardsInEvolveDeckFaceUp;
                    cardsInEvolveDeckFaceUp = faceUpCount;
                    SyncHook_OnEvolveDeckFaceUpCountChanged(oldCountFaceUp, faceUpCount);
                }
            }
        }

        private void SyncHook_OnEvolveDeckFaceDownCountChanged(int oldCount, int newCount) { OnCardsInEvolveDeckFaceDownChanged?.Invoke(newCount); }
        private void SyncHook_OnEvolveDeckFaceUpCountChanged(int oldCount, int newCount) { OnCardsInEvolveDeckFaceUpChanged?.Invoke(newCount); }

        // -----

        public void SetBanishedZoneCount(int count)
        {
            if(isServer)
                cardsInBanishedZone = count;
            else
            {
                int oldCount = cardsInBanishedZone;
                cardsInBanishedZone = count;
                SyncHook_OnBanishedZoneCountChanged(oldCount, count); // See complaint in: SetDeckCount()
            }
        }

        private void SyncHook_OnBanishedZoneCountChanged(int oldCount, int newCount) { OnCardsInBanishedZoneChanged?.Invoke(newCount); }

        #endregion

        // ------------------------------

        #region Play Points / Evolve Points

        private void InitializePlayPointMeters()
        {
            // Player
            playerInfo.namedStats[SVEProperties.PlayerStats.MaxPlayPoints].onValueChanged += (oldAmount, newAmount) =>
            {
                OnUpdateMaxPlayPoints(FieldManager.PlayerPlayPoints, newAmount);
            };
            playerInfo.namedStats[SVEProperties.PlayerStats.PlayPoints].onValueChanged += (oldAmount, newAmount) =>
            {
                OnUpdateCurrentPlayPoints(FieldManager.PlayerPlayPoints, newAmount);
            };
            OnUpdateMaxPlayPoints(FieldManager.PlayerPlayPoints, 0);

            // Opponent
            opponentInfo.namedStats[SVEProperties.PlayerStats.MaxPlayPoints].onValueChanged += (oldAmount, newAmount) =>
            {
                OnUpdateMaxPlayPoints(FieldManager.OpponentPlayPoints, newAmount);
            };
            opponentInfo.namedStats[SVEProperties.PlayerStats.PlayPoints].onValueChanged += (oldAmount, newAmount) =>
            {
                OnUpdateCurrentPlayPoints(FieldManager.OpponentPlayPoints, newAmount);
            };
            OnUpdateMaxPlayPoints(FieldManager.OpponentPlayPoints, 0);

            // TODO - maybe need to safe unsubscribe from events?
        }

        private void OnUpdateMaxPlayPoints(PlayPointMeter meter, int amount)
        {
            meter.SetMaxPoints(amount);
        }

        private void OnUpdateCurrentPlayPoints(PlayPointMeter meter, int amount)
        {
            meter.SetCurrentPoints(amount);
        }

        // -----

        public void InitializeEvolvePointDisplays(bool isGoingFirst)
        {
            ZoneController.evolvePointDisplay.Initialize(!isGoingFirst);
            OppZoneController.evolvePointDisplay.Initialize(isGoingFirst);

            EvolvePointDisplay displayToConnect = isGoingFirst ? OppZoneController.evolvePointDisplay : ZoneController.evolvePointDisplay;
            PlayerInfo playerInfoToConnect = isGoingFirst ? opponentInfo : playerInfo;
            playerInfoToConnect.namedStats[SVEProperties.PlayerStats.EvolutionPoints].onValueChanged += (oldAmount, newAmount) =>
            {
                displayToConnect.SetEvolvePointCount(newAmount);
            };
        }

        #endregion

    }
}
