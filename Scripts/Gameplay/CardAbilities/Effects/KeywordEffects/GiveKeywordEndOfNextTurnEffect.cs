using System;
using System.Collections.Generic;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public class GiveKeywordEndOfNextTurnEffect : GiveKeywordEndOfTurnEffect
    {
        protected override SVEProperties.PassiveDuration duration => SVEProperties.PassiveDuration.EndOfNextTurn;
    }
}
