using System.Collections.Generic;
using CCGKit;

namespace SVESimulator.DeckBuilder
{
    public static class DeckConstructionRules
    {
        private const int DEFAULT_CARD_COUNT = 3;
        private static Dictionary<int, int> SpecialCardCountsByID = new()
        {
            { UmaUtilities.CarrotCardId, 10 } // Umamusume Carrot
        };

        // ------------------------------

        public static int GetMaxCardCount(Card card)
        {
            if(card == null)
                return 0;
            return SpecialCardCountsByID.GetValueOrDefault(card.id, DEFAULT_CARD_COUNT);
        }
    }
}
