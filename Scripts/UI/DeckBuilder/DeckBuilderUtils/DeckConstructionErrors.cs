using System;

namespace SVESimulator
{
    [Flags]
    public enum DeckConstructionErrors
    {
        None = 0,
        NoName = 1,
        NoLeader = 2,
        TooFewMainDeck = 4,
        TooMuchMainDeck = 8,
        TooMuchEvolveDeck = 16,
    }
}
