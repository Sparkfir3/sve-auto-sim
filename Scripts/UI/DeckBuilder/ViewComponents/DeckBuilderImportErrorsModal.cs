using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SVESimulator.Database;
using TMPro;

namespace SVESimulator.DeckBuilder
{
    public class DeckBuilderImportErrorsModal : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI deckNameText;
        [SerializeField]
        private string deckNameFormat = "Deck Name: {0}";
        [SerializeField]
        private TextMeshProUGUI errorsText;
        [SerializeField]
        private string errorLineFormat = "{0}x {1} (Internal ID {2})";

        public void Open(in string deckName, in List<DeckSaveLoadUtils.CardAmountPair> errors)
        {
            deckNameText.text = string.Format(deckNameFormat, deckName);
            errorsText.text = string.Join("\n", errors.Select(x => string.Format(errorLineFormat, x.amount, x.id + "\t", x.ccgId)));
            gameObject.SetActive(true);
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }
    }
}
