using System;
using System.Collections.Generic;

namespace SVESimulator
{
    public class CemeteryToDeckAndShuffleEffect : ChooseFromCemeteryEffect
    {
        protected override void ConfirmationAction(PlayerController player, List<CardObject> selectedCards, Action onComplete)
        {
            foreach(CardObject card in selectedCards)
            {
                card.Interactable = player.isActivePlayer;
                player.LocalEvents.SendToBottomDeck(card, SVEProperties.Zones.Cemetery);
            }
            player.LocalEvents.ShuffleDeck();
            onComplete?.Invoke();
        }
    }
}
