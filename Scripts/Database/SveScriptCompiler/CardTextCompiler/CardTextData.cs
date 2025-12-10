using Newtonsoft.Json;

namespace SVESimulator.CardTextData
{
    public class CardTextData
    {
        public string id;
        public string name;
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public string cardText;
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public EffectText[] effectText;
    }

    public class EffectText
    {
        public string key;
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public string trigger;
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public string cost;
        public string body;
    }
}
