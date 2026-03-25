using System;
using UnityEngine;
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

        public bool IsPayingCosts { get; private set; }
        public Action OnFinishSpell;

        // ------------------------------

        #region Game Flow

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

        public void SetGamePhase(SVEProperties.GamePhase phase)
        {
            sveEffectSolver.SetGamePhase(phase);
            GameUIManager.GameControlsUI.SetPhase(phase);

            SetGamePhaseMessage msg = new()
            {
                playerNetId = netIdentity,
                phase = phase
            };
            NetworkClient.Send(msg);
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

        #region Zone Controls

        public void ShuffleDeck()
        {
            sveEffectSolver.ShuffleDeck(netIdentity, out int rngAdvances);
            LocalShuffleDeckMessage msg = new()
            {
                playerNetId = netIdentity,
                rngAdvances = rngAdvances
            };
            NetworkClient.Send(msg);
        }

        public void DiscardRandomCards(PlayerInfo targetPlayer, int amount)
        {
            bool isLocalPlayer = targetPlayer.netId.isLocalPlayer;
            PlayerCardZoneController targetZoneController = isLocalPlayer ? localZoneController : oppZoneController;
            if(amount >= targetZoneController.handZone.AllCards.Count)
            {
                List<CardObject> cards = new(targetZoneController.handZone.AllCards);
                foreach(CardObject card in cards)
                    SendToCemetery(card, SVEProperties.Zones.Hand);
                return;
            }

            sveEffectSolver.DiscardRandomCards(targetPlayer.netId, amount, out List<RuntimeCard> discardedCards);
            foreach(RuntimeCard card in discardedCards)
            {
                if(!targetZoneController.handZone.TryGetCard(card.instanceId, out CardObject cardObject))
                {
                    Debug.LogError($"Failed to find card with instance ID {card.instanceId} in player's hand when attempting to discard random card!");
                    continue;
                }
                SendToCemetery(cardObject, SVEProperties.Zones.Hand, onlyMoveObject: true);
            }

            LocalDiscardRandomCardsMessage msg = new()
            {
                playerNetId = netIdentity,
                targetNetId = targetPlayer.netId,
                amount = amount
            };
            NetworkClient.Send(msg);
        }

        public void RevealTopDeck(Action<CardObject> onComplete)
        {
            RuntimeCard runtimeCard = localZoneController.deckZone.Runtime.cards[0];
            CardObject card = localZoneController.CreateNewCardObjectTopDeck(runtimeCard);
            localZoneController.FlipCardToFaceUp(card, onComplete: () => onComplete?.Invoke(card));

            LocalFlipTopDeckMessage msg = new()
            {
                playerNetId = netIdentity,
                cardInstanceId = runtimeCard.instanceId,
                toFaceUp = true
            };
            NetworkClient.Send(msg);
        }

        public void FlipTopDeckToFaceDown(CardObject card = null)
        {
            if(!card)
            {
                RuntimeCard runtimeCard = localZoneController.deckZone.Runtime.cards[0];
                card = CardManager.Instance.GetCardByInstanceId(runtimeCard.instanceId);
            }
            if(!card)
                return;
            localZoneController.FlipCardToFaceDown(card);

            LocalFlipTopDeckMessage msg = new()
            {
                playerNetId = netIdentity,
                cardInstanceId = card.RuntimeCard.instanceId,
                toFaceUp = false
            };
            NetworkClient.Send(msg);
        }

        public void FlipEvolveDeckCards(bool toFaceDown, List<RuntimeCard> targetCards = null)
        {
            targetCards ??= localZoneController.evolveDeckZone.Runtime.cards.Where(x => x.namedStats.TryGetValue(SVEProperties.CardStats.FaceUp, out Stat faceUpStat)
                && faceUpStat.effectiveValue == (toFaceDown ? 1 : 0)).ToList();

            List<CardObject> cardsToFlip = new();
            foreach(RuntimeCard card in targetCards)
            {
                CardObject cardObject = CardManager.Instance.GetCardByInstanceId(card.instanceId);
                if(cardObject)
                {
                    cardsToFlip.Add(cardObject);
                    continue;
                }
                cardsToFlip.Add(CardManager.Instance.RequestCard(card));
            }
            for(int i = 0; i < cardsToFlip.Count; i++)
            {
                if(toFaceDown)
                    localZoneController.FlipCardToFaceDown(cardsToFlip[i], i * 0.15f);
                else
                    localZoneController.FlipCardToFaceUp(cardsToFlip[i], i * 0.15f);
            }

            sveEffectSolver.FlipEvolveDeckCards(netIdentity, targetCards, toFaceDown);
            LocalFlipEvolveDeckCardsMessage msg = new()
            {
                playerNetId = netIdentity,
                cardInstanceIds = targetCards.Select(x => x.instanceId).ToArray(),
                toFaceDown = toFaceDown
            };
            NetworkClient.Send(msg);
        }

        #endregion

        // ------------------------------

        #region Card Movement

        public void DrawCard(CardObject cardObject = null, bool reveal = false)
        {
            if(!cardObject && localZoneController.deckZone.Runtime.cards.Count == 0)
                return;

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

        public void MillDeck(bool targetLocalPlayer, int count, Action onComplete)
        {
            StartCoroutine(MillCoroutine());
            IEnumerator MillCoroutine() // Use delay to prevents cards from moving all at once
            {
                int movedCount = 0;
                PlayerCardZoneController targetZoneController = targetLocalPlayer ? localZoneController : oppZoneController;
                for(int i = 0; i < count; i++)
                {
                    RuntimeCard runtimeCard = targetZoneController.deckZone.Runtime.cards[0];
                    CardObject cardObject = targetZoneController.CreateNewCardObjectTopDeck(runtimeCard);

                    targetZoneController.SendCardToCemetery(cardObject, onComplete: () => { movedCount++; });
                    sveEffectSolver.SendToCemetery(targetLocalPlayer ? netIdentity : opponentInfo.netId, runtimeCard, SVEProperties.Zones.Deck);
                    LocalSendCardToCemeteryMessage msg = new()
                    {
                        playerNetId = netIdentity,
                        cardInstanceId = runtimeCard.instanceId,
                        isOpponentCard = !targetLocalPlayer,
                        originZone = SVEProperties.Zones.Deck
                    };
                    NetworkClient.Send(msg);
                    yield return new WaitForSeconds(0.15f);
                }
                yield return new WaitUntil(() => movedCount >= count);
                onComplete?.Invoke();
            }
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
            playerController.AdditionalStats.CardsPlayedThisTurn.Add(new PlayedCardData(card.RuntimeCard.instanceId, card.RuntimeCard.cardId));
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

        public void SendToCemetery(CardObject card, string originZone = null, bool onlyMoveObject = false, bool handleStack = true, bool isDestroy = false)
        {
            if(handleStack && CounterUtilities.HandleStackLeaveField(playerController, card))
                return;

            RuntimeCard runtimeCard = card.RuntimeCard;
            originZone ??= card.CurrentZone.Runtime.name;
            bool isLocalPlayersCard = card.CurrentZone.IsLocalPlayerZone;
            PlayerCardZoneController targetZoneController = isLocalPlayersCard ? localZoneController : oppZoneController;

            if(!onlyMoveObject)
                sveEffectSolver.SendToCemetery(isLocalPlayersCard ? netIdentity : opponentInfo.netId, runtimeCard, originZone, isDestroy);
            if(isLocalPlayersCard)
            {
                if(isDestroy && originZone.Equals(SVEProperties.Zones.Field))
                    playerController.AdditionalStats.CardsDestroyedThisTurn.Add(new PlayedCardData(card.RuntimeCard.instanceId, card.RuntimeCard.cardId));
                else if(originZone.Equals(SVEProperties.Zones.Hand))
                    playerController.AdditionalStats.CardsDiscardedThisTurn.Add(new PlayedCardData(card.RuntimeCard.instanceId, card.RuntimeCard.cardId));
            }
            StandardSendCardObjectToZone(card, targetZoneController, (x, onComplete) => targetZoneController.SendCardToCemetery(x, onComplete));

            // ---

            if(onlyMoveObject)
                return;
            LocalSendCardToCemeteryMessage msg = new()
            {
                playerNetId = netIdentity,
                cardInstanceId = runtimeCard.instanceId,
                isOpponentCard = !isLocalPlayersCard,
                originZone = originZone,
                isDestroy = isDestroy
            };
            NetworkClient.Send(msg);
        }

        public void DestroyCard(CardObject card, bool handleStack = true)
        {
            if(handleStack && CounterUtilities.HandleStackLeaveField(playerController, card))
                return;
            if(card.CurrentZone.IsLocalPlayerZone)
            {
                SendToCemetery(card, handleStack: handleStack, isDestroy: true);
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

        public void BanishCard(CardObject card, string originZone = null, bool sendMessage = true, bool onlyMoveObject = false)
        {
            if(CounterUtilities.HandleStackLeaveField(playerController, card))
                return;

            RuntimeCard runtimeCard = card.RuntimeCard;
            originZone ??= card.CurrentZone.Runtime.name;
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
            // Show if adding from cemetery, otherwise normal move logic
            if(!sourceZone.Equals(SVEProperties.Zones.Cemetery))
                StandardSendCardObjectToZone(card, targetZoneController, (x, onComplete) => targetZoneController.AddCardToHand(x, onComplete));
            else
                localZoneController.RevealCard(card, onComplete: () => localZoneController.AddCardToHand(card, () => card.Interactable = playerController.isActivePlayer));

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
            RuntimeCard tokenRuntimeCard = null;
            int slotId = targetZone.GetSlotNumber(targetCard);
            bool isLocalPlayersCard = targetCard.CurrentZone.IsLocalPlayerZone;
            if(!isLocalPlayersCard)
                goto sendMessage; // skip local logic, send message tells opponent to perform the effect and send logic back to us

            BanishCard(targetCard, sendMessage: false);
            tokenRuntimeCard = sveEffectSolver.CreateAndAddToken(netIdentity, tokenLibraryCard.id,
                isLocalPlayersCard ? playerInfo.currentCardInstanceId++ : opponentInfo.currentCardInstanceId++, targetZone.Runtime);
            CardObject tokenCardObject = CardManager.Instance.RequestCard(tokenRuntimeCard);
            localZoneController.AddAndPlaceToken(tokenCardObject, targetZone, slotId);

            // Send message
            sendMessage:
            LocalTransformCardMessage msg = new()
            {
                playerNetId = netIdentity,
                targetCardInstanceId = targetRuntimeCard.instanceId,
                isOpponentCard = !isLocalPlayersCard,
                originZone = targetZone.Runtime.name,

                libraryCardId = tokenLibraryCard.id,
                tokenRuntimeCardInstanceId = tokenRuntimeCard?.instanceId ?? -1,
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
            playerController.AdditionalStats.CardsPlayedThisTurn.Add(new PlayedCardData(card.RuntimeCard.instanceId, card.RuntimeCard.cardId));
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

        #region Combat & Attack Handling

        private void DeclareAttack(CardObject attackingCard, bool isAttackingLeader)
        {
            playerController.AdditionalStats.CardsAttackedThisTurn.Add(new PlayedCardData(attackingCard.RuntimeCard.instanceId, attackingCard.RuntimeCard.cardId));
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
            SVEEffectPool.Instance.OnNextConfirmationTimingEnd += () =>
            {
                CardManager.Animator.PlayAttackPreview(attackingCard, defendingCard);
                SVEQuickTimingController.Instance.CallQuickTimingCombat(attackingCard, defendingCard, () =>
                {
                    CardManager.Animator.EndAttackPreview();
                    if(attackingCard.CurrentZone != localZoneController.fieldZone) // cancel attack if attacking card is no longer on field
                        return;

                    CardManager.Animator.PlayAttackAnimation(attackingCard, defendingCard, () =>
                    {
                        sveEffectSolver.FightFollower(netIdentity, attackingCard.RuntimeCard, defendingCard.RuntimeCard, out bool attackerDestroyed, out bool defenderDestroyed);
                        // Runtime card gets moved in the effect solver, so we only need to move the game object here
                        if(attackerDestroyed)
                            SendToCemetery(attackingCard, onlyMoveObject: true, isDestroy: true);
                        if(defenderDestroyed)
                            SendToCemetery(defendingCard, onlyMoveObject: true, isDestroy: true);
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
            SVEEffectPool.Instance.OnNextConfirmationTimingEnd += () =>
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

        #endregion

        // ------------------------------

        #region Card Stats

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
            sveEffectSolver.SetCardStat(card, statId, value, out bool isDestroyed);
            if(isDestroyed)
            {
                // Runtime card gets moved in the effect solver, so we only need to move the game object here
                CardObject cardObject = CardManager.Instance.GetCardByInstanceId(card.instanceId);
                SendToCemetery(cardObject, onlyMoveObject: true, isDestroy: true);
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

        public void SetLeaderDefense(PlayerInfo targetPlayer, int amount)
        {
            int difference = amount - targetPlayer.namedStats[SVEProperties.PlayerStats.Defense].effectiveValue;
            if(difference != 0)
                AddLeaderDefense(targetPlayer, difference);
        }

        public void ApplyModifierToCard(RuntimeCard card, int statId, int value, bool adding, int duration = 0)
        {
            bool isLocalPlayersCard = card.ownerPlayer.netId.isLocalPlayer;
            sveEffectSolver.ApplyCardStatModifier(card, statId, value, adding, duration, out bool isDestroyed);
            if(isDestroyed)
            {
                // Runtime card gets moved in the effect solver, so we only need to move the game object here
                CardObject cardObject = CardManager.Instance.GetCardByInstanceId(card.instanceId);
                SendToCemetery(cardObject, onlyMoveObject: true, isDestroy: true);
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

        #endregion

        // ------------------------------

        #region Keywords & Counters

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

        #region Player Stats

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

        public void AddEvolvePoints(PlayerInfo targetPlayer, int amount)
        {
            sveEffectSolver.AddEvolvePoints(targetPlayer, amount);
            LocalAddEvolvePointsMessage msg = new()
            {
                playerNetId = netIdentity,
                targetPlayer = targetPlayer.netId,
                amount = amount
            };
            NetworkClient.Send(msg);
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
                IsPayingCosts = false;
                onComplete?.Invoke();
                return;
            }
            StartCoroutine(Resolve());
            IEnumerator Resolve()
            {
                IsPayingCosts = true;
                // Pay cost locally/visuals only - do not use event functions or actual data handling in order to avoid sending overlapping network messages
                string cardOriginZone = card.CurrentZone.Runtime.name;
                List<MoveCardToZoneData> cardsToMove = new();
                List<RemoveCounterData> countersToRemove = new();
                foreach(Cost cost in costs)
                {
                    if(cost is not SveCost sveCost)
                        continue;

                    if(sveCost is RemoveCountersCost removeCounterCost)
                        yield return StartCoroutine(removeCounterCost.PayCost(playerController, card, abilityName, countersToRemove));
                    else
                        yield return StartCoroutine(sveCost.PayCost(playerController, card, abilityName, cardsToMove));
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
                IsPayingCosts = false;
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
