using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace SVESimulator.DeckBuilder
{
    public class DeckBuilderFilterType : DeckBuilderFilterButtonGroup<CardTypeFilter>
    {
        public override void Initialize(CardTypeFilter initialValue = 0)
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
                (Toggle toggle, CardTypeFilter filter) = (buttonInfo.Key, buttonInfo.Value);
                if(initialValue.HasFlag(filter))
                    toggle.isOn = true;
            }
        }

        protected override void ClearFilter()
        {
            model.Filters.cardType = (CardTypeFilter)0;
            model.OnUpdateFilters?.Invoke();
        }

        protected override void EnableFilter(CardTypeFilter filterValue)
        {
            model.Filters.cardType |= filterValue;
            model.OnUpdateFilters?.Invoke();
        }

        protected override void DisableFilter(CardTypeFilter filterValue)
        {
            model.Filters.cardType ^= filterValue;
            model.OnUpdateFilters?.Invoke();
        }
    }
}
