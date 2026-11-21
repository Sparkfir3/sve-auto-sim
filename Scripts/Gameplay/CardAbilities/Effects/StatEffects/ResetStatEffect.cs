using System.Linq;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public class ResetStatEffect : SetStatEffect
    {
        protected override int GetStatAmount(PlayerController player, int sourceCardInstanceId, string sourceCardZone)
        {
            Card libraryCard = LibraryCardCache.GetCardFromInstanceId(sourceCardInstanceId);
            string[] stats = targetStats.AsNamedStatArray();
            Debug.Assert(stats.Length == 1, $"{nameof(ResetStatEffect)} from card {sourceCardInstanceId} in {sourceCardZone} had " +
                $"invalid targetStats value of {targetStats}. Only 1 type of stat at a time is supported");
            return libraryCard.stats.FirstOrDefault(x => x.name.Equals(stats[0]))?.baseValue ?? 0;
        }
    }
}
