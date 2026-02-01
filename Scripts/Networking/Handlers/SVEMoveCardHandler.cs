using System.Collections.Generic;
using System.Linq;
using CCGKit;
using Mirror;
using UnityEngine;

namespace SVESimulator
{
    public class SVEMoveCardHandler : SVEServerHandlerBase
    {
        #region Initialization

        public SVEMoveCardHandler(Server server) : base(server) { }

        public override void RegisterNetworkHandlers()
        {
            // Game initialization
            NetworkServer.RegisterHandler<SetGoingFirstPlayerMessage>(SetGoingFirst);
            NetworkServer.RegisterHandler<LocalPerformMulliganMessage>(DoMulligan);
            NetworkServer.RegisterHandler<SetMaxPlayPointsMessage>(SetMaxPlayPoints);
            NetworkServer.RegisterHandler<SetCurrentPlayPointsMessage>(SetCurrentPlayPoints);
            NetworkServer.RegisterHandler<LocalInitDeckAndLeaderMessage>(OnInitDeckAndLeader);
            NetworkServer.RegisterHandler<SetGamePhaseMessage>(OnSetGamePhase);

            // Zone Controls
            NetworkServer.RegisterHandler<LocalShuffleDeckMessage>(OnShuffleDeck);

            // Deck movement
            NetworkServer.RegisterHandler<LocalDrawCardMessage>(OnDrawCard);
            NetworkServer.RegisterHandler<LocalTellOppDrawCardMessage>(OnTellOpponentDrawCard);
            NetworkServer.RegisterHandler<LocalTellOppMillDeckMessage>(OnTellOpponentMillDeck);

            // Play cards on field
            NetworkServer.RegisterHandler<LocalPlayCardMessage>(OnPlayCard);
            NetworkServer.RegisterHandler<LocalCreateTokenMessage>(OnCreateToken);
            NetworkServer.RegisterHandler<LocalTransformCardMessage>(OnTransformCard);
            NetworkServer.RegisterHandler<LocalEvolveCardMessage>(OnEvolveCard);

            // Send card to zone
            NetworkServer.RegisterHandler<LocalSendCardToCemeteryMessage>(OnSendCardToCemetery);
            NetworkServer.RegisterHandler<LocalBanishCardMessage>(OnBanishCard);
            NetworkServer.RegisterHandler<LocalDestroyOpponentCardMessage>(OnDestroyOpponentCard);
            NetworkServer.RegisterHandler<LocalReturnToHandMessage>(OnReturnCardToHand);
            NetworkServer.RegisterHandler<LocalSendToBottomDeckMessage>(OnSendCardToBottomDeck);
            NetworkServer.RegisterHandler<LocalSendToTopDeckMessage>(OnSendCardToTopDeck);
            NetworkServer.RegisterHandler<LocalSendToExAreaMessage>(OnSendCardToExArea);

            // Spells and effect costs
            NetworkServer.RegisterHandler<LocalPlaySpellMessage>(OnPlaySpell);
            NetworkServer.RegisterHandler<LocalFinishSpellMessage>(OnFinishSpell);
            NetworkServer.RegisterHandler<LocalPayEffectCostMessage>(OnPayEffectCost);

            // Other
            NetworkServer.RegisterHandler<LocalTellOpponentPerformEffectMessage>(OnTellOpponentPerformEffect);
        }

        public override void UnregisterNetworkHandlers()
        {
            // Game initialization
            NetworkServer.UnregisterHandler<SetGoingFirstPlayerMessage>();
            NetworkServer.UnregisterHandler<LocalPerformMulliganMessage>();
            NetworkServer.UnregisterHandler<SetMaxPlayPointsMessage>();
            NetworkServer.UnregisterHandler<SetCurrentPlayPointsMessage>();
            NetworkServer.UnregisterHandler<LocalInitDeckAndLeaderMessage>();
            NetworkServer.UnregisterHandler<SetGamePhaseMessage>();

            // Zone Controls
            NetworkServer.UnregisterHandler<LocalShuffleDeckMessage>();

            // Deck movement
            NetworkServer.UnregisterHandler<LocalDrawCardMessage>();
            NetworkServer.UnregisterHandler<LocalTellOppDrawCardMessage>();
            NetworkServer.UnregisterHandler<OpponentTellOppMillDeckMessage>();

            // Play cards on field
            NetworkServer.UnregisterHandler<LocalPlayCardMessage>();
            NetworkServer.UnregisterHandler<LocalCreateTokenMessage>();
            NetworkServer.UnregisterHandler<LocalTransformCardMessage>();
            NetworkServer.UnregisterHandler<LocalEvolveCardMessage>();

            // Send card to zone
            NetworkServer.UnregisterHandler<LocalSendCardToCemeteryMessage>();
            NetworkServer.UnregisterHandler<LocalBanishCardMessage>();
            NetworkServer.UnregisterHandler<LocalDestroyOpponentCardMessage>();
            NetworkServer.UnregisterHandler<LocalReturnToHandMessage>();
            NetworkServer.UnregisterHandler<LocalSendToBottomDeckMessage>();
            NetworkServer.UnregisterHandler<LocalSendToTopDeckMessage>();
            NetworkServer.UnregisterHandler<LocalSendToExAreaMessage>();

            // Spells and effect costs
            NetworkServer.UnregisterHandler<LocalPlaySpellMessage>();
            NetworkServer.UnregisterHandler<LocalFinishSpellMessage>();
            NetworkServer.UnregisterHandler<LocalPayEffectCostMessage>();

            // Other
            NetworkServer.UnregisterHandler<LocalTellOpponentPerformEffectMessage>();
        }

        #endregion

        // ------------------------------

        #region Game Initialization

        private void SetGoingFirst(NetworkConnection conn, SetGoingFirstPlayerMessage msg)
        {
            PlayerInfo player = server.gameState.players.Find(x => x.netId == msg.playerNetId);
            player.isGoingFirstDecided = true;
            player.isGoingFirst = true;
            player.namedStats[SVEProperties.PlayerStats.EvolutionPoints].baseValue = 0;

            List<PlayerInfo> otherPlayers = server.gameState.players.FindAll(x => x.netId != msg.playerNetId);
            foreach(PlayerInfo otherPlayer in otherPlayers)
            {
                otherPlayer.isGoingFirstDecided = true;
                otherPlayer.isGoingFirst = false;
                otherPlayer.namedStats[SVEProperties.PlayerStats.EvolutionPoints].baseValue = 3;
            }

            SetGoingFirstPlayerMessage newMsg = new()
            {
                playerNetId = msg.playerNetId
            };
            server.SafeSendToClient(server.gameState.currentOpponent, newMsg);
        }

        private void DoMulligan(NetworkConnection conn, LocalPerformMulliganMessage msg)
        {
            PlayerInfo player = server.gameState.players.Find(x => x.netId == msg.playerNetId);
            RuntimeZone handZone = player.namedZones[SVEProperties.Zones.Hand];
            foreach(int cardId in msg.cardIdsInOrder)
            {
                RuntimeCard card = handZone.cards.Find(x => x.instanceId == cardId);
                (server.effectSolver as SVEEffectSolver).SendCardHandToBottomDeck(msg.playerNetId, card);
                // Re-drawing hand is handled by PlayerController drawing cards
            }

            OpponentPerformMulliganMessage mulliganMsg = new()
            {
                playerNetId = msg.playerNetId
            };
            server.SafeSendToClient(server.gameState.currentOpponent, mulliganMsg);
        }

        private void SetMaxPlayPoints(NetworkConnection conn, SetMaxPlayPointsMessage msg)
        {
            PlayerInfo player = server.gameState.players.Find(x => x.netId == msg.playerNetId);

            (server.effectSolver as SVEEffectSolver).SetMaxPlayPoints(player.netId, msg.maxPlayPoints, msg.updateCurrentPoints);
            server.SafeSendToClient(server.gameState.currentOpponent, msg);
        }

        private void SetCurrentPlayPoints(NetworkConnection conn, SetCurrentPlayPointsMessage msg)
        {
            PlayerInfo player = server.gameState.players.Find(x => x.netId == msg.playerNetId);

            (server.effectSolver as SVEEffectSolver).SetCurrentPlayPoints(player.netId, msg.currentPlayPoints);
            server.SafeSendToClient(server.gameState.currentOpponent, msg);
        }

        private void OnInitDeckAndLeader(NetworkConnection conn, LocalInitDeckAndLeaderMessage msg)
        {
            PlayerInfo player = server.gameState.players.Find(x => x.netId == msg.playerNetId);
            List<RuntimeCard> evolvedCards = player.namedZones[SVEProperties.Zones.Deck].cards.Where(x => msg.evolvedCardsInstanceIds.Contains(x.instanceId)).ToList();
            RuntimeCard leaderCard = player.namedZones[SVEProperties.Zones.Deck].cards.Find(x => x.instanceId == msg.leaderCardInstanceId);
            if(leaderCard == null)
            {
                Debug.LogError($"[Init Deck/Leader] Leader card was null!");
                Debug.LogError(player.namedZones[SVEProperties.Zones.Leader].cards.Count);
                return;
            }

            OpponentInitDeckAndLeaderMessage initMsg = new()
            {
                playerNetId = msg.playerNetId,
                evolveDeckSize = msg.evolvedCardsInstanceIds.Length,
                leaderCard = NetworkingUtils.GetNetCard(leaderCard)
            };
            server.SafeSendToClient(server.gameState.currentOpponent, initMsg);
            foreach(RuntimeCard card in evolvedCards)
            {
                (server.effectSolver as SVEEffectSolver).MoveCard(msg.playerNetId, card, SVEProperties.Zones.Deck, SVEProperties.Zones.EvolveDeck);
            }
            (server.effectSolver as SVEEffectSolver).MoveCard(msg.playerNetId, leaderCard, SVEProperties.Zones.Deck, SVEProperties.Zones.Leader);
        }

        private void OnSetGamePhase(NetworkConnection conn, SetGamePhaseMessage msg)
        {
            server.gameState.currentPhase = msg.phase;
            server.SafeSendToClient(server.gameState.currentOpponent, msg);
        }

        #endregion

        // ------------------------------

        #region Zone Controls


        private void OnShuffleDeck(NetworkConnection conn, LocalShuffleDeckMessage msg)
        {
            OpponentShuffleDeckMessage shuffleMsg = new()
            {
                playerNetId = msg.playerNetId
            };
            server.SafeSendToClient(server.gameState.currentOpponent, shuffleMsg);
            (server.effectSolver as SVEEffectSolver).ShuffleDeck(msg.playerNetId);
        }

        #endregion

        // ------------------------------

        #region Deck Movement

        private void OnDrawCard(NetworkConnection conn, LocalDrawCardMessage msg)
        {
            PlayerInfo player = server.gameState.players.Find(x => x.netId == msg.playerNetId);
            RuntimeZone originZone = player.namedZones[SVEProperties.Zones.Deck];
            RuntimeCard card = originZone.cards.Find(x => x.instanceId == msg.cardInstanceId);
            if(card == null)
            {
                Debug.LogError($"[Draw Card] P{player.id} attempted to draw card w/ instance ID {msg.cardInstanceId} when it does not exist in the player's deck!");
                return;
            }

            OpponentDrawCardMessage cardMovedMsg = new()
            {
                playerNetId = msg.playerNetId,
                card = NetworkingUtils.GetNetCard(card),
                reveal = msg.reveal
            };
            server.SafeSendToClient(server.gameState.currentOpponent, cardMovedMsg);
            (server.effectSolver as SVEEffectSolver).DrawCard(msg.playerNetId, card);
        }

        private void OnTellOpponentDrawCard(NetworkConnection conn, LocalTellOppDrawCardMessage msg)
        {
            OpponentTellOppDrawCardMessage drawMsg = new()
            {
                playerNetId = msg.playerNetId,
                count = msg.count
            };
            server.SafeSendToClient(server.gameState.currentOpponent, drawMsg);
        }

        private void OnTellOpponentMillDeck(NetworkConnection conn, LocalTellOppMillDeckMessage msg)
        {
            OpponentTellOppMillDeckMessage millMsg = new()
            {
                playerNetId = msg.playerNetId,
                count = msg.count
            };
            server.SafeSendToClient(server.gameState.currentOpponent, millMsg);
        }

        #endregion

        // ------------------------------

        #region Play Cards on Field

        private void OnPlayCard(NetworkConnection conn, LocalPlayCardMessage msg)
        {
            PlayerInfo player = server.gameState.players.Find(x => x.netId == msg.playerNetId);
            RuntimeZone originZone = player.namedZones[msg.originZone];
            RuntimeCard card = originZone.cards.Find(x => x.instanceId == msg.cardInstanceId);
            if(card == null)
            {
                Debug.LogError($"[Play Card] P{player.id} attempted to play card w/ instance ID {msg.cardInstanceId} when it does not exist in zone {msg.originZone}!");
                return;
            }

            (server.effectSolver as SVEEffectSolver).PlayCard(msg.playerNetId, card, msg.originZone, msg.playPointCost);
            OpponentPlayCardMessage playedCardMsg = new()
            {
                playerNetId = msg.playerNetId,
                card = NetworkingUtils.GetNetCard(card),
                fieldSlotId = msg.fieldSlotId,
                originZone = msg.originZone,
                playPointCost = msg.playPointCost
            };
            server.SafeSendToClient(server.gameState.currentOpponent, playedCardMsg);
        }

        private void OnCreateToken(NetworkConnection conn, LocalCreateTokenMessage msg)
        {
            PlayerInfo player = server.gameState.players.Find(x => x.netId == msg.playerNetId);
            RuntimeZone targetZone = player.namedZones[msg.createOnField ? SVEProperties.Zones.Field : SVEProperties.Zones.ExArea];

            player.currentCardInstanceId++;
            RuntimeCard card = (server.effectSolver as SVEEffectSolver).CreateAndAddToken(msg.playerNetId, msg.libraryCardId, msg.runtimeCardInstanceId, targetZone);
            OpponentCreateTokenMessage createTokenMessage = new()
            {
                playerNetId = msg.playerNetId,
                card = NetworkingUtils.GetNetCard(card),
                createOnField = msg.createOnField,
                slotId = msg.slotId
            };
            server.SafeSendToClient(server.gameState.currentOpponent, createTokenMessage);
        }

        private void OnTransformCard(NetworkConnection conn, LocalTransformCardMessage msg)
        {
            PlayerInfo player = GetPlayerInfo(msg.playerNetId, msg.isOpponentCard);
            RuntimeZone targetZone = player.namedZones[msg.originZone];
            RuntimeCard targetCard = player.namedZones[msg.originZone].cards.Find(x => x.instanceId == msg.targetCardInstanceId);
            Debug.Assert(targetZone != null, $"Transform card effect received invalid zone {msg.originZone}");
            Debug.Assert(targetCard != null, $"Failed to find card with instance ID {msg.targetCardInstanceId} in zone {msg.originZone}");

            (server.effectSolver as SVEEffectSolver).BanishCard(player.netId, targetCard, msg.originZone);
            player.currentCardInstanceId++;
            RuntimeCard tokenCard = (server.effectSolver as SVEEffectSolver).CreateAndAddToken(msg.playerNetId, msg.libraryCardId, msg.tokenRuntimeCardInstanceId, targetZone);

            OpponentTransformCardMessage transformMessage = new()
            {
                playerNetId = msg.playerNetId,
                targetCard = NetworkingUtils.GetNetCard(targetCard),
                isOpponentCard = msg.isOpponentCard,
                originZone = msg.originZone,

                tokenCard = NetworkingUtils.GetNetCard(tokenCard),
                slotId = msg.slotId
            };
            server.SafeSendToClient(server.gameState.currentOpponent, transformMessage);
        }

        private void OnEvolveCard(NetworkConnection conn, LocalEvolveCardMessage msg)
        {
            PlayerInfo player = server.gameState.players.Find(x => x.netId == msg.playerNetId);
            RuntimeCard baseCard = player.namedZones[SVEProperties.Zones.Field].cards.Find(x => x.instanceId == msg.baseCardInstanceId);
            RuntimeCard evolvedCard = player.namedZones[SVEProperties.Zones.EvolveDeck].cards.Find(x => x.instanceId == msg.evolvedCardInstanceId);
            if(baseCard == null)
            {
                Debug.LogError($"[Evolve Card] P{player.id} attempted to evolve card w/ instance ID {msg.baseCardInstanceId} when it does not exist on the player's field!");
                return;
            }
            if(evolvedCard == null)
            {
                Debug.LogError($"[Evolve Card] P{player.id} attempted to evolve card w/ instance ID {msg.evolvedCardInstanceId} when it does not exist in the player's evolve deck!");
                return;
            }

            OpponentEvolveCardMessage evolveCardMessage = new()
            {
                playerNetId = msg.playerNetId,
                baseCard = NetworkingUtils.GetNetCard(baseCard),
                evolvedCard = NetworkingUtils.GetNetCard(evolvedCard),
                fieldSlotId = msg.fieldSlotId,
                useEvolvePoint = msg.useEvolvePoint,
                useEvolveCost = msg.useEvolveCost
            };
            server.SafeSendToClient(server.gameState.currentOpponent, evolveCardMessage);
            (server.effectSolver as SVEEffectSolver).EvolveCard(msg.playerNetId, baseCard, evolvedCard, msg.useEvolvePoint, msg.useEvolveCost);
        }

        #endregion

        // ------------------------------

        #region Send Card to Zone

        private void OnSendCardToCemetery(NetworkConnection conn, LocalSendCardToCemeteryMessage msg)
        {
            PlayerInfo player = GetPlayerInfo(msg.playerNetId, msg.isOpponentCard);
            RuntimeCard card = player.namedZones[msg.originZone].cards.Find(x => x.instanceId == msg.cardInstanceId);
            Debug.Assert(card != null, $"Failed to find card with instance ID {msg.cardInstanceId} in zone {msg.originZone} to send to cemetery");

            OpponentSendCardToCemeteryMessage sendCardToCemeteryMessage = new()
            {
                playerNetId = msg.playerNetId,
                card = NetworkingUtils.GetNetCard(card),
                isOpponentCard = msg.isOpponentCard,
                originZone = msg.originZone,
                isDestroy = msg.isDestroy
            };
            server.SafeSendToClient(server.gameState.currentOpponent, sendCardToCemeteryMessage);
            (server.effectSolver as SVEEffectSolver).SendToCemetery(player.netId, card, msg.originZone, msg.isDestroy);
        }

        private void OnBanishCard(NetworkConnection conn, LocalBanishCardMessage msg)
        {
            PlayerInfo player = GetPlayerInfo(msg.playerNetId, msg.isOpponentCard);
            RuntimeCard card = player.namedZones[msg.originZone].cards.Find(x => x.instanceId == msg.cardInstanceId);
            Debug.Assert(card != null, $"Failed to find card with instance ID {msg.cardInstanceId} in zone {msg.originZone} to banish");

            OpponentBanishCardMessage banishCardMessage = new()
            {
                playerNetId = msg.playerNetId,
                card = NetworkingUtils.GetNetCard(card),
                isOpponentCard = msg.isOpponentCard,
                originZone = msg.originZone
            };
            server.SafeSendToClient(server.gameState.currentOpponent, banishCardMessage);
            (server.effectSolver as SVEEffectSolver).BanishCard(player.netId, card, msg.originZone);
        }

        private void OnDestroyOpponentCard(NetworkConnection conn, LocalDestroyOpponentCardMessage msg)
        {
            OpponentDestroyOpponentCardMessage destroyMsg = new()
            {
                playerNetId = msg.playerNetId,
                cardInstanceId = msg.cardInstanceId
            };
            server.SafeSendToClient(server.gameState.currentOpponent, destroyMsg);
            // Don't need to call effect solver here, this message is only meant to tell the opponent to perform the action, not actually do anything
        }

        private void OnReturnCardToHand(NetworkConnection conn, LocalReturnToHandMessage msg)
        {
            PlayerInfo player = GetPlayerInfo(msg.playerNetId, msg.isOpponentCard);
            RuntimeZone sourceZone = player.namedZones[msg.originZone];
            RuntimeCard card = sourceZone.cards.Find(x => x.instanceId == msg.cardInstanceId);
            Debug.Assert(card != null, $"Failed to find card with instance ID {msg.cardInstanceId} in zone {msg.originZone} to return to hand");

            OpponentReturnToHandMessage returnCardMessage = new()
            {
                playerNetId = msg.playerNetId,
                card = NetworkingUtils.GetNetCard(card),
                isOpponentCard = msg.isOpponentCard,
                originZone = msg.originZone
            };
            server.SafeSendToClient(server.gameState.currentOpponent, returnCardMessage);
            (server.effectSolver as SVEEffectSolver).ReturnCardToHand(player, card, msg.originZone);
        }

        private void OnSendCardToBottomDeck(NetworkConnection conn, LocalSendToBottomDeckMessage msg)
        {
            PlayerInfo player = GetPlayerInfo(msg.playerNetId, msg.isOpponentCard);
            RuntimeCard card = player.namedZones[msg.originZone].cards.Find(x => x.instanceId == msg.cardInstanceId);
            Debug.Assert(card != null, $"Failed to find card with instance ID {msg.cardInstanceId} in zone {msg.originZone} to send to bottom deck");

            OpponentSendToBottomDeckMessage bottomDeckMessage = new()
            {
                playerNetId = msg.playerNetId,
                cardInstanceId = msg.cardInstanceId,
                isOpponentCard = msg.isOpponentCard,
                originZone = msg.originZone
            };
            server.SafeSendToClient(server.gameState.currentOpponent, bottomDeckMessage);
            (server.effectSolver as SVEEffectSolver).SendCardToBottomDeck(player, card, msg.originZone);
        }

        private void OnSendCardToTopDeck(NetworkConnection conn, LocalSendToTopDeckMessage msg)
        {
            PlayerInfo player = GetPlayerInfo(msg.playerNetId, msg.isOpponentCard);
            RuntimeCard card = player.namedZones[msg.originZone].cards.Find(x => x.instanceId == msg.cardInstanceId);
            Debug.Assert(card != null, $"Failed to find card with instance ID {msg.cardInstanceId} in zone {msg.originZone} to send to top deck");

            OpponentSendToTopDeckMessage topDeckMessage = new()
            {
                playerNetId = msg.playerNetId,
                cardInstanceId = msg.cardInstanceId,
                isOpponentCard = msg.isOpponentCard,
                originZone = msg.originZone
            };
            server.SafeSendToClient(server.gameState.currentOpponent, topDeckMessage);
            (server.effectSolver as SVEEffectSolver).SendCardToTopDeck(player, card, msg.originZone);
        }

        private void OnSendCardToExArea(NetworkConnection conn, LocalSendToExAreaMessage msg)
        {
            PlayerInfo player = GetPlayerInfo(msg.playerNetId, msg.isOpponentCard);
            RuntimeCard card = player.namedZones[msg.originZone].cards.Find(x => x.instanceId == msg.cardInstanceId);
            Debug.Assert(card != null, $"Failed to find card with instance ID {msg.cardInstanceId} in zone {msg.originZone} to send to EX area");

            OpponentSendToExAreaMessage topDeckMessage = new()
            {
                playerNetId = msg.playerNetId,
                card = NetworkingUtils.GetNetCard(card),
                isOpponentCard = msg.isOpponentCard,
                fieldSlotId = msg.fieldSlotId,
                originZone = msg.originZone
            };
            server.SafeSendToClient(server.gameState.currentOpponent, topDeckMessage);
            (server.effectSolver as SVEEffectSolver).SendCardToExArea(player, card, msg.originZone);
        }

        #endregion

        // ------------------------------

        #region Spells and Effect Costs

        private void OnPlaySpell(NetworkConnection conn, LocalPlaySpellMessage msg)
        {
            PlayerInfo player = server.gameState.players.Find(x => x.netId == msg.playerNetId);
            RuntimeCard card = player.namedZones[msg.originZone].cards.Find(x => x.instanceId == msg.cardInstanceId);
            Debug.Assert(card != null, $"Failed to find card with instance ID {msg.cardInstanceId} in zone {msg.originZone} to play spell with");

            OpponentPlaySpellMessage playSpellMsg = new()
            {
                playerNetId = msg.playerNetId,
                cardInstanceId = msg.cardInstanceId,
                originZone = msg.originZone,
                playPointCost = msg.playPointCost
            };
            server.SafeSendToClient(server.gameState.currentOpponent, playSpellMsg);
            (server.effectSolver as SVEEffectSolver).PlaySpell(msg.playerNetId, card, msg.originZone, msg.playPointCost);
        }

        private void OnFinishSpell(NetworkConnection conn, LocalFinishSpellMessage msg)
        {
            PlayerInfo player = server.gameState.players.Find(x => x.netId == msg.playerNetId);
            RuntimeCard card = player.namedZones[SVEProperties.Zones.Resolution].cards.Find(x => x.instanceId == msg.cardInstanceId);
            Debug.Assert(card != null, $"Failed to find card with instance ID {msg.cardInstanceId} in zone {SVEProperties.Zones.Resolution} to finish spell");

            OpponentFinishSpellMessage finishSpellMsg = new()
            {
                playerNetId = msg.playerNetId,
                cardInstanceId = msg.cardInstanceId
            };
            server.SafeSendToClient(server.gameState.currentOpponent, finishSpellMsg);
            (server.effectSolver as SVEEffectSolver).FinishSpell(msg.playerNetId, card);
        }

        private void OnPayEffectCost(NetworkConnection conn, LocalPayEffectCostMessage msg)
        {
            PlayerInfo player = server.gameState.players.Find(x => x.netId == msg.playerNetId);
            RuntimeCard card = player.namedZones[msg.originZone].cards.Find(x => x.instanceId == msg.cardInstanceId);
            Debug.Assert(card != null, $"Failed to find card with instance ID {msg.cardInstanceId} in zone {msg.originZone} to pay effect cost for");
            Ability ability = msg.abilityName.Equals(CounterUtilities.InnateStackAbility.name)
                ? CounterUtilities.InnateStackAbility
                : LibraryCardCache.GetCard(card.cardId, server.gameState.config).abilities.FirstOrDefault(x => x.name.Equals(msg.abilityName));
            Debug.Assert(ability != null, $"Failed to find ability to pay cost for: {msg.abilityName}");

            List<Cost> costList = ability switch
            {
                ActivatedAbility activatedAbility => activatedAbility.costs,
                TriggeredAbility { trigger: SveTrigger sveTrigger } => sveTrigger.Costs,
                _ => null
            };

            OpponentPayEffectCostMessage payCostMsg = new()
            {
                playerNetId = msg.playerNetId,
                cardInstanceId = msg.cardInstanceId,
                originZone = msg.originZone,
                abilityName = msg.abilityName,
                cardsMoveToZoneData = msg.cardsMoveToZoneData,
                countersToRemove = msg.countersToRemove,
            };
            server.SafeSendToClient(server.gameState.currentOpponent, payCostMsg);
            (server.effectSolver as SVEEffectSolver).PayAbilityCosts(player, card, costList, msg.cardsMoveToZoneData, msg.countersToRemove);
        }

        #endregion

        // ------------------------------

        #region Other

        private void OnTellOpponentPerformEffect(NetworkConnection conn, LocalTellOpponentPerformEffectMessage msg)
        {
            OpponentTellOpponentPerformEffectMessage sendMsg = new()
            {
                playerNetId = msg.playerNetId,
                libraryCardId = msg.libraryCardId,
                cardInstanceId = msg.cardInstanceId,
                cardZone = msg.cardZone,
                effectName = msg.effectName,
                targetInstanceIds = msg.targetInstanceIds
            };
            server.SafeSendToClient(server.gameState.currentOpponent, sendMsg);
        }

        #endregion
    }
}
