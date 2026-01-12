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
        protected TextMeshProUGUI textOverflow;
        [SerializeField]
        protected TextMeshProUGUI textSanguine;

        private int combo;
        private bool overflow;
        private bool sanguine;

        // ------------------------------

        private void LateUpdate()
        {
            if(!player)
                return;
            if(player.Combo != combo)
                UpdateCombo();
            if(!overflow && player.Overflow != overflow)
                UpdateOverflow();
            if(player.Sanguine != sanguine)
                UpdateSanguine();
        }

        // ------------------------------

        protected override void Initialize()
        {
            base.Initialize();
            player.OnCardsInDeckChanged += UpdateCardCount;
            UpdateCombo();
            UpdateOverflow();
            UpdateSanguine();
        }

        private void UpdateCombo()
        {
            combo = player.Combo;
            textCombo.text = string.Format(comboFormatString, combo);
        }

        private void UpdateOverflow()
        {
            overflow = player.Overflow;
            textOverflow.gameObject.SetActive(overflow);
        }

        private void UpdateSanguine()
        {
            sanguine = player.Sanguine;
            textSanguine.gameObject.SetActive(sanguine);
        }
    }
}
