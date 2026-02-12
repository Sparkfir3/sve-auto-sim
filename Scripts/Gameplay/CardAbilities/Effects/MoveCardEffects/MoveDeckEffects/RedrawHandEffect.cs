using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
            player.StartCoroutine(ResolveOverTime());
            IEnumerator ResolveOverTime()
            {
                yield return null;
                int drawCount = amount.IsNullOrWhiteSpace() ? player.ZoneController.handZone.AllCards.Count : SVEFormulaParser.ParseValue(amount, player);

                List<CardObject> handCards = new(player.ZoneController.handZone.AllCards);
                foreach(CardObject card in handCards)
                    player.LocalEvents.SendToBottomDeck(card, SVEProperties.Zones.Hand);

                yield return new WaitUntil(() => handCards.All(x => !x.gameObject.activeInHierarchy));
                yield return new WaitForSeconds(0.25f);
                player.LocalEvents.ShuffleDeck();
                yield return new WaitForSeconds(0.75f);

                for(int i = 0; i < drawCount; i++)
                    player.LocalEvents.DrawCard();
                yield return null;
                onComplete?.Invoke();
            }
        }
    }
}
