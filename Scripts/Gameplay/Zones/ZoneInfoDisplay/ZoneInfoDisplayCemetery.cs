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
            zone.Player.OnSpellchainChanged += UpdateSpellchain;
        }

        protected void UpdateSpellchain(int spellchain)
        {
            textSpellchain.text = spellchain.ToString();
        }
    }
}
