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

        public CardSelectionArea selectionArea;
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

            if(!IsLocalPlayer)
                return;
            playerInfo.namedZones[SVEProperties.Zones.Deck].onZoneChanged += x => Player.CardsInDeck = x;
            playerInfo.namedZones[SVEProperties.Zones.Cemetery].onZoneChanged += x => Player.CardsInCemetery = x;
            playerInfo.namedZones[SVEProperties.Zones.EvolveDeck].onZoneChanged += x => Player.CardsInEvolveDeck = x;
            if(!isHost) // Client player's Mirror SyncVar hooks are not getting called their instance (idk why man), so we manually call the hook functions for that user
            {
                playerInfo.namedZones[SVEProperties.Zones.Deck].onZoneChanged += x => Player.SyncHook_OnDeckCountChanged(x, x);
                playerInfo.namedZones[SVEProperties.Zones.Cemetery].onZoneChanged += x => Player.SyncHook_OnCemeteryCountChanged(x, x);
                playerInfo.namedZones[SVEProperties.Zones.EvolveDeck].onZoneChanged += x => Player.SyncHook_OnEvolveDeckCountChanged(x, x);
            }
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

        #region Card Zone Controls (Player)

        public CardObject CreateNewCardObjectTopDeck(RuntimeCard runtimeCard)
        {
            CardObject card = CardManager.Instance.RequestCard(runtimeCard);
            card.transform.SetPositionAndRotation(deckZone.GetTopStackPosition(), SVEProperties.CardFaceDownRotation * (IsLocalPlayer ? Quaternion.identity : SVEProperties.OpponentCardRotation));
            card.CurrentZone = deckZone;
            return card;
        }

        public void AddCardToHand(CardObject card, Action onComplete = null)
        {
            MoveCardZone(card, card.CurrentZone, handZone);
            MoveCardLocalTransform(card, handZone.transform.InverseTransformPoint(handZone.GetLastCardPosition()), handZone.CardRotation, onComplete: onComplete);
            card.SetStatOverlayActive(false);
            card.SetCostOverlayActive(true);
        }

        public void PlayCardToField(CardObject cardToPlay, int slotNumber)
        {
            Debug.Assert(cardToPlay);
            Debug.Assert(fieldZone.IsSlotNumberValid(slotNumber));

            MoveCardZone(cardToPlay, cardToPlay.CurrentZone, fieldZone);
            MoveCardTransform(cardToPlay, fieldZone.GetSlotPosition(slotNumber),
                cardToPlay.RuntimeCard.namedStats.TryGetValue(SVEProperties.CardStats.Engaged, out Stat engagedStat) && engagedStat.effectiveValue > 0
                ? SVEProperties.CardEngagedRotation
                : SVEProperties.CardFaceUpRotation);

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

        public void SendCardToTopDeck(CardObject card, Action onComplete = null)
        {
            if(!card)
                return;

            MoveCardZone(card, card.CurrentZone, deckZone);
            MoveCardTransform(card, deckZone.GetTopStackPosition(), SVEProperties.CardFaceDownRotation, disableOnComplete: true, onComplete: onComplete);
            card.SetStatOverlayActive(false);
            card.SetCostOverlayActive(false);
        }

        public void SendCardToBottomDeck(CardObject card, Action onComplete = null)
        {
            if(!card)
                return;

            MoveCardZone(card, card.CurrentZone, deckZone);
            MoveCardTransform(card, deckZone.GetBottomStackPosition(), SVEProperties.CardFaceDownRotation, disableOnComplete: true, onComplete: onComplete);
            card.SetStatOverlayActive(false);
            card.SetCostOverlayActive(false);
        }

        public void SendCardToCemetery(CardObject card, Action onComplete = null)
        {
            if(!card)
                return;

            CardZone startZone = card.CurrentZone;
            MoveCardZone(card, card.CurrentZone, cemeteryZone);
            MoveCardTransform(card, cemeteryZone.GetTopStackPosition(), SVEProperties.CardFaceUpRotation, onComplete: onComplete);
            if(startZone is PlayerHandZone)
                RearrangeHand();
            card.SetStatOverlayActive(false);
            card.SetCostOverlayActive(false);
        }

        public void SendCardToBanishedZone(CardObject card, Action onComplete = null)
        {
            if(!card)
                return;

            MoveCardZone(card, card.CurrentZone, banishedZone);
            MoveCardTransform(card, banishedZone.GetTopStackPosition(), SVEProperties.CardFaceUpRotation, onComplete: onComplete);
            card.SetStatOverlayActive(false);
            card.SetCostOverlayActive(false);
        }

        public void SendCardToEvolveDeck(CardObject card)
        {
            if(!card)
                return;

            MoveCardZone(card, card.CurrentZone, evolveDeckZone);
            MoveCardTransform(card, evolveDeckZone.GetTopStackPosition(), SVEProperties.CardFaceUpRotation);
            card.SetStatOverlayActive(false);
        }

        public void EvolveCard(CardObject baseCard, CardObject evolvedCard, int slotNumber)
        {
            baseCard.AttachToCard(evolvedCard);

            bool engaged = baseCard.RuntimeCard.namedStats[SVEProperties.CardStats.Engaged].effectiveValue > 0;
            evolvedCard.transform.SetPositionAndRotation(evolveDeckZone.GetTopStackPosition(), SVEProperties.CardFaceUpRotation);
            MoveCardZone(evolvedCard, evolveDeckZone, fieldZone);
            MoveCardTransform(evolvedCard, fieldZone.GetSlotPosition(slotNumber) + (Vector3.up * 0.1f), SVEProperties.CardFaceUpRotation * (engaged ? SVEProperties.CardEngagedRotation : Quaternion.identity));
            fieldZone.MoveCardToSlot(evolvedCard, slotNumber,
                newInteractionType: IsLocalPlayer ? TargetableSlot.InteractionType.None : TargetableSlot.InteractionType.AttackCard);
            evolvedCard.SetStatOverlayActive(true);
        }

        public void SendCardToResolution(CardObject card, Action onComplete = null)
        {
            if(!card)
                return;

            MoveCardZone(card, card.CurrentZone, resolutionZone);
            MoveCardTransform(card, resolutionZone.GetTopStackPosition(), SVEProperties.CardFaceUpRotation);
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

        public void MoveCardToSelectionArea(CardObject card, bool rearrangeHand = true)
        {
            int slot = selectionArea.GetFirstOpenSlotId();
            if(slot == -1)
                slot = 0;
            MoveCardToSelectionArea(card, slot, rearrangeHand);
        }

        public void MoveCardToSelectionArea(CardObject card, int slotNumber, bool rearrangeHand = true)
        {
            if(!card)
                return;

            MoveCardZone(card, card.CurrentZone, selectionArea);
            MoveCardTransform(card, selectionArea.GetSlotPosition(slotNumber), SVEProperties.CardFaceUpRotation, selectionArea.SlotScale);
            selectionArea.MoveCardToSlot(card, slotNumber, selectionArea.endInteractionType);
            if(rearrangeHand)
                RearrangeHand();
        }

        public bool SwapCardSlots(CardObject cardA, CardObject cardB)
        {
            if(!cardA || !cardB || cardA.CurrentZone is not CardPositionedZone zoneA || cardB.CurrentZone is not CardPositionedZone zoneB || zoneA != zoneB)
                return false;

            if(!zoneA.SwapCardsInSlots(cardA, cardB))
                return false;
            MoveCardTransform(cardA, zoneA.GetSlotPosition(zoneA.GetSlotNumber(cardA)), cardA.transform.rotation, targetScale: null);
            MoveCardTransform(cardB, zoneA.GetSlotPosition(zoneA.GetSlotNumber(cardB)), cardB.transform.rotation, targetScale: null);
            return true;
        }

        #endregion

        // ------------------------------

        #region Card Movement Controls (Single Zone-Contained)

        public void RearrangeHand()
        {
            foreach(CardObject card in handZone.AllCards)
            {
                MoveCardLocalTransform(card, handZone.transform.InverseTransformPoint(handZone.GetCardPosition(card)), handZone.CardRotation);
            }
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

        private void MoveCardTransform(CardObject card, Vector3 targetPosition, Quaternion targetRotation, float? targetScale = 1f,
            bool rotateIfOpponent = true, bool instant = false, bool disableOnComplete = false, Action onComplete = null)
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
            CardManager.Animator.MoveCardToPosition(card, targetPosition, targetRotation, targetScale, onComplete: () =>
            {
                if(disableOnComplete && card)
                    CardManager.Instance.ReleaseCard(card);
                onComplete?.Invoke();
            });
        }

        private void MoveCardLocalTransform(CardObject card, Vector3 targetPosition, Quaternion targetRotation, float? targetScale = 1f,
            bool rotateIfOpponent = true, bool instant = false, Action onComplete = null)
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
            CardManager.Animator.MoveCardToLocalPosition(card, targetPosition, targetRotation, targetScale, onComplete: () =>
            {
                onComplete?.Invoke();
            });
        }

        #endregion
    }
}
