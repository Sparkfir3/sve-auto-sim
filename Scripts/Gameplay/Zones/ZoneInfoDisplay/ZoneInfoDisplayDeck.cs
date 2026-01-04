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
        }

        private void UpdateCombo()
        {
            combo = player.Combo;
            textCombo.text = combo.ToString();
        }

        private void UpdateOverflow()
        {
            overflow = player.Overflow;
            textOverflow.text = overflow.ToString();
        }

        private void UpdateSanguine()
        {
            sanguine = player.Sanguine;
            textSanguine.text = sanguine.ToString();
        }
    }
}
