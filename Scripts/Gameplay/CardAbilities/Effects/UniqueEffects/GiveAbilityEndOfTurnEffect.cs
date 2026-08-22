using System;
using System.Collections.Generic;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public class GiveAbilityEndOfTurnEffect : GiveAbilityEffect
    {
        protected override  SVEProperties.PassiveDuration duration => SVEProperties.PassiveDuration.EndOfTurn;
    }
}
