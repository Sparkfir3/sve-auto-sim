using System.Text;
using Random = UnityEngine.Random;

namespace Sparkfire.Utility
{
    public static class GeneralUtility
    {
        public static string RandomAlphaString(int length)
        {
            string output = "";
            for(int i = 0; i < length; i++)
            {
                output += (char)(Random.Range(65, 91) + (Random.Range(0f, 1f) > 0.5f ? 32 : 0));
            }
            return output;
        }
    }
}
