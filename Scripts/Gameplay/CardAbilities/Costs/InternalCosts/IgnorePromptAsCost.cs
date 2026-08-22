using System.Collections;
using System.Collections.Generic;
using CCGKit;

namespace SVESimulator
{
    public class IgnorePromptAsCost : SveCost
    {
        public override bool IsInternalCost => true;

        // ------------------------------

        public override string GetReadableString(GameConfiguration config)
        {
            return "Ignore Prompt";
        }

        public override bool CanPayCost(PlayerController player, RuntimeCard card, string abilityName)
        {
            return false;
        }
    }
}
