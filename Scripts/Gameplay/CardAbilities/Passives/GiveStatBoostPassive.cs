using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using CCGKit;

namespace SVESimulator
{
    public class GiveStatBoostPassive : SvePassiveEffect
    {
        [EnumField("Stat Boost"), Order(1)]
        public SVEProperties.StatBoostType targetStats;

        [StringField("Amount", width = 200), Order(3)]
        public string amount;

        // ------------------------------

        public override void ApplyPassive(RuntimeCard card, PlayerController player)
        {
            int boostAmount = SVEFormulaParser.ParseValue(amount, player);
            foreach(string stat in targetStats.AsNamedStatArray())
            {
                player.LocalEvents.ApplyModifierToCard(card, card.namedStats[stat].statId, boostAmount, true);
            }
        }

        public override void RemovePassive(RuntimeCard card, PlayerController player)
        {
            int boostAmount = SVEFormulaParser.ParseValue(amount, player);
            foreach(string stat in targetStats.AsNamedStatArray())
            {
                player.LocalEvents.ApplyModifierToCard(card, card.namedStats[stat].statId, boostAmount, false);
            }
        }
    }
}
