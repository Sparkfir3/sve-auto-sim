using CCGKit;

namespace SVESimulator
{
    public class OncePerTurnCost : XTimesPerTurnCost
    {
        protected override int Amount => 1;

        public override string GetReadableString(GameConfiguration config)
        {
            return "Once Per Turn";
        }
    }
}
