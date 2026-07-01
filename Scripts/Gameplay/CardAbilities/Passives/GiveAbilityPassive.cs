using UnityEngine;
using System.Collections;
using System.Linq;
using CCGKit;
using Sparkfire.Utility;

namespace SVESimulator
{
    public class GiveAbilityPassive : SvePassiveEffect
    {
        [KeywordTypeField("Effect"), Order(1)]
        public string effectName;

        private Ability ability;

        // ------------------------------

        public override void ApplyPassive(RuntimeCard card, PlayerController player) { }

        public override void RemovePassive(RuntimeCard card, PlayerController player) { }

        public Ability GetAbility(int sourceCardId)
        {
            if(ability != null || effectName.IsNullOrWhiteSpace())
                return ability;

            Card libraryCard = LibraryCardCache.GetCard(sourceCardId);
            ability = libraryCard.abilities.FirstOrDefault(x => x.name.Equals(effectName))?.Copy();
            if((ability as TriggeredAbility)?.trigger is SveTrigger sveTrigger)
            {
                // TODO - remove internal cost from trigger
            }
            if(ability is ActivatedAbility)
                Debug.LogError($"{nameof(GiveAbilityPassive)} does not currently support giving Act abilities.\nTarget effect name: {effectName}");
            return ability;
        }
    }
}
