using System;
using System.Collections.Generic;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public class SvePlaySpellFromCemeterySetCostEffect : SvePlaySpellFromCemeteryEffect
    {
        [StringField("Play Point Cost", width = 100), Order(3)]
        public string amount2;

        // ------------------------------

        protected override void ConfirmationAction(PlayerController player, List<CardObject> selectedCards, Action onComplete)
        {
            if(selectedCards.Count == 0)
            {
                onComplete?.Invoke();
                return;
            }

            int playPointCost = SVEFormulaParser.ParseValue(amount2, player);
            bool spellPlayed = player.LocalEvents.PlaySpell(selectedCards[0], SVEProperties.Zones.Cemetery, fixedCost: playPointCost);
            if(!spellPlayed)
            {
                onComplete?.Invoke();
                return;
            }
            player.LocalEvents.OnFinishSpell += onComplete;
        }
    }
}
