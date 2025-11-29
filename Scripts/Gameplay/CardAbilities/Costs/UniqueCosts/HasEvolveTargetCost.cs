using CCGKit;

namespace SVESimulator
{
    public class HasEvolveTargetCost : SveCost
    {
        public override string GetReadableString(GameConfiguration config)
        {
            return "Has Evolve Target";
        }

        public override bool CanPayCost(PlayerController player, RuntimeCard card, string abilityName)
        {
            return card != null && player.ZoneController.EvolveDeckHasEvolvedVersionOf(card);
        }
    }
}
