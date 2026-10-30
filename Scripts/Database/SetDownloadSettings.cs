using System;
using System.Collections.Generic;
using UnityEngine;

namespace SVESimulator.Database.ImageDownloader
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

        // ------------------------------

        public static List<SetDownloadSettings> ParseDownloadSettingsLine(in string line)
        {
            List<SetDownloadSettings> list = new();
            string[] arguments = line?.Split('|') ?? null;
            if(arguments == null || arguments.Length < 2)
            {
                Debug.LogError($"Missing Arguments Error when processing set download settings: {line}");
                return list;
            }

            string setCode = arguments[0].Trim();
            for(int i = 1; i < arguments.Length; i++)
            {
                try
                {
                    string[] split = arguments[i].Trim().Split('-');
                    if(split.Length == 0)
                        continue;

                    int numberStartIndex = 0;
                    for(; numberStartIndex < split[0].Length; numberStartIndex++)
                        if(char.IsDigit(split[0][numberStartIndex]))
                            break;

                    string cardTypeRaw = numberStartIndex == 0 ? "" : split[0][..numberStartIndex].Trim();
                    string startRangeRaw = split[0][numberStartIndex..];
                    string endRangeRaw = split.Length > 1 ? split[1] : startRangeRaw;

                    CardType cardType = cardTypeRaw switch
                    {
                        "T" => CardType.Token,
                        "LD" => CardType.Leader,
                        "L" => CardType.Leader,
                        _ => CardType.Standard
                    };
                    int startIndex = int.Parse(startRangeRaw);
                    int endIndex = int.Parse(endRangeRaw);

                    list.Add(new SetDownloadSettings()
                    {
                        setCode = setCode,
                        cardType = cardType,
                        startIndex = startIndex,
                        endIndex = endIndex
                    });
                }
                catch(Exception e)
                {
                    Debug.LogError(e);
                }
            }
            return list;
        }
    }
}
