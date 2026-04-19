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
        PlayLocalJoin       = 5,
        BackToMain          = 6,
        Quit                = 7
    }

    public enum MainMenuAction
    {
        Other               = -1,
        SelectPlayOnline    = 0,
        SelectPlayLocal     = 1,
        Back                = 2,
        PlayOnlineBack      = 3,
        PlayLocalBack       = 4,
    }

    public enum MainMenuCardPosition
    {
        Static = 0,
        MainA = 1,
        MainB = 2,
        MainC = 3,
        Back = 4,
        CenterA = 5,
        CenterB = 6,
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
                _ => MainMenuAction.Other
            };
        }
    }
}
