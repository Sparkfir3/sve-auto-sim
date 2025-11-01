using System.Collections.Generic;
using CCGKit;

namespace SVESimulator
{
    public static class CounterUtilities
    {
        public static readonly ActivatedAbility InnateStackAbility = new()
        {
            name = "Innate Stack",
            effect = new SveMoveCountersEffect
            {
                text = "<sprite index=23>: Select another amulet with Stack on your field and transfer all this card's Stack counters to that card.",
                keywordType = (int)SVEProperties.Counters.Stack,
                keywordValue = 0,
                amount = null,
                target = SVEProperties.SVEEffectTarget.TargetPlayerCard,
                filter = "Ar(Stack)X",
            },
            target = null,

            zoneId = 0,
            costs = new List<Cost>
            {
                new ConditionAsCost()
                {
                    condition = "f(Ar(Stack)X)"
                },
                new EngageSelfCost()
            },
        };

        // ------------------------------

        /// <summary>
        /// Handle rules processes when a card with Stack would leave the field
        /// </summary>
        /// <param name="player">Owner player</param>
        /// <param name="card">The card leaving the field</param>
        /// <returns>Returns true if the card has Stack and rules processes were handled, and false otherwise</returns>
        public static bool HandleStackLeaveField(PlayerController player, CardObject card)
        {
            if(!card || card.CurrentZone != player.ZoneController.fieldZone)
                return false;
            int stackCount = card.CountOfCounter(SVEProperties.Counters.Stack);
            if(stackCount <= 0)
                return false;

            // Send to cemetery is handled by RemoveCountersFromCard if remaining Stack is 0
            player.LocalEvents.RemoveCountersFromCard(card.RuntimeCard, SVEProperties.Counters.Stack, 1);
            return true;
        }
    }
}
