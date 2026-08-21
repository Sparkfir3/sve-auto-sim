using System;
using System.Collections.Generic;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public class PlaySpellFromCemeteryEffect : ChooseFromCemeteryEffect
    {
        public override void Resolve(PlayerController player, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, Action onComplete = null)
        {
            amount = "1"; // always can only choose 1 spell at a time
            UpdateFilterForSpells();
            base.Resolve(player, triggeringCardInstanceId, triggeringCardZone, sourceCardInstanceId, sourceCardZone, onComplete);
        }

        protected virtual void UpdateFilterForSpells()
        {
            if(!filter.Contains("S"))
            {
                var filterDict = SVEFormulaParser.ParseCardFilterFormula(filter);
                if(!filterDict.ContainsKey(SVEFormulaParser.CardFilterSetting.Spell))
                    filter += "S";
            }
        }

        protected override void ConfirmationAction(PlayerController player, List<CardObject> selectedCards, Action onComplete)
        {
            if(selectedCards.Count == 0)
            {
                onComplete?.Invoke();
                return;
            }
            PlaySpell(player, selectedCards[0], onComplete);
        }

        protected virtual void PlaySpell(PlayerController player, CardObject card, Action onComplete, int? fixedCost = null)
        {
            bool spellPlayed = player.LocalEvents.PlaySpell(card, SVEProperties.Zones.Cemetery, fixedCost: fixedCost);
            if(!spellPlayed)
            {
                onComplete?.Invoke();
                return;
            }
            player.LocalEvents.OnFinishSpell += onComplete;
        }
    }
}
