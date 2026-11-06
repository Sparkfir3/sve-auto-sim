using System;

namespace SVESimulator
{
    public class FuseEffect : EffectSequence
    {
        public override void Resolve(PlayerController player, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, Action onComplete = null)
        {
            ResolveOnTarget(player, triggeringCardInstanceId, triggeringCardZone, sourceCardInstanceId, sourceCardZone, SVEProperties.SVEEffectTarget.SelfFromHand, null, onTargetFound: targets =>
            {
                foreach(CardObject card in targets)
                {
                    player.LocalEvents.SendToExArea(card);
                }
                base.Resolve(player, triggeringCardInstanceId, triggeringCardZone, sourceCardInstanceId, sourceCardZone, onComplete);
            });
        }
    }
}
