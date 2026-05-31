using System.Linq;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public abstract class ChooseFromEvolveDeckEffect : ChooseFromCardStackEffect
    {
        protected abstract bool TargetFaceDownCards { get; }
        protected abstract bool TargetFaceUpCards { get; }

        protected override void InitializeSelectionArea(PlayerController player, CardSelectionArea selectionArea)
        {
            int cardCount = TargetFaceUpCards && TargetFaceDownCards
                ? player.ZoneController.evolveDeckZone.Runtime.cards.Count
                : (player.GetPlayerInfo().namedZones[SVEProperties.Zones.EvolveDeck].cards.Count(x =>
                    x.namedStats.TryGetValue(SVEProperties.CardStats.FaceUp, out Stat faceUpStat) && faceUpStat.effectiveValue == (TargetFaceUpCards ? 1 : 0)));
            selectionArea.Enable(CardSelectionArea.SelectionMode.SelectCardsFromEvolveDeck, cardCount, cardCount, slotBackgroundsActive: false);
            selectionArea.SetFilter(filter);
            selectionArea.AddEvolveDeck(TargetFaceDownCards, TargetFaceUpCards);
        }
    }
}
