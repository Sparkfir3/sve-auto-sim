using System;

namespace SVESimulator.DeckBuilder
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
        NonStandard = 32,
    }
}
