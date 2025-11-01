using System;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CCGKit;
using Mirror;

namespace SVESimulator
{
    public abstract class PlayerEventControllerBase : MonoBehaviour
    {
        #region Variables

        [TitleGroup("Object References"), SerializeField, Required]
        protected PlayerController playerController;

        protected PlayerInfo playerInfo;
        protected PlayerInfo opponentInfo;
        protected Stat playPointStat;
        protected SVEEffectSolver sveEffectSolver;
        protected NetworkIdentity netIdentity;

        protected abstract bool isLocal { get; }

        protected bool isActivePlayer => playerController.isActivePlayer;
        protected PlayerCardZoneController localZoneController => playerController.ZoneController;
        protected PlayerCardZoneController oppZoneController => playerController.OppZoneController;

        #endregion

        // ------------------------------

        #region Unity Functions

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if(!playerController)
                playerController = GetComponent<PlayerController>();
        }
#endif

        #endregion

        // ------------------------------

        #region Initialization

        public virtual void Initialize(PlayerInfo pInfo, PlayerInfo oppInfo, Stat playPointStat, SVEEffectSolver effectSolver, NetworkIdentity netId)
        {
            playerInfo = pInfo;
            opponentInfo = oppInfo;
            this.playPointStat = playPointStat;
            sveEffectSolver = effectSolver;
            netIdentity = netId;
        }

        protected void InitRuntimeCard(ref RuntimeCard runtimeCard, NetCard netCard) => InitRuntimeCard(ref runtimeCard, netCard, isLocal ? playerInfo : opponentInfo);
        protected void InitRuntimeCard(ref RuntimeCard runtimeCard, NetCard netCard, PlayerInfo ownerInfo)
        {
            runtimeCard.cardId = netCard.cardId;
            runtimeCard.instanceId = netCard.instanceId;
            runtimeCard.ownerPlayer = ownerInfo;
            foreach(NetStat stat in netCard.stats)
            {
                Stat runtimeStat = NetworkingUtils.GetRuntimeStat(stat);
                runtimeCard.stats[stat.statId] = runtimeStat;
                Card libraryCard = GameManager.Instance.config.GetCard(netCard.cardId);
                string statName = libraryCard.stats.Find(x => x.statId == stat.statId).name;
                runtimeCard.namedStats[statName] = runtimeStat;
            }
            foreach(NetKeyword keyword in netCard.keywords)
            {
                RuntimeKeyword runtimeKeyword = NetworkingUtils.GetRuntimeKeyword(keyword);
                runtimeCard.keywords.Add(runtimeKeyword);
            }
        }

        #endregion

        // ------------------------------

        #region Move Card Objects

        /// <summary>
        /// Sends given card to desired zone, with handling evolved cards and tokens having additional logic compared to regular cards
        /// </summary>
        /// <param name="card">The card to move</param>
        /// <param name="zoneController">The zone controller the card belongs to</param>
        /// <param name="moveCardAction">An Action that performs regular (non-evolved) card movement, with parameters (Card, onComplete)</param>
        protected void StandardSendCardObjectToZone(CardObject card, PlayerCardZoneController zoneController, Action<CardObject, Action> moveCardAction)
        {
            List<CardObject> cardsToMove = new() { card };
            cardsToMove.AddRange(card.GetAttachedCards());

            foreach(CardObject cardToMove in cardsToMove)
            {
                cardToMove.SetHighlightMode(CardObject.HighlightMode.None);
                if(cardToMove.IsEvolvedType())
                {
                    zoneController.SendCardToEvolveDeck(cardToMove);
                    continue;
                }

                Action onComplete = card.IsToken() ? () => { CardManager.Instance.ReleaseCard(cardToMove); } : null;
                moveCardAction?.Invoke(cardToMove, onComplete);
            }
        }

        #endregion

        // ------------------------------

        #region Set Play Points

        public void IncrementMaxPlayPoints(int amount = 1, bool updateCurrentPoints = false, bool sendMessage = true)
        {
            PlayerInfo player = isLocal ? playerInfo : opponentInfo;
            SetMaxPlayPoints(player.namedStats[SVEProperties.PlayerStats.MaxPlayPoints].baseValue + amount, updateCurrentPoints, sendMessage);
        }

        public void SetMaxPlayPoints(int value, bool updateCurrentPoints = false, bool sendMessage = true)
        {
            PlayerInfo player = isLocal ? playerInfo : opponentInfo;
            sveEffectSolver.SetMaxPlayPoints(player.netId, value, updateCurrentPoints);
            PlayPointMeter meter = isLocal ? FieldManager.PlayerPlayPoints : FieldManager.OpponentPlayPoints;
            meter.SetMaxPoints(value, updateCurrentPoints);

            if(sendMessage)
            {
                SetMaxPlayPointsMessage msg = new()
                {
                    playerNetId = player.netId,
                    maxPlayPoints = player.namedStats[SVEProperties.PlayerStats.MaxPlayPoints].baseValue,
                    updateCurrentPoints = updateCurrentPoints
                };
                NetworkClient.Send(msg);
            }
        }

        public void IncrementCurrentPlayPoints(int amount = 1, bool sendMessage = true)
        {
            PlayerInfo player = isLocal ? playerInfo : opponentInfo;
            SetCurrentPlayPoints(player.namedStats[SVEProperties.PlayerStats.PlayPoints].baseValue + amount, sendMessage);
        }

        public void SetCurrentPlayPoints(int value, bool sendMessage = true)
        {
            PlayerInfo player = isLocal ? playerInfo : opponentInfo;
            value = Mathf.Clamp(value, 0, player.namedStats[SVEProperties.PlayerStats.MaxPlayPoints].baseValue);
            sveEffectSolver.SetCurrentPlayPoints(player.netId, value);

            PlayPointMeter meter = isLocal ? FieldManager.PlayerPlayPoints : FieldManager.OpponentPlayPoints;
            meter.SetCurrentPoints(value);

            if(sendMessage)
            {
                SetCurrentPlayPointsMessage msg = new()
                {
                    playerNetId = player.netId,
                    currentPlayPoints = value
                };
                NetworkClient.Send(msg);
            }
        }

        #endregion

        // ------------------------------

        #region Pay Cost Checks

        public bool CanPayPlayPointsCost(int amount)
        {
            return playPointStat.effectiveValue >= amount;
        }

        public bool HasEvolvePoint()
        {
            return playerInfo.namedStats[SVEProperties.PlayerStats.EvolutionPoints].effectiveValue > 0;
        }

        public bool CanPayEvolveCost(int amount, bool tryUseEvolvePoint = false)
        {
            return CanPayPlayPointsCost(tryUseEvolvePoint && HasEvolvePoint() ? Mathf.Max(amount - 1, 0) : amount);
        }

        protected CardObject GetEvolvedCardOf(RuntimeCard baseCard)
        {
            PlayerCardZoneController zoneController = isLocal ? localZoneController : oppZoneController;
            if(!zoneController.EvolveDeckHasEvolvedVersionOf(baseCard, out RuntimeCard evolvedCard))
                return null;

            if(!zoneController.evolveDeckZone.TryGetCard(evolvedCard.instanceId, out CardObject evolvedCardObject))
                evolvedCardObject = CardManager.Instance.RequestCard(evolvedCard);
            return evolvedCardObject;
        }

        #endregion
    }
}
