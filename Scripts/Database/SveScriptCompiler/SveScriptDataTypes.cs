using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static SVESimulator.SveScript.SveScriptKeywordCompiler;

namespace SVESimulator.SveScript
{
    internal static partial class SveScriptData
    {
        internal class CardInfo
        {
            [JsonProperty(PropertyName = "cardTypeId")]
            public int ccgCardTypeId;
            public string name = null;

            public List<string> costs = new(); // blank array, unused CCG Kit data
            public List<CardProperty> properties = new();
            public List<CardStat> stats = new();
            public List<Keyword> keywords = new();
            public List<Ability> abilities = new();

            [JsonProperty(PropertyName = "id")]
            public int ccgID = -1;

            // ------------------------------

            // Card Properties
            [JsonIgnore] public string cardClass;
            [JsonIgnore] public string universe;
            [JsonIgnore] public string trait;
            [JsonIgnore] public string rarity;
            [JsonIgnore] public string text;
            [JsonIgnore] public string cardID;

            // Card Stats
            [JsonIgnore] public int attack = -1;
            [JsonIgnore] public int defense = -1;
            [JsonIgnore] public int evolveCost = -1;
            [JsonIgnore] public int cost = -1;
        }

        // ------------------------------

        internal abstract class CardProperty
        {
            [JsonProperty(Order = 1)]
            public string name;
            [JsonProperty(Order = 2, PropertyName = "$type")]
            public abstract string type { get; }
        }

        internal class CardPropertyInt : CardProperty
        {
            [JsonProperty(Order = 0)]
            public int value;
            public override string type => "CCGKit.IntProperty";

            public CardPropertyInt(int value, string name)
            {
                this.value = value;
                this.name = name;
            }
        }

        internal class CardPropertyString : CardProperty
        {
            [JsonProperty(Order = 0)]
            public string value;
            public override string type => "CCGKit.StringProperty";

            public CardPropertyString(string value, string name)
            {
                this.value = value;
                this.name = name;
            }
        }

        // ------------------------------

        internal class CardStat
        {
            public int baseValue;
            public int statId;
            public string name;
            public int originalValue;
            public int minValue;
            public int maxValue;
            public List<int> modifiers = new(); // blank array, unused CCG Kit data

            public CardStat(int value, int statId, string name, int min, int max)
            {
                baseValue = value;
                this.statId = statId;
                this.name = name;
                originalValue = value;
                minValue = min;
                maxValue = max;
            }
        }

        // ------------------------------

        internal abstract class Ability
        {
            [JsonProperty(Order = 11, PropertyName = "type")]
            public abstract string abilityType { get; }
            [JsonProperty(Order = 14, PropertyName = "$type")]
            public abstract string ccgType { get; }

            [JsonProperty(Order = 10)]
            public string name;
            [JsonProperty(Order = 12)]
            public JObject effect;
            [JsonProperty(Order = 13)]
            public object target => null;
        }

        internal class TriggeredAbility : Ability
        {
            public override string abilityType => "Triggered";
            public override string ccgType => "CCGKit.TriggeredAbility";

            [JsonProperty(Order = 0)]
            public JObject trigger;
        }

        internal class PassiveAbility : TriggeredAbility { }

        internal class ActivatedAbility : Ability
        {
            public override string abilityType => "Activated";
            public override string ccgType => "CCGKit.ActivatedAbility";

            [JsonProperty(Order = 0)]
            public int zoneId = 0;
            [JsonProperty(Order = 1)]
            public List<JObject> costs;
        }

        // ------------------------------

        internal class Deck
        {
            public string name;
            public List<DeckCard> cards = new();
        }

        internal class DeckCard
        {
            public int id;
            public int amount;

            public DeckCard(int id, int amount)
            {
                this.id = id;
                this.amount = amount;
            }
        }
    }
}
