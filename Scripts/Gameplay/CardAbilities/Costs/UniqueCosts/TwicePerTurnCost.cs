using CCGKit;

namespace SVESimulator
{
    public class TwicePerTurnCost : XTimesPerTurnCost
    {
        protected override int Amount => 2;

        public override string GetReadableString(GameConfiguration config)
        {
            return "Twice Per Turn";
        }
    }
}
