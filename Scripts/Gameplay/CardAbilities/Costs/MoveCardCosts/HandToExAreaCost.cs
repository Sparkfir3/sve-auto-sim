using System.Collections.Generic;
using System.Linq;
using CCGKit;

namespace SVESimulator
{
    public class HandToExAreaCost : DiscardCardCost
    {
        public override string GetReadableString(GameConfiguration config)
        {
            return $"Hand to EX Area {amount} {filter}";
        }

        public override bool CanPayCost(PlayerController player, RuntimeCard card, string abilityName)
        {
            var filters = SVEFormulaParser.ParseCardFilterFormula(filter);
            int value = SVEFormulaParser.ParseValue(amount, player);
            return player.GetPlayerInfo().namedZones[SVEProperties.Zones.Hand].cards.Count(x => filters.MatchesCard(x)) >= value
                && player.GetPlayerInfo().namedZones[SVEProperties.Zones.ExArea].cards.Count + value <= 5;
        }

        protected override void MoveCard(PlayerController player, CardObject card, List<MoveCardToZoneData> cardsToMove)
        {
            player.LocalEvents.SendToExArea(card, originZone: SVEProperties.Zones.Hand, onlyMoveObject: true);
            cardsToMove.Add(new MoveCardToZoneData(card.RuntimeCard.instanceId, SVEProperties.Zones.Hand, SVEProperties.Zones.ExArea));
        }
    }
}
