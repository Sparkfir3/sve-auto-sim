using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CCGKit;
using SVESimulator.UI;

namespace SVESimulator
{
    public class PlayerEventControllerOpponent : PlayerEventControllerBase
    {
        protected override bool isLocal => false;

        // ------------------------------

        #region Game Flow

        public void SetGoingFirst(SetGoingFirstPlayerMessage msg)
        {
            playerInfo.isGoingFirstDecided = true;
            playerInfo.isGoingFirst = msg.playerNetId == playerInfo.netId;
            opponentInfo.isGoingFirstDecided = true;
            opponentInfo.isGoingFirst = msg.playerNetId == opponentInfo.netId;

            GameUIManager.GoingFirstScreen.SetDisplayMode(SelectGoingFirstScreen.Mode.None, false);
            playerController.InitializeEvolvePointDisplays(playerInfo.isGoingFirst);
        }
        
        public void Mulligan(OpponentPerformMulliganMessage msg)
        {
            List<CardObject> cards = oppZoneController.handZone.AllCards.ToList(); // copy to not modify collection while iterating
            foreach(CardObject card in cards)
            {
                oppZoneController.SendCardToBottomDeck(card);
                sveEffectSolver.SendCardHandToBottomDeck(netIdentity, card.RuntimeCard);
            }
            // Re-drawing hand is handled by opponent's PlayerController drawing cards and sending draw card messages
        }
        
        #endregion

        // ------------------------------

        #region Zone Controls

        public void ShuffleDeck(OpponentShuffleDeckMessage msg)
        {
            sveEffectSolver.ShuffleDeck(msg.playerNetId);
        }

        #endregion
        
        // ------------------------------
        
        #region Card Movement
        
        public void InitializeDeckAndLeader(OpponentInitDeckAndLeaderMessage msg)
        {
            // Evolve deck
            oppZoneController.evolveDeckZone.SetStackHeight(msg.evolveDeckSize);
            
            // Leader
            RuntimeCard leader = new RuntimeCard();
            InitRuntimeCard(ref leader, msg.leaderCard);
            oppZoneController.InitializeLeaderCard(leader, leader.cardId);
            oppZoneController.deckZone.Runtime.RemoveCard(leader);
            oppZoneController.leaderZone.Runtime.AddCard(leader);
        }
        
        public void DrawCard(OpponentDrawCardMessage msg)
        {
            RuntimeCard runtimeCard = new RuntimeCard();;
            InitRuntimeCard(ref runtimeCard, msg.card);
            oppZoneController.deckZone.Runtime.RemoveCard(runtimeCard);
            oppZoneController.handZone.Runtime.AddCard(runtimeCard);
            
            CardObject cardObject = oppZoneController.CreateNewCardObjectTopDeck(runtimeCard);
            if(!msg.reveal)
                oppZoneController.AddCardToHand(cardObject);
            else
                oppZoneController.RevealCard(cardObject, onComplete: () => oppZoneController.AddCardToHand(cardObject));
        }
        
        public void PlayCardToField(OpponentPlayCardMessage msg)
        {
            CardZone originZone = oppZoneController.AllZones[msg.originZone];
            if(!originZone.TryGetCard(msg.card.instanceId, out CardObject card))
            {
                if(msg.originZone.Equals(SVEProperties.Zones.Deck))
                {
                    RuntimeCard runtimeCard = new RuntimeCard();
                    InitRuntimeCard(ref runtimeCard, msg.card);
                    card = oppZoneController.CreateNewCardObjectTopDeck(runtimeCard);
                }
                else
                {
                    return;
                }
            }
            if(card.IsCardType(SVEProperties.CardTypes.Spell))
                return;

            sveEffectSolver.PlayCard(msg.playerNetId, card.RuntimeCard, msg.originZone, msg.playPointCost);
            oppZoneController.PlayCardToField(card, msg.fieldSlotId);
        }
        
        public void EvolveCard(OpponentEvolveCardMessage msg)
        {
            // Init objects
            CardObject baseCard = oppZoneController.fieldZone.GetCard(msg.fieldSlotId);
            if(!oppZoneController.evolveDeckZone.TryGetCard(msg.evolvedCard.instanceId, out CardObject evolvedCard))
            {
                RuntimeCard runtimeCard = new RuntimeCard();
                InitRuntimeCard(ref runtimeCard, msg.evolvedCard);
                evolvedCard = CardManager.Instance.RequestCard(runtimeCard);
            }

            // Evolve
            oppZoneController.EvolveCard(baseCard, evolvedCard, msg.fieldSlotId);
            sveEffectSolver.EvolveCard(msg.playerNetId, baseCard.RuntimeCard, evolvedCard.RuntimeCard, msg.useEvolvePoint, msg.useEvolveCost);
        }

        public void SendToCemetery(OpponentSendCardToCemeteryMessage msg)
        {
            PlayerCardZoneController targetZoneController = msg.isOpponentCard ? localZoneController : oppZoneController;
            CardObject card = CardManager.Instance.GetCardByInstanceId(msg.card.instanceId);
            if(!card)
            {
                if(msg.originZone.Equals(SVEProperties.Zones.Deck))
                {
                    RuntimeCard runtimeCard = new RuntimeCard();;
                    InitRuntimeCard(ref runtimeCard, msg.card);
                    card = targetZoneController.CreateNewCardObjectTopDeck(runtimeCard);
                }
                else
                {
                    return;
                }
            }

            sveEffectSolver.SendToCemetery((msg.isOpponentCard ? playerInfo : opponentInfo).netId, card.RuntimeCard, msg.originZone, msg.isDestroy);
            if(msg.isOpponentCard && msg.isDestroy && msg.originZone.Equals(SVEProperties.Zones.Field))
                playerController.AdditionalStats.CardsDestroyedThisTurn.Add(new PlayedCardData(card.RuntimeCard.instanceId, card.RuntimeCard.cardId));
            StandardSendCardObjectToZone(card, targetZoneController, (x, onComplete) => targetZoneController.SendCardToCemetery(x, onComplete));
        }

        public void DestroyOpponentCard(OpponentDestroyOpponentCardMessage msg)
        {
            if(!localZoneController.fieldZone.TryGetCard(msg.cardInstanceId, out CardObject card))
            {
                Debug.LogError($"Failed to find card with id {msg.cardInstanceId} to destroy");
                return;
            }
            playerController.LocalEvents.SendToCemetery(card, isDestroy: true);
        }

        public void BanishCard(OpponentBanishCardMessage msg)
        {
            PlayerCardZoneController targetZoneController = msg.isOpponentCard ? localZoneController : oppZoneController;
            CardObject card = CardManager.Instance.GetCardByInstanceId(msg.card.instanceId);
            if(!card)
            {
                if(msg.originZone.Equals(SVEProperties.Zones.Deck))
                {
                    RuntimeCard runtimeCard = new RuntimeCard();
                    InitRuntimeCard(ref runtimeCard, msg.card);
                    card = CardManager.Instance.RequestCard(runtimeCard);
                    card.CurrentZone = targetZoneController.deckZone;
                    card.transform.SetPositionAndRotation(targetZoneController.deckZone.GetTopStackPosition(),
                        SVEProperties.CardFaceDownRotation * (msg.isOpponentCard ? Quaternion.identity : SVEProperties.OpponentCardRotation));
                }
                else
                {
                    return;
                }
            }

            sveEffectSolver.BanishCard((msg.isOpponentCard ? playerInfo : opponentInfo).netId, card.RuntimeCard, msg.originZone);
            StandardSendCardObjectToZone(card, targetZoneController, (x, onComplete) => targetZoneController.SendCardToBanishedZone(x, onComplete));
        }

        public void ReturnCardToHand(OpponentReturnToHandMessage msg)
        {
            PlayerCardZoneController targetZoneController = msg.isOpponentCard ? localZoneController : oppZoneController;
            CardObject card = CardManager.Instance.GetCardByInstanceId(msg.card.instanceId);
            if(!card)
            {
                if(msg.originZone.Equals(SVEProperties.Zones.Deck))
                {
                    RuntimeCard runtimeCard = new RuntimeCard();
                    InitRuntimeCard(ref runtimeCard, msg.card);
                    card = CardManager.Instance.RequestCard(runtimeCard);
                    card.CurrentZone = targetZoneController.deckZone;
                    card.transform.SetPositionAndRotation(targetZoneController.deckZone.GetTopStackPosition(),
                        SVEProperties.CardFaceDownRotation * (msg.isOpponentCard ? Quaternion.identity : SVEProperties.OpponentCardRotation));
                }
                else
                {
                    Debug.LogError($"Failed to find card in zone {msg.originZone} with id {msg.card.instanceId} to return to hand");
                    return;
                }
            }

            sveEffectSolver.ReturnCardToHand(msg.isOpponentCard ? playerInfo : opponentInfo, card.RuntimeCard, msg.originZone);
            StandardSendCardObjectToZone(card, targetZoneController, (x, onComplete) => targetZoneController.AddCardToHand(x, onComplete));
        }

        public void SendToBottomDeck(OpponentSendToBottomDeckMessage msg)
        {
            if(!msg.isOpponentCard && msg.originZone.Equals(SVEProperties.Zones.Deck))
                return;

            PlayerCardZoneController targetZoneController = msg.isOpponentCard ? localZoneController : oppZoneController;
            CardObject card = CardManager.Instance.GetCardByInstanceId(msg.cardInstanceId);
            if(!card)
            {
                Debug.LogError($"Failed to find card in zone {msg.originZone} with id {msg.cardInstanceId} to send to bottom deck");
                return;
            }

            sveEffectSolver.SendCardToBottomDeck(msg.isOpponentCard ? playerInfo : opponentInfo, card.RuntimeCard, msg.originZone);
            StandardSendCardObjectToZone(card, targetZoneController, (x, onComplete) => targetZoneController.SendCardToBottomDeck(x, onComplete));
        }

        public void SendToTopDeck(OpponentSendToTopDeckMessage msg)
        {
            if(!msg.isOpponentCard && msg.originZone.Equals(SVEProperties.Zones.Deck))
                return;

            PlayerCardZoneController targetZoneController = msg.isOpponentCard ? localZoneController : oppZoneController;
            CardObject card = CardManager.Instance.GetCardByInstanceId(msg.cardInstanceId);
            if(!card)
            {
                Debug.LogError($"Failed to find card in zone {msg.originZone} with id {msg.cardInstanceId} to send to top deck");
                return;
            }

            sveEffectSolver.SendCardToTopDeck(msg.isOpponentCard ? playerInfo : opponentInfo, card.RuntimeCard, msg.originZone);
            StandardSendCardObjectToZone(card, targetZoneController, (x, onComplete) => targetZoneController.SendCardToTopDeck(x, onComplete));
        }

        public void SendToExArea(OpponentSendToExAreaMessage msg)
        {
            PlayerCardZoneController targetZoneController = msg.isOpponentCard ? localZoneController : oppZoneController;
            CardObject card = CardManager.Instance.GetCardByInstanceId(msg.card.instanceId);
            if(!card)
            {
                if(msg.originZone.Equals(SVEProperties.Zones.Deck))
                {
                    RuntimeCard runtimeCard = new RuntimeCard();
                    InitRuntimeCard(ref runtimeCard, msg.card);
                    card = targetZoneController.CreateNewCardObjectTopDeck(runtimeCard);
                }
                else
                {
                    return;
                }
            }

            sveEffectSolver.SendCardToExArea(msg.isOpponentCard ? playerInfo : opponentInfo, card.RuntimeCard, msg.originZone);
            StandardSendCardObjectToZone(card, targetZoneController, (x, onComplete) => targetZoneController.SendCardToExArea(x, msg.fieldSlotId, onComplete));
        }
        
        #endregion

        // ------------------------------

        #region Tokens

        public void CreateToken(OpponentCreateTokenMessage msg)
        {
            RuntimeCard runtimeCard = new();
            InitRuntimeCard(ref runtimeCard, msg.card);

            CardPositionedZone targetZone = msg.createOnField ? oppZoneController.fieldZone : oppZoneController.exAreaZone;
            targetZone.Runtime.AddCard(runtimeCard);

            CardObject cardObject = CardManager.Instance.RequestCard(runtimeCard);
            oppZoneController.AddAndPlaceToken(cardObject, targetZone, msg.slotId);
            // TODO - check why does this function not call the effect solver? and why has it not broken anything yet?
        }

        public void TransformCard(OpponentTransformCardMessage msg)
        {
            BanishCard(new OpponentBanishCardMessage()
            {
                playerNetId = msg.playerNetId,
                card = msg.targetCard,
                isOpponentCard = msg.isOpponentCard,
                originZone = msg.originZone
            });
            CreateToken(new OpponentCreateTokenMessage()
            {
                playerNetId = msg.playerNetId,
                card = msg.tokenCard,
                createOnField = msg.originZone.Equals(SVEProperties.Zones.Field),
                slotId = msg.slotId
            });
        }

        #endregion

        // ------------------------------

        #region Spells

        public void PlaySpell(OpponentPlaySpellMessage msg)
        {
            CardObject spell = CardManager.Instance.GetCardByInstanceId(msg.cardInstanceId);
            if(!spell)
            {
                Debug.LogError($"Failed to find card in zone {msg.originZone} to cast spell with");
                return;
            }

            sveEffectSolver.PlaySpell(netIdentity, spell.RuntimeCard, msg.originZone, msg.playPointCost);
            oppZoneController.SendCardToResolution(spell);
        }

        public void FinishSpell(OpponentFinishSpellMessage msg)
        {
            if(!oppZoneController.resolutionZone.TryGetCard(msg.cardInstanceId, out CardObject spell))
            {
                Debug.LogError($"Failed to find card in resolution zone to finish casting spell with");
                return;
            }

            sveEffectSolver.FinishSpell(netIdentity, spell.RuntimeCard);
            if(spell.IsToken())
                CardManager.Instance.ReleaseCard(spell);
            else
                oppZoneController.SendCardToCemetery(spell);
        }

        #endregion
        
        // ------------------------------
        
        #region Combat

        public void DeclareAttack(OpponentDeclareAttackMessage msg)
        {
            if(!oppZoneController.fieldZone.TryGetCard(msg.cardInstanceId, out CardObject attacker))
            {
                Debug.LogError($"Failed to find card to attack/defend with");
                return;
            }
            sveEffectSolver.EngageCard(attacker.RuntimeCard);
        }

        public void AttackFollower(OpponentAttackFollowerMessage msg)
        {
            if(!oppZoneController.fieldZone.TryGetCard(msg.attackingCard.instanceId, out CardObject attackingCard)
               || !localZoneController.fieldZone.TryGetCard(msg.defendingCard.instanceId, out CardObject defendingCard))
            {
                Debug.LogError($"Failed to find card to attack/defend with");
                return;
            }
            CardManager.Animator.PlayAttackAnimation(attackingCard, defendingCard, () =>
            {
                sveEffectSolver.FightFollower(msg.attackingPlayerNetId, attackingCard.RuntimeCard, defendingCard.RuntimeCard);
                // Runtime card gets moved in the effect solver, so we only need to move the game object here
                if(attackingCard.RuntimeCard.namedStats[SVEProperties.CardStats.Defense].effectiveValue <= 0 || defendingCard.RuntimeCard.HasKeyword(SVEProperties.Keywords.Bane))
                    playerController.LocalEvents.SendToCemetery(attackingCard, onlyMoveObject: true, isDestroy: true);
                if(defendingCard.RuntimeCard.namedStats[SVEProperties.CardStats.Defense].effectiveValue <= 0 || attackingCard.RuntimeCard.HasKeyword(SVEProperties.Keywords.Bane))
                    playerController.LocalEvents.SendToCemetery(defendingCard, onlyMoveObject: true, isDestroy: true);
            });
        }

        public void AttackLeader(OpponentAttackLeaderMessage msg)
        {
            if(!oppZoneController.fieldZone.TryGetCard(msg.attackingCard.instanceId, out CardObject attacker))
            {
                Debug.LogError($"Failed to find card to attack with");
                return;
            }

            CardManager.Animator.PlayAttackAnimation(attacker, localZoneController.LeaderCardObject, () =>
            {
                sveEffectSolver.FightLeader(msg.playerNetId, attacker.RuntimeCard, playerInfo);
            });
        }

        public void AddLeaderDefense(OpponentAddLeaderDefenseMessage msg)
        {
            PlayerInfo targetPlayer = msg.targetPlayer.netId == playerInfo.netId.netId ? playerInfo : opponentInfo;
            sveEffectSolver.AddLeaderDefense(targetPlayer, msg.amount);
        }

        public void ReserveCard(OpponentReserveCardMessage msg)
        {
            CardZone targetZone = msg.isOpponentCard ? localZoneController.fieldZone : oppZoneController.fieldZone;
            if(!targetZone.TryGetCard(msg.card.instanceId, out CardObject card))
            {
                Debug.LogError($"Failed to find card with id {msg.card.instanceId} to reserve");
                return;
            }
            sveEffectSolver.ReserveCard(card.RuntimeCard);
        }

        public void EngageCard(OpponentEngageCardMessage msg)
        {
            CardZone targetZone = msg.isOpponentCard ? localZoneController.fieldZone : oppZoneController.fieldZone;
            if(!targetZone.TryGetCard(msg.card.instanceId, out CardObject card))
            {
                Debug.LogError($"Failed to find card with id {msg.card.instanceId} to engage");
                return;
            }
            sveEffectSolver.EngageCard(card.RuntimeCard);
        }

        public void SetCardStat(OpponentSetCardStatMessage msg)
        {
            CardObject card = CardManager.Instance.GetCardByInstanceId(msg.cardInstanceId);
            if(!card)
            {
                Debug.LogError($"Failed to find card with id {msg.cardInstanceId} on {(msg.isOpponentCard ? "player" : "opponent")}'s field to update stat");
                return;
            }
            sveEffectSolver.SetCardStat(card.RuntimeCard, msg.statId, msg.value);
            if(msg.value == 0 && card.RuntimeCard.namedStats[SVEProperties.CardStats.Defense].effectiveValue <= 0)
                playerController.LocalEvents.SendToCemetery(card, onlyMoveObject: true, isDestroy: true);
        }

        public void ApplyModifierToCard(OpponentCardStatModifierMessage msg)
        {
            CardObject card = CardManager.Instance.GetCardByInstanceId(msg.cardInstanceId);
            if(!card)
            {
                Debug.LogError($"Failed to find card with id {msg.cardInstanceId} on {(msg.isOpponentCard ? "player" : "opponent")}'s field to update stat");
                return;
            }
            sveEffectSolver.ApplyCardStatModifier(card.RuntimeCard, msg.statId, msg.value, msg.adding, msg.duration);
            if(card.RuntimeCard.namedStats[SVEProperties.CardStats.Defense].effectiveValue <= 0)
                playerController.LocalEvents.SendToCemetery(card, onlyMoveObject: true, isDestroy: true);
        }

        public void ApplyKeywordToCard(OpponentApplyKeywordMessage msg)
        {
            CardObject card = CardManager.Instance.GetCardByInstanceId(msg.cardInstanceId);
            if(!card)
            {
                Debug.LogError($"Failed to find card with id {msg.cardInstanceId} on {(msg.isOpponentCard ? "player" : "opponent")}'s field to " +
                    $"give keyword with id ({msg.keywordType}, {msg.keywordValue})");
                return;
            }
            sveEffectSolver.ApplyKeywordToCard(card.RuntimeCard, msg.keywordType, msg.keywordValue, msg.adding);
            if(msg.isOpponentCard && isActivePlayer && localZoneController.fieldZone.TryGetCard(msg.cardInstanceId, out CardObject cardObject))
                cardObject.CalculateCanAttackStatus();
        }
        
        #endregion

        // ------------------------------

        #region Effect Costs

        public void PayCostForEffect(OpponentPayEffectCostMessage msg)
        {
            CardObject card = CardManager.Instance.GetCardByInstanceId(msg.cardInstanceId);
            if(!card)
            {
                Debug.LogError($"Failed to find card with instance ID {msg.cardInstanceId} in zone {msg.originZone} to pay effect cost for");
                return;
            }
            Ability ability = msg.abilityName.Equals(CounterUtilities.InnateStackAbility.name)
                ? CounterUtilities.InnateStackAbility
                : LibraryCardCache.GetCard(card.RuntimeCard.cardId).abilities.FirstOrDefault(x => x.name.Equals(msg.abilityName));
            List<Cost> costList = ability switch
            {
                ActivatedAbility activatedAbility => activatedAbility.costs,
                TriggeredAbility { trigger: SveTrigger sveTrigger } => sveTrigger.Costs,
                _ => new List<Cost>()
            };

            sveEffectSolver.PayAbilityCosts(opponentInfo, card.RuntimeCard, costList, msg.cardsMoveToZoneData, msg.countersToRemove);

            foreach(Cost cost in costList)
            {
                if(cost is EngageSelfCost)
                {
                    CardManager.Animator.RotateCard(card, SVEProperties.CardEngagedRotation);
                }
            }
            for(int i = 0; i < msg.cardsMoveToZoneData.Length; i++)
            {
                CardObject cardToMove = CardManager.Instance.GetCardByInstanceId(msg.cardsMoveToZoneData[i].cardInstanceId);
                StandardSendCardObjectToZone(cardToMove, oppZoneController, msg.cardsMoveToZoneData[i].endZone switch
                {
                    SVEProperties.Zones.Cemetery => (x, onComplete) => oppZoneController.SendCardToCemetery(x, onComplete),
                    SVEProperties.Zones.Hand => (x, onComplete) => oppZoneController.AddCardToHand(x, onComplete),
                    SVEProperties.Zones.Banished => (x, onComplete) => oppZoneController.SendCardToBanishedZone(x, onComplete),
                    _ => (_, _) => Debug.LogError($"Attempted to send card with instance ID {cardToMove.RuntimeCard.instanceId} to an invalid zone as part of paying cost")
                });
            }
            oppZoneController.RearrangeHand();
        }

        #endregion

        // ------------------------------

        #region Other

        public void TellPerformEffect(OpponentTellOpponentPerformEffectMessage msg)
        {
            CardObject card = CardManager.Instance.GetCardByInstanceId(msg.cardInstanceId);
            if(!card)
            {
                Debug.LogError($"Failed to find card in zone {msg.cardZone} with id {msg.cardInstanceId} to reference for performing effect {msg.effectName}");
                return;
            }
            Ability targetAbility = card.LibraryCard.abilities.FirstOrDefault(x => x.name.Equals(msg.effectName) && x.effect is SveEffect);
            if(targetAbility == null)
            {
                Debug.LogError($"Failed to find target ability {msg.effectName} from card with ID {msg.libraryCardId} (instance {msg.cardInstanceId} in {msg.cardZone}) to perform.");
                return;
            }

            string additionalFilters = msg.targetInstanceIds != null && msg.targetInstanceIds.Length > 0 ? $"i({string.Join(',', msg.targetInstanceIds)})" : null;
            SveEffect effectToPerform = (targetAbility.effect as SveEffect).CopyWithAddFilters(additionalFilters);
            SVEEffectPool.Instance.ResolveEffectImmediate(effectToPerform, card.RuntimeCard, msg.cardZone);
            // opponent knows when we finished after ResolveEffectImmediate() sets SVEEffectPool.IsResolvingEffect to false
        }

        #endregion
    }
}
