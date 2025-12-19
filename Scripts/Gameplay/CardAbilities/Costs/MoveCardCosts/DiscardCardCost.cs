using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public class DiscardCardCost : SveCost
    {
        [StringField("Amount", width = 100), Order(1)]
        public string amount;

        [StringField("Target Filter", width = 100), Order(2)]
        public string filter;

        // ------------------------------

        public override string GetReadableString(GameConfiguration config)
        {
            return $"Discard {amount} {filter}";
        }

        public override bool CanPayCost(PlayerController player, RuntimeCard card, string abilityName)
        {
            var filters = SVEFormulaParser.ParseCardFilterFormula(filter);
            int value = SVEFormulaParser.ParseValue(amount, player);
            return player.GetPlayerInfo().namedZones[SVEProperties.Zones.Hand].cards.Count(x => filters.MatchesCard(x)) >= value;
        }

        public override IEnumerator PayCost(PlayerController player, CardObject card, string abilityName, List<MoveCardToZoneData> cardsToMove)
        {
            bool waiting = true;
            int discardAmount = SVEFormulaParser.ParseValue(amount);
            CardSelectionArea selectionArea = player.ZoneController.selectionArea;

            selectionArea.Enable(CardSelectionArea.SelectionMode.PlaceCardsFromHand, discardAmount, discardAmount);
            selectionArea.SetFilter(filter);
            selectionArea.SetConfirmAction(LibraryCardCache.GetCard(card.RuntimeCard.cardId).name,
                "Discard",
                LibraryCardCache.GetEffectTextCost(card.LibraryCard.id, abilityName),
                discardAmount, discardAmount,
                cards =>
                {
                    foreach(CardObject discard in cards)
                    {
                        player.ZoneController.SendCardToCemetery(discard);
                        cardsToMove.Add(new MoveCardToZoneData(discard.RuntimeCard.instanceId, SVEProperties.Zones.Hand, SVEProperties.Zones.Cemetery));
                    }
                    waiting = false;
                });

            yield return new WaitUntil(() => !waiting);
            selectionArea.Disable();
            yield return null;
        }
    }
}
