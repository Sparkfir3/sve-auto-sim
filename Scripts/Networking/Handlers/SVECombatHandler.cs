using CCGKit;
using Mirror;
using UnityEngine;

namespace SVESimulator
{
    public class SVECombatHandler : SVEServerHandlerBase
    {
        #region Initialization

        public SVECombatHandler(Server server) : base(server) { }

        public override void RegisterNetworkHandlers()
        {
            // Attack handling
            NetworkServer.RegisterHandler<LocalDeclareAttackMessage>(OnDeclareAttack);
            NetworkServer.RegisterHandler<LocalAttackFollowerMessage>(OnAttackFollower);
            NetworkServer.RegisterHandler<LocalAttackLeaderMessage>(OnAttackLeader);

            // Stat and keyword handling
            NetworkServer.RegisterHandler<LocalAddLeaderDefenseMessage>(OnAddLeaderDefense);
            NetworkServer.RegisterHandler<LocalReserveCardMessage>(OnReserveCard);
            NetworkServer.RegisterHandler<LocalEngageCardMessage>(OnEngageCard);
            NetworkServer.RegisterHandler<LocalSetCardStatMessage>(OnSetCardStat);
            NetworkServer.RegisterHandler<LocalCardStatModifierMessage>(OnApplyCardStatModifier);
            NetworkServer.RegisterHandler<LocalApplyKeywordMessage>(OnApplyKeyword);
        }

        public override void UnregisterNetworkHandlers()
        {
            // Attack handling
            NetworkServer.UnregisterHandler<LocalDeclareAttackMessage>();
            NetworkServer.UnregisterHandler<LocalAttackFollowerMessage>();
            NetworkServer.UnregisterHandler<LocalAttackLeaderMessage>();

            // Stat and keyword handling
            NetworkServer.UnregisterHandler<LocalAddLeaderDefenseMessage>();
            NetworkServer.UnregisterHandler<LocalReserveCardMessage>();
            NetworkServer.UnregisterHandler<LocalEngageCardMessage>();
            NetworkServer.UnregisterHandler<LocalSetCardStatMessage>();
            NetworkServer.UnregisterHandler<LocalCardStatModifierMessage>();
            NetworkServer.UnregisterHandler<LocalApplyKeywordMessage>();
        }

        #endregion

        // ------------------------------

        #region Attack Handling

        private void OnDeclareAttack(NetworkConnection conn, LocalDeclareAttackMessage msg)
        {
            PlayerInfo player = server.gameState.players.Find(x => x.netId == msg.playerNetId);
            RuntimeCard card = player.namedZones[SVEProperties.Zones.Field].cards.Find(x => x.instanceId == msg.cardInstanceId);
            Debug.Assert(card != null);

            OpponentDeclareAttackMessage oppAtkMsg = new()
            {
                playerNetId = msg.playerNetId,
                cardInstanceId = msg.cardInstanceId
            };
            server.SafeSendToClient(server.gameState.currentOpponent, oppAtkMsg);
            (server.effectSolver as SVEEffectSolver).DeclareAttack(player.netId, card, msg.isAttackingLeader);
        }

        private void OnAttackFollower(NetworkConnection conn, LocalAttackFollowerMessage msg)
        {
            if(conn.identity.netId != server.gameState.currentPlayer.connectionId)
            {
                return;
            }

            PlayerInfo attackingPlayer = server.gameState.players.Find(x => x.netId == msg.attackingPlayerNetId);
            PlayerInfo defendingPlayer = server.gameState.players.Find(x => x.netId != msg.attackingPlayerNetId);
            RuntimeCard attackingCard = attackingPlayer.namedZones[SVEProperties.Zones.Field].cards.Find(x => x.instanceId == msg.attackerInstanceId);
            RuntimeCard defendingCard = defendingPlayer.namedZones[SVEProperties.Zones.Field].cards.Find(x => x.instanceId == msg.defenderInstanceId);

            Debug.Assert(attackingCard != null);
            Debug.Assert(defendingCard != null);
            OpponentAttackFollowerMessage oppAtkMsg = new()
            {
                attackingPlayerNetId = msg.attackingPlayerNetId,
                attackingCard = NetworkingUtils.GetNetCard(attackingCard),
                defendingCard = NetworkingUtils.GetNetCard(defendingCard)
            };
            server.SafeSendToClient(server.gameState.currentOpponent, oppAtkMsg);
            (server.effectSolver as SVEEffectSolver).FightFollower(msg.attackingPlayerNetId, attackingCard, defendingCard);
        }

        private void OnAttackLeader(NetworkConnection conn, LocalAttackLeaderMessage msg)
        {
            if(conn.identity.netId != server.gameState.currentPlayer.connectionId)
            {
                return;
            }

            PlayerInfo attackingPlayer = server.gameState.players.Find(x => x.netId == msg.playerNetId);
            PlayerInfo defendingPlayer = server.gameState.players.Find(x => x.netId != msg.playerNetId);
            RuntimeCard attackingCard = attackingPlayer.namedZones[SVEProperties.Zones.Field].cards.Find(x => x.instanceId == msg.attackerInstanceId);

            Debug.Assert(attackingCard != null);
            OpponentAttackLeaderMessage oppAtkMsg = new()
            {
                playerNetId = msg.playerNetId,
                attackingCard = NetworkingUtils.GetNetCard(attackingCard)
            };
            server.SafeSendToClient(server.gameState.currentOpponent, oppAtkMsg);
            (server.effectSolver as SVEEffectSolver).FightLeader(attackingPlayer.netId, attackingCard, defendingPlayer);
        }

        #endregion

        // ------------------------------

        #region Stat & Keyword Handling

        private void OnAddLeaderDefense(NetworkConnection conn, LocalAddLeaderDefenseMessage msg)
        {
            PlayerInfo targetPlayer = server.gameState.players.Find(x => x.netId == msg.targetPlayer);

            OpponentAddLeaderDefenseMessage addDefMsg = new()
            {
                playerNetId = msg.playerNetId,
                targetPlayer = msg.targetPlayer,
                amount = msg.amount
            };
            server.SafeSendToClient(server.gameState.currentOpponent, addDefMsg);
            (server.effectSolver as SVEEffectSolver).AddLeaderDefense(targetPlayer, msg.amount);
        }

        private void OnReserveCard(NetworkConnection conn, LocalReserveCardMessage msg)
        {
            PlayerInfo player = GetPlayerInfo(msg.playerNetId, msg.isOpponentCard);
            RuntimeCard card = player.namedZones[SVEProperties.Zones.Field].cards.Find(x => x.instanceId == msg.cardInstanceId);
            Debug.Assert(card != null);

            OpponentReserveCardMessage oppMsg = new()
            {
                playerNetId = msg.playerNetId,
                isOpponentCard = msg.isOpponentCard,
                card = NetworkingUtils.GetNetCard(card)
            };
            server.SafeSendToClient(server.gameState.currentOpponent, oppMsg);
            (server.effectSolver as SVEEffectSolver).ReserveCard(card);
        }

        private void OnEngageCard(NetworkConnection conn, LocalEngageCardMessage msg)
        {
            PlayerInfo player = GetPlayerInfo(msg.playerNetId, msg.isOpponentCard);
            RuntimeCard card = player.namedZones[SVEProperties.Zones.Field].cards.Find(x => x.instanceId == msg.cardInstanceId);
            Debug.Assert(card != null);

            OpponentEngageCardMessage oppMsg = new()
            {
                playerNetId = msg.playerNetId,
                isOpponentCard = msg.isOpponentCard,
                card = NetworkingUtils.GetNetCard(card)
            };
            server.SafeSendToClient(server.gameState.currentOpponent, oppMsg);
            (server.effectSolver as SVEEffectSolver).EngageCard(card);
        }

        private void OnSetCardStat(NetworkConnection conn, LocalSetCardStatMessage msg)
        {
            PlayerInfo player = GetPlayerInfo(msg.playerNetId, msg.isOpponentCard);
            RuntimeCard card = player.GetCardOnFieldOrEXArea(msg.cardInstanceId);
            Debug.Assert(card != null);

            OpponentSetCardStatMessage oppMsg = new()
            {
                playerNetId = msg.playerNetId,
                isOpponentCard = msg.isOpponentCard,
                cardInstanceId = msg.cardInstanceId,
                statId = msg.statId,
                value = msg.value
            };
            server.SafeSendToClient(server.gameState.currentOpponent, oppMsg);
            (server.effectSolver as SVEEffectSolver).SetCardStat(card, msg.statId, msg.value);
        }

        private void OnApplyCardStatModifier(NetworkConnection conn, LocalCardStatModifierMessage msg)
        {
            PlayerInfo player = GetPlayerInfo(msg.playerNetId, msg.isOpponentCard);
            RuntimeCard card = player.GetCardOnFieldOrEXArea(msg.cardInstanceId);
            Debug.Assert(card != null, $"ApplyCardStatModifier: Failed to find {(msg.isOpponentCard ? "opponent" : "player")}'s card with ID {msg.cardInstanceId}.");

            OpponentCardStatModifierMessage oppMsg = new()
            {
                playerNetId = msg.playerNetId,
                isOpponentCard = msg.isOpponentCard,
                cardInstanceId = msg.cardInstanceId,
                statId = msg.statId,
                value = msg.value,
                adding = msg.adding,
                duration = msg.duration
            };
            server.SafeSendToClient(server.gameState.currentOpponent, oppMsg);
            (server.effectSolver as SVEEffectSolver).ApplyCardStatModifier(card, msg.statId, msg.value, msg.adding, msg.duration);
        }

        private void OnApplyKeyword(NetworkConnection conn, LocalApplyKeywordMessage msg)
        {
            PlayerInfo player = GetPlayerInfo(msg.playerNetId, msg.isOpponentCard);
            RuntimeCard card = player.namedZones[SVEProperties.Zones.Field].cards.Find(x => x.instanceId == msg.cardInstanceId);
            if(card == null && !msg.adding) // catch for if trying to remove a keyword from a card that has already left the field
                return;
            Debug.Assert(card != null);

            OpponentApplyKeywordMessage oppMsg = new()
            {
                playerNetId = msg.playerNetId,
                isOpponentCard = msg.isOpponentCard,
                cardInstanceId = msg.cardInstanceId,
                keywordType = msg.keywordType,
                keywordValue = msg.keywordValue,
                adding = msg.adding,
            };
            server.SafeSendToClient(server.gameState.currentOpponent, oppMsg);
            (server.effectSolver as SVEEffectSolver).ApplyKeywordToCard(card, msg.keywordType, msg.keywordValue, msg.adding);
        }

        #endregion
    }
}
