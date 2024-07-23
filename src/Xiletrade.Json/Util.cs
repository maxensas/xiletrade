using XiletradeJson.Models;
using System.Text;
using CsvHelper.Configuration;
using System.Globalization;
using CsvHelper;
using System.Data;
using System.Collections.Generic;

namespace XiletradeJson
{
    internal static class Util
    {
        internal static Bases? BasesOrigin { get; set; }
        internal static Bases? BasesEn { get; set; }
        internal static Mods? ModsEn { get; set; }
        internal static Monsters? MonstersEn { get; set; }

        // not used atm, progam run once.
        internal static void ReInitEnglishData() 
        {
            BasesEn = null;
            ModsEn = null;
            MonstersEn = null;
        }

        // Method that create what Xiletrade needs: smallest possible json files. Refactor needed.
        internal static string? CreateJson(string csvRawData, string datName, string jsonPath)
        {
            try
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true
                };
                
                var reader = new StringReader(csvRawData);
                var csv = new CsvReader(reader, config);

                bool isBases = datName == Strings.BaseItemTypes;
                bool isMods = datName == Strings.Mods;
                bool isMonsters = datName == Strings.MonsterVarieties;
                bool isWords = datName == Strings.Words;
                bool isGems = datName == Strings.Gems;

                KeyValuePair<int, string>[]? arrayIndex = (isBases ? Strings.BasesIndex
                            : isMods ? Strings.ModsIndex
                            : isMonsters ? Strings.MonstersIndex
                            : isWords ? Strings.WordsIndex
                            : isGems ? Strings.GemsIndex: null) ?? throw new Exception("Header not found for DAT : " + datName);
                
                List<ResultData> listResultData = new();
                List<WordResultData> listWordResultData = new();
                List<GemResultData> listGemResultData = new();

                // PARSING CSV TO JSON
                string[]? header = null;
                while (csv.Read())
                {
                    if(header is null)
                    {
                        csv.ReadHeader();
                        header = csv.HeaderRecord;
                        foreach (var idx in arrayIndex)
                        {
                            if (header?[idx.Key] != idx.Value)
                            {
                                throw new Exception($"Index not found in the header : {header?[idx.Key]} does not equal {idx.Value}");
                            }
                        }
                        continue;
                    }

                    if (isBases)
                    {
                        ResultData d = new()
                        {
                            ID = csv.GetField(0)?.Replace(Strings.Parser.MetaItem.Key, Strings.Parser.MetaItem.Value),
                            Name = csv.GetField(4)?.Replace(Strings.Parser.HexSpaceSring.Key, Strings.Parser.HexSpaceSring.Value), // 0xa0 => 0x20;
                            InheritsFrom = csv.GetField(5)?.Replace(Strings.Parser.MetaItem.Key, Strings.Parser.MetaItem.Value)
                        };

                        if (BasesEn is not null)
                        {
                            var resultDat = BasesEn.Result?[0].Data?.FirstOrDefault(x => x.ID == d.ID);
                            if (resultDat is null)
                            {
                                continue;
                            }
                            d.NameEn = resultDat.Name;
                        }
                        else
                        {
                            d.NameEn = d.Name;
                        }

                        bool continueLoop = false;

                        if (d.Name!.Contains(Strings.Parser.NameBaseUnwanted, StringComparison.Ordinal))
                        {
                            continueLoop = true;
                        }
                        if (d.InheritsFrom == Strings.Parser.StackableCurrency && 
                            !d.ID!.Contains(Strings.Parser.IncursionVial, StringComparison.Ordinal))
                        {
                            continue;
                        }
                        foreach (var str in Strings.Parser.InheritsBaseUnwanted)
                        {
                            if (d.InheritsFrom == str)
                            {
                                continueLoop = true;
                                break;
                            }
                        }
                        if (continueLoop) continue;

                        if (listResultData.FirstOrDefault(x => x.Name == d.Name) == null) listResultData.Add(d);
                    }
                    if (isMods)
                    {
                        if (csv.GetField(9)?.Length == 0)
                        {
                            continue;
                        }

                        ResultData d = new()
                        {
                            ID = csv.GetField(0),
                            InheritsFrom = Strings.Parser.ModsInherits,
                            Name = ParseMultipleName(csv.GetField(9))
                        };
                        
                        bool checkId = false;
                        foreach (var id in Strings.Parser.IdModsUnwanted)
                        {
                            if (d.ID?.IndexOf(id, StringComparison.Ordinal) == 0)
                            {
                                checkId = true;
                                break;
                            }
                        }
                        if (checkId)
                        {
                            continue;
                        }

                        if (ModsEn is not null)
                        {
                            var resultDat = ModsEn.Result?[0].Data?.FirstOrDefault(x => x.ID == d.ID);
                            if (resultDat is null)
                            {
                                continue;
                            }
                            d.NameEn = resultDat.Name;
                        }
                        else
                        {
                            d.NameEn = d.Name;
                        }

                        if (listResultData.FirstOrDefault(x => x.Name == d.Name) == null) listResultData.Add(d);
                    }
                    if (isMonsters)
                    {
                        ResultData d = new()
                        {
                            ID = csv.GetField(0)?.Replace(Strings.Parser.MetaMonster.Key, Strings.Parser.MetaMonster.Value),
                            Name = csv.GetField(32),
                            InheritsFrom = csv.GetField(8)?.Replace(Strings.Parser.MetaMonster.Key, Strings.Parser.MetaMonster.Value)
                        };

                        if (MonstersEn is not null)
                        {
                            var resultDat = MonstersEn.Result?[0].Data?.FirstOrDefault(x => x.ID == d.ID);
                            if (resultDat is null)
                            {
                                continue;
                            }
                            d.NameEn = resultDat.Name;
                        }
                        else
                        {
                            d.NameEn = d.Name;
                        }
                        if (listResultData.FirstOrDefault(x => x.Name == d.Name) == null) listResultData.Add(d);
                    }
                    if (isWords)
                    {
                        WordResultData d = new()
                        {
                            NameEn = csv.GetField(1)?.Trim(),
                            Name = ParseMultipleName(csv.GetField(5))
                        };

                        if (listWordResultData.FirstOrDefault(x => x.Name == d.Name) == null) listWordResultData.Add(d);
                    }
                    if (isGems)
                    {
                        GemResultData d = new()
                        {
                            Id = csv.GetField(0)?.Trim(),
                            Name = csv.GetField(1)?.Trim(),
                            Type = string.Empty,
                            Disc = string.Empty
                        };

                        if (d.Id is null || d.Name!.Length == 0 || d.Name!.Contains(Strings.Parser.NameBaseUnwanted, StringComparison.Ordinal))
                        {
                            continue;
                        }

                        string delimiter = "Alt";
                        d.Disc = d.Id[d.Id.LastIndexOf(delimiter)..].ToLowerInvariant().Insert(delimiter.Length, "_");
                        var shortId = d.Id[..(d.Id.LastIndexOf(delimiter))];
                        if (shortId?.Length > 0)
                        {
                            var resultDat = BasesOrigin?.Result?[0].Data?.FirstOrDefault(x => x.ID.EndsWith(shortId));
                            d.Type = resultDat?.Name;
                        }

                        if (listGemResultData.FirstOrDefault(x => x.Name == d.Name) == null) listGemResultData.Add(d);
                    }
                }
                // END OF PARSING
                if (listWordResultData.Count > 0)
                {
                    return WriteJson(datName, jsonPath, listWordResultData);
                }
                if (listGemResultData.Count > 0)
                {
                    return WriteJson(datName, jsonPath, listGemResultData);
                }
                return WriteJson(datName, jsonPath, listResultData);
            }
            catch (Exception e)
            {
                throw;
                //string mess = e.Message;
            }
        }

        internal static string? ParseMultipleName(string? str) // Grammatical Rules
        {
            if (str!.StartsWith(Strings.Parser.NameRules[0].Key, StringComparison.Ordinal))
            {
                StringBuilder sb = new(str);
                foreach (var kvp in Strings.Parser.NameRules)
                {
                    sb.Replace(kvp.Key, kvp.Value);
                }
                var tmpList = sb.ToString().Split('/', StringSplitOptions.TrimEntries).Distinct();
                return String.Join('/', tmpList);
            }
            return str.Trim();
        }

        internal static string? WriteJson(string datName, string jsonPath, List<ResultData> listResultData)
        {
            string? outputJson = null;

            if (listResultData.Count == 0)
            {
                return null;
            }

            if (datName == Strings.BaseItemTypes)
            {
                Bases bases = new();
                bases.Result = new Result[1];
                bases.Result[0] = new();
                bases.Result[0].Data = new ResultData[listResultData.Count];
                bases.Result[0].Data = listResultData.ToArray();

                BasesOrigin = bases;
                if (BasesEn is null)
                {
                    BasesEn = bases;
                }

                outputJson = jsonPath + "Bases.json";
                using (StreamWriter writer = new(outputJson, false, Encoding.UTF8))
                {
                    writer.Write(Json.Serialize<Bases>(bases));
                }
            }
            if (datName == Strings.Mods)
            {
                Mods mods = new();
                mods.Result = new Result[1];
                mods.Result[0] = new();
                mods.Result[0].Data = new ResultData[listResultData.Count];
                mods.Result[0].Data = listResultData.ToArray();

                if (ModsEn is null)
                {
                    ModsEn = mods;
                }

                outputJson = jsonPath + "Mods.json";
                using (StreamWriter writer = new(outputJson, false, Encoding.UTF8))
                {
                    writer.Write(Json.Serialize<Mods>(mods));
                }
            }
            if (datName == Strings.MonsterVarieties)
            {
                Monsters monsters = new();
                monsters.Result = new Result[1];
                monsters.Result[0] = new();
                monsters.Result[0].Data = new ResultData[listResultData.Count];
                monsters.Result[0].Data = listResultData.ToArray();

                if (MonstersEn is null)
                {
                    MonstersEn = monsters;
                }

                outputJson = jsonPath + "Monsters.json";
                using (StreamWriter writer = new(outputJson, false, Encoding.UTF8))
                {
                    writer.Write(Json.Serialize<Monsters>(monsters));
                }
            }
            return outputJson;
        }

        internal static string? WriteJson(string datName, string jsonPath, List<WordResultData> listWordResultData)
        {
            string? outputJson = null;

            if (datName == Strings.Words && listWordResultData.Count > 0)
            {
                Words words = new();
                words.Result = new WordResult[1];
                words.Result[0] = new();
                words.Result[0].Data = new WordResultData[listWordResultData.Count];
                words.Result[0].Data = listWordResultData.ToArray();

                outputJson = jsonPath + "Words.json";
                using (StreamWriter writer = new(outputJson, false, Encoding.UTF8))
                {
                    writer.Write(Json.Serialize<Words>(words));
                }
            }
            return outputJson;
        }

        internal static string? WriteJson(string datName, string jsonPath, List<GemResultData> listGemResultData)
        {
            string? outputJson = null;

            if (datName == Strings.Gems && listGemResultData.Count > 0)
            {
                Gems gems = new();
                gems.Result = new GemResult[1];
                gems.Result[0] = new();
                gems.Result[0].Data = new GemResultData[listGemResultData.Count];
                gems.Result[0].Data = listGemResultData.ToArray();

                outputJson = jsonPath + "Gems.json";
                using (StreamWriter writer = new(outputJson, false, Encoding.UTF8))
                {
                    writer.Write(Json.Serialize<Gems>(gems));
                }
            }
            return outputJson;
        }
    }
}
