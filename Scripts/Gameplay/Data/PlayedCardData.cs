using System;

namespace SVESimulator
{
    [Serializable]
    public struct PlayedCardData
    {
        public int instanceId;
        public int cardId;

        public PlayedCardData(int instanceId, int cardId)
        {
            this.instanceId = instanceId;
            this.cardId = cardId;
            LibraryCardCache.GetCard(this.cardId); // add card to cache
        }
    }
}
