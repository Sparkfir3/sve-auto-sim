using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CCGKit;
using TMPro;

namespace SVESimulator.DeckBuilder
{
    public class DeckBuilderDeckStats : MonoBehaviour
    {
        #region Variables

        [Title("Object References"), SerializeField]
        private DeckBuilderModel model;
        [SerializeField]
        private List<RectTransform> costBreakdownBars;

        [SerializeField]
        private TextMeshProUGUI textDeckCurrentAmt;
        [SerializeField]
        private TextMeshProUGUI textDeckMaxAmt;
        [SerializeField]
        private TextMeshProUGUI textAmtFollowers;
        [SerializeField]
        private TextMeshProUGUI textAmtSpells;
        [SerializeField]
        private TextMeshProUGUI textAmtAmulets;

        [SerializeField]
        private TextMeshProUGUI textEvolveDeckCurrentAmt;
        [SerializeField]
        private TextMeshProUGUI textAmtEvolved;

        [Title("Settings"), SerializeField]
        private string stringTemplate = "{0}: {1}";

        #endregion

        // ------------------------------

        #region UI Controls

        public void UpdateDeckStats()
        {
            UpdateMainDeckGraph();
            UpdateMainDeckText();
            UpdateEvolveDeckText();
        }

        private void UpdateMainDeckGraph()
        {
            int mainDeckCount = model.MainDeckCount;
            for(int i = 0; i < costBreakdownBars.Count; i++)
            {
                int count = model.CurrentMainDeck.Where(x =>
                {
                    Stat stat = x.Key.stats.FirstOrDefault(y => y.name.Equals(SVEProperties.CardStats.Cost));
                    return stat != null && ((i == costBreakdownBars.Count - 1) ? stat.baseValue >= i : stat.baseValue == i);
                }).Sum(x => x.Value);

                float percent = mainDeckCount > 0 ? (float)count / mainDeckCount : 0f;
                costBreakdownBars[i].anchoredPosition = new Vector2(0f, -(1f - percent) * costBreakdownBars[i].rect.height);
            }
        }

        private void UpdateMainDeckText()
        {
            int mainDeckCount = model.MainDeckCount;
            textDeckCurrentAmt.text = mainDeckCount.ToString();
            textDeckMaxAmt.text = Mathf.Min(Mathf.Max(mainDeckCount, 40), 50).ToString();

            textAmtFollowers.text = string.Format(stringTemplate, "Followers",
                model.CurrentMainDeck.Where(x => x.Key.IsFollowerOrEvolvedFollower(model.gameConfig)).Sum(x => x.Value));
            textAmtSpells.text = string.Format(stringTemplate, "Spells",
                model.CurrentMainDeck.Where(x => x.Key.IsSpell(model.gameConfig)).Sum(x => x.Value));
            textAmtAmulets.text = string.Format(stringTemplate, "Amulets",
                model.CurrentMainDeck.Where(x => x.Key.IsAmulet(model.gameConfig)).Sum(x => x.Value));
        }

        private void UpdateEvolveDeckText()
        {
            textEvolveDeckCurrentAmt.text = model.EvolveDeckCount.ToString();

            textAmtEvolved.text = string.Format(stringTemplate, "Evolved",
                model.CurrentEvolveDeck.Where(x => x.Key.IsFollowerOrEvolvedFollower(model.gameConfig)).Sum(x => x.Value));
        }

        #endregion

        // ------------------------------

        #region Unity Functions

#if UNITY_EDITOR
        private void OnValidate()
        {
            if(!model)
                model = GetComponentInParent<DeckBuilderModel>();
        }
#endif

        #endregion
    }
}
