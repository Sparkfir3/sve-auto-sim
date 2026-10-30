using System;
using System.Collections.Generic;
using System.Linq;
using Sparkfire.Utility;
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
                string idSet = idSegments[0];
                string idNumber = idSegments[1];

                string setTypeRaw = new(idSet.Where(x => !char.IsDigit(x)).ToArray());
                int setTypeId = SetTypeToID.GetValueOrDefault(setTypeRaw, 99);
                string setNumberRaw = new(idSet.Where(char.IsDigit).ToArray());
                int setNumber = setNumberRaw.IsNullOrWhiteSpace() ? 0 : int.Parse(setNumberRaw);

                int language = idNumber.EndsWith("EN") ? 1 : 0;
                idNumber = idNumber.Replace("EN", "");
                int cardType = 1;
                if(idNumber.StartsWith("LD")) // Leader
                {
                    cardType = 2;
                    idNumber = idNumber.Replace("LD", "");
                }
                else if(idNumber.StartsWith("T")) // Token
                {
                    cardType = 3;
                    idNumber = idNumber.Replace("T", "");
                }
                int cardNumber = int.Parse(idNumber);

                return int.Parse($"{setTypeId}{language}{setNumber:D3}{cardType}{cardNumber:D3}");
            }
            catch(Exception e)
            {
                SveScriptDebugger.LogError($"Invalid Card ID Provided: {cardId}\n{e}");
                return 0;
            }
        }

        private static readonly Dictionary<string, int> SetTypeToID = new()
        {
            { "BP",     1 },
            { "SD",     2 },
            { "CP",     3 },
            { "CSD",    4 },
            { "CSDa",   4 },
            { "CSDb",   5 },
            { "CSDc",   6 },
            { "SS",     7 },
            { "SP",     8 },
            { "ECP",    9 },
            { "SDD",    10 },
            { "PCS",    11 },

            { "PR",     20 },
            // Default/Custom Set: 99
        };
    }
}
