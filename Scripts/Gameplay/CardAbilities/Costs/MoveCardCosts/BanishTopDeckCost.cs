using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public class BanishTopDeckCost : SveCost
    {
        [StringField("Amount", width = 100), Order(1)]
        public string amount;

        // ------------------------------

        public override string GetReadableString(GameConfiguration config)
        {
            return $"Banish {amount} from Top Deck";
        }

        public override bool CanPayCost(PlayerController player, RuntimeCard card, string abilityName)
        {
            int value = SVEFormulaParser.ParseValue(amount, player);
            return player.GetPlayerInfo().namedZones[SVEProperties.Zones.Deck].cards.Count >= value;
        }

        public override IEnumerator PayCost(PlayerController player, CardObject card, string abilityName, List<MoveCardToZoneData> cardsToMove)
        {
            bool waiting = true;
            int value = SVEFormulaParser.ParseValue(amount, player);
            player.LocalEvents.MillDeckToBanished(true, value, movedCards =>
            {
                foreach(RuntimeCard movedCard in movedCards)
                    cardsToMove.Add(new MoveCardToZoneData(movedCard.instanceId, SVEProperties.Zones.Deck, SVEProperties.Zones.Banished));
                waiting = false;
            }, onlyMoveObjects: true);
            yield return new WaitUntil(() => !waiting);
        }
    }
}
