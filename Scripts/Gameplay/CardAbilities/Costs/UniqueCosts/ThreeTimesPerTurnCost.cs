using CCGKit;

namespace SVESimulator
{
    public class ThreeTimesPerTurnCost : XTimesPerTurnCost
    {
        protected override int Amount => 3;

        public override string GetReadableString(GameConfiguration config)
        {
            return "Three Times Per Turn";
        }
    }
}
