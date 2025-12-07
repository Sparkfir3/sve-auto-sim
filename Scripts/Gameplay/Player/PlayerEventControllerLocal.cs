using System;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CCGKit;
using Mirror;
using Sparkfire.Utility;
using SVESimulator.UI;

namespace SVESimulator
{
    public class PlayerEventControllerLocal : PlayerEventControllerBase
    {
        protected override bool isLocal => true;

        public Action OnFinishSpell;

        // ------------------------------

        #region Game Flow

        public void SetGoingFirst(bool localUserIsFirst)
        {
            if(playerInfo.isGoingFirstDecided)
                return;

            SetGoingFirstPlayerMessage msg = new()
            {
                playerNetId = localUserIsFirst ? netIdentity : opponentInfo.netId
            };
            NetworkClient.Send(msg);

            GameUIManager.GoingFirstScreen.SetDisplayMode(SelectGoingFirstScreen.Mode.None, false);
            // playerController.InitializeEvolvePointDisplays(localUserIsFirst); // don't need to call it here??? how much duct tape did i use to make this system lmao
            StartCoroutine(playerController.StopTurnOnDelay());
        }
        
        public void Mulligan(bool performMulligan, bool endTurn = true)
        {
            if(!performMulligan)
            {
                if(endTurn)
                    playerController.StopTurn();
                return;
            }
            
            // ---

            // Choose card order for mulligan
            int cardCount = localZoneController.handZone.AllCards.Count;
            localZoneController.selectionArea.Enable(CardSelectionArea.SelectionMode.MoveSelectionArea, cardCount, cardCount);
            localZoneController.selectionArea.AddAllCardsInHand();
            localZoneController.selectionArea.SetFilter(null);
            localZoneController.selectionArea.SetConfirmAction(null, "Confirm Order", "Rearrange Cards for Mulligan", 1, 0, _ =>
            {
                localZoneController.selectionArea.Disable();
                List<CardObject> cards = localZoneController.selectionArea.GetAllPrimaryCards();
                for(int i = 0; i < cards.Count; i++)
                {
                    bool isLastCard = i == cards.Count - 1;
                    sveEffectSolver.SendCardHandToBottomDeck(netIdentity, cards[i].RuntimeCard);
                    localZoneController.SendCardToBottomDeck(cards[i], onComplete: () =>
                    {
                        if(isLastCard)
                        {
                            playerController.DrawStartingHand(delayBeforeDrawing: 1f);
                            if(endTurn)
                                playerController.StopTurn();
                        }
                    });
                }

                LocalPerformMulliganMessage msg = new()
                {
                    playerNetId = netIdentity,
                    cardIdsInOrder = cards.Select(x => x.RuntimeCard.instanceId).ToArray()
                };
                NetworkClient.Send(msg);
            }, showTargetingToOpponent: false);

            // Skip option
            GameUIManager.MultipleChoice.AddSingleEntry("Cancel", () =>
            {
                foreach(CardObject card in localZoneController.selectionArea.GetAllPrimaryCards())
                    localZoneController.AddCardToHand(card);
                localZoneController.selectionArea.Disable();
                if(endTurn)
                    playerController.StopTurn();
            });
        }

        public void ExtraTurn()
        {
            if(!isActivePlayer)
            {
                Debug.LogError($"Player {playerInfo.id} attempted to take an extra turn while they are not the active player!");
                return;
            }
            ExtraTurnMessage msg = new()
            {
                playerNetId = netIdentity
            };
            NetworkClient.Send(msg);
        }

        #endregion
        
        // ------------------------------
        
        #region Card Movement

        public void InitializeDeckAndLeader()
        {
            // Evolve deck
            List<RuntimeCard> evolvedCards = localZoneController.InitializeEvolveDeck();
            foreach(RuntimeCard card in evolvedCards)
            {
                sveEffectSolver.MoveCard(netIdentity, card, SVEProperties.Zones.Deck, SVEProperties.Zones.EvolveDeck);
            }

            // Leader
            // for some reason RuntimeZones aren't getting updated, probably because this gets called on game start??? idk man
            // leaving the effect solver call in here anyways, though
            RuntimeCard leader = localZoneController.deckZone.Runtime.cards.FirstOrDefault(x => x.cardType.name.Equals(SVEProperties.CardTypes.Leader));
            Debug.Assert(leader != null);
            CardObject leaderCard = localZoneController.InitializeLeaderCard(leader, leader.cardId);
            sveEffectSolver.MoveCard(netIdentity, leaderCard.RuntimeCard, SVEProperties.Zones.Deck, SVEProperties.Zones.Leader);

            // Send msg
            LocalInitDeckAndLeaderMessage msg = new()
            {
                playerNetId = netIdentity,
                evolvedCardsInstanceIds = evolvedCards.Select(x => x.instanceId).ToArray(),
                leaderCardInstanceId = leader.instanceId
            };
            NetworkClient.Send(msg);
        }

        public void DrawCard(CardObject cardObject = null, bool reveal = false)
        {
            // TODO - check deck size before drawing
            RuntimeCard runtimeCard = cardObject ? cardObject.RuntimeCard : null;
            if(runtimeCard == null)
            {
                runtimeCard = localZoneController.deckZone.Runtime.cards[0];
                cardObject = localZoneController.CreateNewCardObjectTopDeck(runtimeCard);
            }
            if(!reveal)
                localZoneController.AddCardToHand(cardObject);
            else
                localZoneController.RevealCard(cardObject, onComplete: () => localZoneController.AddCardToHand(cardObject, () => cardObject.Interactable = playerController.isActivePlayer));
            sveEffectSolver.DrawCard(netIdentity, runtimeCard);

            LocalDrawCardMessage msg = new()
            {
                playerNetId = netIdentity,
                cardInstanceId = runtimeCard.instanceId,
                reveal = reveal
            };
            NetworkClient.Send(msg);
        }

        public void TellOpponentDrawCard(int count = 1)
        {
            LocalTellOppDrawCardMessage msg = new()
            {
                playerNetId = netIdentity,
                count = count
            };
            NetworkClient.Send(msg);
        }

        public void MillDeck(int count)
        {
            StartCoroutine(MillCoroutine());
            IEnumerator MillCoroutine()
            {
                for(int i = 0; i < count; i++)
                {
                    RuntimeCard runtimeCard = localZoneController.deckZone.Runtime.cards[0];
                    CardObject cardObject = localZoneController.CreateNewCardObjectTopDeck(runtimeCard);
                    localZoneController.SendCardToCemetery(cardObject);
                    sveEffectSolver.SendToCemetery(netIdentity, runtimeCard, SVEProperties.Zones.Deck);

                    LocalSendCardToCemeteryMessage msg = new()
                    {
                        playerNetId = netIdentity,
                        cardInstanceId = runtimeCard.instanceId,
                        originZone = SVEProperties.Zones.Deck
                    };
                    NetworkClient.Send(msg);
                    yield return new WaitForSeconds(0.5f);
                }
            }
        }

        public void TellOpponentMillDeck(int count = 1)
        {
            LocalTellOppMillDeckMessage msg = new()
            {
                playerNetId = netIdentity,
                count = count
            };
            NetworkClient.Send(msg);
        }

        public bool PlayCardToField(CardObject card, string originZone = null, bool payCost = true) =>
            PlayCardToField(card, localZoneController.fieldZone.GetFirstOpenSlotId(), originZone, payCost);
        public bool PlayCardToField(CardObject card, int slot, string originZone = null, bool payCost = true, bool ignoreAltCosts = false)
        {
            if(card.IsCardType(SVEProperties.CardTypes.Spell))
                return false;

            // Valid slot check
            if(!localZoneController.fieldZone.IsSlotNumberValid(slot))
                return false;

            // Cost check
            int playPointCost = 0;
            if(payCost)
            {
                playPointCost = card.RuntimeCard.PlayPointCost(playerController);
                if(!ignoreAltCosts && card.RuntimeCard.HasAvailableAlternateCost(playerController, out List<TriggeredAbility> alternateCostAbilities))
                {
                    if(!CanPayPlayPointsCost(playPointCost) && !alternateCostAbilities.Any(x => CanPayCosts(card.RuntimeCard, (x.trigger as SveTrigger)?.Costs, x.name)))
                        return false;
                    ChoosePlayCardCostOption(card, playPointCost, alternateCostAbilities, usePlayPointCost =>
                    {
                        PlayCardToField(card, slot, originZone, usePlayPointCost, ignoreAltCosts: true);
                    });
                    return true;
                }
                if(!CanPayPlayPointsCost(playPointCost))
                    return false;
            }

            // Play card to field
            playerController.CardsPlayedThisTurn.Add(new PlayedCardData(card.RuntimeCard.instanceId, card.RuntimeCard.cardId));
            bool cardHasWard = card.RuntimeCard.HasKeyword(SVEProperties.Keywords.Ward);

            if(originZone.IsNullOrWhiteSpace())
                originZone = localZoneController.handZone.ContainsCard(card) ? SVEProperties.Zones.Hand : SVEProperties.Zones.ExArea;
            sveEffectSolver.PlayCard(netIdentity, card.RuntimeCard, originZone, playPointCost, executeConfirmationTiming: !cardHasWard);
            localZoneController.PlayCardToField(card, slot);

            // Network message
            LocalPlayCardMessage msg = new()
            {
                playerNetId = netIdentity,
                cardInstanceId = card.RuntimeCard.instanceId,
                fieldSlotId = slot,
                originZone = originZone,
                playPointCost = playPointCost
            };
            NetworkClient.Send(msg);

            // Handle Ward
            if(cardHasWard)
                GameUIManager.MultipleChoice.OpenEngageWardCardOptions(playerController, card);

            return true;
        }

        public bool EvolveCard(CardObject baseCard, bool useEvolvePoint, bool useEvolveCost = true)
        {
            // Condition checks
            if(!isActivePlayer || playerController.EvolvedThisTurn)
                return false;
            if(useEvolveCost)
            {
                int evolveCost = baseCard.GetEvolveCost();
                if(evolveCost < 0 || !CanPayEvolveCost(evolveCost, useEvolvePoint))
                    return false;
            }
            int slot = localZoneController.fieldZone.GetSlotNumber(baseCard);
            if(slot <= -1)
            {
                Debug.LogError($"Failed to find a corresponding slot for card {baseCard.name} on the player's field!");
                return false;
            }
            CardObject evolvedCard = GetEvolvedCardOf(baseCard.RuntimeCard);
            if(!evolvedCard)
                return false;

            // Evolve logic
            sveEffectSolver.EvolveCard(netIdentity, baseCard.RuntimeCard, evolvedCard.RuntimeCard, useEvolvePoint, useEvolveCost);
            localZoneController.EvolveCard(baseCard, evolvedCard, slot);
            evolvedCard.NumberOfTurnsOnBoard = baseCard.NumberOfTurnsOnBoard;
            evolvedCard.CanAttack = baseCard.RuntimeCard.namedStats[SVEProperties.CardStats.Engaged].effectiveValue == 0;
            evolvedCard.CanAttackLeader = evolvedCard.CanAttack && (evolvedCard.NumberOfTurnsOnBoard > 0 || evolvedCard.RuntimeCard.HasKeyword(SVEProperties.Keywords.Storm));

            baseCard.SetHighlightMode(CardObject.HighlightMode.None);
            localZoneController.fieldZone.HighlightCardsCanAttack();
            playerController.EvolvedThisTurn = true;

            // Networking
            LocalEvolveCardMessage msg = new()
            {
                playerNetId = netIdentity,
                baseCardInstanceId = baseCard.RuntimeCard.instanceId,
                evolvedCardInstanceId = evolvedCard.RuntimeCard.instanceId,
                fieldSlotId = slot,
                useEvolvePoint = useEvolvePoint,
                useEvolveCost = useEvolveCost
            };
            NetworkClient.Send(msg);

            // Transfer attack/defense modifiers to card
            SVEEffectPool.Instance.RemovePassivesFromCard(baseCard.RuntimeCard, playerController);
            int atkDiff = baseCard.RuntimeCard.namedStats[SVEProperties.CardStats.Attack].effectiveValue - baseCard.RuntimeCard.namedStats[SVEProperties.CardStats.Attack].baseValue;
            int defDiff = baseCard.RuntimeCard.namedStats[SVEProperties.CardStats.Defense].effectiveValue - baseCard.RuntimeCard.namedStats[SVEProperties.CardStats.Defense].baseValue;
            ApplyModifierToCard(evolvedCard.RuntimeCard, evolvedCard.RuntimeCard.namedStats[SVEProperties.CardStats.Attack].statId, atkDiff, true);
            ApplyModifierToCard(evolvedCard.RuntimeCard, evolvedCard.RuntimeCard.namedStats[SVEProperties.CardStats.Defense].statId, defDiff, true);

            return true;
        }

        public void SendToCemetery(CardObject card, string originZone = null, bool onlyMoveObject = false, bool handleStack = true)
        {
            if(handleStack && CounterUtilities.HandleStackLeaveField(playerController, card))
                return;

            RuntimeCard runtimeCard = card.RuntimeCard;
            originZone ??= card.CurrentZone.Runtime.name;
            bool isLocalPlayersCard = card.CurrentZone.IsLocalPlayerZone;
            PlayerCardZoneController targetZoneController = isLocalPlayersCard ? localZoneController : oppZoneController;

            if(!onlyMoveObject)
                sveEffectSolver.SendToCemetery(isLocalPlayersCard ? netIdentity : opponentInfo.netId, runtimeCard, originZone);
            StandardSendCardObjectToZone(card, targetZoneController, (x, onComplete) => targetZoneController.SendCardToCemetery(x, onComplete));

            // ---
            
            if(onlyMoveObject)
                return;
            LocalSendCardToCemeteryMessage msg = new()
            {
                playerNetId = netIdentity,
                cardInstanceId = runtimeCard.instanceId,
                isOpponentCard = !isLocalPlayersCard,
                originZone = originZone
            };
            NetworkClient.Send(msg);
        }

        public void DestroyCard(CardObject card, bool handleStack = true)
        {
            if(handleStack && CounterUtilities.HandleStackLeaveField(playerController, card))
                return;
            if(card.CurrentZone.IsLocalPlayerZone)
            {
                SendToCemetery(card, handleStack: handleStack);
                return;
            }

            // We cannot destroy an opponent's card directly, we must tell them to destroy it themselves
            // TODO - we actually can, so need to fix it
            LocalDestroyOpponentCardMessage msg = new()
            {
                playerNetId = netIdentity,
                cardInstanceId = card.RuntimeCard.instanceId
            };
            NetworkClient.Send(msg);
        }

        public void BanishCard(CardObject card, bool sendMessage = true, bool onlyMoveObject = false)
        {
            if(CounterUtilities.HandleStackLeaveField(playerController, card))
                return;

            RuntimeCard runtimeCard = card.RuntimeCard;
            string originZone = card.CurrentZone.Runtime.name;
            bool isLocalPlayersCard = card.CurrentZone.IsLocalPlayerZone;
            PlayerCardZoneController targetZoneController = isLocalPlayersCard ? localZoneController : oppZoneController;

            if(!onlyMoveObject)
                sveEffectSolver.BanishCard(isLocalPlayersCard ? netIdentity : opponentInfo.netId, runtimeCard, originZone);
            StandardSendCardObjectToZone(card, targetZoneController, (x, onComplete) => targetZoneController.SendCardToBanishedZone(x, onComplete));

            // ---

            if(!sendMessage || onlyMoveObject)
                return;
            LocalBanishCardMessage msg = new()
            {
                playerNetId = netIdentity,
                cardInstanceId = runtimeCard.instanceId,
                isOpponentCard = !isLocalPlayersCard,
                originZone = originZone
            };
            NetworkClient.Send(msg);
        }

        public void ReturnToHand(CardObject card, string sourceZone, bool onlyMoveObject = false)
        {
            if(CounterUtilities.HandleStackLeaveField(playerController, card))
                return;

            bool isLocalPlayersCard = card.CurrentZone.IsLocalPlayerZone || localZoneController.selectionArea.ContainsCard(card);
            PlayerCardZoneController targetZoneController = isLocalPlayersCard ? localZoneController : oppZoneController;
            RuntimeCard runtimeCard = card.RuntimeCard;

            if(!onlyMoveObject)
                sveEffectSolver.ReturnCardToHand(isLocalPlayersCard ? playerInfo : opponentInfo, runtimeCard, sourceZone);
            StandardSendCardObjectToZone(card, targetZoneController, (x, onComplete) => targetZoneController.AddCardToHand(x, onComplete));

            // ---

            if(onlyMoveObject)
                return;
            LocalReturnToHandMessage msg = new()
            {
                playerNetId = netIdentity,
                isOpponentCard = !isLocalPlayersCard,
                cardInstanceId = runtimeCard.instanceId,
                originZone = sourceZone
            };
            NetworkClient.Send(msg);
        }

        public void SendToBottomDeck(CardObject card, string originZone = null)
        {
            if(CounterUtilities.HandleStackLeaveField(playerController, card))
                return;

            RuntimeCard runtimeCard = card.RuntimeCard;
            originZone ??= card.CurrentZone.Runtime.name;
            bool isLocalPlayersCard = card.CurrentZone.IsLocalPlayerZone;
            PlayerCardZoneController targetZoneController = isLocalPlayersCard ? localZoneController : oppZoneController;

            sveEffectSolver.SendCardToBottomDeck(isLocalPlayersCard ? playerInfo : opponentInfo, runtimeCard, originZone);
            StandardSendCardObjectToZone(card, targetZoneController, (x, onComplete) => targetZoneController.SendCardToBottomDeck(x, onComplete));

            // ---

            LocalSendToBottomDeckMessage msg = new()
            {
                playerNetId = netIdentity,
                isOpponentCard = !isLocalPlayersCard,
                cardInstanceId = runtimeCard.instanceId,
                originZone = originZone
            };
            NetworkClient.Send(msg);
        }

        public void SendToTopDeck(CardObject card, string originZone = null)
        {
            if(CounterUtilities.HandleStackLeaveField(playerController, card))
                return;

            RuntimeCard runtimeCard = card.RuntimeCard;
            originZone ??= card.CurrentZone.Runtime.name;
            bool isLocalPlayersCard = card.CurrentZone.IsLocalPlayerZone;
            PlayerCardZoneController targetZoneController = isLocalPlayersCard ? localZoneController : oppZoneController;

            sveEffectSolver.SendCardToTopDeck(isLocalPlayersCard ? playerInfo : opponentInfo, runtimeCard, originZone);
            StandardSendCardObjectToZone(card, targetZoneController, (x, onComplete) => targetZoneController.SendCardToTopDeck(x, onComplete));

            // ---

            LocalSendToTopDeckMessage msg = new()
            {
                playerNetId = netIdentity,
                isOpponentCard = !isLocalPlayersCard,
                cardInstanceId = runtimeCard.instanceId,
                originZone = originZone
            };
            NetworkClient.Send(msg);
        }

        public CardObject TopDeckToExArea(CardObject card = null)
        {
            if(!localZoneController.exAreaZone.HasOpenSlot())
                return card;

            RuntimeCard runtimeCard = card ? card.RuntimeCard : null;
            if(runtimeCard == null)
            {
                runtimeCard = localZoneController.deckZone.Runtime.cards[0];
                card = localZoneController.CreateNewCardObjectTopDeck(runtimeCard);
            }
            SendToExArea(card, SVEProperties.Zones.Deck);
            return card;
        }

        public void SendToExArea(CardObject card, string originZone = null)
        {
            if(!localZoneController.exAreaZone.HasOpenSlot() || CounterUtilities.HandleStackLeaveField(playerController, card))
                return;

            RuntimeCard runtimeCard = card.RuntimeCard;
            originZone ??= card.CurrentZone.Runtime.name;
            bool isLocalPlayersCard = card.CurrentZone.IsLocalPlayerZone;
            PlayerCardZoneController targetZoneController = isLocalPlayersCard ? localZoneController : oppZoneController;
            int targetSlotId = localZoneController.exAreaZone.GetFirstOpenSlotId();

            sveEffectSolver.SendCardToExArea(isLocalPlayersCard ? playerInfo : opponentInfo, runtimeCard, originZone);
            StandardSendCardObjectToZone(card, targetZoneController, (x, onComplete) => targetZoneController.SendCardToExArea(x, targetSlotId, onComplete));

            // ---

            LocalSendToExAreaMessage msg = new()
            {
                playerNetId = netIdentity,
                isOpponentCard = !isLocalPlayersCard,
                cardInstanceId = runtimeCard.instanceId,
                fieldSlotId = targetSlotId,
                originZone = originZone
            };
            NetworkClient.Send(msg);
        }

        #endregion

        // ------------------------------

        #region Tokens

        public CardObject CreateToken(string tokenName, SVEProperties.TokenCreationOption creationOption)
        {
            Card libraryCard = GameManager.Instance.config.cards.FirstOrDefault(x => x.name.Equals(tokenName));
            if(libraryCard == null)
            {
                Debug.LogError($"Failed to find card data for token: {tokenName}");
                return null;
            }
            LibraryCardCache.CacheCard(libraryCard);

            // ---

            // Get target slot
            CardPositionedZone targetObjectZone;
            int targetSlotId = -1;
            switch(creationOption)
            {
                case SVEProperties.TokenCreationOption.Field:
                    targetObjectZone = localZoneController.fieldZone;
                    targetSlotId = localZoneController.fieldZone.GetFirstOpenSlotId();
                    break;
                case SVEProperties.TokenCreationOption.ExArea:
                    targetObjectZone = localZoneController.exAreaZone;
                    targetSlotId = localZoneController.exAreaZone.GetFirstOpenSlotId();
                    break;
                default:
                    Debug.LogError($"Attempted to create token with invalid TokenCreationOption: {creationOption}");
                    return null;
            }
            if(targetSlotId < 0)
            {
                Debug.Log($"Attempted to create token {tokenName} for local player with option {creationOption}, but no open slots are available");
                return null;
            }

            // Create & place card
            RuntimeCard runtimeCard = sveEffectSolver.CreateAndAddToken(netIdentity, libraryCard.id, playerInfo.currentCardInstanceId++, targetObjectZone.Runtime);
            CardObject cardObject = CardManager.Instance.RequestCard(runtimeCard);
            localZoneController.AddAndPlaceToken(cardObject, targetObjectZone, targetSlotId);

            // Send message
            LocalCreateTokenMessage msg = new()
            {
                playerNetId = netIdentity,
                libraryCardId = libraryCard.id,
                runtimeCardInstanceId = runtimeCard.instanceId,
                createOnField = targetObjectZone == localZoneController.fieldZone,
                slotId = targetSlotId
            };
            NetworkClient.Send(msg);
            return cardObject;
        }

        public void TransformCard(CardObject targetCard, string tokenName)
        {
            if(targetCard.CurrentZone is not CardPositionedZone targetZone)
            {
                Debug.LogError($"Attempted to transform card {targetCard.LibraryCard.name} in invalid zone {targetCard.CurrentZone.Runtime.name} to token {tokenName}");
                return;
            }
            Card tokenLibraryCard = GameManager.Instance.config.cards.FirstOrDefault(x => x.name.Equals(tokenName)); // TODO - optimize library cache usage
            if(tokenLibraryCard == null)
            {
                Debug.LogError($"Failed to find card data for token: {tokenName}");
                return;
            }
            LibraryCardCache.CacheCard(tokenLibraryCard);

            // Banish old card & create token
            RuntimeCard targetRuntimeCard = targetCard.RuntimeCard;
            int slotId = targetZone.GetSlotNumber(targetCard);
            bool isLocalPlayersCard = targetCard.CurrentZone.IsLocalPlayerZone;
            if(!isLocalPlayersCard)
            {
                Debug.LogError("Targeting an opponent's card with Transform is not currently supported");
                return;
            }

            BanishCard(targetCard, sendMessage: false);
            RuntimeCard tokenRuntimeCard = sveEffectSolver.CreateAndAddToken(netIdentity, tokenLibraryCard.id,
                isLocalPlayersCard ? playerInfo.currentCardInstanceId++ : opponentInfo.currentCardInstanceId++, targetZone.Runtime);
            CardObject tokenCardObject = CardManager.Instance.RequestCard(tokenRuntimeCard);
            localZoneController.AddAndPlaceToken(tokenCardObject, targetZone, slotId);

            // Send message
            LocalTransformCardMessage msg = new()
            {
                playerNetId = netIdentity,
                targetCardInstanceId = targetRuntimeCard.instanceId,
                isOpponentCard = !isLocalPlayersCard,
                originZone = targetZone.Runtime.name,

                libraryCardId = tokenLibraryCard.id,
                tokenRuntimeCardInstanceId = tokenRuntimeCard.instanceId,
                slotId = slotId
            };
            NetworkClient.Send(msg);
        }

        #endregion

        // ------------------------------

        #region Spells

        public bool PlaySpell(CardObject card, string originZone, int? fixedCost = null, bool ignoreAltCosts = false)
        {
            if(!card.IsCardType(SVEProperties.CardTypes.Spell))
                return false;

            // Condition Check
            TriggeredAbility spellAbility = card.LibraryCard.abilities.FirstOrDefault(x => x is TriggeredAbility { trigger: SpellAbility }) as TriggeredAbility;
            string condition = (spellAbility?.trigger as SveTrigger)?.condition;
            if(spellAbility != null && !condition.IsNullOrWhiteSpace() && !SVEFormulaParser.ParseValueAsCondition(condition, playerController, card.RuntimeCard))
                return false;

            // Cost Check
			int playPointCost = fixedCost ?? card.RuntimeCard.PlayPointCost(playerController);
            if(!ignoreAltCosts && card.RuntimeCard.HasAvailableAlternateCost(playerController, out List<TriggeredAbility> alternateCostAbilities))
            {
                if(!CanPayPlayPointsCost(playPointCost) && !alternateCostAbilities.Any(x => CanPayCosts(card.RuntimeCard, (x.trigger as SveTrigger)?.Costs, x.name)))
                    return false;
                ChoosePlayCardCostOption(card, playPointCost, alternateCostAbilities, usePlayPointCost =>
                {
                    PlaySpell(card, originZone, usePlayPointCost ? playPointCost : 0, ignoreAltCosts: true);
                });
                return true;
            }
            if(!CanPayPlayPointsCost(playPointCost))
                return false;

            // Play Spell
            card.SetHighlightMode(CardObject.HighlightMode.None);
            localZoneController.SendCardToResolution(card);
            playerController.CardsPlayedThisTurn.Add(new PlayedCardData(card.RuntimeCard.instanceId, card.RuntimeCard.cardId));
            sveEffectSolver.PlaySpell(netIdentity, card.RuntimeCard, originZone, playPointCost, onComplete: () =>
            {
                FinishSpell(card);
            });

            LocalPlaySpellMessage msg = new()
            {
                playerNetId = netIdentity,
                cardInstanceId = card.RuntimeCard.instanceId,
                originZone = originZone,
				playPointCost = playPointCost
            };
            NetworkClient.Send(msg);
            return true;
        }

        public void FinishSpell(CardObject card)
        {
            StartCoroutine(FinishOnDelay());
            IEnumerator FinishOnDelay()
            {
                yield return new WaitForSeconds(0.5f);
                RuntimeCard runtimeCard = card.RuntimeCard;

                sveEffectSolver.FinishSpell(netIdentity, runtimeCard);
                card.SetHighlightMode(CardObject.HighlightMode.None);
                if(card.IsToken())
                    CardManager.Instance.ReleaseCard(card);
                else
                    localZoneController.SendCardToCemetery(card);

                LocalFinishSpellMessage msg = new()
                {
                    playerNetId = netIdentity,
                    cardInstanceId = runtimeCard.instanceId
                };
                NetworkClient.Send(msg);

                SVEEffectPool.Instance.CmdExecuteConfirmationTiming();
                OnFinishSpell?.Invoke();
                OnFinishSpell = null;
            }
        }

        #endregion
        
        // ------------------------------
        
        #region Combat

        private void DeclareAttack(CardObject attackingCard, bool isAttackingLeader)
        {
            sveEffectSolver.DeclareAttack(netIdentity, attackingCard.RuntimeCard, isAttackingLeader);
            LocalDeclareAttackMessage msg = new()
            {
                playerNetId = netIdentity,
                cardInstanceId = attackingCard.RuntimeCard.instanceId,
                isAttackingLeader = isAttackingLeader
            };
            NetworkClient.Send(msg);
        }
        
        public void AttackFollower(CardObject attackingCard, CardObject defendingCard)
        {
            if(!isActivePlayer || attackingCard == null || defendingCard == null)
                return;

            DeclareAttack(attackingCard, isAttackingLeader: false);
            SVEEffectPool.Instance.OnConfirmationTimingEnd += () =>
            {
                CardManager.Animator.PlayAttackPreview(attackingCard, defendingCard);
                SVEQuickTimingController.Instance.CallQuickTimingCombat(attackingCard, defendingCard, () =>
                {
                    CardManager.Animator.EndAttackPreview();
                    if(attackingCard.CurrentZone != localZoneController.fieldZone) // cancel attack if attacking card is no longer on field
                        return;

                    CardManager.Animator.PlayAttackAnimation(attackingCard, defendingCard, () =>
                    {
                        sveEffectSolver.FightFollower(netIdentity, attackingCard.RuntimeCard, defendingCard.RuntimeCard);
                        // Runtime card gets moved in the effect solver, so we only need to move the game object here
                        if(attackingCard.RuntimeCard.namedStats[SVEProperties.CardStats.Defense].effectiveValue <= 0 || defendingCard.RuntimeCard.HasKeyword(SVEProperties.Keywords.Bane))
                            SendToCemetery(attackingCard, onlyMoveObject: true);
                        if(defendingCard.RuntimeCard.namedStats[SVEProperties.CardStats.Defense].effectiveValue <= 0 || attackingCard.RuntimeCard.HasKeyword(SVEProperties.Keywords.Bane))
                            SendToCemetery(defendingCard, onlyMoveObject: true);
                    });

                    LocalAttackFollowerMessage msg = new()
                    {
                        attackingPlayerNetId = netIdentity,
                        attackerInstanceId = attackingCard.RuntimeCard.instanceId,
                        defenderInstanceId = defendingCard.RuntimeCard.instanceId
                    };
                    NetworkClient.Send(msg);
                });
            };
        }

        public void AttackLeader(CardObject attackingCard)
        {
            if(!isActivePlayer || attackingCard == null)
                return;

            DeclareAttack(attackingCard, isAttackingLeader: true);
            SVEEffectPool.Instance.OnConfirmationTimingEnd += () =>
            {
                CardManager.Animator.PlayAttackPreview(attackingCard, oppZoneController.LeaderCardObject);
                SVEQuickTimingController.Instance.CallQuickTimingCombat(attackingCard, oppZoneController.LeaderCardObject, () =>
                {
                    CardManager.Animator.EndAttackPreview();
                    if(attackingCard.CurrentZone != localZoneController.fieldZone) // cancel attack if attacking card is no longer on field
                        return;

                    CardManager.Animator.PlayAttackAnimation(attackingCard, oppZoneController.LeaderCardObject, () => { sveEffectSolver.FightLeader(playerInfo.netId, attackingCard.RuntimeCard, opponentInfo); });
                    LocalAttackLeaderMessage msg = new()
                    {
                        playerNetId = netIdentity,
                        attackerInstanceId = attackingCard.RuntimeCard.instanceId
                    };
                    NetworkClient.Send(msg);
                });
            };
        }

        public void AddLeaderDefense(PlayerInfo targetPlayer, int amount)
        {
            sveEffectSolver.AddLeaderDefense(targetPlayer, amount);
            LocalAddLeaderDefenseMessage msg = new()
            {
                playerNetId = netIdentity,
                targetPlayer = targetPlayer.netId,
                amount = amount
            };
            NetworkClient.Send(msg);
        }

        public void ReserveCard(RuntimeCard card)
        {
            bool isLocalPlayersCard = card.ownerPlayer.netId.isLocalPlayer;
            sveEffectSolver.ReserveCard(card);
            LocalReserveCardMessage msg = new()
            {
                playerNetId = netIdentity,
                isOpponentCard = !isLocalPlayersCard,
                cardInstanceId = card.instanceId
            };
            NetworkClient.Send(msg);
        }

        public void EngageCard(RuntimeCard card)
        {
            bool isLocalPlayersCard = card.ownerPlayer.netId.isLocalPlayer;
            sveEffectSolver.EngageCard(card);
            LocalEngageCardMessage msg = new()
            {
                playerNetId = netIdentity,
                isOpponentCard = !isLocalPlayersCard,
                cardInstanceId = card.instanceId
            };
            NetworkClient.Send(msg);
        }

        public void SetCardStat(RuntimeCard card, int statId, int value)
        {
            bool isLocalPlayersCard = card.ownerPlayer.netId.isLocalPlayer;
            sveEffectSolver.SetCardStat(card, statId, value);
            if(card.namedStats[SVEProperties.CardStats.Defense].effectiveValue <= 0)
            {
                // Runtime card gets moved in the effect solver, so we only need to move the game object here
                CardObject cardObject = CardManager.Instance.GetCardByInstanceId(card.instanceId);
                SendToCemetery(cardObject, onlyMoveObject: true);
            }

            LocalSetCardStatMessage msg = new()
            {
                playerNetId = netIdentity,
                isOpponentCard = !isLocalPlayersCard,
                cardInstanceId = card.instanceId,
                statId = statId,
                value = value
            };
            NetworkClient.Send(msg);
        }

        public void ApplyModifierToCard(RuntimeCard card, int statId, int value, bool adding, int duration = 0)
        {
            bool isLocalPlayersCard = card.ownerPlayer.netId.isLocalPlayer;
            sveEffectSolver.ApplyCardStatModifier(card, statId, value, adding, duration);
            if(card.namedStats[SVEProperties.CardStats.Defense].effectiveValue <= 0)
            {
                // Runtime card gets moved in the effect solver, so we only need to move the game object here
                CardObject cardObject = CardManager.Instance.GetCardByInstanceId(card.instanceId);
                SendToCemetery(cardObject, onlyMoveObject: true);
            }

            // Assuming we can only apply modifiers to cards on the field, might have to fix later
            LocalCardStatModifierMessage msg = new()
            {
                playerNetId = netIdentity,
                isOpponentCard = !isLocalPlayersCard,
                cardInstanceId = card.instanceId,
                statId = statId,
                value = value,
                adding = adding,
                duration = duration
            };
            NetworkClient.Send(msg);
        }

        public void ApplyKeywordToCard(RuntimeCard card, int type, int value, bool adding)
        {
            bool isLocalPlayersCard = card.ownerPlayer.netId.isLocalPlayer;
            sveEffectSolver.ApplyKeywordToCard(card, type, value, adding);

            // keywords Rush and Storm can change attacker status, so must recalculate attack status
            if(isLocalPlayersCard && isActivePlayer)
            {
                CardObject cardObject = CardManager.Instance.GetCardByInstanceId(card.instanceId);
                if(cardObject)
                    cardObject.CalculateCanAttackStatus();
            }

            LocalApplyKeywordMessage msg = new()
            {
                playerNetId = netIdentity,
                isOpponentCard = !isLocalPlayersCard,
                cardInstanceId = card.instanceId,
                keywordType = type,
                keywordValue = value,
                adding = adding
            };
            NetworkClient.Send(msg);
        }

        public void AddCountersToCard(RuntimeCard card, SVEProperties.Counters counterType, int amount)
        {
            if(amount <= 0)
            {
                Debug.LogError($"Attempted to add negative or zero amount {amount} of {counterType} counters to card with instance ID {card.instanceId}");
                return;
            }

            int currentAmount = 0;
            RuntimeKeyword counterAsKeyword = card.keywords.FirstOrDefault(x => x.keywordId == (int)counterType);
            if(counterAsKeyword != null)
            {
                currentAmount = counterAsKeyword.valueId;
                ApplyKeywordToCard(card, counterAsKeyword.keywordId, counterAsKeyword.valueId, adding: false);
            }
            ApplyKeywordToCard(card, (int)counterType, currentAmount + amount, adding: true);
        }

        public void RemoveCountersFromCard(RuntimeCard card, SVEProperties.Counters counterType, int amount)
        {
            if(amount < 0)
            {
                Debug.LogError($"Attempted to remove negative amount {amount} of {counterType} counters to card with instance ID {card.instanceId}");
                return;
            }

            RuntimeKeyword counterAsKeyword = card.keywords.FirstOrDefault(x => x.keywordId == (int)counterType);
            if(counterAsKeyword == null)
                return;
            int targetAmount = counterAsKeyword.valueId - amount;

            // Handle unique Stack counter logic
            if(counterType == SVEProperties.Counters.Stack && targetAmount <= 0)
            {
                CardObject cardObject = CardManager.Instance.GetCardByInstanceId(card.instanceId);
                Debug.Assert(cardObject);
                if(cardObject)
                    DestroyCard(cardObject, handleStack: false);
                return;  // Return here to not send ApplyKeyword messages to a card that has been moved (keywords should get reset upon entering cemetery anyways)
            }

            if(targetAmount == counterAsKeyword.valueId)
                return;
            ApplyKeywordToCard(card, counterAsKeyword.keywordId, counterAsKeyword.valueId, adding: false);
            if(targetAmount > 0)
                ApplyKeywordToCard(card, (int)counterType, targetAmount, adding: true);
        }
        
        #endregion

        // ------------------------------

        #region Effect Costs

        public bool CanPayCosts(RuntimeCard card, List<Cost> costs, string abilityName)
        {
            return costs == null || costs.Count == 0 || costs.All(x => x is not SveCost cost || cost.CanPayCost(playerController, card, abilityName));
        }

        public void PayAbilityCosts(CardObject card, List<Cost> costs, SveEffect effect, string abilityName, Action onComplete)
        {
            if(costs == null || costs.Count == 0)
            {
                onComplete?.Invoke();
                return;
            }
            StartCoroutine(Resolve());
            IEnumerator Resolve()
            {
                // Pay cost locally/visuals only - do not use event functions or actual data handling in order to avoid sending overlapping network messages
                string cardOriginZone = card.CurrentZone.Runtime.name;
                List<MoveCardToZoneData> cardsToMove = new();
                List<RemoveCounterData> countersToRemove = new();
                foreach(Cost cost in costs)
                {
                    if(cost is not SveCost sveCost)
                        continue;

                    if(sveCost is RemoveCountersCost removeCounterCost)
                        yield return StartCoroutine(removeCounterCost.PayCost(playerController, card, effect, countersToRemove));
                    else
                        yield return StartCoroutine(sveCost.PayCost(playerController, card, effect, cardsToMove));
                }

                // Cards with Stack - implicit convert "move card" into "remove stack counter"
                for(int i = 0; i < cardsToMove.Count; i++)
                {
                    if(!cardsToMove[i].startZone.Equals(SVEProperties.Zones.Field))
                        continue;
                    CardObject cardObject = CardManager.Instance.GetCardByInstanceId(cardsToMove[i].cardInstanceId);
                    if(cardObject)
                    {
                        RuntimeKeyword counterAsKeyword = cardObject.RuntimeCard.keywords.FirstOrDefault(x => x.keywordId == (int)SVEProperties.Counters.Stack);
                        if(counterAsKeyword != null)
                        {
                            countersToRemove.Add(new RemoveCounterData(cardsToMove[i].cardInstanceId, cardsToMove[i].startZone, counterAsKeyword.keywordId, counterAsKeyword.valueId, 1));
                            cardsToMove.RemoveAt(i--);
                        }
                    }
                }

                // Resolve paying cost
                sveEffectSolver.PayAbilityCosts(playerInfo, card.RuntimeCard, costs, cardsToMove.ToArray(), countersToRemove.ToArray());

                LocalPayEffectCostMessage msg = new()
                {
                    playerNetId = netIdentity,
                    cardInstanceId = card.RuntimeCard.instanceId,
                    originZone = cardOriginZone,
                    abilityName = abilityName,
                    cardsMoveToZoneData = cardsToMove.ToArray(),
                    countersToRemove = countersToRemove.ToArray(),
                };
                NetworkClient.Send(msg);

                // Handle cards with 0 Stack counters
                foreach(RemoveCounterData removeCounterData in countersToRemove)
                {
                    if(removeCounterData.keywordType == (int)SVEProperties.Counters.Stack && removeCounterData.keywordValue <= removeCounterData.amount)
                    {
                        CardObject cardObject = CardManager.Instance.GetCardByInstanceId(removeCounterData.cardInstanceId);
                        Debug.Assert(cardObject);
                        if(cardObject)
                            DestroyCard(cardObject, handleStack: false);
                    }
                }

                // Resolve
                yield return new WaitForSeconds(0.1f);
                onComplete?.Invoke();
            }
        }

        #endregion

        // ------------------------------

        #region Other

        public void TellOpponentToPerformEffect(CardObject card, string effectName, int[] targetInstanceIds = null)
        {
            LocalTellOpponentPerformEffectMessage msg = new()
            {
                playerNetId = playerInfo.netId,
                libraryCardId = card.LibraryCard.id,
                cardInstanceId = card.RuntimeCard.instanceId,
                cardZone = card.CurrentZone.Runtime.name,
                effectName = effectName,
                targetInstanceIds = targetInstanceIds
            };
            NetworkClient.Send(msg);
        }

        private void ChoosePlayCardCostOption(CardObject card, int playPointCost, List<TriggeredAbility> alternateCostAbilities, Action<bool> onChooseCost)
        {
            if(alternateCostAbilities == null || alternateCostAbilities.Count == 0)
            {
                onChooseCost?.Invoke(true);
                return;
            }

            // TODO - send card to resolution
            List<MultipleChoiceWindow.MultipleChoiceEntryData> playOptions = new()
            {
                new MultipleChoiceWindow.MultipleChoiceEntryData()
                {
                    text = $"{playPointCost} Play Points",
                    onSelect = () => onChooseCost?.Invoke(true),
                    disabled = !CanPayPlayPointsCost(playPointCost)
                }
            };
            foreach(TriggeredAbility ability in alternateCostAbilities)
            {
                SveTrigger trigger = ability.trigger as SveTrigger;
                AlternateCostEffect effect = ability.effect as AlternateCostEffect;
                Debug.Assert(trigger != null && effect != null);

                MultipleChoiceWindow.MultipleChoiceEntryData entry = effect.AsMultipleChoiceEntry(() =>
                {
                    PayAbilityCosts(card, trigger.Costs, effect, ability.name, () => onChooseCost?.Invoke(false));
                });
                entry.disabled = !CanPayCosts(card.RuntimeCard, trigger.Costs, ability.name);
                playOptions.Add(entry);
            }

            GameUIManager.MultipleChoice.Open(playerController, card.LibraryCard.name, playOptions, "");
        }

        #endregion
    }
}
