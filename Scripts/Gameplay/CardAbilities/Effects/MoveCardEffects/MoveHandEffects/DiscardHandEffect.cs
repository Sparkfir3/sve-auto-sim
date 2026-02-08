using System;
using System.Collections.Generic;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public class DiscardHandEffect : SveEffect
    {
        [EnumField("Target", width = 200), Order(1)]
        public SVEProperties.SVEEffectTarget target = SVEProperties.SVEEffectTarget.Self;

        // ------------------------------

        public override void Resolve(PlayerController player, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, Action onComplete = null)
        {
            bool selfTarget = false, oppTarget = false;
            switch(target)
            {
                case SVEProperties.SVEEffectTarget.Self:
                case SVEProperties.SVEEffectTarget.Leader:
                    selfTarget = true;
                    break;
                case SVEProperties.SVEEffectTarget.Opponent:
                case SVEProperties.SVEEffectTarget.OpponentLeader:
                    oppTarget = true;
                    break;
                case SVEProperties.SVEEffectTarget.AllPlayers:
                case SVEProperties.SVEEffectTarget.AllLeaders:
                    selfTarget = true;
                    oppTarget = true;
                    break;
                default:
                    Debug.LogError($"Invalid effect target mode {target} for DiscardHandEffect");
                    break;
            }

            if(selfTarget)
            {
                List<CardObject> cards = new(player.ZoneController.handZone.AllCards);
                foreach(CardObject card in cards)
                {
                    player.LocalEvents.SendToCemetery(card, SVEProperties.Zones.Hand);
                }
            }
            if(oppTarget)
            {
                Debug.LogError($"DiscardHandEffect currently does not support targeting the opponent player (Attempted target mode: {target}).");
            }
            onComplete?.Invoke();
        }
    }
}
