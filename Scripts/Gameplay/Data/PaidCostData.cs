namespace SVESimulator
{
    public struct PaidCostData
    {
        public MoveCardToZoneData[] CardsToMove;
        public MoveCardToZoneData[] FusedCards;
        public RemoveCounterData[] CountersToRemove;
        public int[] EngagedCardsInstanceIds;
    }
}
