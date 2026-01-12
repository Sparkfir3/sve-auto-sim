using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using TMPro;

namespace SVESimulator
{
    public class ZoneInfoDisplayEvolveDeck : ZoneInfoDisplayBase
    {
        [SerializeField]
        protected TextMeshProUGUI textCardCountFaceUp;

        // ------------------------------

        protected override void Initialize()
        {
            base.Initialize();
            player.OnCardsInEvolveDeckFaceDownChanged += UpdateCardCount;
            player.OnCardsInEvolveDeckFaceUpChanged += UpdateCardCountFaceUp;
        }

        protected virtual void UpdateCardCountFaceUp(int count)
        {
            textCardCountFaceUp.text = count.ToString();
        }
    }
}
