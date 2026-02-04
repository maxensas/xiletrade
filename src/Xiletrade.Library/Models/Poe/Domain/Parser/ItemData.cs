using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Models.Application.Configuration.DTO.Extension;
using Xiletrade.Library.Models.Poe.Contract.Extension;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.Models.Poe.Domain.Parser;

//WIP
internal sealed class ItemData
{
    // immutable, init with constructor
    private readonly DataManagerService _dm;
    internal ItemFlag Flag { get; }
    internal string[] Data { get; }
    internal string Class { get; }
    internal string Rarity { get; }
    internal Lang Lang { get; }
    internal bool IsPoe2 { get; }

    // non-immutable
    internal TotalStats Stats { get; }
    internal Dictionary<string, string> Option { get; } = InitListOption();
    internal string Quality =>
        RegexUtil.NumericalPattern().Replace(Option[Resources.Resources.General035_Quality].Trim(), string.Empty);
    internal string Name { get; private set; }
    internal string Type { get; private set; }
    internal string NameEn { get; private set; }
    internal string TypeEn { get; private set; }
    internal string Inherits { get; private set; } = string.Empty;
    internal string Id { get; private set; } = string.Empty;
    internal string IdCurrency { get; private set; } = string.Empty;
    internal string MapName { get; private set; } = string.Empty;
    internal double TotalIncPhys { get; private set; } = 0;
    internal bool IsExchangeCurrency { get; private set; }
    internal bool IsSpecialBase { get; private set; }
    internal bool IsBlightMap { get; private set; }
    internal bool IsBlightRavagedMap { get; private set; }

    // not private set
    internal bool IsConqMap { get; set; }

    internal ItemData(DataManagerService dm, InfoDescription infodesc)
    {
        _dm = dm;
        Lang = (Lang)_dm.Config.Options.Language;
        IsPoe2 = _dm.Config.Options.GameVersion is 1;
        Stats = new(IsPoe2);
        Data = infodesc.Item[0].Trim().Split(Strings.CRLF, StringSplitOptions.None);
        Class = Data[0].Split(':')[1].Trim();
        var rarityPrefix = Data[1].Split(':');
        Rarity = rarityPrefix.Length > 1 ? rarityPrefix[1].Trim() : string.Empty;

        Name = Data.Length > 3 && Data[2].Length > 0 ? Data[2] ?? string.Empty : string.Empty;
        Type = Data.Length > 3 && Data[3].Length > 0 ? Data[3] ?? string.Empty
            : Data.Length > 2 && Data[2].Length > 0 ? Data[2] ?? string.Empty
            : Data.Length > 1 && Data[1].Length > 0 ? Data[1] ?? string.Empty
            : string.Empty;

        if (_dm.Config.Options.DevMode && _dm.Config.Options.Language is not 0)
        {
            var tuple = GetTranslatedItemNameAndType(Name, Type);
            Name = tuple.Item1;
            Type = tuple.Item2;
        }

        Flag = new ItemFlag(infodesc, Rarity, Type, Class);

        if (!IsPoe2 && Flag.Unique && Name.Contain(Resources.Resources.General166_Foulborn))
        {
            Name = Name.Replace(Resources.Resources.General166_Foulborn, string.Empty).Trim();
        }
    }

    /// <summary>
    /// Update Option dictionnary.
    /// </summary>
    /// <remarks>
    /// Return 'true' to skip mod parsing or 'false' to proceed.
    /// </remarks>
    /// <param name="data"></param>
    /// <param name="BelowMaxMods"></param>
    /// <returns></returns>
    internal bool UpdateOption(ReadOnlySpan<char> data, bool BelowMaxMods)
    {
        int idx = data.IndexOf(':');
        ReadOnlySpan<char> keySpan = idx is -1 ? data.Trim() : data[..idx].Trim();
        ReadOnlySpan<char> valueSpan = idx is -1 ? [] : data[(idx + 1)..].Trim();

        if (keySpan.Contain(Resources.Resources.General110_FoilUnique))
        {
            keySpan = Resources.Resources.General110_FoilUnique; // Ignore Foil Variation 
        }
        if (keySpan.StartWith(Resources.Resources.General035_Quality))
        {
            keySpan = Resources.Resources.General035_Quality; // Ignore catalyst quality type
        }

        var keyStr = keySpan.ToString();
        if (Option.TryGetValue(keyStr, out string value))
        {
            if (value.Length is 0)
            {
                Option[keyStr] = valueSpan.Length > 1 ? valueSpan.ToString() : Strings.TrueOption;
            }
            return true;
        }

        if (Flag.Gems)
        {
            if (keySpan.Contain(Resources.Resources.General038_Vaal))
            {
                Option[Resources.Resources.General038_Vaal] = Strings.TrueOption;
            }
            return true;
        }

        var cond = (Flag.ItemLevel || Flag.AreaLevel || Flag.FilledCoffin) && BelowMaxMods;
        if (!cond || SkipBetweenBrackets(data, Flag.Ultimatum))
        {
            return true;
        }

        return false;
    }

    internal string[] GetSanctumMods()
    {
        List<string> lMods = new(), lEntrie = new();

        var majBoons = Option[Resources.Resources.General118_SanctumMajorBoons].Split(',', StringSplitOptions.TrimEntries);
        if (majBoons[0].Length > 0)
        {
            lEntrie.AddRange(majBoons);
        }
        var majAfflictions = Option[Resources.Resources.General120_SanctumMajorAfflictions].Split(',', StringSplitOptions.TrimEntries);
        if (majAfflictions[0].Length > 0)
        {
            lEntrie.AddRange(majAfflictions);
        }
        var pacts = Option[Resources.Resources.General123_SanctumPacts].Split(',', StringSplitOptions.TrimEntries);
        if (pacts[0].Length > 0)
        {
            lEntrie.AddRange(pacts);
        }

        if (lEntrie.Count > 0)
        {
            foreach (string mod in lEntrie)
            {
                var modTxt = _dm.Filter.GetFilterResultWithLabel(Strings.CurrencyTypePoe1.Sanctum)?
                    .FindEntryByType(mod, sequenceEquality: false)?.Text;
                if (!string.IsNullOrEmpty(modTxt))
                {
                    lMods.Add(modTxt);
                }
            }
        }

        lEntrie = new();
        var floorRewards = Option[Resources.Resources.General121_RewardsFloorCompletion].Split(',', StringSplitOptions.TrimEntries);
        if (floorRewards[0].Length > 0)
        {
            lEntrie.AddRange(floorRewards);
        }
        if (lEntrie.Count > 0)
        {
            foreach (string mod in lEntrie)
            {
                var match = RegexUtil.DecimalNoPlusPattern().Matches(mod);
                string modKind = RegexUtil.DecimalPattern().Replace(mod, "#").Replace("Orb ", "Orbs ").Replace("Mirror ", "Mirrors ");

                var entry = _dm.Filter.GetFilterResultWithLabel(Strings.CurrencyTypePoe1.Sanctum)?
                    .FindEntryByType(modKind, sequenceEquality: false);
                if (entry.ID.StartWith("sanctum.sanctum_floor_reward") 
                    && !string.IsNullOrEmpty(entry.Text))
                {
                    var modTxt = entry.Text;
                    if (match.Count is 1)
                    {
                        modTxt = modTxt.Replace("#", match[0].Value);
                    }
                    lMods.Add(modTxt);
                }
            }
        }

        lEntrie = new();
        var sanctumRewards = Option[Resources.Resources.General122_RewardsSanctumCompletion].Split(',', StringSplitOptions.TrimEntries);
        if (sanctumRewards[0].Length > 0)
        {
            lEntrie.AddRange(sanctumRewards);
        }
        if (lEntrie.Count > 0)
        {
            foreach (string mod in lEntrie)
            {
                var match = RegexUtil.DecimalNoPlusPattern().Matches(mod);
                string modKind = RegexUtil.DecimalPattern().Replace(mod, "#").Replace("Orb ", "Orbs ").Replace("Mirror ", "Mirrors ");

                var entry = _dm.Filter.GetFilterResultWithLabel(Strings.CurrencyTypePoe1.Sanctum)?
                    .FindEntryByType(modKind, sequenceEquality: false);
                if (entry.ID.StartWith("sanctum.sanctum_final_reward")
                    && !string.IsNullOrEmpty(entry.Text))
                {
                    var modTxt = entry.Text;
                    if (match.Count is 1)
                    {
                        modTxt = modTxt.Replace("#", match[0].Value);
                    }
                    lMods.Add(modTxt);
                }
            }
        }
        return [.. lMods];
    }
    
    internal string UpdateMapNameAndExchangeFlag()
    {
        string tier = Option[Resources.Resources.General034_MaTier].Replace(" ", string.Empty);
        if (Flag.Map && !Flag.Unique && Type.Length > 0)
        {
            var mapName = _dm.Currencies.FindEntryByTypeAndEndId(Type, Strings.tierPrefix + tier)?.Text;
            if (!string.IsNullOrEmpty(mapName))
            {
                IsExchangeCurrency = true;
                MapName = mapName;
            }
        }
        if (!Flag.Unidentified)
        {
            if (Flag.Map && Flag.Unique && Name.Length > 0)
            {
                var mapName = _dm.Currencies.FindEntryByTypeAndEndId(Name, Strings.tierPrefix + tier)?.Text;
                if (!string.IsNullOrEmpty(mapName))
                {
                    IsExchangeCurrency = true;
                    MapName = mapName;
                }
            }
            else
            {
                if (_dm.Currencies.FindEntryByType(Type) is not null)
                {
                    IsExchangeCurrency = true;
                }
            }
        }
        return tier;
    }

    internal void UpdateNameAndType()
    {
        if (Flag.ShowDetail)
        {
            var tmpBaseType = _dm.Bases.FindBaseByName(Type);
            Type = tmpBaseType is null ? Type : tmpBaseType.Name;
            TypeEn = tmpBaseType is null ? string.Empty : tmpBaseType.NameEn;
        }
        if (!Flag.ShowDetail)
        {
            BaseResultData baseResult = null;
            if (Flag.CapturedBeast)
            {
                baseResult = _dm.Monsters.FindMonsterByName(Type, nospirit: true);
                Type = baseResult is null ? Type : baseResult.Name.Replace("\"", string.Empty);
                TypeEn = baseResult is null ? string.Empty : baseResult.NameEn.Replace("\"", string.Empty);
                Name = string.Empty;
            }
            else
            {
                var cultureEn = new CultureInfo(Strings.Culture[0]);
                var rm = new System.Resources.ResourceManager(typeof(Resources.Resources));
                baseResult = _dm.Bases.FindBaseByName(Type);
                Type = baseResult is null ? Type : baseResult.Name;
                TypeEn = baseResult is null ? string.Empty : baseResult.NameEn;
                if (IsBlightMap)
                {
                    Type = Type.Replace(Resources.Resources.General040_Blighted, string.Empty).Trim();
                    TypeEn = TypeEn.Replace(rm.GetString("General040_Blighted", cultureEn), string.Empty).Trim();
                }
                else if (IsBlightRavagedMap)
                {
                    Type = Type.Replace(Resources.Resources.General100_BlightRavaged, string.Empty).Trim();
                    TypeEn = TypeEn.Replace(rm.GetString("General100_BlightRavaged", cultureEn), string.Empty).Trim();
                }
            }
        }

        if (TypeEn.Length is 0) //!item.Is.CapturedBeast
        {
            if (Lang is Lang.English)
            {
                TypeEn = Type;
            }
            else
            {
                var typeEn = _dm.CurrenciesEn.FindEntryById(Id);
                if (typeEn is not null && !string.IsNullOrEmpty(typeEn.Text))
                {
                    TypeEn = typeEn.Text;
                }
            }
        }

        NameEn = string.Empty;
        if (Lang is Lang.English)
        {
            NameEn = Name;
        }
        else if (Name.Length > 0)
        {
            var wordRes = _dm.Words.FindWordByName(Name);
            if (wordRes is not null)
            {
                NameEn = wordRes.NameEn;
            }
        }
    }

    internal void UpdateItemData(ReadOnlyMemory<string> data)
    {
        if (Flag.CapturedBeast)
        {
            var tmpBaseType = _dm.Monsters.FindMonsterByName(Type);
            if (tmpBaseType is not null)
            {
                Id = tmpBaseType.Id;
                Inherits = tmpBaseType.InheritsFrom;
            }
        }
        if (!Flag.CapturedBeast)
        {
            if (Flag.Gems)
            {
                var gemName = string.Empty;
                if (Flag.Corrupted && Option[Resources.Resources.General038_Vaal] 
                    is Strings.TrueOption)
                {
                    var span = data.Span;
                    for (int i = 3; i < span.Length; i++)
                    {
                        string seekVaal = span[i].Replace(Strings.CRLF, string.Empty).Trim();
                        var tmpBaseType = _dm.Bases.FindBaseByName(seekVaal);
                        if (tmpBaseType is not null)
                        {
                            gemName = Type;
                            Type = tmpBaseType.Name;
                            break;
                        }
                    }
                }
                if (Flag.Transfigured)
                {
                    var findGem = _dm.Gems.FindGemByName(Type);
                    if (findGem is not null)
                    {
                        if (gemName.Length is 0 && findGem.Type != findGem.Name) // transfigured normal gem
                        {
                            Type = findGem.Type;
                            Inherits = findGem.Disc;
                        }
                        if (gemName.Length > 0 && findGem.Type == findGem.Name)
                        {
                            
                            var findGem2 = _dm.Gems.FindGemByName(gemName);
                            if (findGem2 is not null) // transfigured vaal gem
                            {
                                Inherits = findGem2.Disc;
                            }
                        }
                    }
                }
            }

            if (Flag.Unidentified || Flag.Normal)
            {
                Type = Type.RemoveStringFromArrayDesc(Resources.Resources.General030_Higher.Split('/'));
                Type = Type.RemoveStringFromArrayDesc(Resources.Resources.General159_Exceptional.Split('/'));
            }

            if (Flag.Map && Type.Length > 5)
            {
                if (Type.Contain(Resources.Resources.General040_Blighted))
                {
                    IsBlightMap = true;
                }
                else if (Type.Contain(Resources.Resources.General100_BlightRavaged))
                {
                    IsBlightRavagedMap = true;
                }
            }
            else if (Option[Resources.Resources.General047_Synthesis] is Strings.TrueOption)
            {
                Type = Type.RemoveStringFromArrayDesc(Resources.Resources.General048_Synthesised.Split('/'));
            }

            if (!Flag.Unidentified && !Flag.Map && Flag.Magic)
            {
                string longestName = _dm.Bases.GetLongestMatchingName(Type);
                if (!string.IsNullOrEmpty(longestName))
                {
                    if (Flag.MemoryLine)
                    {
                        Name = Type;
                    }
                    Type = longestName;
                }
            }
            var tmpBaseType2 = _dm.Bases.FindBaseByName(Type);
            if (tmpBaseType2 is not null)
            {
                IsSpecialBase = Strings.lSpecialBases.Contains(tmpBaseType2.NameEn);
            }
        }

        if (Inherits.Length is 0)
        {
            if (Flag.Map || Flag.Waystones)
            {
                if (!Flag.Unidentified && Flag.Magic)
                {
                    var affixes = _dm.Mods.GetMatchingAffixesList(Type);
                    if (affixes.Count > 0)
                    {
                        foreach (var affix in affixes)
                        {
                            Type = Type.Replace(affix, string.Empty).Trim();
                        }
                    }
                }
                var mapKind = IsPoe2 ? Strings.CurrencyTypePoe2.Waystones : string.Empty;
                if (!IsPoe2)
                {
                    mapKind = IsBlightMap || IsBlightRavagedMap ? Strings.CurrencyTypePoe1.MapsBlighted :
                        Flag.Unique ? Strings.CurrencyTypePoe1.MapsUnique : Strings.CurrencyTypePoe1.Maps;
                }

                var entry = _dm.Currencies.FindMapEntryByType(Type, mapKind);
                if (entry is not null && entry.Id is not null)
                {
                    Id = entry.Id;
                }

                Inherits = Flag.Map ? "Maps/AbstractMap" : "Waystones";
            }
            else if (Flag.Currency || Flag.Divcard || Flag.MapFragment)
            {
                var (Entry, GroupId) = _dm.Currencies.FindEntryAndGroupIdByType(Type, image: false);
                if (Entry is not null)
                {
                    Id = Entry.Id;
                    IdCurrency = GroupId;

                    Inherits = IdCurrency is Strings.CurrencyTypePoe1.Cards ? "DivinationCards/DivinationCardsCurrency"
                        : IdCurrency is Strings.CurrencyTypePoe1.Delve ? "Delve/DelveSocketableCurrency"
                        : IdCurrency is Strings.CurrencyTypePoe1.Fragments && Id != "ritual-vessel"
                        && Id != "valdos-puzzle-box" ? "MapFragments/AbstractMapFragment"
                        : IdCurrency is Strings.CurrencyTypePoe1.Incubators ? "Legion/Incubator"
                        : "Currency/StackableCurrency";
                }
            }

            if (Inherits.Length is 0)
            {
                var tmpBaseType = _dm.Bases.FindBaseByName(Type);
                if (tmpBaseType is not null)
                {
                    Id = tmpBaseType.Id;
                    Inherits = tmpBaseType.InheritsFrom;
                }
            }
        }
        if (Flag.Jewel || Flag.ArmourPiece || Flag.Weapon)
        {
            Inherits = Id;
        }
    }

    internal void UpdateTotalStatsAndPhys(ModFilter modFilter, ReadOnlySpan<char> currentMod, double minFilter)
    {
        if (!Flag.Unique && !Flag.Jewel)
        {
            Stats.Fill(_dm.FilterEn, modFilter, Lang, currentMod);
        }
        if (modFilter.Entrie.ID.Contain(Strings.Stat.Generic.IncPhys) && minFilter > 0 && minFilter < 9999)
        {
            TotalIncPhys += minFilter;
        }
    }

    //private
    private static bool SkipBetweenBrackets(ReadOnlySpan<char> data, bool ultimatum)
    {
        if (ultimatum)
        {
            return data.StartsWith('(') || data.EndsWith(')');
        }
        return data.StartsWith('(') && data.EndsWith(')');
    }

    /// <summary>
    /// Fix for item name/type not translated for non-english.
    /// </summary>
    /// <remarks>
    /// Only for unit tests in dev mode, not optimized.
    /// </remarks>
    /// <param name="itemName"></param>
    /// <param name="itemType"></param>
    /// <returns></returns>
    private Tuple<string, string> GetTranslatedItemNameAndType(string itemName, string itemType)
    {
        string name = itemName;
        string type = itemType;

        if (name.Length > 0)
        {
            var word = _dm.Words.FindWordByNameEn(name);
            if (word is not null && !word.Name.Contain('/'))
            {
                name = word.Name;
            }
        }
        var longestName = _dm.Bases.GetLongestMatchingNameEn(type);
        if (!string.IsNullOrEmpty(longestName))
        {
            type = longestName;
        }
        
        var baseType = _dm.Bases.FindBaseByNameEn(type);
        if (baseType is not null && !baseType.Name.Contain('/'))
        {
            type = baseType.Name;
        }

        if (type == itemType)
        {
            bool isMap = type.Contain("Map");
            if (isMap)
            {
                CultureInfo cultureEn = new(Strings.Culture[0]);
                System.Resources.ResourceManager rm = new(typeof(Resources.Resources));
                type = type.Replace(rm.GetString("General040_Blighted", cultureEn), string.Empty)
                    .Replace(rm.GetString("General100_BlightRavaged", cultureEn), string.Empty).Trim();
            }

            var enCurId = _dm.CurrenciesEn.FindEntryByType(type, isMap);
            if (enCurId is not null && !string.IsNullOrEmpty(enCurId.Id))
            {
                var cur = _dm.Currencies.FindEntryById(enCurId.Id);
                if (cur is not null && !string.IsNullOrEmpty(cur.Text))
                {
                    type = isMap
                        ? cur.Text[..cur.Text.IndexOf('(')].Trim()
                        : cur.Text;
                }
            }
        }

        if (type == itemType)
        {
            var findGem = _dm.Gems.FindGemByNameEn(type);
            if (findGem is not null)
            {
                type = findGem.Name;
            }
        }

        return new Tuple<string, string>(name, type);
    }

    private static Dictionary<string, string> InitListOption()
    {
        return new Dictionary<string, string>()
        {
            { Resources.Resources.General035_Quality, string.Empty },
            { Resources.Resources.General031_Lv, string.Empty },
            { Resources.Resources.General032_ItemLv, string.Empty },
            { Resources.Resources.General033_TalTier, string.Empty },
            { Resources.Resources.General034_MaTier, string.Empty },
            { Resources.Resources.General067_AreaLevel, string.Empty },
            { Resources.Resources.General036_Socket, string.Empty },
            { Resources.Resources.General055_Armour, string.Empty },
            { Resources.Resources.General056_Energy, string.Empty },
            { Resources.Resources.General057_Evasion, string.Empty },
            { Resources.Resources.General095_Ward, string.Empty },
            { Resources.Resources.General058_PhysicalDamage, string.Empty },
            { Resources.Resources.General059_ElementalDamage, string.Empty },
            { Resources.Resources.General060_ChaosDamage, string.Empty },
            { Resources.Resources.General061_AttacksPerSecond, string.Empty },
            { Resources.Resources.General041_Shaper, string.Empty },
            { Resources.Resources.General042_Elder, string.Empty },
            { Resources.Resources.General043_Crusader, string.Empty },
            { Resources.Resources.General044_Redeemer, string.Empty },
            { Resources.Resources.General045_Hunter, string.Empty },
            { Resources.Resources.General046_Warlord, string.Empty },
            { Resources.Resources.General047_Synthesis, string.Empty },
            { Resources.Resources.General038_Vaal, string.Empty },
            { Resources.Resources.Main154_tbFacetor, string.Empty },
            { Resources.Resources.General070_ReqSacrifice, string.Empty },
            { Resources.Resources.General071_Reward, string.Empty },
            { Resources.Resources.General114_SanctumResolve, string.Empty },
            { Resources.Resources.General115_SanctumInspiration, string.Empty },
            { Resources.Resources.General116_SanctumAureus, string.Empty },
            { Resources.Resources.General117_SanctumMinorBoons, string.Empty },
            { Resources.Resources.General118_SanctumMajorBoons, string.Empty },
            { Resources.Resources.General119_SanctumMinorAfflictions, string.Empty },
            { Resources.Resources.General120_SanctumMajorAfflictions, string.Empty },
            { Resources.Resources.General123_SanctumPacts, string.Empty },
            { Resources.Resources.General121_RewardsFloorCompletion, string.Empty },
            { Resources.Resources.General122_RewardsSanctumCompletion, string.Empty },
            { Resources.Resources.General128_Monster, string.Empty },
            { Resources.Resources.General129_CorpseLevel, string.Empty },
            { Resources.Resources.General130_MonsterCategory, string.Empty },
            { Resources.Resources.General136_ItemQuantity, string.Empty },
            { Resources.Resources.General137_ItemRarity, string.Empty },
            { Resources.Resources.General138_MonsterPackSize, string.Empty },
            { Resources.Resources.General139_MoreCurrency, string.Empty },
            { Resources.Resources.General140_MoreScarabs, string.Empty },
            { Resources.Resources.General141_MoreMaps, string.Empty },
            { Resources.Resources.General142_MoreDivinationCards, string.Empty },
            { Resources.Resources.General162_RareMonsters, string.Empty },
            { Resources.Resources.General161_MagicMonsters, string.Empty },
            { Resources.Resources.General143_WaystoneTier, string.Empty },
            { Resources.Resources.General146_LightningDamage, string.Empty },
            { Resources.Resources.General147_CriticalHitChance, string.Empty },
            { Resources.Resources.General148_ColdDamage, string.Empty },
            { Resources.Resources.General149_FireDamage, string.Empty },
            { Resources.Resources.General155_Requires, string.Empty },
            { Resources.Resources.General156_MemoryStrands, string.Empty },
        };
    }

    /// <summary>
    /// Translate item name and type in the correct language used by the trade gateway
    /// </summary>
    /// <param name="item"></param>
    internal void TranslateCurrentItemGateway()
    {
        if (_dm.Config.Options.Gateway == _dm.Config.Options.Language)
        {
            return;
        }

        //name
        if (Name.Length > 0 && NameEn.Length > 0)
        {
            var word = _dm.WordsGateway.FindWordByNameEn(NameEn);
            if (word is not null && word.Name.Length > 0 && word.Name.IndexOf('/') is -1)
            {
                Name = word.Name;
            }
        }

        //type
        if (Type.Length > 0 && TypeEn.Length > 0)
        {
            var bases = _dm.BasesGateway.FindBaseByNameEn(TypeEn);
            if (bases is not null && bases.Name.Length > 0)
            {
                Type = bases.Name;
            }
            if (bases is null)
            {
                var cur = _dm.Currencies.FindEntryByType(Type);
                if (cur is not null)
                {
                    if (!string.IsNullOrEmpty(cur.Id))
                    {
                        var curGateway = _dm.CurrenciesGateway.FindEntryById(cur.Id);
                        if (curGateway is not null && !string.IsNullOrEmpty(curGateway.Text))
                        {
                            Type = curGateway.Text;
                        }
                    }
                }
            }
        }
    }
}
