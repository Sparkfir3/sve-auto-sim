using System.Collections;
using System.Collections.Generic;
using CCGKit;

namespace SVESimulator
{
    public abstract class SveCost : Cost
    {
        public abstract bool CanPayCost(PlayerController player, RuntimeCard card, string abilityName);

        // Pay cost locally/visuals only - do not use event functions or actual data handling in order to avoid sending overlapping network messages
        public virtual IEnumerator PayCost(PlayerController player, CardObject card, SveEffect effect, List<MoveCardToZoneData> cardsToMove)
        {
            yield return null;
        }
    }
}
