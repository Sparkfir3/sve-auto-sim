using System;
using System.Collections.Generic;
using UnityEngine;
using CCGKit;
using SVESimulator.UI;

namespace SVESimulator
{
    public class SveSendToTopOrBottomDeckEffect : SveEffect
    {
        [EnumField("Target", width = 200), Order(1)]
        public SVEProperties.SVEEffectTarget target = SVEProperties.SVEEffectTarget.Self;

        [StringField("Target Filter", width = 100), Order(2)]
        public string filter;

        // ------------------------------

        public override void Resolve(PlayerController player, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, Action onComplete = null)
        {
            CardObject cardObject = CardManager.Instance.GetCardByInstanceId(sourceCardInstanceId);
            Card libraryCard = LibraryCardCache.GetCard(cardObject.RuntimeCard.cardId, GameManager.Instance.config);

            ResolveOnTarget(player, triggeringCardInstanceId, triggeringCardZone, sourceCardInstanceId, sourceCardZone, target, filter, onTargetFound: targets =>
            {
                List<MultipleChoiceWindow.MultipleChoiceEntryData> choices = new()
                {
                    new MultipleChoiceWindow.MultipleChoiceEntryData()
                    {
                        text = "Top Deck",
                        onSelect = () =>
                        {
                            foreach(CardObject card in targets)
                                player.LocalEvents.SendToTopDeck(card);
                            onComplete?.Invoke();
                        }
                    },
                    new MultipleChoiceWindow.MultipleChoiceEntryData()
                    {
                        text = "Bottom Deck",
                        onSelect = () =>
                        {
                            foreach(CardObject card in targets)
                                player.LocalEvents.SendToBottomDeck(card);
                            onComplete?.Invoke();
                        }
                    },
                };
                GameUIManager.MultipleChoice.Open(player, libraryCard?.name ?? "", choices, "Select top or bottom deck to send target cards.");
            });
        }
    }
}
