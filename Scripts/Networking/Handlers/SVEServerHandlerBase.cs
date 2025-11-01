using CCGKit;
using Mirror;
using UnityEngine;

namespace SVESimulator
{
    public abstract class SVEServerHandlerBase : ServerHandler
    {
        public SVEServerHandlerBase(Server server) : base(server) { }

        protected PlayerInfo GetPlayerInfo(NetworkIdentity netId, bool isOpponent = false) =>
            server.gameState.players.Find(x => isOpponent ? (x.netId != netId) : (x.netId == netId));
    }
}
