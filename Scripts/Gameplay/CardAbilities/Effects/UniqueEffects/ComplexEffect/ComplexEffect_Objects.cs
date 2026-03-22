using System.Threading.Tasks;
using CCGKit;

namespace SVESimulator
{
    public partial class ComplexEffect
    {
        private abstract class CE_Object
        {
            public abstract Task<CE_Object> GetValue(PlayerController player, string token, string[] parameters);
        }

        private class CE_Value : CE_Object
        {
            public string value;

            public override Task<CE_Object> GetValue(PlayerController player, string token, string[] parameters) => Task.FromResult<CE_Object>(this);
        }

        private class CE_Card : CE_Object
        {
            public RuntimeCard card;

            public override Task<CE_Object> GetValue(PlayerController player, string token, string[] parameters)
            {
                switch(token)
                {
                    case "getValue":
                        return Task.FromResult<CE_Object>(new CE_Value()
                        {
                            value = parameters.Length > 0 ? SVEFormulaParser.ParseValue(parameters[0], player, card).ToString() : ""
                        });
                    default:
                        return Task.FromResult<CE_Object>(null);
                }
            }
        }
    }
}
