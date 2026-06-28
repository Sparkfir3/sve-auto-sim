namespace SVESimulator.UI
{
    public enum MainMenuViewState
    {
        Main            = 0,
        PlayOnline      = 1,
        PlayLocal       = 2,
        Disconnect      = 3,
        Connecting      = 4,
        ReadyToStart    = 5,
    }

    public enum MainMenuButton
    {
        SelectPlayOnline    = 0,
        PlayOnlineHost      = 1,
        PlayOnlineJoin      = 2,
        SelectPlayLocal     = 3,
        PlayLocalHost       = 4,
        PlayLocalJoin       = 5,
        BackToMain          = 6,
        Quit                = 7,
        WaitingOnOpponent   = 8,
        StartGame           = 9,
    }

    public enum MainMenuAction
    {
        Other               = -1,
        SelectPlayOnline    = 0,
        SelectPlayLocal     = 1,
        Back                = 2,
        PlayOnlineBack      = 3,
        PlayLocalBack       = 4,
        Disconnect          = 5,
        Connecting          = 6,
        ReadyToStart        = 7,
        OppDisconnected     = 8,
    }

    public enum MainMenuCardPosition
    {
        Static = 0,
        MainA = 1,
        MainALower1 = 11,
        MainALower2 = 12,
        MainB = 2,
        MainBLower1 = 21,
        MainBLower2 = 22,
        MainC = 3,
        MainCLower1 = 31,
        MainCLower2 = 32,
        Back = 4,
        BackLower1 = 41,
        BackLower2 = 42,
        CenterA = 5,
        CenterB = 6,
        CenterC = 7,
    }

    // ------------------------------

    public static class MainMenuEnumExtensions
    {
        public static MainMenuAction BackAction(this MainMenuViewState state)
        {
            return state switch
            {
                MainMenuViewState.PlayOnline => MainMenuAction.PlayOnlineBack,
                MainMenuViewState.PlayLocal => MainMenuAction.PlayLocalBack,
                MainMenuViewState.Connecting => MainMenuAction.Disconnect,
                MainMenuViewState.ReadyToStart => MainMenuAction.Disconnect,
                _ => MainMenuAction.Other
            };
        }
    }
}
