using System.Linq;
using CCGKit;

namespace SVESimulator
{
    public abstract class XTimesPerTurnCost : SveCost
    {
        public override bool IsInternalCost => true;
        protected abstract int Amount { get; }

        public override bool CanPayCost(PlayerController player, RuntimeCard card, string abilityName)
        {
            return player.AdditionalStats.AbilitiesUsedThisTurn.Count(x => x.instanceId == card.instanceId && x.cardId == card.cardId
                && x.abilityName.Equals(abilityName)) < Amount;
        }
    }
}
