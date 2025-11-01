using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using CCGKit;
using Mirror;

namespace SVESimulator
{
    public class SVETurnSequenceHandler : TurnSequenceHandler
    {
        private SVESimServer sveServer;

        public SVETurnSequenceHandler(Server server) : base(server)
        {
            this.server = server;
            sveServer = server as SVESimServer;
        }

        public override void RegisterNetworkHandlers()
        {
            base.RegisterNetworkHandlers();
            NetworkServer.RegisterHandler<ExtraTurnMessage>(OnExtraTurn);
        }

        public override void UnregisterNetworkHandlers()
        {
            base.UnregisterNetworkHandlers();
            NetworkServer.UnregisterHandler<ExtraTurnMessage>();
        }

        protected override void OnStopTurn(NetworkConnection conn, StopTurnMessage msg)
        {
            if(sveServer.ExtraTurn)
                sveServer.StopTurnWithExtraTurn();
            else
                server.StopTurn();
        }

        private void OnExtraTurn(NetworkConnection conn, ExtraTurnMessage msg)
        {
            if(server.gameState.currentPlayer == server.gameState.players.Find(x => x.netId == msg.playerNetId))
                sveServer.ExtraTurn = true;
        }
    }
}
