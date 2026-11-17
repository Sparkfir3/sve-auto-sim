using CCGKit;

namespace SVESimulator
{
    public class FourTimesPerTurn : XTimesPerTurnCost
    {
        protected override int Amount => 4;

        public override string GetReadableString(GameConfiguration config)
        {
            return "Four Times Per Turn";
        }
    }
}
