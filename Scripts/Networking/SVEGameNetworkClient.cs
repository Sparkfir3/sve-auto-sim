using System.Collections.Generic;
using System.Linq;
using CCGKit;
using Mirror;

namespace SVESimulator
{
    public class SVEGameNetworkClient : GameNetworkClient
    {
        #region Initialization

        /// <summary>
        /// Registers the network handlers for the network messages we are interested in handling.
        /// </summary>
        protected override void RegisterNetworkHandlers()
        {
            base.RegisterNetworkHandlers();

            // Game initialization
            NetworkClient.RegisterHandler<SetGoingFirstPlayerMessage>(SetGoingFirst);
            NetworkClient.RegisterHandler<OpponentPerformMulliganMessage>(DoOpponentMulligan);
            NetworkClient.RegisterHandler<SetMaxPlayPointsMessage>(OnOpponentUpdateMaxPlayPoints);
            NetworkClient.RegisterHandler<SetCurrentPlayPointsMessage>(OnOpponentUpdateCurrentPlayPoints);
            NetworkClient.RegisterHandler<OpponentInitDeckAndLeaderMessage>(OnOpponentInitDeckAndLeader);

            // Deck movement
            NetworkClient.RegisterHandler<OpponentDrawCardMessage>(OnOpponentDrawCard);
            NetworkClient.RegisterHandler<OpponentTellOppDrawCardMessage>(OnOpponentTellOppDrawCard);
            NetworkClient.RegisterHandler<OpponentTellOppMillDeckMessage>(OnOpponentTellOppMillDeck);

            // Play cards on field
            NetworkClient.RegisterHandler<OpponentPlayCardMessage>(OnOpponentPlayCard);
            NetworkClient.RegisterHandler<OpponentCreateTokenMessage>(OnOpponentCreateToken);
            NetworkClient.RegisterHandler<OpponentTransformCardMessage>(OnOpponentTransformCard);
            NetworkClient.RegisterHandler<OpponentEvolveCardMessage>(OnOpponentEvolveCard);

            // Send card to zone
            NetworkClient.RegisterHandler<OpponentSendCardToCemeteryMessage>(OnOpponentSendCardToCemetery);
            NetworkClient.RegisterHandler<OpponentBanishCardMessage>(OnOpponentBanishCard);
            NetworkClient.RegisterHandler<OpponentDestroyOpponentCardMessage>(OnOpponentDestroyOpponentCard);
            NetworkClient.RegisterHandler<OpponentReturnToHandMessage>(OnOpponentReturnCardToHand);
            NetworkClient.RegisterHandler<OpponentSendToBottomDeckMessage>(OnOpponentSendCardToBottomDeck);
            NetworkClient.RegisterHandler<OpponentSendToTopDeckMessage>(OnOpponentSendCardToTopDeck);
            NetworkClient.RegisterHandler<OpponentSendToExAreaMessage>(OnOpponentSendCardToExArea);

            // Spells and effect costs
            NetworkClient.RegisterHandler<OpponentPlaySpellMessage>(OnOpponentPlaySpell);
            NetworkClient.RegisterHandler<OpponentFinishSpellMessage>(OnOpponentFinishSpell);
            NetworkClient.RegisterHandler<OpponentPayEffectCostMessage>(OnOpponentPayEffectCost);

            // Attack handling
            NetworkClient.RegisterHandler<OpponentDeclareAttackMessage>(OnOpponentDeclareAttack);
            NetworkClient.RegisterHandler<OpponentAttackFollowerMessage>(OnOpponentAttackFollower);
            NetworkClient.RegisterHandler<OpponentAttackLeaderMessage>(OnOpponentAttackLeader);

            // Stat and keyword handling
            NetworkClient.RegisterHandler<OpponentAddLeaderDefenseMessage>(OnOpponentAddLeaderDefense);
            NetworkClient.RegisterHandler<OpponentReserveCardMessage>(OnOpponentReserveCard);
            NetworkClient.RegisterHandler<OpponentEngageCardMessage>(OnOpponentEngageCard);
            NetworkClient.RegisterHandler<OpponentSetCardStatMessage>(OnOpponentSetCardStat);
            NetworkClient.RegisterHandler<OpponentCardStatModifierMessage>(OnOpponentApplyCardStatModifier);
            NetworkClient.RegisterHandler<OpponentApplyKeywordMessage>(OnOpponentApplyKeyword);

            // Other
            NetworkClient.RegisterHandler<OpponentTellOpponentPerformEffectMessage>(OnOpponentTellOpponentPerformEffect);
        }

        /// <summary>
        /// Unregisters the network handlers for the network messages we are interested in handling.
        /// </summary>
        protected override void UnregisterNetworkHandlers()
        {
            // Game initialization
            NetworkClient.UnregisterHandler<SetGoingFirstPlayerMessage>();
            NetworkClient.UnregisterHandler<OpponentPerformMulliganMessage>();
            NetworkClient.UnregisterHandler<SetMaxPlayPointsMessage>();
            NetworkClient.UnregisterHandler<SetCurrentPlayPointsMessage>();
            NetworkClient.UnregisterHandler<OpponentInitDeckAndLeaderMessage>();

            // Deck movement
            NetworkClient.UnregisterHandler<OpponentDrawCardMessage>();
            NetworkClient.UnregisterHandler<OpponentTellOppDrawCardMessage>();
            NetworkClient.UnregisterHandler<OpponentTellOppMillDeckMessage>();

            // Play cards on field
            NetworkClient.UnregisterHandler<OpponentPlayCardMessage>();
            NetworkClient.UnregisterHandler<OpponentCreateTokenMessage>();
            NetworkClient.UnregisterHandler<OpponentTransformCardMessage>();
            NetworkClient.UnregisterHandler<OpponentEvolveCardMessage>();

            // Send card to zone
            NetworkClient.UnregisterHandler<OpponentSendCardToCemeteryMessage>();
            NetworkClient.UnregisterHandler<OpponentBanishCardMessage>();
            NetworkClient.UnregisterHandler<OpponentDestroyOpponentCardMessage>();
            NetworkClient.UnregisterHandler<OpponentReturnToHandMessage>();
            NetworkClient.UnregisterHandler<OpponentSendToBottomDeckMessage>();
            NetworkClient.UnregisterHandler<OpponentSendToTopDeckMessage>();
            NetworkClient.UnregisterHandler<OpponentSendToExAreaMessage>();

            // Spells and effect costs
            NetworkClient.UnregisterHandler<OpponentPlaySpellMessage>();
            NetworkClient.UnregisterHandler<OpponentFinishSpellMessage>();
            NetworkClient.UnregisterHandler<OpponentPayEffectCostMessage>();

            // Attack handling
            NetworkClient.UnregisterHandler<OpponentDeclareAttackMessage>();
            NetworkClient.UnregisterHandler<OpponentAttackFollowerMessage>();
            NetworkClient.UnregisterHandler<OpponentAttackLeaderMessage>();

            // Stat and keyword handling
            NetworkClient.UnregisterHandler<OpponentAddLeaderDefenseMessage>();
            NetworkClient.UnregisterHandler<OpponentReserveCardMessage>();
            NetworkClient.UnregisterHandler<OpponentEngageCardMessage>();
            NetworkClient.UnregisterHandler<OpponentSetCardStatMessage>();
            NetworkClient.UnregisterHandler<OpponentCardStatModifierMessage>();
            NetworkClient.UnregisterHandler<OpponentApplyKeywordMessage>();

            // Other
            NetworkClient.UnregisterHandler<OpponentTellOpponentPerformEffectMessage>();

            base.UnregisterNetworkHandlers();
        }

        #endregion

        // ------------------------------

        #region Game Initialization

        private void SetGoingFirst(SetGoingFirstPlayerMessage msg)
        {
            PlayerController firstPlayer = localPlayers.Find(x => x.netIdentity == msg.playerNetId) as PlayerController;
            if(firstPlayer != null)
            {
                PlayerInfo playerInfo = firstPlayer.GetPlayerInfo();
                playerInfo.isGoingFirstDecided = true;
                playerInfo.isGoingFirst = true;
                playerInfo.namedStats[SVEProperties.PlayerStats.EvolutionPoints].baseValue = 0;
            }

            List<PlayerController> otherPlayers = localPlayers.FindAll(x => x.netIdentity != msg.playerNetId).Select(x => x as PlayerController).ToList();
            foreach(PlayerController otherPlayer in otherPlayers)
            {
                PlayerInfo playerInfo = otherPlayer.GetPlayerInfo();
                playerInfo.isGoingFirstDecided = true;
                playerInfo.isGoingFirst = false;
                playerInfo.namedStats[SVEProperties.PlayerStats.EvolutionPoints].baseValue = 3;
            }

            PlayerController localPlayer = localPlayers.Find(x => x.isLocalPlayer) as PlayerController;
            if(localPlayer)
                localPlayer.OpponentEvents.SetGoingFirst(msg);
        }

        private void DoOpponentMulligan(OpponentPerformMulliganMessage msg)
        {
            PlayerController player = localPlayers.Find(x => x.netIdentity != msg.playerNetId) as PlayerController;
            if(!player)
                return;

            player.OpponentEvents.Mulligan(msg);
        }

        private void OnOpponentUpdateMaxPlayPoints(SetMaxPlayPointsMessage msg)
        {
            PlayerController player = localPlayers.Find(x => x.netIdentity != msg.playerNetId) as PlayerController;
            if(!player)
                return;

            player.OpponentEvents.SetMaxPlayPoints(msg.maxPlayPoints, msg.updateCurrentPoints, false);
        }

        private void OnOpponentUpdateCurrentPlayPoints(SetCurrentPlayPointsMessage msg)
        {
            PlayerController player = localPlayers.Find(x => x.netIdentity != msg.playerNetId) as PlayerController;
            if(!player)
                return;

            player.OpponentEvents.SetCurrentPlayPoints(msg.currentPlayPoints, false);
        }

        private void OnOpponentInitDeckAndLeader(OpponentInitDeckAndLeaderMessage msg)
        {
            PlayerController player = localPlayers.Find(x => x.netIdentity != msg.playerNetId) as PlayerController;
            if(!player)
                return;

            player.OpponentEvents.InitializeDeckAndLeader(msg);
        }

        #endregion

        // ------------------------------

        #region Deck Movement

        private void OnOpponentDrawCard(OpponentDrawCardMessage msg)
        {
            PlayerController player = localPlayers.Find(x => x.netIdentity != msg.playerNetId) as PlayerController;
            if(!player)
                return;

            player.OpponentEvents.DrawCard(msg);
        }

        private void OnOpponentTellOppDrawCard(OpponentTellOppDrawCardMessage msg)
        {
            PlayerController player = localPlayers.Find(x => x.netIdentity != msg.playerNetId) as PlayerController;
            if(!player)
                return;

            for(int i = 0; i < msg.count; i++)
                player.LocalEvents.DrawCard();
        }

        private void OnOpponentTellOppMillDeck(OpponentTellOppMillDeckMessage msg)
        {
            PlayerController player = localPlayers.Find(x => x.netIdentity != msg.playerNetId) as PlayerController;
            if(!player)
                return;

            player.LocalEvents.MillDeck(msg.count);
        }

        #endregion

        // ------------------------------

        #region Play Cards on Field

        private void OnOpponentPlayCard(OpponentPlayCardMessage msg)
        {
            PlayerController player = localPlayers.Find(x => x.netIdentity != msg.playerNetId) as PlayerController;
            if(!player)
                return;

            player.OpponentEvents.PlayCardToField(msg);
        }

        private void OnOpponentCreateToken(OpponentCreateTokenMessage msg)
        {
            PlayerController player = localPlayers.Find(x => x.netIdentity != msg.playerNetId) as PlayerController;
            if(!player)
                return;

            player.OpponentEvents.CreateToken(msg);
        }

        private void OnOpponentTransformCard(OpponentTransformCardMessage msg)
        {
            PlayerController player = localPlayers.Find(x => x.netIdentity != msg.playerNetId) as PlayerController;
            if(!player)
                return;

            player.OpponentEvents.TransformCard(msg);
        }

        private void OnOpponentEvolveCard(OpponentEvolveCardMessage msg)
        {
            PlayerController player = localPlayers.Find(x => x.netIdentity != msg.playerNetId) as PlayerController;
            if(!player)
                return;

            player.OpponentEvents.EvolveCard(msg);
        }

        #endregion

        // ------------------------------

        #region Send Card to Zone

        private void OnOpponentSendCardToCemetery(OpponentSendCardToCemeteryMessage msg)
        {
            PlayerController player = localPlayers.Find(x => x.netIdentity != msg.playerNetId) as PlayerController;
            if(!player)
                return;

            player.OpponentEvents.SendToCemetery(msg);
        }

        private void OnOpponentBanishCard(OpponentBanishCardMessage msg)
        {
            PlayerController player = localPlayers.Find(x => x.netIdentity != msg.playerNetId) as PlayerController;
            if(!player)
                return;

            player.OpponentEvents.BanishCard(msg);
        }

        private void OnOpponentDestroyOpponentCard(OpponentDestroyOpponentCardMessage msg)
        {
            PlayerController player = localPlayers.Find(x => x.netIdentity != msg.playerNetId) as PlayerController;
            if(!player)
                return;

            player.OpponentEvents.DestroyOpponentCard(msg);
        }

        private void OnOpponentReturnCardToHand(OpponentReturnToHandMessage msg)
        {
            PlayerController player = localPlayers.Find(x => x.netIdentity != msg.playerNetId) as PlayerController;
            if(!player)
                return;

            player.OpponentEvents.ReturnCardToHand(msg);
        }

        private void OnOpponentSendCardToBottomDeck(OpponentSendToBottomDeckMessage msg)
        {
            PlayerController player = localPlayers.Find(x => x.netIdentity != msg.playerNetId) as PlayerController;
            if(!player)
                return;

            player.OpponentEvents.SendToBottomDeck(msg);
        }

        private void OnOpponentSendCardToTopDeck(OpponentSendToTopDeckMessage msg)
        {
            PlayerController player = localPlayers.Find(x => x.netIdentity != msg.playerNetId) as PlayerController;
            if(!player)
                return;

            player.OpponentEvents.SendToTopDeck(msg);
        }

        private void OnOpponentSendCardToExArea(OpponentSendToExAreaMessage msg)
        {
            PlayerController player = localPlayers.Find(x => x.netIdentity != msg.playerNetId) as PlayerController;
            if(!player)
                return;

            player.OpponentEvents.SendToExArea(msg);
        }

        #endregion

        // ------------------------------

        #region Spells and Effect Costs

        private void OnOpponentPlaySpell(OpponentPlaySpellMessage msg)
        {
            PlayerController player = localPlayers.Find(x => x.netIdentity != msg.playerNetId) as PlayerController;
            if(!player)
                return;

            player.OpponentEvents.PlaySpell(msg);
        }

        private void OnOpponentFinishSpell(OpponentFinishSpellMessage msg)
        {
            PlayerController player = localPlayers.Find(x => x.netIdentity != msg.playerNetId) as PlayerController;
            if(!player)
                return;

            player.OpponentEvents.FinishSpell(msg);
        }

        private void OnOpponentPayEffectCost(OpponentPayEffectCostMessage msg)
        {
            PlayerController player = localPlayers.Find(x => x.netIdentity != msg.playerNetId) as PlayerController;
            if(!player)
                return;

            player.OpponentEvents.PayCostForEffect(msg);
        }

        #endregion

        // ------------------------------

        #region Attack Handling

        private void OnOpponentDeclareAttack(OpponentDeclareAttackMessage msg)
        {
            PlayerController player = localPlayers.Find(x => x.netIdentity != msg.playerNetId) as PlayerController;
            if(!player)
                return;

            player.OpponentEvents.DeclareAttack(msg);
        }

        private void OnOpponentAttackFollower(OpponentAttackFollowerMessage msg)
        {
            PlayerController player = localPlayers.Find(x => x.netIdentity != msg.attackingPlayerNetId) as PlayerController;
            if(!player)
                return;

            player.OpponentEvents.AttackFollower(msg);
        }

        private void OnOpponentAttackLeader(OpponentAttackLeaderMessage msg)
        {
            PlayerController player = localPlayers.Find(x => x.netIdentity != msg.playerNetId) as PlayerController;
            if(!player)
                return;

            player.OpponentEvents.AttackLeader(msg);
        }

        #endregion

        // ------------------------------

        #region Stat and keyword handling

        protected void OnOpponentAddLeaderDefense(OpponentAddLeaderDefenseMessage msg)
        {
            PlayerController player = localPlayers.Find(x => x.netIdentity != msg.playerNetId) as PlayerController;
            if(!player)
                return;

            player.OpponentEvents.AddLeaderDefense(msg);
        }

        private void OnOpponentReserveCard(OpponentReserveCardMessage msg)
        {
            PlayerController player = localPlayers.Find(x => x.netIdentity != msg.playerNetId) as PlayerController;
            if(!player)
                return;

            player.OpponentEvents.ReserveCard(msg);
        }

        private void OnOpponentEngageCard(OpponentEngageCardMessage msg)
        {
            PlayerController player = localPlayers.Find(x => x.netIdentity != msg.playerNetId) as PlayerController;
            if(!player)
                return;

            player.OpponentEvents.EngageCard(msg);
        }

        private void OnOpponentSetCardStat(OpponentSetCardStatMessage msg)
        {
            PlayerController player = localPlayers.Find(x => x.netIdentity != msg.playerNetId) as PlayerController;
            if(!player)
                return;

            player.OpponentEvents.SetCardStat(msg);
        }

        private void OnOpponentApplyCardStatModifier(OpponentCardStatModifierMessage msg)
        {
            PlayerController player = localPlayers.Find(x => x.netIdentity != msg.playerNetId) as PlayerController;
            if(!player)
                return;

            player.OpponentEvents.ApplyModifierToCard(msg);
        }

        private void OnOpponentApplyKeyword(OpponentApplyKeywordMessage msg)
        {
            PlayerController player = localPlayers.Find(x => x.netIdentity != msg.playerNetId) as PlayerController;
            if(!player)
                return;

            player.OpponentEvents.ApplyKeywordToCard(msg);
        }

        #endregion

        // ------------------------------

        #region Other

        private void OnOpponentTellOpponentPerformEffect(OpponentTellOpponentPerformEffectMessage msg)
        {
            PlayerController player = localPlayers.Find(x => x.netIdentity != msg.playerNetId) as PlayerController;
            if(!player)
                return;

            player.OpponentEvents.TellPerformEffect(msg);
        }

        #endregion
    }
}
