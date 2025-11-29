using CCGKit;
using Mirror;
using UnityEngine;
using Sirenix.OdinInspector;

namespace SVESimulator
{
    #region Game Setup/Game Flow

    public struct SetGoingFirstPlayerMessage : NetworkMessage
    {
        public NetworkIdentity playerNetId;
    }

    public struct LocalPerformMulliganMessage : NetworkMessage
    {
        public NetworkIdentity playerNetId;
        public int[] cardIdsInOrder;
    }

    public struct OpponentPerformMulliganMessage : NetworkMessage
    {
        public NetworkIdentity playerNetId;
    }

    public struct SetMaxPlayPointsMessage : NetworkMessage
    {
        public NetworkIdentity playerNetId;
        public int maxPlayPoints;
        public bool updateCurrentPoints;
    }

    public struct SetCurrentPlayPointsMessage : NetworkMessage
    {
        public NetworkIdentity playerNetId;
        public int currentPlayPoints;
    }

    public struct LocalInitDeckAndLeaderMessage : NetworkMessage
    {
        public NetworkIdentity playerNetId;
        public int[] evolvedCardsInstanceIds;
        public int leaderCardInstanceId;
    }

    public struct OpponentInitDeckAndLeaderMessage : NetworkMessage
    {
        public NetworkIdentity playerNetId;
        public int evolveDeckSize;
        public NetCard leaderCard;
    }

    public struct ExtraTurnMessage : NetworkMessage
    {
        public NetworkIdentity playerNetId;
    }

    #endregion

    // ------------------------------

    #region Draw Cards & Deck Movement

    public struct LocalDrawCardMessage : NetworkMessage
    {
        public NetworkIdentity playerNetId;
        public int cardInstanceId;
        public bool reveal;
    }

    public struct OpponentDrawCardMessage : NetworkMessage
    {
        public NetworkIdentity playerNetId;
        public NetCard card;
        public bool reveal;
    }

    public struct LocalTellOppDrawCardMessage : NetworkMessage
    {
        public NetworkIdentity playerNetId;
        public int count;
    }

    public struct OpponentTellOppDrawCardMessage : NetworkMessage
    {
        public NetworkIdentity playerNetId;
        public int count;
    }

    public struct LocalTellOppMillDeckMessage : NetworkMessage
    {
        public NetworkIdentity playerNetId;
        public int count;
    }

    public struct OpponentTellOppMillDeckMessage : NetworkMessage
    {
        public NetworkIdentity playerNetId;
        public int count;
    }

    #endregion

    // ------------------------------

    #region Card Zone Movement

    public struct LocalPlayCardMessage : NetworkMessage
    {
        public NetworkIdentity playerNetId;
        public int cardInstanceId;
        public int fieldSlotId;
        public string originZone;
        public int playPointCost;
    }

    public struct OpponentPlayCardMessage : NetworkMessage
    {
        public NetworkIdentity playerNetId;
        public NetCard card;
        public int fieldSlotId;
        public string originZone;
        public int playPointCost;
    }

    public struct LocalSendCardToCemeteryMessage : NetworkMessage
    {
        public NetworkIdentity playerNetId;
        public int cardInstanceId;
        public bool isOpponentCard;
        public string originZone;
    }

    public struct OpponentSendCardToCemeteryMessage : NetworkMessage
    {
        public NetworkIdentity playerNetId;
        public NetCard card;
        public bool isOpponentCard;
        public string originZone;
    }

    public struct LocalBanishCardMessage : NetworkMessage
    {
        public NetworkIdentity playerNetId;
        public int cardInstanceId;
        public bool isOpponentCard;
        public string originZone;
    }

    public struct OpponentBanishCardMessage : NetworkMessage
    {
        public NetworkIdentity playerNetId;
        public NetCard card;
        public bool isOpponentCard;
        public string originZone;
    }

    public struct LocalDestroyOpponentCardMessage : NetworkMessage
    {
        public NetworkIdentity playerNetId;
        public int cardInstanceId;
    }

    public struct OpponentDestroyOpponentCardMessage : NetworkMessage
    {
        public NetworkIdentity playerNetId;
        public int cardInstanceId;
    }

    public struct LocalReturnToHandMessage : NetworkMessage
    {
        public NetworkIdentity playerNetId;
        public int cardInstanceId;
        public bool isOpponentCard;
        public string originZone;
    }

    public struct OpponentReturnToHandMessage : NetworkMessage
    {
        public NetworkIdentity playerNetId;
        public NetCard card;
        public bool isOpponentCard;
        public string originZone;
    }

    public struct LocalSendToBottomDeckMessage : NetworkMessage
    {
        public NetworkIdentity playerNetId;
        public int cardInstanceId;
        public bool isOpponentCard;
        public string originZone;
    }

    public struct OpponentSendToBottomDeckMessage : NetworkMessage
    {
        public NetworkIdentity playerNetId;
        public int cardInstanceId;
        public bool isOpponentCard;
        public string originZone;
    }

    public struct LocalSendToTopDeckMessage : NetworkMessage
    {
        public NetworkIdentity playerNetId;
        public int cardInstanceId;
        public bool isOpponentCard;
        public string originZone;
    }

    public struct OpponentSendToTopDeckMessage : NetworkMessage
    {
        public NetworkIdentity playerNetId;
        public int cardInstanceId;
        public bool isOpponentCard;
        public string originZone;
    }

    public struct LocalSendToExAreaMessage : NetworkMessage
    {
        public NetworkIdentity playerNetId;
        public int cardInstanceId;
        public bool isOpponentCard;
        public int fieldSlotId;
        public string originZone;
    }

    public struct OpponentSendToExAreaMessage : NetworkMessage
    {
        public NetworkIdentity playerNetId;
        public NetCard card;
        public bool isOpponentCard;
        public int fieldSlotId;
        public string originZone;
    }

    #endregion

    // ------------------------------

    #region Attacking

    public struct LocalDeclareAttackMessage : NetworkMessage
    {
        public NetworkIdentity playerNetId;
        public int cardInstanceId;
    }

    public struct OpponentDeclareAttackMessage : NetworkMessage
    {
        public NetworkIdentity playerNetId;
        public int cardInstanceId;
    }

    public struct LocalAttackFollowerMessage : NetworkMessage
    {
        public NetworkIdentity attackingPlayerNetId;
        public int attackerInstanceId;
        public int defenderInstanceId;
    }

    public struct OpponentAttackFollowerMessage : NetworkMessage
    {
        public NetworkIdentity attackingPlayerNetId;
        public NetCard attackingCard;
        public NetCard defendingCard;
    }

    public struct LocalAttackLeaderMessage : NetworkMessage
    {
        public NetworkIdentity playerNetId;
        public int attackerInstanceId;
    }

    public struct OpponentAttackLeaderMessage : NetworkMessage
    {
        public NetworkIdentity playerNetId;
        public NetCard attackingCard;
    }

    #endregion

    // ------------------------------

    #region Evolve

    public struct LocalEvolveCardMessage : NetworkMessage
    {
        public NetworkIdentity playerNetId;
        public int baseCardInstanceId;
        public int evolvedCardInstanceId;
        public int fieldSlotId;
        public bool useEvolvePoint;
        public bool useEvolveCost;
    }

    public struct OpponentEvolveCardMessage : NetworkMessage
    {
        public NetworkIdentity playerNetId;
        public NetCard baseCard;
        public NetCard evolvedCard;
        public int fieldSlotId;
        public bool useEvolvePoint;
        public bool useEvolveCost;
    }

    #endregion

    // ------------------------------

    #region Tokens

    public struct LocalCreateTokenMessage : NetworkMessage
    {
        public NetworkIdentity playerNetId;
        public int libraryCardId;
        public int runtimeCardInstanceId;
        public bool createOnField;
        public int slotId;
    }

    public struct OpponentCreateTokenMessage : NetworkMessage
    {
        public NetworkIdentity playerNetId;
        public NetCard card;
        public bool createOnField;
        public int slotId;
    }

    public struct LocalTransformCardMessage : NetworkMessage
    {
        public NetworkIdentity playerNetId;
        public int targetCardInstanceId;
        public bool isOpponentCard;
        public string originZone;

        public int libraryCardId;
        public int tokenRuntimeCardInstanceId;
        public int slotId;
    }

    public struct OpponentTransformCardMessage : NetworkMessage
    {
        public NetworkIdentity playerNetId;
        public NetCard targetCard;
        public bool isOpponentCard;
        public string originZone;

        public NetCard tokenCard;
        public int slotId;
    }

    #endregion

    // ------------------------------

    #region Spells

    public struct LocalPlaySpellMessage : NetworkMessage
    {
        public NetworkIdentity playerNetId;
        public int cardInstanceId;
        public string originZone;
        public int playPointCost;
    }

    public struct OpponentPlaySpellMessage : NetworkMessage
    {
        public NetworkIdentity playerNetId;
        public int cardInstanceId;
        public string originZone;
        public int playPointCost;
    }

    public struct LocalFinishSpellMessage : NetworkMessage
    {
        public NetworkIdentity playerNetId;
        public int cardInstanceId;
        public string originZone;
    }

    public struct OpponentFinishSpellMessage : NetworkMessage
    {
        public NetworkIdentity playerNetId;
        public int cardInstanceId;
        public string originZone;
    }

    #endregion

    // ------------------------------

    #region Card & Leader Stat Controls

    public struct LocalReserveCardMessage : NetworkMessage
    {
        public NetworkIdentity playerNetId;
        public bool isOpponentCard;
        public int cardInstanceId;
    }

    public struct OpponentReserveCardMessage : NetworkMessage
    {
        public NetworkIdentity playerNetId;
        public bool isOpponentCard;
        public NetCard card;
    }

    public struct LocalEngageCardMessage : NetworkMessage
    {
        public NetworkIdentity playerNetId;
        public bool isOpponentCard;
        public int cardInstanceId;
    }

    public struct OpponentEngageCardMessage : NetworkMessage
    {
        public NetworkIdentity playerNetId;
        public bool isOpponentCard;
        public NetCard card;
    }

    public struct LocalSetCardStatMessage : NetworkMessage
    {
        public NetworkIdentity playerNetId;
        public bool isOpponentCard;
        public int cardInstanceId;
        public int statId;
        public int value;
    }

    public struct OpponentSetCardStatMessage : NetworkMessage
    {
        public NetworkIdentity playerNetId;
        public bool isOpponentCard;
        public int cardInstanceId;
        public int statId;
        public int value;
    }

    public struct LocalCardStatModifierMessage : NetworkMessage
    {
        public NetworkIdentity playerNetId;
        public bool isOpponentCard;
        public int cardInstanceId;
        public int statId;
        public int value;
        public bool adding;
        public int duration;
    }

    public struct OpponentCardStatModifierMessage : NetworkMessage
    {
        public NetworkIdentity playerNetId;
        public bool isOpponentCard;
        public int cardInstanceId;
        public int statId;
        public int value;
        public bool adding;
        public int duration;
    }

    public struct LocalAddLeaderDefenseMessage : NetworkMessage
    {
        public NetworkIdentity playerNetId;
        public NetworkIdentity targetPlayer;
        public int amount;
    }

    public struct OpponentAddLeaderDefenseMessage : NetworkMessage
    {
        public NetworkIdentity playerNetId;
        public NetworkIdentity targetPlayer;
        public int amount;
    }

    #endregion

    // ------------------------------

    #region Keywords

    public struct LocalApplyKeywordMessage : NetworkMessage
    {
        public NetworkIdentity playerNetId;
        public bool isOpponentCard;
        public int cardInstanceId;
        public int keywordType;
        public int keywordValue;
        public bool adding;
    }

    public struct OpponentApplyKeywordMessage : NetworkMessage
    {
        public NetworkIdentity playerNetId;
        public bool isOpponentCard;
        public int cardInstanceId;
        public int keywordType;
        public int keywordValue;
        public bool adding;
    }

    #endregion

    // ------------------------------

    #region Effect Costs

    public struct LocalPayEffectCostMessage : NetworkMessage
    {
        public NetworkIdentity playerNetId;
        public int cardInstanceId;
        public string originZone;
        public string abilityName;
        public MoveCardToZoneData[] cardsMoveToZoneData;
        public RemoveCounterData[] countersToRemove;
    }

    public struct OpponentPayEffectCostMessage : NetworkMessage
    {
        public NetworkIdentity playerNetId;
        public int cardInstanceId;
        public string originZone;
        public string abilityName;
        public MoveCardToZoneData[] cardsMoveToZoneData;
        public RemoveCounterData[] countersToRemove;
    }

    public readonly struct MoveCardToZoneData
    {
        public readonly int cardInstanceId;
        public readonly string startZone;
        public readonly string endZone;

        public MoveCardToZoneData(int cardInstanceId, string startZone, string endZone)
        {
            this.cardInstanceId = cardInstanceId;
            this.startZone = startZone;
            this.endZone = endZone;
        }
    }

    public readonly struct RemoveCounterData
    {
        public readonly int cardInstanceId;
        public readonly string cardZone;
        public readonly int keywordType;
        public readonly int keywordValue;
        public readonly int amount;

        public RemoveCounterData(int cardInstanceId, string cardZone, int keywordType, int keywordValue, int amount)
        {
            this.cardInstanceId = cardInstanceId;
            this.cardZone = cardZone;
            this.keywordType = keywordType;
            this.keywordValue = keywordValue;
            this.amount = amount;
        }
    }

    #endregion

    // ------------------------------

    #region Other

    public struct LocalTellOpponentPerformEffectMessage : NetworkMessage
    {
        public NetworkIdentity playerNetId;
        public int libraryCardId;
        public int cardInstanceId;
        public string cardZone;
        public string effectName;
        public int[] targetInstanceIds;
    }

    public struct OpponentTellOpponentPerformEffectMessage : NetworkMessage
    {
        public NetworkIdentity playerNetId;
        public int libraryCardId;
        public int cardInstanceId;
        public string cardZone;
        public string effectName;
        public int[] targetInstanceIds;
    }

    #endregion
}
