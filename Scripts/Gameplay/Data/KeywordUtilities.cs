namespace SVESimulator
{
    // Utility class for quick-accessing keyword IDs and values
    public static class KeywordUtilities
    {
        public class KeywordUtilId
        {
            public string Name;
            public int Id;
            public int Value;
        }

        // ------------------------------

        public static readonly KeywordUtilId Rush = new()
        {
            Name = SVEProperties.Keywords.Rush,
            Id = 0,
            Value = 2
        };

        public static readonly KeywordUtilId IsRacing = new()
        {
            Name = SVEProperties.PassiveAbilities.IsRacing,
            Id = 1,
            Value = 10
        };
    }
}
