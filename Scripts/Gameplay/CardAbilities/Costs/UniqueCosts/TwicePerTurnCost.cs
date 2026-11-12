using System.Linq;
using CCGKit;

namespace SVESimulator
{
    public class TwicePerTurnCost : SveCost
    {
        public override bool IsInternalCost => true;

        public override string GetReadableString(GameConfiguration config)
        {
            return "Twice Per Turn";
        }

        public override bool CanPayCost(PlayerController player, RuntimeCard card, string abilityName)
        {
            return player.AdditionalStats.AbilitiesUsedThisTurn.Count(x => x.instanceId == card.instanceId && x.cardId == card.cardId && x.abilityName.Equals(abilityName)) < 2;
        }
    }
}
