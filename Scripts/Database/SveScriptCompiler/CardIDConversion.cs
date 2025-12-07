using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SVESimulator.SveScript
{
    public static class CardIDConversion
    {
        /*
         * Card ID Format:
         * AAL-BBB-T-XXX
         *      AA  = Set type
         *      L   = Language (1 = EN, 0 = JP)
         *      BBB = Set number
         *      T   = Card type (1 = regular, 2 = leader, 3 = token)
         *      XXX = Card number
         */

        public static int CardIdToCCGKitId(in string cardId)
        {
            try
            {
                string[] idSegments = cardId.Trim().Split('-');

                string setTypeRaw = SetTypeToID.First(x => idSegments[0].Contains(x.Key)).Key;
                int setTypeId = SetTypeToID[setTypeRaw];
                int setNumber = int.Parse(idSegments[0].Replace(setTypeRaw, ""));

                int language = idSegments[1].EndsWith("EN") ? 1 : 0;
                idSegments[1] = idSegments[1].Replace("EN", "");
                int cardType = 1;
                if(idSegments[1].StartsWith("LD")) // Leader
                {
                    cardType = 2;
                    idSegments[1] = idSegments[1].Replace("LD", "");
                }
                else if(idSegments[1].StartsWith("T")) // Token
                {
                    cardType = 3;
                    idSegments[1] = idSegments[1].Replace("T", "");
                }
                int cardNumber = int.Parse(idSegments[1]);

                return int.Parse($"{setTypeId}{language}{setNumber:D3}{cardType}{cardNumber:D3}");
            }
            catch(Exception e)
            {
                Debug.LogError($"Invalid Card ID Provided: {cardId}\n{e}");
                return 0;
            }
        }

        private static readonly Dictionary<string, int> SetTypeToID = new()
        {
            { "BP", 1 },
            { "SD", 2 }
        };
    }
}
