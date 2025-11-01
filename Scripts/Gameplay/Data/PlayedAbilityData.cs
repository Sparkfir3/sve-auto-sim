using System;
using UnityEngine;

namespace SVESimulator
{
    [Serializable]
    public struct PlayedAbilityData
    {
        public int instanceId;
        public int cardId;
        public string abilityName;

        public PlayedAbilityData(int instanceId, int cardId, string abilityName)
        {
            this.instanceId = instanceId;
            this.cardId = cardId;
            this.abilityName = abilityName;
            LibraryCardCache.GetCard(this.cardId); // add card to cache
        }
    }
}
