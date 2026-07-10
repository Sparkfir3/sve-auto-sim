using System.Collections;
using System.Collections.Generic;
using CCGKit;

namespace SVESimulator
{
    public class OptionalEffectAsCost : SveCost
    {
        // Technically an internal effect, but read as non-internal for checking cost payment
        public override bool IsInternalCost => false;

        // ------------------------------

        public override string GetReadableString(GameConfiguration config)
        {
            return "Optional Effect";
        }

        public override bool CanPayCost(PlayerController player, RuntimeCard card, string abilityName)
        {
            return true;
        }
    }
}
