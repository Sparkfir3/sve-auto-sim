using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CCGKit;
using UnityEngine;

namespace SVESimulator
{
    public partial class ComplexEffect
    {
        #region Base/Standard

        private abstract class CE_Object
        {
            public abstract Task<CE_Object> GetValue(PlayerController player, string token, string[] parameters);
        }

        // -----

        private class CE_Value : CE_Object
        {
            public string value;

            public CE_Value(string value)
            {
                this.value = value;
            }

            public override Task<CE_Object> GetValue(PlayerController player, string token, string[] parameters) => Task.FromResult<CE_Object>(this);
        }

        #endregion

        // ------------------------------

        #region Cards

        private class CE_Card : CE_Object
        {
            public RuntimeCard card;

            public CE_Card(RuntimeCard card)
            {
                this.card = card;
            }

            public CE_Card(PlayerController player, int instanceId, string zone)
            {
                card = player.GetPlayerInfo().namedZones[zone].cards.FirstOrDefault(x => x.instanceId == instanceId);
                Debug.Assert(card != null);
            }

            public override Task<CE_Object> GetValue(PlayerController player, string token, string[] parameters)
            {
                switch(token)
                {
                    case "getValue":
                        return Task.FromResult<CE_Object>(new CE_Value(parameters.Length > 0 ? SVEFormulaParser.ParseValue(parameters[0], player, card).ToString() : ""));
                    default:
                        return Task.FromResult<CE_Object>(null);
                }
            }
        }

        private class CE_CardList : CE_Object
        {
            public List<RuntimeCard> cardList;

            public override Task<CE_Object> GetValue(PlayerController player, string token, string[] parameters)
            {
                switch(token)
                {
                    case "filterCount":
                        if(parameters.Length == 0)
                            return Task.FromResult<CE_Object>(new CE_Value(cardList.Count.ToString()));
                        var filter = SVEFormulaParser.ParseCardFilterFormula(parameters[0]);
                        return Task.FromResult<CE_Object>(new CE_Value(cardList.Count(x => filter.MatchesCard(x)).ToString()));
                    default:
                        return Task.FromResult<CE_Object>(null);
                }
            }
        }

        #endregion

        // ------------------------------

        #region Other

        private class CE_EffectCost : CE_Object
        {
            public List<MoveCardToZoneData> movedCardsData;
            public List<RemoveCounterData> removedCountersData;

            public override Task<CE_Object> GetValue(PlayerController player, string token, string[] parameters)
            {
                switch(token)
                {
                    case "movedCardsCount":
                        return Task.FromResult<CE_Object>(new CE_Value(movedCardsData.Count.ToString()));
                    case "removedCountersCount":
                        return Task.FromResult<CE_Object>(new CE_Value(removedCountersData.Sum(x => x.amount).ToString()));
                    default:
                        return Task.FromResult<CE_Object>(null);
                }
            }
        }

        #endregion
    }
}
