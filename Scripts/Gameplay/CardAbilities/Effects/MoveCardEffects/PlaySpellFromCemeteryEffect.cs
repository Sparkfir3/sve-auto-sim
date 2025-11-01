using System;
using System.Collections.Generic;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public class SvePlaySpellFromCemeteryEffect : SveChooseFromCemeteryEffect
    {
        public override void Resolve(PlayerController player, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, Action onComplete = null)
        {
            if(!filter.Contains("S"))
            {
                var filterDict = SVEFormulaParser.ParseCardFilterFormula(filter);
                if(!filterDict.ContainsKey(SVEFormulaParser.CardFilterSetting.Spell))
                    filter += "S";
            }
            amount = "1"; // always can only choose 1 spell at a time
            base.Resolve(player, triggeringCardInstanceId, triggeringCardZone, sourceCardInstanceId, sourceCardZone, onComplete);
        }

        protected override void ConfirmationAction(PlayerController player, List<CardObject> selectedCards, Action onComplete)
        {
            if(selectedCards.Count == 0)
            {
                onComplete?.Invoke();
                return;
            }

            bool spellPlayed = player.LocalEvents.PlaySpell(selectedCards[0], SVEProperties.Zones.Cemetery);
            if(!spellPlayed)
            {
                onComplete?.Invoke();
                return;
            }
            player.LocalEvents.OnFinishSpell += onComplete;
        }
    }
}
