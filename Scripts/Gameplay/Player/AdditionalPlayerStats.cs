using Mirror;
using UnityEngine;

namespace SVESimulator
{
    public class AdditionalPlayerStats : NetworkBehaviour
    {
        public readonly SyncList<PlayedCardData> CardsPlayedThisTurn = new();
        public readonly SyncList<PlayedAbilityData> AbilitiesUsedThisTurn = new();

        // ------------------------------

        public void Reset()
        {
            CardsPlayedThisTurn.Clear();
            AbilitiesUsedThisTurn.Clear();
        }
    }
}
