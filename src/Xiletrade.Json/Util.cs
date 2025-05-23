﻿using System.Text;
using CsvHelper.Configuration;
using System.Globalization;
using CsvHelper;
using Xiletrade.Library.Models.Serializable;

namespace Xiletrade.Json
{
    internal static class Util
    {
        internal static BaseData? BasesOrigin { get; set; }
        internal static BaseData? BasesEn { get; set; }
        internal static BaseData? ModsEn { get; set; }
        internal static BaseData? MonstersEn { get; set; }
        internal static GemData? GemsEn { get; set; }

        // not used atm, progam run once.
        internal static void ReInitEnglishData() 
        {
            BasesEn = null;
            ModsEn = null;
            MonstersEn = null;
            GemsEn = null;
        }

        // Method that create what Xiletrade needs: smallest possible json files. Refactor needed.
        internal static string? CreateJson(GameStrings game, string csvRawData, string datName, string jsonPath, string lang)
        {
            try
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true
                };
                
                var reader = new StringReader(csvRawData);
                var csv = new CsvReader(reader, config);

                bool isBases = datName == game.BaseItemTypes;
                bool isMods = datName == game.Mods;
                bool isMonsters = datName == game.MonsterVarieties;
                bool isWords = datName == game.Words;
                bool isGems = datName == game.Gems;

                KeyValuePair<int, string>[]? arrayIndex = (isBases ? Strings.BasesIndex
                            : isMods ? Strings.ModsIndex
                            : isMonsters ? Strings.MonstersIndex
                            : isWords ? Strings.WordsIndex
                            : isGems ? Strings.GemsIndex: null) ?? throw new Exception("Header not found for DAT : " + datName);
                
                List<BaseResultData> listResultData = new();
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
                        BaseResultData d = new()
                        {
                            Id = csv.GetField(0)?.Replace(Strings.Parser.MetaItem.Key, Strings.Parser.MetaItem.Value),
                            Name = csv.GetField(4)?.Replace(Strings.Parser.HexSpaceSring.Key, Strings.Parser.HexSpaceSring.Value) // 0xa0 => 0x20;
                                .Replace(Strings.Parser.DoNotUse, string.Empty)
                                .Replace(Strings.Parser.UnUsed, string.Empty)
                                .Replace(Strings.Parser.DoNotUseKorean, string.Empty).Trim(), 
                            InheritsFrom = csv.GetField(5)?.Replace(Strings.Parser.MetaItem.Key, Strings.Parser.MetaItem.Value)
                        };
                        if (lang is "russian" && d.Name?.Length > 0)
                        {
                            var value = Strings.BasesNotTranslatedRussian.FirstOrDefault(x => x.Key == d.Name).Value;
                            if (value?.Length > 0)
                            {
                                d.Name = value;
                            }
                        }
                        if (lang is "portuguese" && d.Name?.Length > 0)
                        {
                            var value = Strings.BasesNotTranslatedPortuguese.FirstOrDefault(x => x.Key == d.Name).Value;
                            if (value?.Length > 0)
                            {
                                d.Name = value;
                            }
                        }
                        if (BasesEn is not null)
                        {
                            var resultDat = BasesEn.Result?[0].Data?.FirstOrDefault(x => x.Id == d.Id);
                            if (resultDat is null)
                            {
                                continue;
                            }
                            d.NameEn = resultDat.Name?.Replace(Strings.Parser.DoNotUse, string.Empty)
                                .Replace(Strings.Parser.UnUsed, string.Empty)
                                .Replace(Strings.Parser.DoNotUseKorean, string.Empty).Trim();
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
                            !d.Id!.Contains(Strings.Parser.IncursionVial, StringComparison.Ordinal))
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

                        BaseResultData d = new()
                        {
                            Id = csv.GetField(0),
                            InheritsFrom = Strings.Parser.ModsInherits,
                            Name = ParseMultipleName(csv.GetField(9))
                        };
                        
                        bool checkId = false;
                        foreach (var id in Strings.Parser.IdModsUnwanted)
                        {
                            if (d.Id?.IndexOf(id, StringComparison.Ordinal) == 0)
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
                            var resultDat = ModsEn.Result?[0].Data?.FirstOrDefault(x => x.Id == d.Id);
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
                        BaseResultData d = new()
                        {
                            Id = csv.GetField(0)?.Replace(Strings.Parser.MetaMonster.Key, Strings.Parser.MetaMonster.Value),
                            Name = csv.GetField(32),
                            InheritsFrom = csv.GetField(8)?.Replace(Strings.Parser.MetaMonster.Key, Strings.Parser.MetaMonster.Value)
                        };

                        if (MonstersEn is not null)
                        {
                            var resultDat = MonstersEn.Result?[0].Data?.FirstOrDefault(x => x.Id == d.Id);
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

                        if (GemsEn is not null)
                        {
                            var resultDat = GemsEn.Result?[0].Data?.FirstOrDefault(x => x.Id == d.Id);
                            if (resultDat is null)
                            {
                                continue;
                            }
                            d.NameEn = resultDat.Name;
                            d.TypeEn = resultDat.Type;
                        }
                        else
                        {
                            d.NameEn = d.Name;
                            d.TypeEn = d.Type;  
                        }

                        string delimiter = "Alt";
                        d.Disc = d.Id[d.Id.LastIndexOf(delimiter)..].ToLowerInvariant().Insert(delimiter.Length, "_");
                        var shortId = d.Id[..(d.Id.LastIndexOf(delimiter))];
                        if (shortId?.Length > 0)
                        {
                            var resultDat = BasesOrigin?.Result?[0].Data?.FirstOrDefault(x => x.Id.EndsWith(shortId));
                            d.Type = resultDat?.Name;
                        }
                        if (d.Type is null)
                        {
                            var sb = new StringBuilder(d.Id);
                            sb.Replace("AltX", string.Empty).Replace("AltY", string.Empty);
                            var indexes = sb.ToString().Select((chr, index) => (chr, index))
                                .Where(tuple => Char.IsUpper(tuple.chr))
                                .Select(tuple => tuple.index);
                            int cpt = 0;
                            foreach (var idx in indexes)
                            {
                                if (idx is 0)
                                {
                                    continue;
                                }
                                sb.Insert(idx + cpt, " ");
                                cpt++;
                            }
                            d.Type = sb.ToString();
                        }
                        if (listGemResultData.FirstOrDefault(x => x.Name == d.Name) == null) listGemResultData.Add(d);
                    }
                }
                // END OF PARSING
                if (listWordResultData.Count > 0)
                {
                    return WriteJson(game, datName, jsonPath, listWordResultData);
                }
                if (listGemResultData.Count > 0)
                {
                    return WriteJson(game, datName, jsonPath, listGemResultData);
                }
                return WriteJson(game, datName, jsonPath, listResultData);
            }
            catch (Exception)
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

        internal static string? WriteJson(GameStrings game, string datName, string jsonPath, List<BaseResultData> listResultData)
        {
            string? outputJson = null;

            if (listResultData.Count == 0)
            {
                return null;
            }

            if (datName == game.BaseItemTypes)
            {
                BaseData bases = new();
                bases.Result = new BaseResult[1];
                bases.Result[0] = new();
                bases.Result[0].Data = new BaseResultData[listResultData.Count];
                bases.Result[0].Data = listResultData.ToArray();

                BasesOrigin = bases;
                if (BasesEn is null)
                {
                    BasesEn = bases;
                }

                outputJson = jsonPath + game.Names[game.BaseItemTypes];

                using (StreamWriter writer = new(outputJson, false, Encoding.UTF8))
                {
                    writer.Write(Json.Serialize<BaseData>(bases));
                }
            }
            if (datName == game.Mods)
            {
                BaseData mods = new();
                mods.Result = new BaseResult[1];
                mods.Result[0] = new();
                mods.Result[0].Data = new BaseResultData[listResultData.Count];
                mods.Result[0].Data = listResultData.ToArray();

                if (ModsEn is null)
                {
                    ModsEn = mods;
                }

                outputJson = jsonPath + game.Names[game.Mods];
                using (StreamWriter writer = new(outputJson, false, Encoding.UTF8))
                {
                    writer.Write(Json.Serialize<BaseData>(mods));
                }
            }
            if (datName == game.MonsterVarieties)
            {
                BaseData monsters = new();
                monsters.Result = new BaseResult[1];
                monsters.Result[0] = new();
                monsters.Result[0].Data = new BaseResultData[listResultData.Count];
                monsters.Result[0].Data = listResultData.ToArray();

                if (MonstersEn is null)
                {
                    MonstersEn = monsters;
                }

                outputJson = jsonPath + game.Names[game.MonsterVarieties];
                using (StreamWriter writer = new(outputJson, false, Encoding.UTF8))
                {
                    writer.Write(Json.Serialize<BaseData>(monsters));
                }
            }
            return outputJson;
        }

        internal static string? WriteJson(GameStrings game, string datName, string jsonPath, List<WordResultData> listWordResultData)
        {
            string? outputJson = null;

            if (datName == game.Words && listWordResultData.Count > 0)
            {
                WordData words = new();
                words.Result = new WordResult[1];
                words.Result[0] = new();
                words.Result[0].Data = new WordResultData[listWordResultData.Count];
                words.Result[0].Data = listWordResultData.ToArray();

                outputJson = jsonPath + game.Names[game.Words];
                using (StreamWriter writer = new(outputJson, false, Encoding.UTF8))
                {
                    writer.Write(Json.Serialize<WordData>(words));
                }
            }
            return outputJson;
        }

        internal static string? WriteJson(GameStrings game, string datName, string jsonPath, List<GemResultData> listGemResultData)
        {
            string? outputJson = null;

            if (datName == game.Gems && listGemResultData.Count > 0)
            {
                GemData gems = new();
                gems.Result = new GemResult[1];
                gems.Result[0] = new();
                gems.Result[0].Data = new GemResultData[listGemResultData.Count];
                gems.Result[0].Data = listGemResultData.ToArray();

                if (GemsEn is null)
                {
                    GemsEn = gems;
                }

                outputJson = jsonPath + game.Names[game.Gems];
                using (StreamWriter writer = new(outputJson, false, Encoding.UTF8))
                {
                    writer.Write(Json.Serialize<GemData>(gems));
                }
            }
            return outputJson;
        }
    }
}
