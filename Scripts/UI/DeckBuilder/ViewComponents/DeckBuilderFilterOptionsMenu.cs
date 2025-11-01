using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using Sparkfire.Utility;
using TMPro;

namespace SVESimulator.DeckBuilder
{
    public class DeckBuilderFilterOptionsMenu : MonoBehaviour
    {
        [SerializeField]
        private DeckBuilderModel model;
        [SerializeField]
        private TMP_InputField primaryTextInputField;
        [SerializeField]
        private TMP_InputField secondaryTextInputField;
        [SerializeField]
        private DeckBuilderFilterType cardTypeButtonGroup;
        [SerializeField]
        private DeckBuilderFilterClass cardClassButtonGroup;
        
        [BoxGroup("Cost"), SerializeField]
        private MinMaxSlider costSlider;
        [BoxGroup("Cost"), SerializeField]
        private Toggle useCostToggle;
        // [BoxGroup("Cost"), SerializeField]
        // private TextMeshProUGUI minCostText;
        // [BoxGroup("Cost"), SerializeField]
        // private TextMeshProUGUI maxCostText;
        
        [BoxGroup("Attack"), SerializeField]
        private MinMaxSlider attackSlider;
        [BoxGroup("Attack"), SerializeField]
        private Toggle useAttackToggle;
        // [BoxGroup("Attack"), SerializeField]
        // private TextMeshProUGUI minAttackText;
        // [BoxGroup("Attack"), SerializeField]
        // private TextMeshProUGUI maxAttackText;
        
        [BoxGroup("Defense"), SerializeField]
        private MinMaxSlider defenseSlider;
        [BoxGroup("Defense"), SerializeField]
        private Toggle useDefenseToggle;
        // [BoxGroup("Defense"), SerializeField]
        // private TextMeshProUGUI minDefenseText;
        // [BoxGroup("Defense"), SerializeField]
        // private TextMeshProUGUI maxDefenseText;

        public bool PointerInside { get; set; }

        // ------------------------------
        
        public void Initialize()
        {
            InitializeTextInputFields();
            InitializeSliders();

            cardTypeButtonGroup.Initialize(~CardTypeFilter.Token);
            cardClassButtonGroup.Initialize();
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Mouse0) && !PointerInside)
                gameObject.SetActive(false);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if(!model)
                model = GetComponentInParent<DeckBuilderModel>();
        }
#endif
        
        // ------------------------------
        
        private void InitializeTextInputFields()
        {
            primaryTextInputField.onValueChanged.AddListener(x =>
            {
                model.Filters.text = x;
                secondaryTextInputField.SetTextWithoutNotify(x);
                model.OnUpdateFilters?.Invoke();
            });
            secondaryTextInputField.onValueChanged.AddListener(x =>
            {
                model.Filters.text = x;
                primaryTextInputField.SetTextWithoutNotify(x);
                model.OnUpdateFilters?.Invoke();
            });
        }

        private void InitializeButtonGroups()
        {
            cardTypeButtonGroup.onValueChanged.AddListener(() => model.OnUpdateFilters?.Invoke());
            cardClassButtonGroup.onValueChanged.AddListener(() => model.OnUpdateFilters?.Invoke());
        }

        private void InitializeSliders()
        {
            // Cost
            useCostToggle.onValueChanged.AddListener(x =>
            {
                model.Filters.useCost = x;
                model.OnUpdateFilters?.Invoke();
            });
            costSlider.onValueChanged.AddListener((x, y) =>
            {
                model.Filters.minCost = (int)x;
                model.Filters.maxCost = (int)y;
                // minCostText.text = $"{x}";
                // maxCostText.text = $"{y}";
                useCostToggle.SetIsOnWithoutNotify(true);
                model.Filters.useCost = true;
                model.OnUpdateFilters?.Invoke();
            });
            
            // Attack
            useAttackToggle.onValueChanged.AddListener(x =>
            {
                model.Filters.useAttack = x;
                model.OnUpdateFilters?.Invoke();
            });
            attackSlider.onValueChanged.AddListener((x, y) =>
            {
                model.Filters.minAttack = (int)x;
                model.Filters.maxAttack = (int)y;
                // minAttackText.text = $"{x}";
                // maxAttackText.text = $"{y}";
                useAttackToggle.SetIsOnWithoutNotify(true);
                model.Filters.useAttack = true;
                model.OnUpdateFilters?.Invoke();
            });
            
            // Defense
            useDefenseToggle.onValueChanged.AddListener(x =>
            {
                model.Filters.useDefense = x;
                model.OnUpdateFilters?.Invoke();
            });
            defenseSlider.onValueChanged.AddListener((x, y) =>
            {
                model.Filters.minDefense = (int)x;
                model.Filters.maxDefense = (int)y;
                // minDefenseText.text = $"{x}";
                // maxDefenseText.text = $"{y}";
                useDefenseToggle.SetIsOnWithoutNotify(true);
                model.Filters.useDefense = true;
                model.OnUpdateFilters?.Invoke();
            });
        }
    }
}
