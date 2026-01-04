using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using TMPro;

namespace SVESimulator
{
    public class ZoneInfoDisplayCemetery : ZoneInfoDisplayBase
    {
        [SerializeField]
        protected TextMeshProUGUI textSpellchain;

        // ------------------------------

        protected override void Initialize()
        {
            base.Initialize();
            zone.Player.OnCardsInCemeteryChanged += UpdateCardCount;
        }

        protected override void UpdateCardCount(int count)
        {
            base.UpdateCardCount(count);
            textSpellchain.text = player.Spellchain.ToString();
        }
    }
}
