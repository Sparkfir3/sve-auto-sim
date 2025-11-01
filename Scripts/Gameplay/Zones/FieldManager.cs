using UnityEngine;
using Sirenix.OdinInspector;
using Sparkfire.Utility;

namespace SVESimulator
{
    public class FieldManager : Singleton<FieldManager>
    {
        [TitleGroup("Player"), SerializeField]
        private PlayerCardZoneController playerZoneController;
        [SerializeField]
        private PlayPointMeter playerPlayPointMeter;
        [SerializeField]
        private LeaderHealthDisplay playerLeaderHealth;

        [TitleGroup("Opponent"), SerializeField]
        private PlayerCardZoneController opponentZoneController;
        [SerializeField]
        private PlayPointMeter opponentPlayPointMeter;
        [SerializeField]
        private LeaderHealthDisplay opponentLeaderHealth;

        // ------------------------------

        public static PlayerCardZoneController PlayerZones => Instance.playerZoneController;
        public static PlayPointMeter PlayerPlayPoints => Instance.playerPlayPointMeter;
        public static LeaderHealthDisplay PlayerLeaderHealth => Instance.playerLeaderHealth;

        public static PlayerCardZoneController OpponentZones => Instance.opponentZoneController;
        public static PlayPointMeter OpponentPlayPoints => Instance.opponentPlayPointMeter;
        public static LeaderHealthDisplay OpponentLeaderHealth => Instance.opponentLeaderHealth;

    }
}
