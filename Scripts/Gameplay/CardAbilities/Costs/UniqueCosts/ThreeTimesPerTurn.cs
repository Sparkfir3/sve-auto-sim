using CCGKit;

namespace SVESimulator
{
    public class ThreeTimesPerTurn : XTimesPerTurnCost
    {
        protected override int Amount => 3;

        public override string GetReadableString(GameConfiguration config)
        {
            return "Three Times Per Turn";
        }
    }
}
