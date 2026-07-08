using System.Collections;
using System.Collections.Generic;
using CCGKit;

namespace SVESimulator
{
    public class GivenAbilityAsCost : SveCost
    {
        public override bool IsInternalCost => true;

        // ------------------------------

        public override string GetReadableString(GameConfiguration config)
        {
            return "Is Given Ability";
        }

        public override bool CanPayCost(PlayerController player, RuntimeCard card, string abilityName)
        {
            return false;
        }
    }
}
