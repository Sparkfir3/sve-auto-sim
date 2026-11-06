using System.Linq;
using System.Collections.Generic;
using CCGKit;

namespace SVESimulator
{
    public class FuseCost : SendTargetToZoneCost
    {
        [StringField("Amount", width = 100), Order(0)]
        public string amount;

        // ------------------------------

        protected override string TargetZoneName => SVEProperties.Zones.ExArea;
        protected override string CostName => nameof(FuseCost);

        public override string GetReadableString(GameConfiguration config)
        {
            return "Fuse Cost";
        }

        public override bool CanPayCost(PlayerController player, RuntimeCard card, string abilityName)
        {
            var cardFilter = SVEFormulaParser.ParseCardFilterFormula(filter, card.instanceId);
            SVEFormulaParser.ParseValueAsMinMax(amount, player, card, out int minTargets, out _);

            int handCount = player.GetPlayerInfo().namedZones[SVEProperties.Zones.Hand].cards.Count(x => cardFilter.MatchesCard(x));
            int exAreaCount = player.ZoneController.exAreaZone.GetAllPrimaryCards().Count(x => cardFilter.MatchesCard(x));
            return handCount + exAreaCount >= minTargets;
        }

        protected override TargetCardForCostEffect CostTargetingEffect(string text = null)
        {
            return new TargetCardForCostEffect()
            {
                text = text ?? "",
                target = SVEProperties.SVEEffectTarget.TargetPlayerCardHandOrEx,
                filter = filter
            };
        }

        protected override void MoveCardObjectsToTargetZone(PlayerController player, List<CardObject> cards)
        {
            foreach(CardObject card in cards)
                player.LocalEvents.SendToCemetery(card, onlyMoveObject: true);
        }
    }
}
