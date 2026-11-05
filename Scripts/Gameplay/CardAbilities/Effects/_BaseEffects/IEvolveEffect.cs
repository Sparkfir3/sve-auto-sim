using CCGKit;

namespace SVESimulator
{
    public interface IEvolveEffect
    {
        public bool UseEvolveForTurn { get; }
        public bool CanEvolve(PlayerController player, RuntimeCard card);
    }
}
