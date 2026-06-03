using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace SVESimulator.DeckBuilder
{
    public class DeckBuilderFilterUniverse : DeckBuilderFilterButtonGroup<UniverseFilter>
    {
        public override void Initialize(UniverseFilter initialValue = 0)
        {
            base.Initialize(initialValue);
            if(initialValue == 0)
            {
                if(!allButton.isOn)
                    allButton.isOn = true;
                return;
            }
            foreach(var buttonInfo in filterButtons)
            {
                (Toggle toggle, UniverseFilter filter) = (buttonInfo.Key, buttonInfo.Value);
                if(initialValue.HasFlag(filter))
                    toggle.isOn = true;
            }
        }

        protected override void ClearFilter()
        {
            model.Filters.universe = (UniverseFilter)0;
            model.OnUpdateFilters?.Invoke();
        }

        protected override void EnableFilter(UniverseFilter filterValue)
        {
            model.Filters.universe |= filterValue;
            model.OnUpdateFilters?.Invoke();
        }

        protected override void DisableFilter(UniverseFilter filterValue)
        {
            model.Filters.universe ^= filterValue;
            model.OnUpdateFilters?.Invoke();
        }
    }
}
