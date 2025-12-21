using System;
using System.Collections.Generic;
using UnityEngine;
using CCGKit;
using Sparkfire.Utility;

namespace SVESimulator
{
    public class RedrawHandEffect : SveEffect
    {
        [StringField("Amount", width = 100), Order(2)]
        public string amount = "1";

        // ------------------------------

        public override void Resolve(PlayerController player, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, Action onComplete = null)
        {
            int drawCount = amount.IsNullOrWhiteSpace() ? player.ZoneController.handZone.AllCards.Count : SVEFormulaParser.ParseValue(amount, player);

            List<CardObject> handCards = new(player.ZoneController.handZone.AllCards);
            foreach(CardObject card in handCards)
                player.LocalEvents.SendToBottomDeck(card, SVEProperties.Zones.Hand);
            player.LocalEvents.ShuffleDeck();
            for(int i = 0; i < drawCount; i++)
                player.LocalEvents.DrawCard();
            onComplete?.Invoke();
        }
    }
}
