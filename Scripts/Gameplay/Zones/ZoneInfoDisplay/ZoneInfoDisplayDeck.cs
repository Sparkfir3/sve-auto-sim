using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using TMPro;

namespace SVESimulator
{
    public class ZoneInfoDisplayDeck : ZoneInfoDisplayBase
    {
        [SerializeField]
        protected TextMeshProUGUI textCombo;
        [SerializeField]
        protected string comboFormatString = "Combo {0}";
        [SerializeField]
        protected TextMeshProUGUI textSanguine;

        private int combo;
        private bool sanguine;

        // ------------------------------

        private void LateUpdate()
        {
            if(!player)
                return;
            if(player.Combo != combo)
                UpdateCombo();
            if(player.Sanguine != sanguine)
                UpdateSanguine();
        }

        // ------------------------------

        protected override void Initialize()
        {
            base.Initialize();
            player.OnCardsInDeckChanged += UpdateCardCount;
            UpdateCombo();
            UpdateSanguine();
        }

        private void UpdateCombo()
        {
            combo = player.Combo;
            textCombo.text = string.Format(comboFormatString, combo);
        }

        private void UpdateSanguine()
        {
            sanguine = player.Sanguine;
            textSanguine.gameObject.SetActive(sanguine);
        }
    }
}
