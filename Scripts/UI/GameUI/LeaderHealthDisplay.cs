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
        [SerializeField, InlineEditor]
        private CardStatColorSettings statColorSettings;

        private int baseDefense = 20;

        // ------------------------------

        public void Initialize(Stat playerDefenseStat)
        {
            baseDefense = playerDefenseStat.effectiveValue;
            SetDefense(baseDefense);
            playerDefenseStat.onValueChanged += (oldAmount, newAmount) =>
            {
                SetDefense(newAmount, true, newAmount - oldAmount);
            };
        }

        public void SetDefense(int def, bool playAnimation = false, int difference = 0)
        {
            defenseText.text = def.ToString();
            if(def < baseDefense)
            {
                defenseText.color = statColorSettings.StatDownColor;
                defenseText.fontSharedMaterial = statColorSettings.WhiteOutlineMaterial;
            }
            else
            {
                defenseText.color = def == baseDefense ? statColorSettings.StatBaseColor : statColorSettings.StatBuffedColor;
                defenseText.fontSharedMaterial = statColorSettings.BlackOutlineMaterial;
            }
            if(playAnimation)
                CardManager.Animator.PlayStatChangeAnimation(defenseText.transform.position, difference);
        }
    }
}
