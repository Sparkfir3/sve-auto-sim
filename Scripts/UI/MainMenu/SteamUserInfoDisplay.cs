using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using TMPro;

namespace SVESimulator.UI
{
    public class SteamUserInfoDisplay : MonoBehaviour
    {
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

        // ------------------------------

        private void Start()
        {
            playerInfoCard.gameObject.SetActive(false);
            playerInfoCard.transform.position = playerCardOffScreenPos.position;
            opponentInfoCard.gameObject.SetActive(false);
            opponentInfoCard.transform.position = opponentCardOffScreenPos.position;
        }

        // ------------------------------

        public void HideAll()
        {
            MoveCard(playerInfoCard, playerCardOffScreenPos, false);
            MoveCard(opponentInfoCard, opponentCardOffScreenPos, false);
        }

        public void ShowPlayerInfo(Texture2D profilePic, string username)
        {
            playerProfileImage.texture = profilePic;
            playerUsername.text = username;
            MoveCard(playerInfoCard, playerCardOnScreenPos, true);
        }

        public void HidePlayerInfo()
        {
            MoveCard(playerInfoCard, playerCardOffScreenPos, true);
        }

        public void ShowOpponentInfo(Texture2D profilePic, string username)
        {
            opponentProfileImage.texture = profilePic;
            opponentUsername.text = username;
            MoveCard(opponentInfoCard, opponentCardOnScreenPos, true);
        }

        public void HideOpponentInfo()
        {
            MoveCard(opponentInfoCard, opponentCardOffScreenPos, true);
        }

        // ------------------------------

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
    }
}
