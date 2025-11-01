using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using CCGKit;

namespace SVESimulator
{
    public class GiveKeywordPassive : SvePassiveEffect
    {
        [KeywordTypeField("Type"), Order(1)]
        public int keywordType;

        [KeywordValueField("Value"), Order(2)]
        public int keywordValue;

        // ------------------------------

        public override void ApplyPassive(RuntimeCard card, PlayerController player)
        {
            player.LocalEvents.ApplyKeywordToCard(card, keywordType, keywordValue, true);
        }

        public override void RemovePassive(RuntimeCard card, PlayerController player)
        {
            player.LocalEvents.ApplyKeywordToCard(card, keywordType, keywordValue, false);
        }
    }
}
