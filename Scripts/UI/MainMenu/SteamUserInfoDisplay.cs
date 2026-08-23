using Mirror;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using TMPro;
using ShowInInspector = Sirenix.OdinInspector.ShowInInspectorAttribute;

namespace SVESimulator.UI
{
    public class SteamUserInfoDisplay : MonoBehaviour
    {
        #region Variables

        [field: Title("Runtime Data"), ShowInInspector]
        public bool ShowPlayer { get; set; }
        [ShowInInspector]
        private bool isShowingPlayer;
        [field: ShowInInspector]
        public bool ShowOpponent { get; set; }
        [ShowInInspector]
        private bool isShowingOpponent;

        [Title("Object References & Settings"), SerializeField]
        private CardAnimationController animController;
        [SerializeField]
        private CardMovementType movementType = CardMovementType.StackToStackNormal;

        [BoxGroup("Player Info"), SerializeField]
        private MainMenuCardObject playerInfoCard;
        [BoxGroup("Player Info"), SerializeField]
        private RawImage playerProfileImage;
        [BoxGroup("Player Info"), SerializeField]
        private TextMeshProUGUI playerUsername;
        [BoxGroup("Player Info"), SerializeField]
        private Transform playerCardOffScreenPos;
        [BoxGroup("Player Info"), SerializeField]
        private Transform playerCardOnScreenPos;

        [BoxGroup("Opponent Info"), SerializeField]
        private MainMenuCardObject opponentInfoCard;
        [BoxGroup("Opponent Info"), SerializeField]
        private RawImage opponentProfileImage;
        [BoxGroup("Opponent Info"), SerializeField]
        private TextMeshProUGUI opponentUsername;
        [BoxGroup("Opponent Info"), SerializeField]
        private Transform opponentCardOffScreenPos;
        [BoxGroup("Opponent Info"), SerializeField]
        private Transform opponentCardOnScreenPos;

        #endregion

        // ------------------------------

        #region Unity Functions

        private void Start()
        {
            playerInfoCard.gameObject.SetActive(false);
            playerInfoCard.transform.position = playerCardOffScreenPos.position;
            opponentInfoCard.gameObject.SetActive(false);
            opponentInfoCard.transform.position = opponentCardOffScreenPos.position;
        }

        private void Update()
        {
            if(Time.frameCount % 2 == 0)
                return;
            if(!SVEGameNetworkManager.IsSteamManagerAndConnected || !SVEGameNetworkManager.DataManager)
                return;

            if(isShowingPlayer != ShowPlayer)
            {
                if(!ShowPlayer)
                    HidePlayerInfo();
                else if(SVEGameNetworkManager.DataManager.TryGetLocalProfileInfo(out Texture2D profilePic, out string username))
                    ShowPlayerInfo(profilePic, username);
            }
            if(isShowingOpponent != ShowOpponent)
            {
                if(!ShowOpponent)
                    HideOpponentInfo();
                else if(SVEGameNetworkManager.DataManager.TryGetOpponentProfileInfo(out Texture2D profilePic, out string username))
                    ShowOpponentInfo(profilePic, username);
            }
        }

        #endregion

        // ------------------------------

        #region Public Controls

        public void HideAll()
        {
            ShowPlayer = false;
            ShowOpponent = false;
            HidePlayerInfo();
            HideOpponentInfo();
        }

        #endregion

        // ------------------------------

        #region Show/Hide/Move Controls

        private void ShowPlayerInfo(Texture2D profilePic, string username)
        {
            playerProfileImage.texture = profilePic;
            playerUsername.text = username;
            MoveCard(playerInfoCard, playerCardOnScreenPos, true);
            isShowingPlayer = true;
        }

        private void HidePlayerInfo()
        {
            MoveCard(playerInfoCard, playerCardOffScreenPos, false);
            isShowingPlayer = false;
        }

        private void ShowOpponentInfo(Texture2D profilePic, string username)
        {
            opponentProfileImage.texture = profilePic;
            opponentUsername.text = username;
            MoveCard(opponentInfoCard, opponentCardOnScreenPos, true);
            isShowingOpponent = true;
        }

        private void HideOpponentInfo()
        {
            MoveCard(opponentInfoCard, opponentCardOffScreenPos, false);
            isShowingOpponent = false;
        }

        // -----

        private void MoveCard(MainMenuCardObject card, Transform target, bool toActive)
        {
            if(!card.isActiveAndEnabled && !toActive)
                return;
            card.gameObject.SetActive(true);
            animController.MoveCardToPosition(movementType, card, target.position, target.rotation, onComplete: () =>
            {
                card.gameObject.SetActive(toActive);
            });
        }

        #endregion
    }
}
