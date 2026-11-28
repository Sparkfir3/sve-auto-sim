using UnityEngine;
using Sirenix.OdinInspector;
using CCGKit;
using TMPro;

namespace SVESimulator
{
    public class CardObjectStatsDisplay : MonoBehaviour
    {
        [Title("Object References"), SerializeField]
        private CardObject cardObject;
        [field: SerializeField]
        public GameObject MainStatContainer { get; private set; }
        [field: SerializeField]
        public GameObject CostStatContainer { get; private set; }

        [Title("Stat Displays"), SerializeField]
        private GameObject attackContainer;
        [SerializeField]
        private TextMeshProUGUI attackText;
        [SerializeField]
        private GameObject defenseContainer;
        [SerializeField]
        private TextMeshProUGUI defenseText;
        [SerializeField]
        private GameObject costContainer;
        [SerializeField]
        private TextMeshProUGUI costText;
        [SerializeField, InlineEditor]
        private CardStatColorSettings statColorSettings;

        private RuntimeCard runtimeCard;
        private Stat attackStat;
        private Stat defenseStat;
        private Stat costStat;
        private int baseCost;

        // ------------------------------

        public void SetCard(RuntimeCard card)
        {
            Reset();
            if(card == null)
                return;

            // Attack
            if(card.namedStats.TryGetValue(SVEProperties.CardStats.Attack, out attackStat))
            {
                attackContainer.SetActive(true);
                SetAttackStat(attackStat.effectiveValue);
                attackStat.onValueChanged += SetAttackStat;
            }

            // Defense
            if(card.namedStats.TryGetValue(SVEProperties.CardStats.Defense, out defenseStat))
            {
                defenseContainer.SetActive(true);
                SetDefenseStat(defenseStat.effectiveValue);
                defenseStat.onValueChanged += SetDefenseStat;
            }

            // Cost
            if(card.ownerPlayer.netId.isLocalPlayer && card.namedStats.TryGetValue(SVEProperties.CardStats.Cost, out costStat))
            {
                baseCost = costStat.baseValue;
                int currentCost = cardObject.CurrentZone ? card.PlayPointCost(cardObject.CurrentZone.Player) : baseCost;
                SetCostStat(currentCost);
                SVEEffectPool.Instance.OnConfirmationTimingEndConstant += UpdateCostStat;
            }
            else
            {
                baseCost = -1;
                costContainer.SetActive(false);
            }

            runtimeCard = card;
        }

        public void Reset()
        {
            // Attack
            attackContainer.SetActive(false);
            if(attackStat != null)
            {
                attackStat.onValueChanged -= SetAttackStat;
                attackStat = null;
            }

            // Defense
            defenseContainer.SetActive(false);
            if(defenseStat != null)
            {
                defenseStat.onValueChanged -= SetDefenseStat;
                defenseStat = null;
            }

            // Cost
            costContainer.SetActive(false);
            if(costStat != null)
            {
                SVEEffectPool.Instance.OnConfirmationTimingEndConstant -= UpdateCostStat;
                costStat = null;
            }

            runtimeCard = null;
        }

        private void OnDestroy()
        {
            SVEEffectPool.Instance.OnConfirmationTimingEndConstant -= UpdateCostStat;
        }

        // ------------------------------

        private void SetAttackStat(int oldAtk, int newAtk) => SetAttackStat(newAtk, true, newAtk - oldAtk);
        private void SetAttackStat(int atk, bool playAnimation = false, int difference = 0)
        {
            attackText.text = atk.ToString();
            if(atk < attackStat.baseValue)
            {
                attackText.color = statColorSettings.StatDownColor;
                attackText.fontSharedMaterial = statColorSettings.WhiteOutlineMaterial;
            }
            else
            {
                attackText.color = atk == attackStat.baseValue ? statColorSettings.StatBaseColor : statColorSettings.StatBuffedColor;
                attackText.fontSharedMaterial = statColorSettings.BlackOutlineMaterial;
            }
            if(playAnimation)
                CardManager.Animator.PlayStatChangeAnimation(attackText.transform.position, difference);
        }

        private void SetDefenseStat(int oldDef, int newDef) => SetDefenseStat(newDef, true, newDef - oldDef);
        private void SetDefenseStat(int def, bool playAnimation = false, int difference = 0)
        {
            defenseText.text = def.ToString();
            if(def < defenseStat.baseValue)
            {
                defenseText.color = statColorSettings.StatDownColor;
                defenseText.fontSharedMaterial = statColorSettings.WhiteOutlineMaterial;
            }
            else
            {
                defenseText.color = def == defenseStat.baseValue ? statColorSettings.StatBaseColor : statColorSettings.StatBuffedColor;
                defenseText.fontSharedMaterial = statColorSettings.BlackOutlineMaterial;
            }
            if(playAnimation)
                CardManager.Animator.PlayStatChangeAnimation(defenseText.transform.position, difference);
        }

        public void UpdateCostStat()
        {
            if(costStat == null || !CostStatContainer.activeInHierarchy)
                return;
            SetCostStat(runtimeCard.PlayPointCost(cardObject.CurrentZone ? cardObject.CurrentZone.Player : null));
        }

        public void SetCostStat(int cost)
        {
            if(cost < 0 || cost == baseCost)
            {
                costContainer.SetActive(false);
                return;
            }
            costContainer.SetActive(true);
            costText.text = cost.ToString();
            costText.color = cost < baseCost ? statColorSettings.StatBuffedColor : statColorSettings.StatDownColor;
            costText.fontSharedMaterial = cost <= baseCost ? statColorSettings.BlackOutlineMaterial : statColorSettings.WhiteOutlineMaterial;
        }
    }
}
