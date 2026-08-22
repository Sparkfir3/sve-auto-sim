using System.Collections.Generic;
using UnityEngine;

namespace SVESimulator
{
    public partial class ComplexEffect : SveEffect
    {
        private bool BreakCondition => !player || !Application.isPlaying;

        private string ReplaceWithVariableValues(string line)
        {
            foreach(var kvPair in variables)
            {
                if(kvPair.Value is not CE_Value ceValue)
                    continue;
                (string variable, string value) = (kvPair.Key, ceValue?.value);
                line = line.Replace(variable, value);
            }
            return line;
        }
    }
}
