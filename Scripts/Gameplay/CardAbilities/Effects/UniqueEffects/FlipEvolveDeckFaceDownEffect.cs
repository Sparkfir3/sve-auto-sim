using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public class FlipEvolveDeckFaceDownEffect : SveEffect
    {
        [StringField("Filter", width = 100), Order(1)]
        public string filter;

        // ------------------------------

        public override void Resolve(PlayerController player, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, Action onComplete = null)
        {
            Dictionary<SVEFormulaParser.CardFilterSetting, string> filterDict = SVEFormulaParser.ParseCardFilterFormula(filter, sourceCardInstanceId);
            List<RuntimeCard> cards = filterDict.Count == 0
                ? null
                : player.GetPlayerInfo().namedZones[SVEProperties.Zones.EvolveDeck].cards.Where(x =>
                    x.namedStats.TryGetValue(SVEProperties.CardStats.FaceUp, out Stat faceUpStat) && faceUpStat.effectiveValue == 1
                    && filterDict.MatchesCard(x)).ToList();

            player.LocalEvents.FlipEvolveDeckCards(toFaceDown: true, cards);
            onComplete?.Invoke();
        }
    }
}
