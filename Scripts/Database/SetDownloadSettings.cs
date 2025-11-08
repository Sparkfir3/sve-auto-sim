using System;

namespace SVESimulator.Database.Scraper
{
    [Serializable]
    public class SetDownloadSettings
    {
        public enum CardType { Standard, Token, Leader }
        public enum CardLanguage { JP, EN }

        public string setCode;
        public CardType cardType = CardType.Standard;
        public int startIndex = 1;
        public int endIndex = 100;
        public CardLanguage language = CardLanguage.EN;

        public string GetFullCardId(int id) => $"{setCode}-{GetCardIdNumberWithLanguage(id)}";

        public string GetCardIdNumber(int id) => cardType switch
        {
            CardType.Token => $"T{id.ToString("D2")}",
            CardType.Leader => $"LD{id.ToString("D2")}",
            _ => $"{id.ToString("D3")}"
        };

        public string GetCardIdNumberWithLanguage(int id) => $"{GetCardIdNumber(id)}{(language == CardLanguage.EN ? "EN" : "")}";
    }
}
