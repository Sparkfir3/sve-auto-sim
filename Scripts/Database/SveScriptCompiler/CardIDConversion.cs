using System;
using System.Collections.Generic;
using UnityEngine;

namespace SVESimulator.SveScript
{
    public static class CardIDConversion
    {
        public static int CardIdToCCGKitId(in string cardId)
        {
            try
            {
                string ccgKitId = cardId;
                foreach(KeyValuePair<string, int> kvPair in SpecialCardTypeToID) // Reformat leaders and tokens
                {
                    ccgKitId = ccgKitId.Replace(kvPair.Key, kvPair.Value.ToString());
                }
                ccgKitId = ccgKitId.Replace("-", "");
                foreach(KeyValuePair<string, int> kvPair in SetTypeToID)
                {
                    ccgKitId = ccgKitId.Replace(kvPair.Key, kvPair.Value.ToString());
                }
                return int.Parse(ccgKitId);
            }
            catch
            {
                Debug.LogError($"Invalid Card ID Provided: {cardId}");
                return 0;
            }
        }

        private static readonly Dictionary<string, int> SpecialCardTypeToID = new()
        {
            { "-T", 8 },
            { "-LD", 9 }
        };

        private static readonly Dictionary<string, int> SetTypeToID = new()
        {
            { "SD", 1 },
            { "BP", 2 }
        };
    }
}
