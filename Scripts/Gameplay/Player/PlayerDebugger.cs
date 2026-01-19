using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;

namespace SVESimulator
{
    [RequireComponent(typeof(PlayerController))]
    public class PlayerDebugger : MonoBehaviour
    {
        [SerializeField, Required]
        private PlayerController player;

        [Title("Game Variables"), ShowInInspector, ReadOnly]
        private int combo => player.Combo;
        [ShowInInspector, ReadOnly]
        private int spellchain => player.ZoneController && player.ZoneController.cemeteryZone ? player.Spellchain : 0;
        [ShowInInspector, ReadOnly]
        private bool overflow => player.Overflow;
        [ShowInInspector, ReadOnly]
        private int necrocharge => player.ZoneController && player.ZoneController.cemeteryZone ? player.Necrocharge : 0;

        [Title("Additional Stats"), ShowInInspector, TableList, ReadOnly]
        private List<PlayedCardData> cardsPlayedThisTurn => player.AdditionalStats.CardsPlayedThisTurn.ToList();
        [ShowInInspector, TableList, ReadOnly]
        private List<PlayedAbilityData> abilitiesUsedThisTurn => player.AdditionalStats.AbilitiesUsedThisTurn.ToList();
        [ShowInInspector, TableList, ReadOnly]
        private List<PlayedCardData> cardsDestroyedThisTurn => player.AdditionalStats.CardsDestroyedThisTurn.ToList();

        // ------------------------------

#if UNITY_EDITOR
        private void OnValidate()
        {
            if(!player)
                player = GetComponent<PlayerController>();
        }
#endif

        // ------------------------------

        [Title("Debug Buttons"), Button]
        private void TestFormulaIntParser(string formula = "1c(3)4")
        {
            Debug.Log(SVEFormulaParser.ParseValue(formula, player));
        }

        [Button]
        private void TestFormulaStringParser(string formula = "FKt(Fairy)")
        {
            Debug.Log(string.Join("\n", SVEFormulaParser.ParseCardFilterFormula(formula).Select(pair => $"{pair.Key} - {pair.Value}")));
        }
    }
}
