using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CCGKit;
using UnityEngine;
using Sirenix.OdinInspector;

namespace SVESimulator
{
    /// <summary>
    /// Handles the physical movement of CardObjects from zone to zone
    /// </summary>
    public class PlayerCardZoneController : MonoBehaviour
    {
        #region Variables

        [field: SerializeField] public bool IsLocalPlayer { get; private set; }

        [FoldoutGroup("Zones")]
        public PlayerHandZone handZone;
        [FoldoutGroup("Zones")]
        public CardStack deckZone;
        [FoldoutGroup("Zones")]
        public CardPositionedZone fieldZone;
        [FoldoutGroup("Zones"), LabelText("EX Area Zone")]
        public CardPositionedZone exAreaZone;
        [FoldoutGroup("Zones")]
        public CardStack cemeteryZone;
        [FoldoutGroup("Zones")]
        public CardStack evolveDeckZone;
        [FoldoutGroup("Zones")]
        public CardStack banishedZone;
        [FoldoutGroup("Zones")]
        public CardStack leaderZone;
        [FoldoutGroup("Zones")]
        public CardStack resolutionZone;

        public CardSelectionArea selectionArea; // for selecting effect targets from a zone other than field/EX
        public ZoneViewingArea zoneViewingArea; // for viewing cards in a zone (ex. cemetery) outside of performing effects
        public EvolvePointDisplay evolvePointDisplay;

        [TitleGroup("Runtime References"), ShowInInspector, ReadOnly]
        public PlayerController Player { get; set; }
        [ShowInInspector, ReadOnly] public CardObject LeaderCardObject { get; private set; }

        #endregion

        // ------------------------------

        #region Properties

        public Dictionary<string, CardZone> AllZones => new()
        {
            { SVEProperties.Zones.Hand, handZone },
            { SVEProperties.Zones.Deck, deckZone },
            { SVEProperties.Zones.Field, fieldZone },
            { SVEProperties.Zones.ExArea, exAreaZone },
            { SVEProperties.Zones.Cemetery, cemeteryZone },
            { SVEProperties.Zones.EvolveDeck, evolveDeckZone },
            { SVEProperties.Zones.Banished, banishedZone },
            { SVEProperties.Zones.Leader, leaderZone },
            { SVEProperties.Zones.Resolution, resolutionZone }
        };

        #endregion

        // ------------------------------

        #region Initialization

        private void Awake()
        {
            selectionArea.gameObject.SetActive(false);
            zoneViewingArea.gameObject.SetActive(false);
        }

        public void InitializeZones(PlayerController player, PlayerInfo playerInfo, bool isHost)
        {
            Player = player;
            handZone.Initialize(playerInfo.namedZones[SVEProperties.Zones.Hand], this);
            deckZone.Initialize(playerInfo.namedZones[SVEProperties.Zones.Deck], this);
            fieldZone.Initialize(playerInfo.namedZones[SVEProperties.Zones.Field], this);
            exAreaZone.Initialize(playerInfo.namedZones[SVEProperties.Zones.ExArea], this);
            cemeteryZone.Initialize(playerInfo.namedZones[SVEProperties.Zones.Cemetery], this);
            evolveDeckZone.Initialize(playerInfo.namedZones[SVEProperties.Zones.EvolveDeck], this);
            banishedZone.Initialize(playerInfo.namedZones[SVEProperties.Zones.Banished], this);
            leaderZone.Initialize(playerInfo.namedZones[SVEProperties.Zones.Leader], this);
            resolutionZone.Initialize(playerInfo.namedZones[SVEProperties.Zones.Resolution], this);
            selectionArea.Initialize(null, this);
            zoneViewingArea.Initialize(null, this);

            if(!IsLocalPlayer)
                return;
            playerInfo.namedZones[SVEProperties.Zones.Deck].onZoneChanged += x => Player.SetDeckCount(x);
            playerInfo.namedZones[SVEProperties.Zones.Cemetery].onZoneChanged += x => Player.SetCemeteryCount(x);
            playerInfo.namedZones[SVEProperties.Zones.EvolveDeck].onZoneChanged += x =>
            {
                int faceDownCount = playerInfo.namedZones[SVEProperties.Zones.EvolveDeck].cards.Count(y => y.namedStats[SVEProperties.CardStats.FaceUp].baseValue == 0);
                Player.SetEvolveDeckCount(faceDownCount, x - faceDownCount);
            };
            playerInfo.namedZones[SVEProperties.Zones.Banished].onZoneChanged += x => Player.SetBanishedZoneCount(x);
        }

        public List<RuntimeCard> InitializeEvolveDeck()
        {
            if(!IsLocalPlayer)
                return new List<RuntimeCard>();

            Debug.Assert(deckZone.Runtime != null);
            Debug.Assert(deckZone.Runtime.cards != null);
            Debug.Assert(deckZone.Runtime.cards.All(x => x.cardType != null));
            Debug.Assert(deckZone.Runtime.cards.All(x => x.cardType.name != null));
            List<RuntimeCard> evolveCards = deckZone.Runtime.cards.Where(x => x.cardType.name.Equals(SVEProperties.CardTypes.EvolvedFollower)).ToList();
            evolveDeckZone.SetStackHeight(evolveCards.Count);
            return evolveCards;
        }

        public CardObject InitializeLeaderCard(RuntimeCard runtimeCard, int leaderCardId)
        {
            runtimeCard ??= deckZone.Runtime.cards.FirstOrDefault(x => x.cardType.name.Equals(SVEProperties.CardTypes.Leader));
            CardObject cardObject = CardManager.Instance.RequestCard(runtimeCard);

            MoveCardZone(cardObject, deckZone, leaderZone);
            MoveCardTransform(cardObject, leaderZone.GetTopStackPosition(), SVEProperties.CardFaceUpRotation, instant: true);
            LeaderCardObject = cardObject;
            return LeaderCardObject;
        }

        #endregion

        // ------------------------------

        #region Move Between Zones

        public void AddCardToHand(CardObject card, Action onComplete = null) => AddCardToHand(card, 0f, onComplete);
        public void AddCardToHand(CardObject card, float delay, Action onComplete = null)
        {
            CardMovementType moveType = IsLocalPlayer && card.CurrentZone == deckZone ? CardMovementType.Draw : CardMovementType.Standard;
            MoveCardZone(card, card.CurrentZone, handZone);
            MoveCardLocalTransform(card, handZone.transform.InverseTransformPoint(handZone.GetLastCardPosition()), handZone.CardRotation, moveType: moveType, delay: delay, onComplete: onComplete);
            card.SetStatOverlayActive(false);
            card.SetCostOverlayActive(true);
        }

        public void PlayCardToField(CardObject cardToPlay, int slotNumber, Action onComplete = null)
        {
            Debug.Assert(cardToPlay);
            Debug.Assert(fieldZone.IsSlotNumberValid(slotNumber));

            MoveCardZone(cardToPlay, cardToPlay.CurrentZone, fieldZone);
            MoveCardTransform(cardToPlay, fieldZone.GetSlotPosition(slotNumber),
                cardToPlay.RuntimeCard.namedStats.TryGetValue(SVEProperties.CardStats.Engaged, out Stat engagedStat) && engagedStat.effectiveValue > 0
                    ? SVEProperties.CardEngagedRotation
                    : SVEProperties.CardFaceUpRotation,
                moveType: Vector3.Dot(cardToPlay.transform.up, Vector3.up) > 0.5f ? CardMovementType.PlayFromFaceUp : CardMovementType.PlayFromFaceDown,
                onComplete: onComplete);

            fieldZone.MoveCardToSlot(cardToPlay, slotNumber,
                newInteractionType: IsLocalPlayer ? TargetableSlot.InteractionType.None : TargetableSlot.InteractionType.AttackCard);
            RearrangeHand();
            cardToPlay.SetStatOverlayActive(true);
            cardToPlay.SetCostOverlayActive(false);
            if(cardToPlay.IsFollowerOrEvolvedFollower())
                cardToPlay.CalculateCanAttackStatus();
        }

        public void SendCardToExArea(CardObject card, int slotNumber, Action onComplete = null)
        {
            MoveCardZone(card, card.CurrentZone, exAreaZone);
            MoveCardTransform(card, exAreaZone.GetSlotPosition(slotNumber), SVEProperties.CardFaceUpRotation, onComplete: onComplete);

            exAreaZone.MoveCardToSlot(card, slotNumber, newInteractionType: TargetableSlot.InteractionType.None);

            card.SetStatOverlayActive(true);
            card.SetCostOverlayActive(true);
            card.SetHighlightMode(CardObject.HighlightMode.None);
        }

        public void SendCardToTopDeck(CardObject card, Action onComplete = null) => SendCardToTopDeck(card, 0f, onComplete);
        public void SendCardToTopDeck(CardObject card, float delay, Action onComplete = null)
        {
            if(!card)
                return;

            MoveCardZone(card, card.CurrentZone, deckZone);
            MoveCardTransform(card, deckZone.GetTopStackPosition(), SVEProperties.CardFaceDownRotation,
                moveType: IsLocalPlayer ? CardMovementType.SlideIntoDeckLeft : CardMovementType.SlideIntoDeckRight, delay: delay, disableOnComplete: true, onComplete: onComplete);
            card.SetStatOverlayActive(false);
            card.SetCostOverlayActive(false);
        }

        public void SendCardToBottomDeck(CardObject card, Action onComplete = null) => SendCardToBottomDeck(card, 0f, onComplete);
        public void SendCardToBottomDeck(CardObject card, float delay, Action onComplete = null)
        {
            if(!card)
                return;

            MoveCardZone(card, card.CurrentZone, deckZone);
            MoveCardTransform(card, deckZone.GetBottomStackPosition(), SVEProperties.CardFaceDownRotation,
                moveType: IsLocalPlayer ? CardMovementType.SlideIntoDeckLeft : CardMovementType.SlideIntoDeckRight, delay: delay, disableOnComplete: true, onComplete: onComplete);
            card.SetStatOverlayActive(false);
            card.SetCostOverlayActive(false);
        }

        public void SendCardToCemetery(CardObject card, Action onComplete = null) => SendCardToCemetery(card, 0f, onComplete);
        public void SendCardToCemetery(CardObject card, float delay, Action onComplete = null)
        {
            if(!card)
                return;

            CardZone startZone = card.CurrentZone;
            CardMovementType moveType = startZone is CardSelectionArea ? CardMovementType.StackToStackDown : startZone.Runtime?.name switch
            {
                SVEProperties.Zones.Deck    => CardMovementType.FlipFromDeck,
                SVEProperties.Zones.Hand    => CardMovementType.Standard,
                _                           => CardMovementType.StackToStackNormal
            };

            MoveCardZone(card, card.CurrentZone, cemeteryZone);
            MoveCardTransform(card, cemeteryZone.GetTopStackPosition(), SVEProperties.CardFaceUpRotation, moveType: moveType, delay: delay, onComplete: onComplete);
            if(startZone is PlayerHandZone)
                RearrangeHand();
            card.SetStatOverlayActive(false);
            card.SetCostOverlayActive(false);
        }

        public void SendCardToBanishedZone(CardObject card, Action onComplete = null) => SendCardToBanishedZone(card, 0f, onComplete);
        public void SendCardToBanishedZone(CardObject card, float delay, Action onComplete = null)
        {
            if(!card)
                return;

            CardMovementType moveType = card.CurrentZone is CardSelectionArea ? CardMovementType.StackToStackDown : card.CurrentZone.Runtime?.name switch
            {
                SVEProperties.Zones.Deck    => CardMovementType.FlipFromDeck,
                SVEProperties.Zones.Hand    => CardMovementType.Standard,
                _                           => CardMovementType.StackToStackNormal
            };

            MoveCardZone(card, card.CurrentZone, banishedZone);
            MoveCardTransform(card, banishedZone.GetTopStackPosition(), SVEProperties.CardFaceUpRotation, moveType: moveType, delay: delay, onComplete: onComplete);
            card.SetStatOverlayActive(false);
            card.SetCostOverlayActive(false);
        }

        public void SendCardToEvolveDeck(CardObject card, float delay = 0f)
        {
            if(!card)
                return;

            bool isFaceUp = card.RuntimeCard.namedStats[SVEProperties.CardStats.FaceUp].effectiveValue > 0;
            MoveCardZone(card, card.CurrentZone, evolveDeckZone);
            if(isFaceUp)
                MoveCardTransform(card, evolveDeckZone.GetTopStackPosition(), SVEProperties.CardFaceUpRotation, moveType: CardMovementType.StackToStackNormal, delay: delay);
            else
                MoveCardTransform(card, evolveDeckZone.GetBottomStackPosition(), SVEProperties.CardFaceDownRotation,
                    moveType: IsLocalPlayer ? CardMovementType.SlideIntoDeckRight : CardMovementType.SlideIntoDeckLeft, delay: delay, disableOnComplete: true);
            card.SetStatOverlayActive(false);
        }

        public void SendCardToResolution(CardObject card, Action onComplete = null)
        {
            if(!card)
                return;

            CardMovementType moveType = card.CurrentZone is CardSelectionArea ? CardMovementType.Standard : card.CurrentZone.Runtime?.name switch
            {
                SVEProperties.Zones.Deck        => CardMovementType.FlipFromDeck,
                SVEProperties.Zones.EvolveDeck  => CardMovementType.FlipFromDeck,
                SVEProperties.Zones.Hand        => CardMovementType.Standard,
                _                               => CardMovementType.StackToStackNormal
            };

            MoveCardZone(card, card.CurrentZone, resolutionZone);
            MoveCardTransform(card, resolutionZone.GetTopStackPosition(), SVEProperties.CardFaceUpRotation, moveType: moveType);
            RearrangeHand();
        }

        public void RevealCard(CardObject card, float delay = 1f, Action onComplete = null)
        {
            StartCoroutine(RevealAndWait());

            IEnumerator RevealAndWait()
            {
                SendCardToResolution(card, onComplete: onComplete);
                yield return new WaitForSeconds(delay);
                onComplete?.Invoke();
            }
        }

        public void MoveCardToSelectionArea(CardObject card, bool rearrangeHand = true, float delay = 0f)
        {
            int slot = selectionArea.GetFirstOpenSlotId();
            if(slot == -1)
                slot = 0;
            MoveCardToSelectionArea(card, slot, rearrangeHand, delay);
        }

        public void MoveCardToSelectionArea(CardObject card, int slotNumber, bool rearrangeHand = true, float delay = 0f)
        {
            if(!card)
                return;

            CardMovementType moveType = card.CurrentZone.Runtime?.name switch
            {
                SVEProperties.Zones.Deck        => CardMovementType.Draw,
                SVEProperties.Zones.EvolveDeck  => Vector3.Dot(card.transform.up, Vector3.up) > 0.5f ? CardMovementType.StackToStackUp : CardMovementType.Draw,
                SVEProperties.Zones.Hand        => Vector3.Distance(card.transform.position, selectionArea.GetSlotPosition(slotNumber)) < 8f // if card is nearby/dragging, use faster movement
                    ? CardMovementType.Standard
                    : CardMovementType.StackToStackUp,
                SVEProperties.Zones.Cemetery    => CardMovementType.StackToStackUp,
                SVEProperties.Zones.Banished    => CardMovementType.StackToStackUp,
                _                               => CardMovementType.StackToStackNormal
            };

            MoveCardZone(card, card.CurrentZone, selectionArea);
            MoveCardLocalTransform(card, selectionArea.GetSlotLocalPosition(slotNumber), SVEProperties.CardFaceUpRotation, moveType: moveType, delay: delay, targetScale: selectionArea.SlotScale);
            selectionArea.MoveCardToSlot(card, slotNumber, selectionArea.endInteractionType);
            if(rearrangeHand)
                RearrangeHand();
        }

        public void MoveCardToZoneViewingArea(CardObject card, bool rearrangeHand = false, float delay = 0f)
        {
            int slot = zoneViewingArea.GetFirstOpenSlotId();
            if(slot == -1)
                slot = 0;
            MoveCardToZoneViewingArea(card, slot, rearrangeHand, delay);
        }

        public void MoveCardToZoneViewingArea(CardObject card, int slotNumber, bool rearrangeHand = false, float delay = 0f)
        {
            if(!card)
                return;

            CardMovementType moveType = card.CurrentZone.Runtime?.name switch
            {
                SVEProperties.Zones.Deck        => CardMovementType.Draw,
                SVEProperties.Zones.EvolveDeck  => Vector3.Dot(card.transform.up, Vector3.up) > 0.5f ? CardMovementType.StackToStackUp : CardMovementType.Draw,
                SVEProperties.Zones.Cemetery    => CardMovementType.StackToStackUp,
                SVEProperties.Zones.Banished    => CardMovementType.StackToStackUp,
                _                               => CardMovementType.StackToStackNormal
            };

            MoveCardZone(card, card.CurrentZone, zoneViewingArea);
            MoveCardTransform(card, zoneViewingArea.GetSlotPosition(slotNumber), SVEProperties.CardFaceUpRotation, moveType: moveType, delay: delay, targetScale: zoneViewingArea.SlotScale);
            zoneViewingArea.MoveCardToSlot(card, slotNumber, zoneViewingArea.endInteractionType);
            if(rearrangeHand)
                RearrangeHand();
        }

        public bool SwapCardSlots(CardObject cardA, CardObject cardB)
        {
            if(!cardA || !cardB || cardA.CurrentZone is not CardPositionedZone zoneA || cardB.CurrentZone is not CardPositionedZone zoneB || zoneA != zoneB)
                return false;

            if(!zoneA.SwapCardsInSlots(cardA, cardB))
                return false;
            MoveCardTransform(cardA, zoneA.GetSlotPosition(zoneA.GetSlotNumber(cardA)), cardA.transform.rotation, moveType: CardMovementType.StackToStackNormal, targetScale: null);
            MoveCardTransform(cardB, zoneA.GetSlotPosition(zoneA.GetSlotNumber(cardB)), cardB.transform.rotation, moveType: CardMovementType.StackToStackNormal, targetScale: null);
            return true;
        }

        #endregion

        // ------------------------------

        #region Other Zone Controls

        public CardObject CreateNewCardObjectTopDeck(RuntimeCard runtimeCard) => CreateNewCardObjectTopDeck(runtimeCard, deckZone);
        public CardObject CreateNewCardObjectTopDeck(RuntimeCard runtimeCard, CardStack zone)
        {
            CardObject card = CardManager.Instance.GetCardByInstanceId(runtimeCard.instanceId);
            if(card)
                return card;

            card = CardManager.Instance.RequestCard(runtimeCard);
            card.transform.SetPositionAndRotation(zone.GetTopStackPosition(), SVEProperties.CardFaceDownRotation * (IsLocalPlayer ? Quaternion.identity : SVEProperties.OpponentCardRotation));
            card.CurrentZone = zone;
            return card;
        }

        public void RearrangeHand()
        {
            foreach(CardObject card in handZone.AllCards)
            {
                MoveCardLocalTransform(card, handZone.transform.InverseTransformPoint(handZone.GetCardPosition(card)), handZone.CardRotation);
            }
        }

        public void EvolveCard(CardObject baseCard, CardObject evolvedCard, int slotNumber)
        {
            baseCard.AttachToCard(evolvedCard);

            bool engaged = baseCard.RuntimeCard.namedStats[SVEProperties.CardStats.Engaged].effectiveValue > 0;
            evolvedCard.transform.SetPositionAndRotation(evolveDeckZone.GetTopStackPosition(), SVEProperties.CardFaceUpRotation);
            MoveCardZone(evolvedCard, evolveDeckZone, fieldZone);
            MoveCardTransform(evolvedCard, fieldZone.GetSlotPosition(slotNumber) + (Vector3.up * 0.1f), SVEProperties.CardFaceUpRotation * (engaged ? SVEProperties.CardEngagedRotation : Quaternion.identity),
                moveType: CardMovementType.FlipFromDeck);
            fieldZone.MoveCardToSlot(evolvedCard, slotNumber,
                newInteractionType: IsLocalPlayer ? TargetableSlot.InteractionType.None : TargetableSlot.InteractionType.AttackCard);
            evolvedCard.SetStatOverlayActive(true);
        }

        public void ServeCard(CardObject card, CardObject carrotCard)
        {
            int slotNumber = fieldZone.GetSlotNumber(card);
            carrotCard.AttachToCard(card);
            carrotCard.transform.SetPositionAndRotation(evolveDeckZone.GetTopStackPosition(), SVEProperties.CardFaceUpRotation);
            MoveCardZone(carrotCard, evolveDeckZone, fieldZone);
            MoveCardTransform(carrotCard, fieldZone.GetSlotPosition(slotNumber) - (Vector3.up * 0.1f), SVEProperties.CardFaceUpRotation, moveType: CardMovementType.FlipFromDeck);
            fieldZone.MoveCardToSlot(carrotCard, slotNumber, newInteractionType: TargetableSlot.InteractionType.None);
        }

        public void AddAndPlaceToken(CardObject token, CardPositionedZone zone, int slotNumber)
        {
            token.CurrentZone = zone;
            zone.AddCard(token);
            zone.Runtime.AddCard(token.RuntimeCard);

            MoveCardTransform(token, zone.GetSlotPosition(slotNumber), SVEProperties.CardFaceUpRotation, instant: true);
            zone.MoveCardToSlot(token, slotNumber,
                newInteractionType: IsLocalPlayer ? TargetableSlot.InteractionType.None : TargetableSlot.InteractionType.AttackCard);

            token.SetStatOverlayActive(true);
            token.SetCostOverlayActive(zone == exAreaZone);
            if(token.IsFollowerOrEvolvedFollower() && zone == fieldZone && zone.IsLocalPlayerZone)
                token.CalculateCanAttackStatus();
            else
                token.SetHighlightMode(CardObject.HighlightMode.None);
        }

        public void FlipCardToFaceUp(CardObject card, float delay = 0f, Action onComplete = null)
        {
            MoveCardTransform(card, card.transform.position, SVEProperties.CardFaceUpRotation, moveType: CardMovementType.FlipFromDeck, delay: delay, onComplete: onComplete);
        }

        public void FlipCardToFaceDown(CardObject card, float delay = 0f, Action onComplete = null)
        {
            MoveCardTransform(card, card.transform.position, SVEProperties.CardFaceDownRotation, moveType: CardMovementType.FlipFromDeck, delay: delay, disableOnComplete: true, onComplete: onComplete);
        }

        #endregion

        // ------------------------------

        #region Get Info

        public bool EvolveDeckHasEvolvedVersionOf(RuntimeCard card, bool faceDownOnly = true) => EvolveDeckHasEvolvedVersionOf(card, out _, faceDownOnly);
        public bool EvolveDeckHasEvolvedVersionOf(RuntimeCard card, out RuntimeCard evolvedCard, bool faceDownOnly = true)
        {
            Card baseLibraryCard = LibraryCardCache.GetCard(card.cardId);
            evolvedCard = evolveDeckZone.Runtime.cards.FirstOrDefault(x => LibraryCardCache.GetCard(x.cardId).IsEvolvedVersionOf(baseLibraryCard)
                && (!faceDownOnly || (x.namedStats.TryGetValue(SVEProperties.CardStats.FaceUp, out Stat faceUpStat) && faceUpStat.baseValue == 0)));
            return evolvedCard != null;
        }

        #endregion

        // ------------------------------

        #region Private Movement Controls

        private void MoveCardZone(CardObject card, CardZone startZone, CardZone endZone)
        {
            startZone.RemoveCard(card);
            endZone.AddCard(card);
            card.CurrentZone = endZone;

            card.OnMoveZone();
        }

        private void MoveCardTransform(CardObject card, Vector3 targetPosition, Quaternion targetRotation, CardMovementType moveType = CardMovementType.Standard, float? targetScale = 1f,
            bool rotateIfOpponent = true, float delay = 0f, bool instant = false, bool disableOnComplete = false, Action onComplete = null)
        {
            if(rotateIfOpponent && !IsLocalPlayer)
                targetRotation *= SVEProperties.OpponentCardRotation;
            if(instant)
            {
                card.transform.SetPositionAndRotation(targetPosition, targetRotation);
                if(targetScale.HasValue)
                    card.transform.localScale = Vector3.one * targetScale.Value;
                onComplete?.Invoke();
                return;
            }
            CardManager.Animator.MoveCardToPosition(moveType, card, targetPosition, targetRotation, targetScale, delay, onComplete: () =>
            {
                if(disableOnComplete && card)
                    CardManager.Instance.ReleaseCard(card);
                onComplete?.Invoke();
            });
        }

        private void MoveCardLocalTransform(CardObject card, Vector3 targetPosition, Quaternion targetRotation, CardMovementType moveType = CardMovementType.Standard, float? targetScale = 1f,
            bool rotateIfOpponent = true, float delay = 0f, bool instant = false, Action onComplete = null)
        {
            if(rotateIfOpponent && !IsLocalPlayer)
                targetRotation *= SVEProperties.OpponentCardRotation;
            if(instant)
            {
                card.transform.SetPositionAndRotation(targetPosition, targetRotation);
                if(targetScale.HasValue)
                    card.transform.localScale = Vector3.one * targetScale.Value;
                onComplete?.Invoke();
                return;
            }
            CardManager.Animator.MoveCardToLocalPosition(moveType, card, targetPosition, targetRotation, targetScale, delay, onComplete: () =>
            {
                onComplete?.Invoke();
            });
        }

        #endregion
    }
}
