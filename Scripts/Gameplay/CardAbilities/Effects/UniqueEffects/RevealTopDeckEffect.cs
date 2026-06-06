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
            // Get ability name (for networked text)
            List<Ability> abilities = LibraryCardCache.GetCardFromInstanceId(sourceCardInstanceId)?.abilities
                .Where(x => x is TriggeredAbility ability && ability.effect is RevealTopDeckEffect).ToList();
            if(abilities == null || abilities.Count == 0)
                Debug.LogError($"{nameof(RevealTopDeckEffect)} failed to find an ability matching it's type on card with instance ID {sourceCardInstanceId}.");
            else if(abilities.Count > 1)
                Debug.LogError($"{nameof(RevealTopDeckEffect)} found more than one ability matching it's type on card with instance ID {sourceCardInstanceId}, defaulting to the first one found.");
            string abilityName = abilities?[0]?.name;

            // Resolve
            List<CardObject> cards = new();
            for(int i = 0; i < minCheck; i++)
            {
                selectionArea.AddCardFromTopDeck(out CardObject card);
                cards.Add(card);
                yield return new WaitForSeconds(selectionArea.AddRemoveCardDelay);
            }

            player.LocalEvents.RevealTopDeck(CardManager.Instance.GetCardByInstanceId(sourceCardInstanceId), abilityName, cards);
        }

        protected override void OnCompleteInternal(PlayerController player)
        {
            player.LocalEvents.CloseRevealTopDeck();
        }
    }
}
