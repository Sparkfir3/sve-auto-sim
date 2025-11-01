using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using System.Linq;

namespace SVESimulator
{
    public class GameDebugControls : MonoBehaviour
    {
        [SerializeField]
        private PlayerController _player;

        private PlayerController Player
        {
            get
            {
                if(!_player)
                    _player = FindObjectsByType<PlayerController>(FindObjectsSortMode.None).First(x => x.isLocalPlayer);
                return _player;
            }
        }

        // ------------------------------

        [Button]
        public void DrawCard()
        {
            Player.LocalEvents.DrawCard();
        }

        [Button]
        public void Mulligan()
        {
            Player.LocalEvents.Mulligan(true, endTurn: false);
        }

        [Button]
        public void Search()
        {
            SveSearchDeckEffect searchEffect = new()
            {
                amount = "1",
                filter = null,
                searchDeckAction = SveSearchDeckEffect.SearchDeckAction.Hand,
                text = "[Debug Mode] Search for a card"
            };
            SVEEffectPool.Instance.ResolveEffectImmediate(searchEffect, Player.ZoneController.deckZone.Runtime.cards[0]); // need a dummy card to perform the effect from lol
        }

        [Button]
        public void IncrementPlayPoints()
        {
            Player.LocalEvents.IncrementMaxPlayPoints(updateCurrentPoints: true);
        }

        [Button]
        public void Mill()
        {
            Player.LocalEvents.MillDeck(1);
        }
    }
}
