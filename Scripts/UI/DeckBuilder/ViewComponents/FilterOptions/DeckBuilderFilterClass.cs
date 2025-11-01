using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace SVESimulator.DeckBuilder
{
    public class DeckBuilderFilterClass : DeckBuilderFilterButtonGroup<ClassFilter>
    {
        public override void Initialize(ClassFilter initialValue = 0)
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
                (Toggle toggle, ClassFilter filter) = (buttonInfo.Key, buttonInfo.Value);
                if(initialValue.HasFlag(filter))
                    toggle.isOn = true;
            }
        }

        protected override void ClearFilter()
        {
            model.Filters.cardClass = (ClassFilter)0;
            model.OnUpdateFilters?.Invoke();
        }

        protected override void EnableFilter(ClassFilter filterValue)
        {
            model.Filters.cardClass |= filterValue;
            model.OnUpdateFilters?.Invoke();
        }

        protected override void DisableFilter(ClassFilter filterValue)
        {
            model.Filters.cardClass ^= filterValue;
            model.OnUpdateFilters?.Invoke();
        }
    }
}
