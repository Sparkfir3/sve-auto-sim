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
        [SerializeField]
        protected string spellchainFormatString = "SC {0}";

        // ------------------------------

        protected override void Initialize()
        {
            base.Initialize();
            zone.Player.OnCardsInCemeteryChanged += UpdateCardCount;
            zone.Player.OnSpellchainChanged += UpdateSpellchain;
            UpdateSpellchain(0);
        }

        protected void UpdateSpellchain(int spellchain)
        {
            textSpellchain.text = string.Format(spellchainFormatString, spellchain);
        }
    }
}
