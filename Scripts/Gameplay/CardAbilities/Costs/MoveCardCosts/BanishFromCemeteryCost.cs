using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public class BanishFromCemeteryCost : SveCost
    {
        [StringField("Amount", width = 100), Order(1)]
        public string amount;

        [StringField("Target Filter", width = 100), Order(2)]
        public string filter;

        // ------------------------------

        public override string GetReadableString(GameConfiguration config)
        {
            return $"Banish {amount} {filter} from Cemetery";
        }

        public override bool CanPayCost(PlayerController player, RuntimeCard card, string abilityName)
        {
            var filters = SVEFormulaParser.ParseCardFilterFormula(filter);
            int value = SVEFormulaParser.ParseValue(amount, player);
            return player.GetPlayerInfo().namedZones[SVEProperties.Zones.Cemetery].cards.Count(x => filters.MatchesCard(x)) >= value;
        }

        public override IEnumerator PayCost(PlayerController player, CardObject card, string abilityName, List<MoveCardToZoneData> cardsToMove)
        {
            bool waiting = true;
            int banishAmount = SVEFormulaParser.ParseValue(amount);
            int cemeteryCount = player.ZoneController.cemeteryZone.AllCards.Count;
            CardSelectionArea selectionArea = player.ZoneController.selectionArea;

            selectionArea.Enable(CardSelectionArea.SelectionMode.SelectCardsFromCemetery, cemeteryCount, cemeteryCount, slotBackgroundsActive: false);
            selectionArea.SetFilter(filter);
            selectionArea.AddCemetery();
            selectionArea.SetConfirmAction(LibraryCardCache.GetCard(card.RuntimeCard.cardId).name,
                "Banish",
                LibraryCardCache.GetEffectTextCost(card.LibraryCard.id, abilityName),
                banishAmount, banishAmount,
                cards =>
                {
                    foreach(CardObject banish in cards)
                    {
                        player.ZoneController.SendCardToBanishedZone(banish);
                        cardsToMove.Add(new MoveCardToZoneData(banish.RuntimeCard.instanceId, SVEProperties.Zones.Cemetery, SVEProperties.Zones.Banished));
                    }
                    waiting = false;
                });

            yield return new WaitUntil(() => !waiting);
            selectionArea.Disable();
            yield return null;
        }
    }
}
