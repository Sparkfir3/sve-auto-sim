using System;
using UnityEngine;
using CCGKit;
using SVESimulator.UI;

namespace SVESimulator
{
    public abstract class SveModifiedCostEffect : SveEffect
    {
        public override void Resolve(PlayerController player, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, Action onComplete = null)
        {
            Debug.LogError("Attempted to resolve a modified cost effect, this should not happen.");
            onComplete?.Invoke();
        }

        public virtual MultipleChoiceWindow.MultipleChoiceEntryData AsMultipleChoiceEntry(Action onSelect)
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
