using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public class RevealTopDeckEffect : CheckTopDeckEffect
    {
        protected override bool ShowTargetingToOpponent => false;

        // ------------------------------

        protected override IEnumerator AddCardsFromDeck(PlayerController player, CardSelectionArea selectionArea, int sourceCardInstanceId, int minCheck, int maxCheck)
        {
            // Resolve
            List<CardObject> cards = new();
            for(int i = 0; i < minCheck; i++)
            {
                selectionArea.AddCardFromTopDeck(out CardObject card);
                cards.Add(card);
                yield return new WaitForSeconds(selectionArea.AddRemoveCardDelay);
            }

            player.LocalEvents.RevealTopDeckToOpponent(CardManager.Instance.GetCardByInstanceId(sourceCardInstanceId), name, cards);
        }

        protected override void OnCompleteInternal(PlayerController player)
        {
            player.LocalEvents.CloseRevealTopDeckToOpponent();
        }
    }
}
