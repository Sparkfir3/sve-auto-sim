using System;
using System.Collections.Generic;
using System.Linq;
using CCGKit;
using Mirror;
using UnityEngine;
using Sirenix.OdinInspector;

namespace SVESimulator
{
    public class SVEEffectSolver : EffectSolver
    {
        /// <summary>
        /// Whether or not this effect solver is the local player's effect solver (NOT the server's effect solver)
        /// </summary>
        public bool isPlayerEffectSolver;

        public SVEEffectSolver(GameState gameState, int rngSeed) : base(gameState, rngSeed) { }

        // ------------------------------

        private PlayerInfo GetPlayerInfo(NetworkIdentity playerNetId)
            => gameState.players.Find(x => x.netId == playerNetId);

        // ------------------------------

        #region Overrides

        public override void SetDestroyConditions(RuntimeCard card)
        {
            // Do not use, base function is both not needed and also implies specific zone names we don't have
        }

        #endregion

        // ------------------------------

        #region Game Flow

        public void SetGamePhase(SVEProperties.GamePhase newPhase)
        {
            gameState.currentPhase = newPhase;
        }

        public void SetMaxPlayPoints(NetworkIdentity playerNetId, int value, bool updateCurrentPoints = true)
        {
            PlayerInfo player = GetPlayerInfo(playerNetId);
            if(player == null)
                return;

            player.namedStats[SVEProperties.PlayerStats.MaxPlayPoints].baseValue = Mathf.Clamp(value, 0, SVEProperties.MaxPlayPointsAmount);
            if(updateCurrentPoints)
                player.namedStats[SVEProperties.PlayerStats.PlayPoints].baseValue = player.namedStats[SVEProperties.PlayerStats.MaxPlayPoints].baseValue;
        }

        public void SetCurrentPlayPoints(NetworkIdentity playerNetId, int value)
        {
            PlayerInfo player = GetPlayerInfo(playerNetId);
            if(player == null)
                return;

            player.namedStats[SVEProperties.PlayerStats.PlayPoints].baseValue = Mathf.Clamp(value, 0, player.namedStats[SVEProperties.PlayerStats.MaxPlayPoints].baseValue);
        }

        #endregion

        // ------------------------------

        #region Card Movement

        public void DrawCard(NetworkIdentity playerNetId, RuntimeCard card)
        {
            PlayerInfo player = GetPlayerInfo(playerNetId);
            //Debug.Log($"Effect solver - drawing for player {playerNetId.netId}");
            if(player != null)
            {
                RuntimeZone deck = player.namedZones[SVEProperties.Zones.Deck];
                RuntimeZone hand = player.namedZones[SVEProperties.Zones.Hand];
                if(deck.cards.Count > 0)
                {
                    deck.RemoveCard(card);
                    hand.AddCard(card);
                }
            }
        }

        public void SendCardHandToBottomDeck(NetworkIdentity playerNetId, RuntimeCard card)
        {
            PlayerInfo player = GetPlayerInfo(playerNetId);
            if(player != null)
            {
                RuntimeZone deck = player.namedZones[SVEProperties.Zones.Deck];
                RuntimeZone hand = player.namedZones[SVEProperties.Zones.Hand];
                if(hand.cards.Count > 0)
                {
                    hand.RemoveCard(card);
                    deck.AddCard(card);
                }
            }
        }

        public void PlayCard(NetworkIdentity playerNetId, RuntimeCard card, string originZone, int playPoints, bool executeConfirmationTiming = true)
        {
            PlayerInfo player = GetPlayerInfo(playerNetId);
            if(player != null)
            {
                RuntimeZone oldZone = player.namedZones[originZone];
                RuntimeZone field = player.namedZones[SVEProperties.Zones.Field];

                oldZone.RemoveCard(card);
                field.AddCard(card);

                // TODO - support for more than one attached card
                if(card.namedStats.TryGetValue(SVEProperties.CardStats.AttachedCardInstanceIDs, out Stat attachedCardInfo))
                    attachedCardInfo.baseValue = -1;
                if(card.HasKeyword(SVEProperties.PassiveAbilities.PutOnFieldEngaged))
                    EngageCard(card);

                if(playPoints > 0)
                    player.namedStats[SVEProperties.PlayerStats.PlayPoints].baseValue -= playPoints;

                if(isPlayerEffectSolver && playerNetId.isLocalPlayer)
                {
                    SVEEffectPool.Instance.ApplyAllActivePassivesToCard(card);
                    SVEEffectPool.Instance.RegisterPassiveAbilities(gameState, card);

                    SVEEffectPool.Instance.TriggerPendingEffects<SveOnCardEnterFieldTrigger>(gameState, card, player, _ => true, false);
                    SVEEffectPool.Instance.TriggerPendingEffectsForOtherCardsInZone<SveOnOtherCardEnterFieldTrigger>(gameState, card, field, player,
                        x => x.MatchesFilter(card), false);

                    if(executeConfirmationTiming)
                        SVEEffectPool.Instance.CmdExecuteConfirmationTiming();
                }
            }
        }

        public void EvolveCard(NetworkIdentity playerNetId, RuntimeCard baseCard, RuntimeCard evolvedCard, bool useEvolvePoint, bool useEvolveCost = true)
        {
            PlayerInfo player = GetPlayerInfo(playerNetId);
            if(player == null)
                return;

            RuntimeZone evolve = player.namedZones[SVEProperties.Zones.EvolveDeck];
            RuntimeZone field = player.namedZones[SVEProperties.Zones.Field];
            evolve.RemoveCard(evolvedCard);
            field.AddCard(evolvedCard);

            // TODO - support for more than one attached card
            if(evolvedCard.namedStats.TryGetValue(SVEProperties.CardStats.AttachedCardInstanceIDs, out Stat attachedCardInfo))
                attachedCardInfo.baseValue = baseCard.instanceId;
            else
                Debug.LogError($"Failed to get attached card instance ID value for card (instance id {evolvedCard.instanceId}");

            // Pay cost
            if(useEvolveCost)
            {
                if(!baseCard.namedStats.TryGetValue(SVEProperties.CardStats.EvolveCost, out Stat evolveCostStat))
                    Debug.LogError($"Failed to get evolve cost for card (instance id {baseCard.instanceId}");
                int playPointCost = Mathf.Max((evolveCostStat?.effectiveValue ?? 0) - (useEvolvePoint ? 1 : 0), 0);

                player.namedStats[SVEProperties.PlayerStats.PlayPoints].baseValue -= playPointCost;
                player.namedStats[SVEProperties.PlayerStats.EvolutionPoints].baseValue -= useEvolvePoint ? 1 : 0;
            }

            // Handle engage status
            if(baseCard.namedStats.TryGetValue(SVEProperties.CardStats.Engaged, out Stat engagedStat) && engagedStat.effectiveValue > 0)
                EngageCard(evolvedCard);

            // Set face up
            evolvedCard.namedStats[SVEProperties.CardStats.FaceUp].baseValue = 1;

            // Passive handling & effect triggers
            if(isPlayerEffectSolver && playerNetId.isLocalPlayer)
            {
                SVEEffectPool.Instance.UnregisterPassiveAbilities(baseCard);
                SVEEffectPool.Instance.ApplyAllActivePassivesToCard(evolvedCard);
                SVEEffectPool.Instance.RegisterPassiveAbilities(gameState, evolvedCard);

                SVEEffectPool.Instance.TriggerPendingEffects<SveOnEvolveTrigger>(gameState, evolvedCard, player, _ => true, true);
            }
        }

        public void PlaySpell(NetworkIdentity playerNetId, RuntimeCard card, string originZone, int playPoints, Action onComplete = null)
        {
            PlayerInfo player = GetPlayerInfo(playerNetId);
            RuntimeZone startZone = player.namedZones[originZone];
            RuntimeZone resolution = player.namedZones[SVEProperties.Zones.Resolution];
            startZone.RemoveCard(card);
            resolution.AddCard(card);

            if(playPoints > 0)
                player.namedStats[SVEProperties.PlayerStats.PlayPoints].baseValue -= playPoints;

            if(isPlayerEffectSolver && playerNetId.isLocalPlayer)
            {
                SVEEffectPool.Instance.TriggerPendingEffects<SveStartEndPhaseTrigger>(gameState, card, player, _ => true, executeConfirmationTiming: false,
                    triggerState: SVEEffectPool.EffectTriggerState.StartEndPhase);
                SVEEffectPool.Instance.TriggerPendingEffectsForOtherCardsInZone<SveOnPlaySpellTrigger>(gameState, card, player.namedZones[SVEProperties.Zones.Field], player,
                    _ => true, false);

                SVEEffectPool.Instance.TriggerSpellImmediate(gameState, card, player, onComplete);
            }
        }

        public void FinishSpell(NetworkIdentity playerNetId, RuntimeCard card)
        {
            PlayerInfo player = GetPlayerInfo(playerNetId);
            RuntimeZone resolution = player.namedZones[SVEProperties.Zones.Resolution];
            resolution.RemoveCard(card);
            if(card.IsToken())
            {
                // Do nothing - destroy card
            }
            else
            {
                RuntimeZone cemetery = player.namedZones[SVEProperties.Zones.Cemetery];
                cemetery.AddCard(card);
            }
        }

        public void SendToCemetery(NetworkIdentity playerNetId, RuntimeCard card, string cardZone)
        {
            PlayerInfo player = GetPlayerInfo(playerNetId);
            StandardSendRuntimeCardToZone(player, card, cardZone, SVEProperties.Zones.Cemetery);

            if(cardZone.Equals(SVEProperties.Zones.Field) && isPlayerEffectSolver)
            {
                if(player.netId.isLocalPlayer)
                {
                    SVEEffectPool.Instance.UnregisterPassiveAbilities(card);
                    SVEEffectPool.Instance.TriggerPendingEffects<SveLastWordsTrigger>(gameState, card, card.ownerPlayer, _ => true, false);
                    SVEEffectPool.Instance.TriggerPendingEffects<SveOnCardLeaveFieldTrigger>(gameState, card, card.ownerPlayer, _ => true, false);
                    SVEEffectPool.Instance.TriggerPendingEffectsForOtherCardsInZone<SveOnOtherCardLeaveFieldTrigger>(gameState, card, player.namedZones[SVEProperties.Zones.Field], player,
                        x => x.MatchesFilter(card), false);
                }
                else
                {
                    PlayerInfo localPlayer = gameState.players.Find(x => x.netId.isLocalPlayer);
                    SVEEffectPool.Instance.TriggerPendingEffectsForOtherCardsInZone<SveOnOpponentCardLeaveFieldTrigger>(gameState, card, localPlayer.namedZones[SVEProperties.Zones.Field], localPlayer,
                        x => x.MatchesFilter(card), false);
                }
            }
        }

        public void BanishCard(NetworkIdentity playerNetId, RuntimeCard card, string cardZone)
        {
            PlayerInfo player = GetPlayerInfo(playerNetId);
            StandardSendRuntimeCardToZone(player, card, cardZone, SVEProperties.Zones.Banished);

            if(cardZone.Equals(SVEProperties.Zones.Field) && isPlayerEffectSolver && player.netId.isLocalPlayer)
            {
                SVEEffectPool.Instance.UnregisterPassiveAbilities(card);
                SVEEffectPool.Instance.TriggerPendingEffects<SveOnCardLeaveFieldTrigger>(gameState, card, card.ownerPlayer, _ => true, false);
                SVEEffectPool.Instance.TriggerPendingEffectsForOtherCardsInZone<SveOnOtherCardLeaveFieldTrigger>(gameState, card, player.namedZones[SVEProperties.Zones.Field], player,
                    x => x.MatchesFilter(card), false);
            }
        }

        public void ReturnCardToHand(PlayerInfo player, RuntimeCard card, string cardZone)
        {
            ReserveCard(card);
            StandardSendRuntimeCardToZone(player, card, cardZone, SVEProperties.Zones.Hand);

            if(cardZone.Equals(SVEProperties.Zones.Field) && isPlayerEffectSolver && player.netId.isLocalPlayer)
            {
                SVEEffectPool.Instance.UnregisterPassiveAbilities(card);
                SVEEffectPool.Instance.TriggerPendingEffects<SveOnCardReturnToHandFromField>(gameState, card, player, _ => true, false);
                SVEEffectPool.Instance.TriggerPendingEffects<SveOnCardLeaveFieldTrigger>(gameState, card, card.ownerPlayer, _ => true, false);
                SVEEffectPool.Instance.TriggerPendingEffectsForOtherCardsInZone<SveOnOtherCardLeaveFieldTrigger>(gameState, card, player.namedZones[SVEProperties.Zones.Field], player,
                    x => x.MatchesFilter(card), false);
            }
        }

        public void SendCardToBottomDeck(PlayerInfo player, RuntimeCard card, string cardZone)
        {
            ReserveCard(card);
            StandardSendRuntimeCardToZone(player, card, cardZone, SVEProperties.Zones.Deck);

            if(cardZone.Equals(SVEProperties.Zones.Field) && isPlayerEffectSolver && player.netId.isLocalPlayer)
            {
                SVEEffectPool.Instance.UnregisterPassiveAbilities(card);
                SVEEffectPool.Instance.TriggerPendingEffects<SveOnCardLeaveFieldTrigger>(gameState, card, card.ownerPlayer, _ => true, false);
                SVEEffectPool.Instance.TriggerPendingEffectsForOtherCardsInZone<SveOnOtherCardLeaveFieldTrigger>(gameState, card, player.namedZones[SVEProperties.Zones.Field], player,
                    x => x.MatchesFilter(card), false);
            }
        }

        public void SendCardToTopDeck(PlayerInfo player, RuntimeCard card, string cardZone)
        {
            ReserveCard(card);
            StandardSendRuntimeCardToZone(player, card, cardZone, SVEProperties.Zones.Deck, addToTop: true);

            if(cardZone.Equals(SVEProperties.Zones.Field) && isPlayerEffectSolver && player.netId.isLocalPlayer)
            {
                SVEEffectPool.Instance.UnregisterPassiveAbilities(card);
                SVEEffectPool.Instance.TriggerPendingEffects<SveOnCardLeaveFieldTrigger>(gameState, card, card.ownerPlayer, _ => true, false);
                SVEEffectPool.Instance.TriggerPendingEffectsForOtherCardsInZone<SveOnOtherCardLeaveFieldTrigger>(gameState, card, player.namedZones[SVEProperties.Zones.Field], player,
                    x => x.MatchesFilter(card), false);
            }
        }

        public void SendCardToExArea(PlayerInfo player, RuntimeCard card, string cardZone)
        {
            ReserveCard(card);
            StandardSendRuntimeCardToZone(player, card, cardZone, SVEProperties.Zones.ExArea);

            if(cardZone.Equals(SVEProperties.Zones.Field) && isPlayerEffectSolver && player.netId.isLocalPlayer)
            {
                SVEEffectPool.Instance.UnregisterPassiveAbilities(card);
                SVEEffectPool.Instance.TriggerPendingEffects<SveOnCardLeaveFieldTrigger>(gameState, card, card.ownerPlayer, _ => true, false);
                SVEEffectPool.Instance.TriggerPendingEffectsForOtherCardsInZone<SveOnOtherCardLeaveFieldTrigger>(gameState, card, player.namedZones[SVEProperties.Zones.Field], player,
                    x => x.MatchesFilter(card), false);
            }
        }

        // ---

        /// <summary>
        /// Sends given card to desired zone, with handling evolved cards and tokens having additional logic compared to regular cards
        /// </summary>
        private void StandardSendRuntimeCardToZone(PlayerInfo player, RuntimeCard card, string startZoneName, string endZoneName, bool addToTop = false)
        {
            RuntimeZone startZone = player.namedZones[startZoneName];
            RuntimeZone endZone = player.namedZones[endZoneName];
            List<RuntimeCard> cardsToMove = new() { card };
            if(card.namedStats.TryGetValue(SVEProperties.CardStats.AttachedCardInstanceIDs, out Stat attachedCardInfo)) // TODO - support for more than one attached card
            {
                int attachedCardID = attachedCardInfo.baseValue;
                if(attachedCardID >= 0)
                {
                    RuntimeCard attachedCard = startZone.cards.FirstOrDefault(x => x.instanceId == attachedCardID);
                    if(attachedCard != null)
                        cardsToMove.Add(attachedCard);
                    else
                        Debug.LogError($"Failed to find attached card with instance ID {attachedCardID} to send to cemetery!");
                }
                attachedCardInfo.baseValue = -1;
            }

            foreach(RuntimeCard cardToMove in cardsToMove)
            {
                startZone.RemoveCard(cardToMove);
                if(card.IsToken())
                {
                    // Do nothing - destroy card
                    continue;
                }
                if(card.IsEvolvedType())
                {
                    player.namedZones[SVEProperties.Zones.EvolveDeck].AddCard(cardToMove);
                }
                else
                {
                    if(addToTop)
                        endZone.AddCardToTop(cardToMove);
                    else
                        endZone.AddCard(cardToMove);
                }
                if(startZone.name.Equals(SVEProperties.Zones.Field) && cardToMove.namedStats.TryGetValue(SVEProperties.CardStats.Engaged, out Stat engagedStat))
                    engagedStat.baseValue = 0;
            }
        }

        #endregion

        // ------------------------------

        #region Tokens

        public RuntimeCard CreateAndAddToken(NetworkIdentity playerNetId, int libraryCardId, int runtimeCardInstanceId, RuntimeZone targetZone)
        {
            PlayerInfo player = GetPlayerInfo(playerNetId);
            Card libraryCard = LibraryCardCache.GetCard(libraryCardId, GameManager.Instance.config);
            RuntimeCard card = new();
            card.InitializeFromLibraryCard(libraryCard, runtimeCardInstanceId, player);

            targetZone.AddCard(card);
            if(isPlayerEffectSolver && playerNetId.isLocalPlayer)
            {
                if(targetZone == player.namedZones[SVEProperties.Zones.Field])
                {
                    SVEEffectPool.Instance.ApplyAllActivePassivesToCard(card);
                    SVEEffectPool.Instance.TriggerPendingEffectsForOtherCardsInZone<SveOnOtherCardEnterFieldTrigger>(gameState, card, SVEProperties.Zones.Field, targetZone, player,
                        x => x.MatchesFilter(card), false);
                }
                SVEEffectPool.Instance.CmdExecuteConfirmationTiming();
            }
            return card;
        }

        #endregion

        // ------------------------------

        #region Card Stats/Combat

        public void DeclareAttack(NetworkIdentity playerNetId, RuntimeCard card, bool isAttackingLeader)
        {
            PlayerInfo player = GetPlayerInfo(playerNetId);
            EngageCard(card);
            if(isPlayerEffectSolver && playerNetId.isLocalPlayer)
            {
                SVEEffectPool.Instance.TriggerPendingEffects<SveOnAttackTrigger>(gameState, card, player, _ => true, true);
                if(isAttackingLeader)
                    SVEEffectPool.Instance.TriggerPendingEffects<SveOnAttackLeaderTrigger>(gameState, card, player, _ => true, true);
                else
                    SVEEffectPool.Instance.TriggerPendingEffects<SveOnAttackFollowerTrigger>(gameState, card, player, _ => true, true);

                SVEEffectPool.Instance.TriggerPendingEffectsForOtherCardsInZone<SveOnOtherCardAttackTrigger>(gameState, card, player.namedZones[SVEProperties.Zones.Field], player,
                    x => x.MatchesFilter(card), false);
            }
        }

        public void ReserveCard(RuntimeCard card)
        {
            if(card.namedStats.TryGetValue(SVEProperties.CardStats.Engaged, out Stat engagedStat))
                engagedStat.baseValue = 0;
        }

        public void EngageCard(RuntimeCard card)
        {
            if(card.namedStats.TryGetValue(SVEProperties.CardStats.Engaged, out Stat engagedStat))
                engagedStat.baseValue = 1;
        }

        public void SetCardStat(RuntimeCard card, int statId, int value)
        {
            card.stats[statId].baseValue = value;
            CheckZeroDefenseFollower(card.ownerPlayer.netId, card);
        }

        public void ApplyCardStatModifier(RuntimeCard card, int statId, int value, bool adding, int duration = 0, bool checkDefense = true)
        {
            Modifier modifier = new(value, duration);
            if(adding)
                card.stats[statId].AddModifier(modifier);
            else
                card.stats[statId].RemoveModifier(modifier);
            if(checkDefense)
                CheckZeroDefenseFollower(card.ownerPlayer.netId, card);
        }

        public void ApplyKeywordToCard(RuntimeCard card, int type, int value, bool adding)
        {
            if(adding)
                card.AddKeyword(type, value);
            else
                card.RemoveKeyword(type, value);
        }

        public void FightFollower(NetworkIdentity playerNetId, RuntimeCard attackingCard, RuntimeCard defendingCard)
        {
            int attackerDamage = GetCardDamageOutput(attackingCard, defendingCard);
            int defenderDamage = GetCardDamageOutput(defendingCard, attackingCard);

            // Do not check for 0 defense here - instead check below during Bane handling
            ApplyCardStatModifier(attackingCard, attackingCard.namedStats[SVEProperties.CardStats.Defense].statId, -defenderDamage, true, checkDefense: false);
            ApplyCardStatModifier(defendingCard, attackingCard.namedStats[SVEProperties.CardStats.Defense].statId, -attackerDamage, true, checkDefense: false);

            // Attacker Drain
            if(attackingCard.HasKeyword(SVEProperties.Keywords.Drain))
                AddLeaderDefense(attackingCard.ownerPlayer, attackerDamage);

            // Check defender destruction
            if(defendingCard.HasKeyword(SVEProperties.Keywords.Bane))
                SendToCemetery(attackingCard.ownerPlayer.netId, attackingCard, SVEProperties.Zones.Field);
            else
                CheckZeroDefenseFollower(attackingCard.ownerPlayer.netId, attackingCard);

            // Check attacker destruction
            if(attackingCard.HasKeyword(SVEProperties.Keywords.Bane))
                SendToCemetery(defendingCard.ownerPlayer.netId, defendingCard, SVEProperties.Zones.Field);
            else
                CheckZeroDefenseFollower(defendingCard.ownerPlayer.netId, defendingCard);

            // Confirmation timing
            if(isPlayerEffectSolver && playerNetId.isLocalPlayer)
                SVEEffectPool.Instance.CmdExecuteConfirmationTiming();
        }

        public void FightLeader(NetworkIdentity playerNetId, RuntimeCard attackingCard, PlayerInfo defendingPlayer)
        {
            int damage = GetCardDamageOutput(attackingCard);
            AddLeaderDefense(defendingPlayer, -damage);

            // Attacker Drain
            if(attackingCard.HasKeyword(SVEProperties.Keywords.Drain))
                AddLeaderDefense(attackingCard.ownerPlayer, damage);

            // Confirmation timing
            if(isPlayerEffectSolver && playerNetId.isLocalPlayer)
                SVEEffectPool.Instance.CmdExecuteConfirmationTiming();
        }

        public void AddLeaderDefense(PlayerInfo player, int amount)
        {
            player.namedStats[SVEProperties.PlayerStats.Defense].baseValue += amount;
        }

        public void CheckZeroDefenseFollower(NetworkIdentity playerNetId, RuntimeCard card)
        {
            PlayerInfo player = GetPlayerInfo(playerNetId);
            if(!card.IsFollowerOrEvolvedFollower() || !player.namedZones[SVEProperties.Zones.Field].cards.Contains(card) || card.namedStats[SVEProperties.CardStats.Defense].effectiveValue > 0)
                return;

            SendToCemetery(playerNetId, card, SVEProperties.Zones.Field);
        }

        private int GetCardDamageOutput(RuntimeCard attacker, RuntimeCard defender = null)
        {
            if(attacker.HasKeyword(SVEProperties.PassiveAbilities.CannotDealDamage))
                return 0;

            int damage = attacker.HasKeyword(SVEProperties.PassiveAbilities.UseDefAsAtk)
                ? attacker.namedStats[SVEProperties.CardStats.Defense].effectiveValue
                : attacker.namedStats[SVEProperties.CardStats.Attack].effectiveValue;
            if(attacker.HasKeyword(SVEProperties.PassiveAbilities.Plus1Damage))
                damage += 1;
            if(attacker.HasKeyword(SVEProperties.PassiveAbilities.Plus2Damage))
                damage += 2;
            if(attacker.HasKeyword(SVEProperties.PassiveAbilities.Plus3Damage))
                damage += 3;
            if(attacker.HasKeyword(SVEProperties.PassiveAbilities.Plus4Damage))
                damage += 4;

            if(defender != null && defender.HasKeyword(SVEProperties.PassiveAbilities.DamageReduction1))
                damage = Mathf.Max(damage - 1, 0);

            return damage;
        }

        #endregion

        // ------------------------------

        #region Effect Costs

        public void PayAbilityCosts(PlayerInfo player, RuntimeCard card, List<Cost> costs, MoveCardToZoneData[] cardsMoveToZone, RemoveCounterData[] countersToRemove)
        {
            foreach(Cost cost in costs)
            {
                switch(cost)
                {
                    case PlayPointCost ppCost:
                        player.namedStats[SVEProperties.PlayerStats.PlayPoints].baseValue -= SVEFormulaParser.ParseValue(ppCost.amount);
                        break;
                    case EngageSelfCost:
                        EngageCard(card);
                        break;
                    case LeaderDefenseCost leaderDefCost:
                        AddLeaderDefense(player, -SVEFormulaParser.ParseValue(leaderDefCost.amount));
                        break;
                }
            }

            for(int i = 0; i < cardsMoveToZone.Length; i++)
            {
                RuntimeCard cardToMove = player.namedZones[cardsMoveToZone[i].startZone].cards.FirstOrDefault(x => x.instanceId == cardsMoveToZone[i].cardInstanceId);
                switch(cardsMoveToZone[i].endZone)
                {
                    case SVEProperties.Zones.Cemetery:
                        SendToCemetery(player.netId, cardToMove, cardsMoveToZone[i].startZone);
                        break;
                    case SVEProperties.Zones.Banished:
                        BanishCard(player.netId, cardToMove, cardsMoveToZone[i].startZone);
                        break;
                    case SVEProperties.Zones.Hand:
                        ReturnCardToHand(player, cardToMove, cardsMoveToZone[i].startZone);
                        break;
                }
            }

            for(int i = 0; i < countersToRemove.Length; i++)
            {
                RuntimeCard targetCard = player.namedZones[countersToRemove[i].cardZone].cards.FirstOrDefault(x => x.instanceId == countersToRemove[i].cardInstanceId);
                int targetAmount = countersToRemove[i].keywordValue - countersToRemove[i].amount;
                targetCard.SetCounterAmount((SVEProperties.Counters)countersToRemove[i].keywordType, targetAmount);
            }
        }

        #endregion
    }
}
