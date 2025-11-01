using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CCGKit;
using UnityEngine;
using Sirenix.OdinInspector;

namespace SVESimulator
{
    public class SVESimServer : Server
    {
        public bool ExtraTurn { get; set; }

        protected override void AddServerHandlers()
        {
            // Base CCG Kit Handlers, minus base TurnSequence
            handlers.Add(new PlayerRegistrationHandler(this));
            handlers.Add(new EffectSolverHandler(this));

            // SVE Handlers
            handlers.Add(new SVETurnSequenceHandler(this));
            handlers.Add(new SVEMoveCardHandler(this));
            handlers.Add(new SVECombatHandler(this));
        }

        // ------------------------------

        /// <summary>
        /// Starts the multiplayer game. This is automatically called when the appropriate number of players have joined a room.
        /// Largely copied over from base.StartGame() but with modifications
        /// </summary>
        public override void StartGame()
        {
            Debug.Log("Game has started."); // modified -> use Unity Debug

            currentTurn = 1;
            List<PlayerInfo> players = gameState.players;
            string[] playerNicknames = players.Select(x => x.nickname).ToArray();

            // Set the current player and opponents.
            gameState.currentPlayer = players[currentPlayerIndex];
            gameState.currentOpponent = players.Find(x => x != gameState.currentPlayer);

            var rngSeed = System.Environment.TickCount;
            effectSolver = new SVEEffectSolver(gameState, rngSeed); // modified -> use SVEEffectSolver

            foreach(PlayerInfo player in players)
            {
                effectSolver.SetTriggers(player);
                foreach(var zone in player.zones)
                {
                    foreach(RuntimeCard card in zone.Value.cards)
                    {
                        effectSolver.SetDestroyConditions(card);
                        effectSolver.SetTriggers(card);
                    }
                }
            }

            // Execute the game start actions.
            foreach(GameAction action in GameManager.Instance.config.properties.gameStartActions)
                ExecuteGameAction(action);

            // Send a StartGame message to all the connected players.
            for(int i = 0; i < players.Count; i++)
            {
                PlayerInfo player = players[i];
                StartGameMessage msg = new()
                {
                    recipientNetId = player.netId,
                    playerIndex = i,
                    turnDuration = turnDuration,
                    nicknames = playerNicknames,
                    player = GetPlayerNetworkState(player),
                    opponent = GetOpponentNetworkState(players.Find(x => x != player)),
                    rngSeed = rngSeed
                };
                SafeSendToClient(player, msg);
            }

            // Start running the turn sequence coroutine.
            turnCoroutine = StartCoroutine(RunTurn());
        }

        protected override IEnumerator RunTurn()
        {
            ExtraTurn = false;
            StartTurn();
            yield return null;
            // Do not wait on a timer here like base CCG kit - want to enter end phase manually, never on a timer like in base CCG kit
        }

        public void StopTurnWithExtraTurn()
        {
            if(turnCoroutine != null)
                StopCoroutine(turnCoroutine);
            EndTurn();

            currentPlayerIndex--;
            if(currentPlayerIndex < 0)
                currentPlayerIndex = gameState.players.Count - 1;

            turnCoroutine = StartCoroutine(RunTurn());
        }
    }
}
