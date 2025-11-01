using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CCGKit;

namespace SVESimulator.DeckBuilder
{
    public static class CardListSorting
    {
        public enum SortMode
        {
            ID = 0,
            Cost = 1,
            Class = 2,
        }

        private static Dictionary<string, int> ClassSortOrder = new()
        {
            { SVEProperties.CardClass.Forest, 0 },
            { SVEProperties.CardClass.Sword, 1 },
            { SVEProperties.CardClass.Rune, 2 },
            { SVEProperties.CardClass.Dragon, 3 },
            { SVEProperties.CardClass.Abyss, 4 },
            { SVEProperties.CardClass.Haven, 5 },
            { SVEProperties.CardClass.Neutral, 6 },
        };

        // ------------------------------

        public static void SortCardList(ref List<Card> cardList, SortMode mode = SortMode.ID)
        {
            switch(mode)
            {
                case SortMode.ID:
                    cardList = cardList.OrderBy(x => x.id).ToList();
                    break;
                case SortMode.Cost:
                    cardList = cardList.OrderBy(x =>
                    {
                        Stat cost = x.stats.FirstOrDefault(y => y.name.Equals(SVEProperties.CardStats.Cost));
                        return cost?.baseValue ?? x.cardTypeId switch
                        {
                            1 => 101, // evolved follower
                            5 => 102, // leader
                            _ => 999
                        };
                    }).ThenBy(x =>
                    {
                        string cardClass = x.GetStringProperty(SVEProperties.CardStats.Class);
                        return ClassSortOrder.GetValueOrDefault(cardClass, 99);
                    }).ThenBy(x => x.id).ToList();
                    break;
                case SortMode.Class:
                    cardList = cardList.OrderBy(x =>
                    {
                        string cardClass = x.GetStringProperty(SVEProperties.CardStats.Class);
                        return ClassSortOrder.GetValueOrDefault(cardClass, 99);
                    }).ThenBy(x => x.id).ToList();
                    break;
            }
        }
    }
}
