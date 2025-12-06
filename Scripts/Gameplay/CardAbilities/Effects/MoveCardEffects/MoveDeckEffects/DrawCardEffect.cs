using System;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public class SveDrawCardEffect : SveEffect
    {
        [EnumField("Target", width = 200), Order(1)]
        public SVEProperties.SVEEffectTarget target = SVEProperties.SVEEffectTarget.Self;

        [StringField("Amount", width = 100), Order(2)]
        public string amount = "1";

        // ------------------------------

        public override void Resolve(PlayerController player, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, Action onComplete = null)
        {
            bool selfDraw = false, oppDraw = false;
            switch(target)
            {
                case SVEProperties.SVEEffectTarget.Self:
                case SVEProperties.SVEEffectTarget.Leader:
                    selfDraw = true;
                    break;
                case SVEProperties.SVEEffectTarget.Opponent:
                case SVEProperties.SVEEffectTarget.OpponentLeader:
                    oppDraw = true;
                    break;
                case SVEProperties.SVEEffectTarget.AllPlayers:
                case SVEProperties.SVEEffectTarget.AllLeaders:
                    selfDraw = true;
                    oppDraw = true;
                    break;
                default:
                    Debug.LogError($"Invalid effect target mode {target} for DrawCardEffect");
                    break;
            }

            int drawCount = SVEFormulaParser.ParseValue(amount, player);
            if(selfDraw)
                for(int i = 0; i < drawCount; i++)
                    player.LocalEvents.DrawCard();
            if(oppDraw)
                player.LocalEvents.TellOpponentDrawCard(drawCount);
            onComplete?.Invoke();
        }
    }
}
