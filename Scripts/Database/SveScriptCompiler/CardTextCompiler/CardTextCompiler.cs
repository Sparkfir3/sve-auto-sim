using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sparkfire.Utility;
using SVESimulator.SveScript;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SVESimulator.CardTextData
{
    public static class CardTextCompiler
    {
        private const string RESOURCE_PATH_INFO = "FilePathInfo";

        public static void CompileTextJson(TextAsset baseScript, TextAsset outputFile)
        {
#if !UNITY_EDITOR
            Debug.LogError("Card text compiler to JSON is not supported outside of the Unity editor!");
            return;
#else
            if(!baseScript || !outputFile)
            {
                Debug.Log("Base SVE script or output file was not provided, cannot parse text into JSON");
                return;
            }

            SveScriptCompiler.ParseSveScriptCardSet(baseScript.text, Resources.Load<FilePathInfo>(RESOURCE_PATH_INFO).CardDataPath, out List<string> outputFilePaths);
            List<TextData> oldTextData = JsonConvert.DeserializeObject<List<TextData>>(outputFile.text);
            List<TextData> newTextData = new();

            // Parse text files
            foreach(string file in outputFilePaths)
            {
                // Read basic data
                JObject cardData = JObject.Parse(File.ReadAllText(file));
                JArray properties = cardData["properties"] as JArray;
                if(properties == null)
                    continue;
                string id = properties.FirstOrDefault(x => x.Value<string>("name").Equals("ID"))?["value"]?.ToString();
                string name = cardData["name"]?.ToString();
                string cardText = properties.FirstOrDefault(x => x.Value<string>("name").Equals("Text"))?["value"]?.ToString();
                if(id.IsNullOrWhiteSpace() || name.IsNullOrWhiteSpace())
                    continue;

                id += "EN";
                TextData oldData = oldTextData.FirstOrDefault(x => x.id.Equals(id));
                TextData newData = new()
                {
                    id = id,
                    name = name,
                    cardText = cardText
                };

                // Read effect list
                List<EffectText> effectList = new();
                JToken[] abilities = cardData["abilities"]?.ToArray() ?? new JToken[0];
                foreach(JToken token in abilities)
                {
                    string key = token["name"]?.ToString();
                    EffectText oldEffect = oldData?.effectText?.FirstOrDefault(x => x.key.Equals(key));
                    if(oldData != null && oldEffect == null) // if card data already exists but the effect was removed, do not re-add it
                        continue;

                    EffectText newEffect = new()
                    {
                        key = key,
                        name = oldEffect?.name,
                        text = oldEffect != null ? oldEffect.text : "",
                        trigger = oldEffect != null ? oldEffect.trigger : GetDefaultTriggerText(key),
                        cost = oldEffect != null ? oldEffect.cost : "",
                        body = oldEffect?.body ?? (oldEffect?.text == null ? "" : null)
                    };
                    effectList.Add(newEffect);
                }
                if(effectList.Count > 0)
                    newData.effectText = effectList.ToArray();

                newTextData.Add(newData);
            }

            // Write to output
            string outputFilePath = Path.GetFullPath(Path.Join(Application.dataPath.Replace("/Assets", ""), AssetDatabase.GetAssetPath(outputFile)));
            using(StreamWriter writer = new(outputFilePath))
            {
                using(JsonTextWriter jWriter = new(writer))
                {
                    jWriter.Formatting = Formatting.Indented;
                    jWriter.Indentation = 2;

                    JsonSerializer serializer = new();
                    serializer.Serialize(jWriter, newTextData);
                }
            }
#endif
        }

        private static string GetDefaultTriggerText(in string key)
        {
            return key switch
            {
                "Fanfare" => "[fanfare]",
                "Last Words" => "[lastwords]",
                "OnEvolve" => "On Evolve",
                "Strike" => "Strike",
                "FollowerStrike" => "Follower Strike",
                "LeaderStrike" => "Leader Strike",
                "Act" => "[act]",
                _ => ""
            };
        }
    }
}
