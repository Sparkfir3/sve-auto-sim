using System;
using UnityEngine;
using CCGKit;
using SVESimulator.UI;

namespace SVESimulator
{
    public class SveAlternateCostEffect : SveModifiedCostEffect
    {
        // Cost list should be fetched from the Trigger field

        // ------------------------------

        public MultipleChoiceWindow.MultipleChoiceEntryData AsMultipleChoiceEntry(Action onSelect)
        {
            return new MultipleChoiceWindow.MultipleChoiceEntryData()
            {
                text = text,
                onSelect = () =>
                {
                    onSelect?.Invoke();
                },
            };
        }
    }
}
