using System;
using System.Collections.Generic;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public class PlayCardFromCemeterySetCostEffect : PlaySpellFromCemeteryEffect
    {
        [StringField("Play Point Cost", width = 100), Order(3)]
        public string amount2;

        // ------------------------------

        protected override void UpdateFilterForSpells()
        {
            // Do nothing - we can also select non-spells
        }

        protected override void ConfirmationAction(PlayerController player, List<CardObject> selectedCards, Action onComplete)
        {
            if(selectedCards.Count == 0)
            {
                onComplete?.Invoke();
                return;
            }

            CardObject card = selectedCards[0];
            int playPointCost = SVEFormulaParser.ParseValue(amount2, player);
            if(card.IsSpell())
            {
                PlaySpell(player, card, onComplete, fixedCost: playPointCost);
            }
            else
            {
                if(!player.LocalEvents.PlayCardToField(card, SVEProperties.Zones.Cemetery, fixedCost: playPointCost))
                    Debug.LogError($"PlayCardFromCemeterySetCost Effect - Failed to play target card with instance ID {card.RuntimeCard.instanceId}");
                else
                    card.Interactable = player.isActivePlayer;
                onComplete?.Invoke();
            }
        }
    }
}
