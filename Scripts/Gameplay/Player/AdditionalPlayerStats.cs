using System.Linq;
using Mirror;
using UnityEngine;

namespace SVESimulator
{
    public class AdditionalPlayerStats : NetworkBehaviour
    {
        [SyncVar]
        public bool UseRuneFollowersForSpellchain;
        public readonly SyncList<PlayedCardData> CardsPlayedThisTurn = new();
        public readonly SyncList<PlayedAbilityData> AbilitiesUsedThisTurn = new();
        public readonly SyncList<PlayedCardData> CardsDestroyedThisTurn = new();

        private PlayerController player;

        // ------------------------------

        public void Initialize(PlayerController player)
        {
            this.player = player;
            Reset();
            SVEEffectPool.Instance.OnConfirmationTimingEndConstant += UpdateFieldAbilities;
        }

        public void Reset()
        {
            CardsPlayedThisTurn.Clear();
            AbilitiesUsedThisTurn.Clear();
            CardsDestroyedThisTurn.Clear();
        }

        // ------------------------------

        private void UpdateFieldAbilities()
        {
            bool shouldUseRuneFollowersForSpellchain = player.ZoneController.fieldZone.GetAllPrimaryCards()
                .Any(x => x.RuntimeCard.HasKeyword(SVEProperties.PassiveAbilities.UseRuneFollowersForSpellchain));
            if(UseRuneFollowersForSpellchain != shouldUseRuneFollowersForSpellchain)
            {
                UseRuneFollowersForSpellchain = shouldUseRuneFollowersForSpellchain;
                player.SetCemeteryCount(player.Necrocharge); // necrocharge = shortcut for cards in cemetery count
            }
        }
    }
}
