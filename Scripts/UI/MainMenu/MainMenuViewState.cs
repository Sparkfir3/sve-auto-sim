namespace SVESimulator.UI
{
    public enum MainMenuViewState
    {
        Main = 0,
        PlayOnline = 1,
        PlayLocal = 2
    }

    public enum MainMenuButton
    {
        SelectPlayOnline    = 0,
        PlayOnlineHost      = 1,
        PlayOnlineJoin      = 2,
        SelectPlayLocal     = 3,
        PlayLocalHost       = 4,
        PlayLocalStart      = 5,
        BackToMain          = 6,
        Quit                = 7
    }

    public enum MainMenuAction
    {
        SelectPlayOnline    = 0,
        PlayOnlineBack      = 1,
        SelectPlayLocal     = 2,
        PlayLocalBack       = 3,
    }

    public enum MainMenuCardPosition
    {
        Static = 0,
        A = 1,
        B = 2,
        C = 3,
    }
}
