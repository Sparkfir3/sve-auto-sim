using System;
using UnityEngine;
using CCGKit;

namespace SVESimulator
{
    public class EvolveNotOncePerTurnEffect : EvolveEffect
    {
        public override bool UseEvolveForTurn => false;
    }
}
