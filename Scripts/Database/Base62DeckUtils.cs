using System;
using System.Collections.Generic;
using UnityEngine;

namespace SVESimulator.Database
{
    public static class Base62DeckUtils
    {
        private const int MAX_AMOUNT = 13; // 62^4 = 14,776,336 ==> after allocating last 6 digits to card ID, card amount caps at 13

        public static string CardIdToBase62String(int amount, int cardId)
        {
            Debug.Assert(amount <= MAX_AMOUNT);
            Debug.Assert(cardId <= 999999);

            char[] a = new char[4];
            int currentInput = int.Parse($"{amount}{cardId:D6}");
            for(int i = 1; i <= 4; i++)
            {
                int quotient = currentInput % 62;
                currentInput /= 62;
                a[^i] = Base62IntToChar(quotient);
            }
            return string.Join("", a);

            char Base62IntToChar(int value)
            {
                if(value < 10)
                    return (char)(value + 48); // 48 = ASCII '0'
                if(value < 36)
                    return (char)(value + 87); // 97 = ASCII 'a', -10 to offset first 10 digits
                return (char)(value + 29); // 65 = ASCII 'A', -36 to offset first 36 digits
            }
        }

        public static void Base62StringToCardId(string base62Id, out int amount, out int cardId)
        {
            int output = 0;
            for(int i = 1; i <= base62Id.Length; i++)
            {
                output += Base62CharToInt(base62Id[^i]) * (int)Mathf.Pow(62, i - 1);
            }
            string outputAsString = output.ToString();
            amount = int.Parse(outputAsString[0..^6]);
            cardId = int.Parse(outputAsString[^6..]);

            int Base62CharToInt(char value)
            {
                if(value >= 48 && value <= 57)
                    return value - 48; // 48 = ASCII '0'
                if(value >= 97 && value <= 122)
                    return value - 87; // 97 = ASCII 'a', -10 to offset first 10 digits
                if(value >= 65 && value <= 90)
                    return value - 29; // 65 = ASCII 'A', -36 to offset first 36 digits
                throw new ArgumentOutOfRangeException();
            }
        }

        // ------------------------------

        public class CardAmountPair
        {
            public int id;
            public int amount;

            public CardAmountPair(int id, int amount)
            {
                this.id = id;
                this.amount = amount;
            }
        }

        public static List<CardAmountPair> Base62StringToDeck(string input) => Base62StringToDeck(input, out _);
        public static List<CardAmountPair> Base62StringToDeck(string input, out string deckClass)
        {
            if(input.Length < 4)
            {
                deckClass = null;
                return null;
            }

            deckClass = ClassOfBase62Deck(input);
            input = input[2..];
            List<CardAmountPair> cards = new();
            for(int i = 0; i < input.Length; i += 4)
            {
                Base62StringToCardId(input.Substring(i, 4), out int amount, out int cardId);
                cards.Add(new CardAmountPair(cardId, amount));
            }
            return cards;
        }

        public static string ClassOfBase62Deck(in string input)
        {
            string metaData = input[..2];
            char classDeclaration = metaData.ToLower()[0];
            return classDeclaration switch
            {
                'f' => SVEProperties.CardClass.Forest,
                's' => SVEProperties.CardClass.Sword,
                'r' => SVEProperties.CardClass.Rune,
                'd' => SVEProperties.CardClass.Dragon,
                'a' => SVEProperties.CardClass.Abyss,
                'h' => SVEProperties.CardClass.Haven,
                'n' => SVEProperties.CardClass.Neutral,
                _ => SVEProperties.CardClass.Neutral
            };
        }
    }
}
