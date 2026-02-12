using System.Collections;
using System.Collections.Generic;
using CCGKit;

namespace SVESimulator
{
    public class QuickEffectAsCost : SveCost
    {
        public override bool IsInternalCost => true;

        // ------------------------------

        public override string GetReadableString(GameConfiguration config)
        {
            return "Is Quick Effect";
        }

        public override bool CanPayCost(PlayerController player, RuntimeCard card, string abilityName)
        {
            return true;
        }
    }
}
