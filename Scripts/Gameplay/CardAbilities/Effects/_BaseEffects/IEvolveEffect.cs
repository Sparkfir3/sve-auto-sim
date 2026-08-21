using CCGKit;

namespace SVESimulator
{
    public interface IEvolveEffect
    {
        public bool CanEvolve(PlayerController player, RuntimeCard card);
    }
}
