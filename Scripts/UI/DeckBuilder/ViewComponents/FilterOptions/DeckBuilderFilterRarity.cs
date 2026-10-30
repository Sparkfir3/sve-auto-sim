using UnityEngine.UI;

namespace SVESimulator.DeckBuilder
{
    public class DeckBuilderFilterRarity : DeckBuilderFilterButtonGroup<RarityFilter>
    {
        public override void Initialize(RarityFilter initialValue = 0)
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
                (Toggle toggle, RarityFilter filter) = (buttonInfo.Key, buttonInfo.Value);
                if(initialValue.HasFlag(filter))
                    toggle.isOn = true;
            }
        }

        protected override void ClearFilter()
        {
            model.Filters.rarity = (RarityFilter)0;
            model.OnUpdateFilters?.Invoke();
        }

        protected override void EnableFilter(RarityFilter filterValue)
        {
            model.Filters.rarity |= filterValue;
            model.OnUpdateFilters?.Invoke();
        }

        protected override void DisableFilter(RarityFilter filterValue)
        {
            model.Filters.rarity ^= filterValue;
            model.OnUpdateFilters?.Invoke();
        }
    }
}
