using CCGKit;
using UnityEngine;
using Sirenix.OdinInspector;
using TMPro;

namespace SVESimulator
{
    public class LeaderHealthDisplay : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI defenseText;

        // ------------------------------

        public void Initialize(Stat playerDefenseStat)
        {
            SetDefense(playerDefenseStat.effectiveValue);
            playerDefenseStat.onValueChanged += (oldAmount, newAmount) =>
            {
                SetDefense(newAmount);
            };
        }

        public void SetDefense(int def)
        {
            defenseText.text = def.ToString();
        }
    }
}
