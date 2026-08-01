using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public class DiscardRandomCardCost : SveCost
    {
        [StringField("Amount", width = 100), Order(1)]
        public string amount;

        // ------------------------------

        public override string GetReadableString(GameConfiguration config)
        {
            return $"Discard Random {amount}";
        }

        public override bool CanPayCost(PlayerController player, RuntimeCard card, string abilityName)
        {
            int value = SVEFormulaParser.ParseValue(amount, player);
            return player.GetPlayerInfo().namedZones[SVEProperties.Zones.Hand].cards.Count >= value;
        }

        public override IEnumerator PayCost(PlayerController player, CardObject card, string abilityName, List<MoveCardToZoneData> cardsToMove)
        {
            int discardAmount = SVEFormulaParser.ParseValue(amount, player);
            for(int i = 0; i < discardAmount; i++)
            {
                CardObject toDiscard = player.ZoneController.handZone.AllCards[player.LocalEvents.GetRandomNumber(0, player.ZoneController.handZone.AllCards.Count)];
                player.LocalEvents.SendToCemetery(toDiscard, onlyMoveObject: true);
                cardsToMove.Add(new MoveCardToZoneData(toDiscard.RuntimeCard.instanceId, SVEProperties.Zones.Hand, SVEProperties.Zones.Cemetery));
                yield return new WaitForSeconds(0.15f); // arbitrary delay
            }
            yield return new WaitForSeconds(0.5f); // wait for complete
        }
    }
}
