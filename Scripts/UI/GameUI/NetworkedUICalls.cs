using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using CCGKit;
using Mirror;

namespace SVESimulator.UI
{
    public class NetworkedUICalls : NetworkBehaviour
    {
        [TargetRpc]
        public void TargetRpcShowOpponentChoosingFirst(NetworkConnectionToClient networkConnection)
        {
            GameUIManager.GoingFirstScreen.SetDisplayMode(SelectGoingFirstScreen.Mode.OpponentSelectGoingFirst);
        }

        // ------------------------------

        [Command(requiresAuthority = false)]
        public void CmdShowOpponentMulligan(NetworkIdentity networkIdentity)
        {
            // too lazy to figure out why target rpc doesn't work when targeting the host, here's some duct tape instead
            TargetRpcShowOpponentMulligan(networkIdentity.connectionToClient);
        }

        [TargetRpc]
        private void TargetRpcShowOpponentMulligan(NetworkConnectionToClient networkConnection)
        {
            GameUIManager.MulliganScreen.ShowOpponentMulligan();
        }

        // ------------------------------

        [Command(requiresAuthority = false)]
        public void CmdShowOpponentTargeting(NetworkIdentity networkIdentity, string cardName, string effectText)
        {
            TargetRpcShowOpponentTargeting(networkIdentity.connectionToClient, cardName, effectText);
        }

        [TargetRpc]
        private void TargetRpcShowOpponentTargeting(NetworkConnectionToClient networkConnection, string cardName, string effectText)
        {
            GameUIManager.EffectTargeting.OpenOpponentIsTargeting(cardName, effectText);
        }

        [Command(requiresAuthority = false)]
        public void CmdCloseOpponentTargeting(NetworkIdentity networkIdentity)
        {
            TargetRpcCloseOpponentTargeting(networkIdentity.connectionToClient);
        }

        [TargetRpc]
        private void TargetRpcCloseOpponentTargeting(NetworkConnectionToClient networkConnection)
        {
            GameUIManager.EffectTargeting.CloseOpponentIsTargeting();
        }
    }
}
