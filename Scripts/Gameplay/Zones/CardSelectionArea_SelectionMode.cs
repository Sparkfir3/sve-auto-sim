namespace SVESimulator
{
    public partial class CardSelectionArea
    {
        public enum SelectionMode
        {
            // Move cards
            PlaceCardsFromHand = 0,
            MoveSelectionArea = 4,

            // Select cards
            SelectCardsFromDeck = 1,
            SelectCardsFromCemetery = 2,
            SelectCardsFromOppHand = 3,
            SelectCardsFromEvolveDeck = 12,

            // Select cards & move
            SelectCardsFromDeckAndMove = 11,

            // View zone
            ViewCardsOppDeck = 13,
            ViewCardsCemetery = 5,
            ViewCardsOppCemetery = 6,
            ViewCardsEvolveDeck = 7,
            ViewCardsOppEvolveDeck = 8,
            ViewCardsBanished = 9,
            ViewCardsOppBanished = 10,
        }

        public static bool IsModeSelectCards(SelectionMode mode) => mode is SelectionMode.SelectCardsFromDeck or SelectionMode.SelectCardsFromCemetery
            or SelectionMode.SelectCardsFromOppHand or SelectionMode.SelectCardsFromEvolveDeck or SelectionMode.SelectCardsFromDeckAndMove;
    }
}
