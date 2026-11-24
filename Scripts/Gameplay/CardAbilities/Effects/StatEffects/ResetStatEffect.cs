using System;
using System.Linq;
using UnityEngine;
using CCGKit;
using StatBoostType = SVESimulator.SVEProperties.StatBoostType;

namespace SVESimulator
{
    public class ResetStatEffect : SveEffect
    {
        [EnumField("Target", width = 200), Order(1)]
        public SVEProperties.SVEEffectTarget target = SVEProperties.SVEEffectTarget.Self;

        [StringField("Target Filter", width = 100), Order(2)]
        public string filter;

        [EnumField("Stat", width = 100), Order(3)]
        public StatBoostType targetStats;

        // ------------------------------

        public override void Resolve(PlayerController player, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, Action onComplete = null)
        {
            ResolveOnTarget(player, triggeringCardInstanceId, triggeringCardZone, sourceCardInstanceId, sourceCardZone, target, filter, onTargetFound: targets =>
            {
                foreach(CardObject card in targets)
                {
                    Card libraryCard = card.LibraryCard;
                    foreach(string stat in targetStats.AsNamedStatArray())
                    {
                        int targetAmount = libraryCard.stats.FirstOrDefault(x => x.name.Equals(stat))?.baseValue ?? 0;
                        player.LocalEvents.SetCardStat(card.RuntimeCard, card.RuntimeCard.namedStats[stat].statId, targetAmount);
                    }
                }
                onComplete?.Invoke();
            });
        }
    }
}
