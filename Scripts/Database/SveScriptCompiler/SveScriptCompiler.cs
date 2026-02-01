using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sparkfire.Utility;
using UnityEngine;
using static SVESimulator.SveScript.SveScriptData;
using static SVESimulator.SveScript.SveScriptParseCardInfo;
using static SVESimulator.SveScript.SveScriptKeywordCompiler;
using static SVESimulator.SveScript.SveScriptAbilityCompiler;

namespace SVESimulator.SveScript
{
    public static partial class SveScriptCompiler
    {
        public static void ParseSveScriptFile(in string fileName, in string outputPath)
        {
            if(!File.Exists(fileName) || string.IsNullOrWhiteSpace(outputPath))
                return;
            ParseSveScriptCardSet(File.ReadAllText(fileName), outputPath);
        }

        public static void ParseSveScriptCardSet(in string text, in string outputPath) => ParseSveScriptCardSet(text, outputPath, out _);
        public static void ParseSveScriptCardSet(in string text, in string outputPath, out List<string> outputFilePaths)
        {
            outputFilePaths = new List<string>();
            if(string.IsNullOrWhiteSpace(outputPath))
                return;

            int pointer = 0;
            while(pointer < text.Length)
            {
                string cardJson = ParseSveScriptCard(text[pointer..], out int parseLength, out string cardID);
                pointer += parseLength;

                string folderName = cardID.Split("-")?[0];
                string fullFolderPath = !string.IsNullOrWhiteSpace(folderName) ? Path.Combine(outputPath, folderName) : outputPath;
                string outputFile = Path.Combine(fullFolderPath, $"{cardID}.json");
                outputFilePaths.Add(outputFile);

                if(!Directory.Exists(fullFolderPath))
                    Directory.CreateDirectory(fullFolderPath);
                File.WriteAllText(outputFile, cardJson);
            }
        }

        public static string ParseSveScriptCard(in string text, out int parseLength, out string cardID, bool breakOnNewName = true)
        {
            SveScriptData.CardInfo cardInfo = new();
            int pointer = 0;
            while(pointer < text.Length)
            {
                if(text[pointer] == ' ' || text[pointer] == '\n' || text[pointer] == '\t' || text[pointer] == ';')
                {
                    pointer++;
                    continue;
                }

                string trimmedText = text[pointer..];
                string scriptKeyword = trimmedText.Split()[0].ToLower();
                int nextIndex = pointer;
                switch(scriptKeyword.Trim())
                {
                    case "name":
                        if(cardInfo.name != null && breakOnNewName)
                            goto exit;
                        nextIndex = text.IndexOf(';', pointer);
                        if(nextIndex <= pointer)
                            throw new Exception();
                        cardInfo.name = text[(pointer + scriptKeyword.Length)..nextIndex].Trim();
                        break;

                    case "id":
                        nextIndex = text.IndexOf(';', pointer);
                        if(nextIndex <= pointer)
                            throw new Exception();
                        cardInfo.cardID = text[(pointer + scriptKeyword.Length)..nextIndex].Trim();
                        break;

                    // ------------------------------

                    case "class":
                        nextIndex = text.IndexOf(';', pointer);
                        if(nextIndex <= pointer)
                            throw new Exception();
                        cardInfo.cardClass = ParseClass(text[(pointer + scriptKeyword.Length)..nextIndex].Trim());
                        break;

                    case "universe":
                        nextIndex = text.IndexOf(';', pointer);
                        if(nextIndex <= pointer)
                            throw new Exception();
                        cardInfo.cardClass = ParseUniverse(text[(pointer + scriptKeyword.Length)..nextIndex].Trim());
                        break;

                    case "type":
                        nextIndex = text.IndexOf(';', pointer);
                        if(nextIndex <= pointer)
                            throw new Exception();
                        ParseAndSaveCardType(text[(pointer + scriptKeyword.Length)..nextIndex].Trim(), ref cardInfo);
                        break;

                    case "trait":
                        nextIndex = text.IndexOf(';', pointer);
                        if(nextIndex <= pointer)
                            throw new Exception();
                        string trait = text[(pointer + scriptKeyword.Length)..nextIndex].Trim();
                        cardInfo.trait = cardInfo.trait.IsNullOrWhiteSpace() ? trait : $"{trait} / {cardInfo.trait}";
                        break;

                    case "rarity":
                        nextIndex = text.IndexOf(';', pointer);
                        if(nextIndex <= pointer)
                            throw new Exception();
                        cardInfo.rarity = ParseRarity(text[(pointer + scriptKeyword.Length)..nextIndex].Trim());
                        break;

                    case "text":
                        nextIndex = text.IndexOf(';', pointer);
                        if(nextIndex <= pointer)
                            throw new Exception();
                        ParseAndSaveCardText(text[(pointer + scriptKeyword.Length)..nextIndex].Trim(), ref cardInfo);
                        break;

                    // ------------------------------

                    case "cost":
                        nextIndex = text.IndexOf(';', pointer);
                        if(nextIndex <= pointer)
                            throw new Exception();
                        cardInfo.cost = int.Parse(text[(pointer + scriptKeyword.Length)..nextIndex].Trim());
                        break;

                    case "stat":
                    case "stats":
                        nextIndex = text.IndexOf(';', pointer);
                        if(nextIndex <= pointer)
                            throw new Exception();
                        ParseAndSaveAtkDef(text[(pointer + scriptKeyword.Length)..nextIndex].Trim(), ref cardInfo);
                        break;

                    case "evolve":
                        nextIndex = text.IndexOf(';', pointer);
                        if(nextIndex <= pointer)
                            throw new Exception();
                        cardInfo.evolveCost = int.Parse(text[(pointer + scriptKeyword.Length)..nextIndex].Trim());
                        break;

                    // ------------------------------

                    case "keyword":
                    case "keywords":
                        nextIndex = text.IndexOf(';', pointer);
                        if(nextIndex <= pointer)
                            throw new Exception();
                        ParseAndAddKeywords(text[(text.IndexOf(' ', pointer) + 1)..nextIndex], ref cardInfo);
                        break;

                    case "ability":
                        nextIndex = text.IndexOf('}', pointer);
                        if(nextIndex <= pointer)
                            throw new Exception();
                        ParseAndAddAbility(text[(pointer + 7)..nextIndex], ref cardInfo); // 7 = length of "ability"
                        break;

                    default:
                        nextIndex = text.IndexOf('\n', pointer + 1);
                        pointer = nextIndex != -1 ? nextIndex : text.Length;
                        continue;
                }
                pointer = nextIndex + 1; // +1 to move past ';' or '}'
            }

            exit:
            parseLength = pointer;
            cardID = cardInfo.cardID;
            CompileCardProperties(ref cardInfo);
            CompileCardStats(ref cardInfo);
            return JObject.FromObject(cardInfo).ToString(Formatting.Indented);
        }

        // ------------------------------

        public static void GenerateAndSaveFullCardLibrary(in FilePathInfo pathInfo, string libraryOutputFilePath)
        {
            // Compile individual sets
            string[] setCodes = Directory.GetDirectories(pathInfo.CardDataPath)
                .Select(x => x.Split("\\")[^1]).ToArray();
            foreach(string setCode in setCodes)
            {
                // Set up individual set output
                string setOutputFile = Path.Combine(pathInfo.CardDataPath, $"{setCode}-FullSet.json");
                if(File.Exists(setOutputFile))
                    File.Delete(setOutputFile);
                Directory.CreateDirectory(pathInfo.CardDataPath);
                using(FileStream _ = File.Create(setOutputFile)) { }

                // Iterate through file list
                using(StreamWriter writer = new(setOutputFile))
                {
                    string[] cardFiles = Directory.GetFiles(Path.Combine(pathInfo.CardDataPath, setCode), "*.json");
                    writer.Write($"{{\n" +
                        $"\"name\": \"{SetNames[setCode]}\",\n" +
                        $"\"cards\": [\n");
                    for(int i = 0; i < cardFiles.Length; i++)
                    {
                        writer.Write(File.ReadAllText(cardFiles[i]));
                        if(i < cardFiles.Length - 1)
                            writer.Write(",\n");
                    }
                    writer.Write("\n]\n}");
                }
            }

            // Compile full library
            string[] setFiles = Directory.GetFiles(pathInfo.CardDataPath, "*.json", SearchOption.TopDirectoryOnly);
            using(StreamWriter writer = new(libraryOutputFilePath))
            {
                using(JsonTextWriter jWriter = new(writer))
                {
                    jWriter.Formatting = Formatting.Indented;
                    jWriter.Indentation = 2;

                    JArray setJsons = new();
                    for(int i = 0; i < setFiles.Length; i++)
                        setJsons.Add(JToken.Parse(File.ReadAllText(setFiles[i])));

                    JsonSerializer serializer = new();
                    serializer.Serialize(jWriter, setJsons);
                }
            }
            Debug.Log("Successfully compiled SVE scripts");
        }
    }
}
