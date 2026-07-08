using System.Collections.Generic;
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
        public readonly SyncList<PlayedCardData> CardsDiscardedThisTurn = new();
        public readonly SyncList<PlayedCardData> CardsAttackedThisTurn = new();
        public readonly SyncList<PlayedCardData> CardsReturnedToHandFromField = new();

        public List<PlayedCardData> SpellsPlayedThisTurn => CardsPlayedThisTurn.Where(x =>
            LibraryCardCache.GetCard(x.cardId).IsSpell()).ToList();

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
            CardsDiscardedThisTurn.Clear();
            CardsAttackedThisTurn.Clear();
            CardsReturnedToHandFromField.Clear();
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
