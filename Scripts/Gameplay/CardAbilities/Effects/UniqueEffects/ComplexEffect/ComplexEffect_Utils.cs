using System.Collections.Generic;
using UnityEngine;

namespace SVESimulator
{
    public partial class ComplexEffect : SveEffect
    {
        private bool BreakCondition => !player || !Application.isPlaying;

        private string ReplaceWithVariableValues(string line, Dictionary<string, string> variables)
        {
            foreach(var kvPair in variables)
            {
                (string variable, string value) = (kvPair.Key, kvPair.Value);
                line = line.Replace(variable, value);
            }
            return line;
        }
    }
}
