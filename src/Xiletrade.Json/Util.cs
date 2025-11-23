using System.Text;
using CsvHelper.Configuration;
using System.Globalization;
using CsvHelper;
using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Models.Application.Serialization;

namespace Xiletrade.Json
{
    internal static class Util
    {
        internal static BaseData? BasesOrigin { get; set; }
        internal static BaseData? BasesEn { get; set; }
        internal static BaseData? ModsEn { get; set; }
        internal static BaseData? MonstersEn { get; set; }
        internal static GemData? GemsEn { get; set; }
        internal static ItemClassData? ItemClassEn { get; set; }
        internal static UniqueData? UniqueDataEn { get; set; }
        internal static List<UniqueType>? UniqueTypeEn { get; set; }

        internal static List<UniqueType>? UniqueType { get; set; }
        internal static List<WordResultData>? Words { get; set; }

        internal static JsonHelper Json { get; } = new();

        // not used atm, progam run once.
        internal static void ReInitEnglishData() 
        {
            BasesEn = null;
            ModsEn = null;
            MonstersEn = null;
            GemsEn = null;
            ItemClassEn = null;
            UniqueDataEn = null;
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
                bool isClass = datName == game.ItemClass;
                bool isUniquesTypes = datName == game.UniquesTypes;
                bool isUniques = datName == game.UniquesLayout;

                KeyValuePair<int, string>[]? arrayIndex = (isBases ? Strings.BasesIndex
                            : isMods ? Strings.ModsIndex
                            : isMonsters ? Strings.MonstersIndex
                            : isWords ? Strings.WordsIndex
                            : isClass ? Strings.ClassIndex
                            : isUniquesTypes ? Strings.UniquesTypeIndex
                            : isUniques ? Strings.UniquesIndex
                            : isGems ? Strings.GemsIndex: null) 
                            ?? throw new Exception("Header not found for DAT : " + datName);
                
                List<BaseResultData> listResultData = new();
                List<WordResultData> listWordResultData = new();
                List<GemResultData> listGemResultData = new();
                List<ItemClass> listItemClass = new();
                List<UniqueType> listUniquesTypes = new();
                List<Unique> listUniques = new();

                int itemClassId = 0, WordId = 0, UniqueTypeId = 0;

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

                    if (isClass)
                    {
                        ItemClass d = new()
                        {
                            Id = itemClassId,
                            IdOrigin = csv.GetField(0),
                            Name = csv.GetField(1)
                        };
                        itemClassId++;
                        if (ItemClassEn is not null)
                        {
                            var resultDat = ItemClassEn.ItemClass.FirstOrDefault(x => x.Id == d.Id);
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
                        if (listItemClass.FirstOrDefault(x => x.Name == d.Name) == null) listItemClass.Add(d);
                        //listItemClass.Add(d);
                    }

                    if (isBases)
                    {
                        var idClass = csv.GetField(1)?.Replace(Strings.Parser.BeginKey, string.Empty)
                            .Replace(Strings.Parser.EndingKey, string.Empty);
                        if (!int.TryParse(idClass, out int integerIdClass))
                        {
                            integerIdClass = -1;
                        }

                        BaseResultData d = new()
                        {
                            Id = csv.GetField(0)?.Replace(Strings.Parser.MetaItem.Key, Strings.Parser.MetaItem.Value),
                            Name = csv.GetField(4)?.Replace(Strings.Parser.HexSpaceSring.Key, Strings.Parser.HexSpaceSring.Value) // 0xa0 => 0x20;
                                .Replace(Strings.Parser.DoNotUse, string.Empty)
                                .Replace(Strings.Parser.UnUsed, string.Empty)
                                .Replace(Strings.Parser.DoNotUseKorean, string.Empty).Trim(),
                            InheritsFrom = csv.GetField(5)?.Replace(Strings.Parser.MetaItem.Key, Strings.Parser.MetaItem.Value),
                            ItemClassId = integerIdClass
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
                            Id = WordId,
                            NameEn = csv.GetField(1)?.Trim(),
                            Name = ParseMultipleName(csv.GetField(5))
                        };

                        WordId++;
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
                    if (isUniquesTypes)
                    {
                        UniqueType d = new()
                        {
                            Id = UniqueTypeId,
                            IdOrigin = csv.GetField(0)?.Trim(),
                            Name = csv.GetField(6)?.Trim()
                        };
                        UniqueTypeId++;
                        listUniquesTypes.Add(d);
                    }
                    if (isUniques && UniqueType?.Count > 0 && UniqueTypeEn?.Count > 0 
                        && Words?.Count > 0)
                    {
                        var idWord = csv.GetField(0)?.Replace(Strings.Parser.BeginKey, string.Empty)
                            .Replace(Strings.Parser.EndingKey, string.Empty);
                        if (!int.TryParse(idWord, out int integerIdWord))
                        {
                            integerIdWord = -1;
                        }
                        var idType = csv.GetField(2)?.Replace(Strings.Parser.BeginKey, string.Empty)
                            .Replace(Strings.Parser.EndingKey, string.Empty);
                        if (!int.TryParse(idType, out int integerIdType))
                        {
                            integerIdType = -1;
                        }

                        var word = integerIdWord < Words.Count ? Words.FirstOrDefault(x => x.Id == integerIdWord) : null;

                        Unique d = new()
                        {
                            Type = integerIdType < UniqueType.Count ? UniqueType.FirstOrDefault(x => x.Id == integerIdType)?.Name : string.Empty,
                            TypeEn = integerIdType < UniqueTypeEn.Count ? UniqueTypeEn.FirstOrDefault(x => x.Id == integerIdType)?.Name : string.Empty,
                            Name = word?.Name,
                            NameEn = word?.NameEn,
                        };
                        if (listUniques.FirstOrDefault(x => x.Name == d.Name) == null) listUniques.Add(d);
                    }
                }
                // END OF PARSING
                if (listWordResultData.Count > 0)
                {
                    Words = listWordResultData;
                    return WriteJson(game, datName, jsonPath, listWordResultData);
                }
                if (listGemResultData.Count > 0)
                {
                    return WriteJson(game, datName, jsonPath, listGemResultData);
                }
                if (listItemClass.Count > 0)
                {
                    return WriteJson(game, datName, jsonPath, listItemClass);
                }
                if (listResultData.Count > 0)
                {
                    return WriteJson(game, datName, jsonPath, listResultData);
                }
                if (listUniques.Count > 0)
                {
                    return WriteJson(game, datName, jsonPath, listUniques);
                }
                if (listUniquesTypes.Count > 0)
                {
                    UniqueType = listUniquesTypes; // overwrite
                    if (UniqueTypeEn is null) // first
                    {
                        UniqueTypeEn = listUniquesTypes;
                    }
                    return null;
                }
                return null;
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

                // handle Black Mórrigan duplicates
                var cnt = monsters.Result[0].Data.Where(x => x.Id is "LeagueAzmeri/GullGoliath_").Count()
                    + monsters.Result[0].Data.Where(x => x.Id is "LeagueAzmeri/GullGoliathBestiary_").Count();
                if (cnt is 2)
                {
                    monsters.Result[0].Data = [.. monsters.Result[0].Data.Where(x => x.Id is not "LeagueAzmeri/GullGoliath_")];
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

        internal static string? WriteJson(GameStrings game, string datName, string jsonPath, List<ItemClass> listItemClass)
        {
            string? outputJson = null;

            if (datName == game.ItemClass && listItemClass.Count > 0)
            {
                ItemClassData itemClass = new();
                itemClass.ItemClass = listItemClass.ToArray();

                if (ItemClassEn is null)
                {
                    ItemClassEn = itemClass;
                }

                outputJson = jsonPath + game.Names[game.ItemClass];
                using (StreamWriter writer = new(outputJson, false, Encoding.UTF8))
                {
                    writer.Write(Json.Serialize<ItemClassData>(itemClass));
                }
            }
            return outputJson;
        }

        internal static string? WriteJson(GameStrings game, string datName, string jsonPath, List<Unique> listUniques)
        {
            string? outputJson = null;

            if (datName == game.UniquesLayout && listUniques.Count > 0)
            {
                UniqueData unique = new();
                unique.Unique = listUniques.ToArray();

                outputJson = jsonPath + game.Names[game.UniquesLayout];
                using (StreamWriter writer = new(outputJson, false, Encoding.UTF8))
                {
                    writer.Write(Json.Serialize<UniqueData>(unique));
                }
            }
            return outputJson;
        }
    }
}
