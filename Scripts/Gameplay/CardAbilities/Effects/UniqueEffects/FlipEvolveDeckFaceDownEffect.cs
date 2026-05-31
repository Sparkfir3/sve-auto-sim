using System;
using System.Collections.Generic;
using System.Linq;
using Sparkfire.Utility;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public class FlipEvolveDeckFaceDownEffect : ChooseFromEvolveDeckEffect
    {
        protected override bool TargetFaceDownCards => false;
        protected override bool TargetFaceUpCards => true;

        // ------------------------------

        public override void Resolve(PlayerController player, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, Action onComplete = null)
        {
            if(!amount.IsNullOrWhiteSpace())
            {
                base.Resolve(player, triggeringCardInstanceId,  triggeringCardZone, sourceCardInstanceId, sourceCardZone, onComplete);
                return;
            }

            Dictionary<SVEFormulaParser.CardFilterSetting, string> filterDict = SVEFormulaParser.ParseCardFilterFormula(filter, sourceCardInstanceId);
            List<RuntimeCard> cards = filterDict.Count == 0
                ? null
                : player.GetPlayerInfo().namedZones[SVEProperties.Zones.EvolveDeck].cards.Where(x =>
                    x.namedStats.TryGetValue(SVEProperties.CardStats.FaceUp, out Stat faceUpStat) && faceUpStat.effectiveValue == 1
                    && filterDict.MatchesCard(x)).ToList();

            player.LocalEvents.FlipEvolveDeckCards(toFaceDown: true, cards);
            onComplete?.Invoke();
        }

        protected override void ConfirmationAction(PlayerController player, List<CardObject> selectedCards, Action onComplete)
        {
            player.LocalEvents.FlipEvolveDeckCards(toFaceDown: true, selectedCards.Select(x => x.RuntimeCard).ToList());
            onComplete?.Invoke();
        }
    }
}
