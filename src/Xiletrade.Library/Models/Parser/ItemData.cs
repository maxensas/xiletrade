using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Xiletrade.Library.Models.Enums;
using Xiletrade.Library.Models.Serializable;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Parser;

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
    internal string MapName { get; private set; } = string.Empty;
    internal double TotalIncPhys { get; private set; } = 0;
    internal bool IsExchangeCurrency { get; private set; }
    internal bool IsSpecialBase { get; private set; }
    internal bool IsBlightMap { get; private set; }
    internal bool IsBlightRavagedMap { get; private set; }

    // not private set
    internal bool IsConqMap { get; set; }

    internal ItemData(DataManagerService dm, string[] clipData)
    {
        _dm = dm;
        Lang = (Lang)_dm.Config.Options.Language;
        IsPoe2 = _dm.Config.Options.GameVersion is 1;
        Stats = new(IsPoe2);
        Data = clipData[0].Trim().Split(Strings.CRLF, StringSplitOptions.None);
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

        Flag = new ItemFlag(clipData, Rarity, Type, Class);
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
    internal bool UpdateOption(string data, bool BelowMaxMods)
    {
        var splitData = data.Split(':', StringSplitOptions.TrimEntries);
        if (splitData[0].Contain(Resources.Resources.General110_FoilUnique))
        {
            splitData[0] = Resources.Resources.General110_FoilUnique; // Ignore Foil Variation 
        }
        if (splitData[0].StartWith(Resources.Resources.General035_Quality))
        {
            splitData[0] = Resources.Resources.General035_Quality; // Ignore catalyst quality type
        }

        if (Option.TryGetValue(splitData[0], out string value))
        {
            if (value.Length is 0)
            {
                Option[splitData[0]] = splitData.Length > 1 ? splitData[1] : Strings.TrueOption;
            }
            return true;
        }

        if (Flag.Gems)
        {
            if (splitData[0].Contain(Resources.Resources.General038_Vaal))
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
                var modTxt = _dm.Filter.Result.SelectMany(result => result.Entries)
                    .FirstOrDefault(filter => filter.Type is "sanctum" && filter.Text.Contains(mod))?.Text;
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

                var modTxt = _dm.Filter.Result.SelectMany(result => result.Entries)
                    .FirstOrDefault(filter => filter.Text.Contain(modKind) &&
                        filter.ID.StartWith("sanctum.sanctum_floor_reward"))?.Text;
                if (!string.IsNullOrEmpty(modTxt))
                {
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

                var modTxt = _dm.Filter.Result.SelectMany(result => result.Entries)
                    .FirstOrDefault(filter => filter.Text.Contains(modKind) &&
                        filter.ID.StartsWith("sanctum.sanctum_final_reward"))?.Text;
                if (!string.IsNullOrEmpty(modTxt))
                {
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
            var mapName = _dm.Currencies.SelectMany(result => result.Entries)
                .FirstOrDefault(cur => cur.Text.Contains(Type) &&
                    cur.Id.EndsWith(Strings.tierPrefix + tier))?.Text;
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
                var mapName = _dm.Currencies.SelectMany(result => result.Entries)
                    .FirstOrDefault(cur => cur.Text.Contains(Name) &&
                        cur.Id.EndsWith(Strings.tierPrefix + tier))?.Text;
                if (!string.IsNullOrEmpty(mapName))
                {
                    IsExchangeCurrency = true;
                    MapName = mapName;
                }
            }
            else
            {
                bool exists = _dm.Currencies.SelectMany(result => result.Entries)
                    .Any(cur => cur.Text == Type);
                if (exists)
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
            var tmpBaseType = _dm.Bases.FirstOrDefault(x => x.Name == Type);

            Type = tmpBaseType is null ? Type : tmpBaseType.Name;
            TypeEn = tmpBaseType is null ? string.Empty : tmpBaseType.NameEn;
        }
        if (!Flag.ShowDetail)
        {
            BaseResultData baseResult = null;
            if (Flag.CapturedBeast)
            {
                baseResult = _dm.Monsters.FirstOrDefault(x => x.Name.Contain(Type));
                Type = baseResult is null ? Type : baseResult.Name.Replace("\"", string.Empty);
                TypeEn = baseResult is null ? string.Empty : baseResult.NameEn.Replace("\"", string.Empty);
                Name = string.Empty;
            }
            else
            {
                var cultureEn = new CultureInfo(Strings.Culture[0]);
                var rm = new System.Resources.ResourceManager(typeof(Resources.Resources));
                baseResult = _dm.Bases.FirstOrDefault(x => x.Name == Type);
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
                var typeEn = _dm.CurrenciesEn.SelectMany(result => result.Entries)
                    .FirstOrDefault(cur => cur.Id == Id)?.Text;
                if (!string.IsNullOrEmpty(typeEn))
                {
                    TypeEn = typeEn;
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
            var wordRes = _dm.Words.FirstOrDefault(x => x.Name == Name);
            if (wordRes is not null)
            {
                NameEn = wordRes.NameEn;
            }
        }
    }

    internal void UpdateItemData(string[] clipData)
    {
        if (Flag.CapturedBeast)
        {
            var tmpBaseType = _dm.Monsters.FirstOrDefault(x => x.Name.Contain(Type));
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
                    for (int i = 3; i < clipData.Length; i++)
                    {
                        string seekVaal = clipData[i].Replace(Strings.CRLF, string.Empty).Trim();
                        var tmpBaseType = _dm.Bases.FirstOrDefault(x => x.Name == seekVaal);
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
                    var findGem = _dm.Gems.FirstOrDefault(x => x.Name == Type);
                    if (findGem is not null)
                    {
                        if (gemName.Length is 0 && findGem.Type != findGem.Name) // transfigured normal gem
                        {
                            Type = findGem.Type;
                            Inherits = findGem.Disc;
                        }
                        if (gemName.Length > 0 && findGem.Type == findGem.Name)
                        {
                            var findGem2 = _dm.Gems.FirstOrDefault(x => x.Name == gemName);
                            if (findGem2 is not null) // transfigured vaal gem
                            {
                                Inherits = findGem2.Disc;
                            }
                        }
                    }
                }
            }

            if ((Flag.Unidentified || Flag.Normal))
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
                var matchingNames = _dm.Bases.Where(result => result.Name.Length > 0
                     && Type.Contain(result.Name)&& !result.Id.StartWith("Gems"))
                    .Select(result => result.Name);
                if (matchingNames.Any())
                {
                    string longestName = matchingNames
                        .OrderByDescending(name => name.Length).First();
                    if (Flag.MemoryLine)
                    {
                        Name = Type;
                    }
                    Type = longestName;
                }
            }
            var tmpBaseType2 = _dm.Bases.FirstOrDefault(x => x.Name == Type);
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
                    var affixes = _dm.Mods.SelectMany(result => result.Name.Split('/'))
                        .Where(name => !string.IsNullOrEmpty(name) && Type.Contain(name));
                    if (affixes.Any())
                    {
                        foreach (var affix in affixes)
                        {
                            Type = Type.Replace(affix, string.Empty).Trim();
                        }
                    }
                }

                string mapKind = IsBlightMap || IsBlightRavagedMap ? Strings.CurrencyTypePoe1.MapsBlighted :
                    Flag.Unique ? Strings.CurrencyTypePoe1.MapsUnique : Strings.CurrencyTypePoe1.Maps;

                var mapId = _dm.Currencies
                    .Where(result => result.Id == mapKind || result.Id == Strings.CurrencyTypePoe2.Waystones)
                    .SelectMany(result => result.Entries)
                    .FirstOrDefault(e => e.Text.StartWith(Type) || e.Text.EndWith(Type))?.Id;
                if (mapId is not null)
                {
                    Id = mapId;
                }

                Inherits = Flag.Map ? "Maps/AbstractMap" : "Waystones";
            }
            else if (Flag.Currency || Flag.Divcard || Flag.MapFragment)
            {
                var curResult =
                    from resultDat in _dm.Currencies
                    from Entrie in resultDat.Entries
                    where Entrie.Text == Type
                    select (Entrie.Id, resultDat.Id);
                if (curResult.Any())
                {
                    Id = curResult.FirstOrDefault().Item1;
                    string cur = curResult.FirstOrDefault().Item2;

                    Inherits = cur is Strings.CurrencyTypePoe1.Cards ? "DivinationCards/DivinationCardsCurrency"
                        : cur is Strings.CurrencyTypePoe1.Delve ? "Delve/DelveSocketableCurrency"
                        : cur is Strings.CurrencyTypePoe1.Fragments && Id != "ritual-vessel"
                        && Id != "valdos-puzzle-box" ? "MapFragments/AbstractMapFragment"
                        : cur is Strings.CurrencyTypePoe1.Incubators ? "Legion/Incubator"
                        : "Currency/StackableCurrency";
                }
            }

            if (Inherits.Length is 0)
            {
                var tmpBaseType = _dm.Bases.FirstOrDefault(x => x.Name == Type);
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

    internal void UpdateTotalIncPhys(ModFilter modFilter, double valMin)
    {
        if (modFilter.Entrie.ID.Contain(Strings.Stat.Generic.IncPhys) && valMin > 0 && valMin < 9999)
        {
            TotalIncPhys += valMin;
        }
    }

    //private
    private static bool SkipBetweenBrackets(string data, bool ultimatum)
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
            var word = _dm.Words.FirstOrDefault(x => x.NameEn == name);
            if (word is not null && !word.Name.Contain('/'))
            {
                name = word.Name;
            }
        }

        var longestName = _dm.Bases.Where(result => result.NameEn.Length > 0
            && type.Contain(result.NameEn) && !result.Id.StartWith("Gems"))
            .OrderByDescending(result => result.NameEn.Length)
            .Select(result => result.NameEn).FirstOrDefault();
        if (!string.IsNullOrEmpty(longestName))
        {
            type = longestName;
        }

        var baseType = _dm.Bases.FirstOrDefault(x => x.NameEn == type);
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

            var enCurId = _dm.CurrenciesEn.SelectMany(result => result.Entries)
                .Where(e => isMap ? e.Text.Contain(type) : e.Text == type)
                .Select(e => e.Id).FirstOrDefault();
            if (!string.IsNullOrEmpty(enCurId))
            {
                var curText = _dm.Currencies.SelectMany(result => result.Entries)
                    .Where(e => e.Id == enCurId)
                    .Select(e => e.Text).FirstOrDefault();
                if (!string.IsNullOrEmpty(curText))
                {
                    type = isMap
                        ? curText[..curText.IndexOf('(')].Trim()
                        : curText;
                }
            }
        }

        if (type == itemType)
        {
            var findGem = _dm.Gems.FirstOrDefault(x => x.NameEn == type);
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
            var word = _dm.WordsGateway.FirstOrDefault(x => x.NameEn == NameEn);
            if (word is not null && word.Name.Length > 0 && word.Name.IndexOf('/') is -1)
            {
                Name = word.Name;
            }
        }

        //type
        if (Type.Length > 0 && TypeEn.Length > 0)
        {
            var bases = _dm.BasesGateway.FirstOrDefault(x => x.NameEn == TypeEn);
            if (bases is not null && bases.Name.Length > 0)
            {
                Type = bases.Name;
            }
            if (bases is null)
            {
                var curId = _dm.Currencies.SelectMany(result => result.Entries)
                    .Where(e => e.Text == Type).Select(e => e.Id)
                    .FirstOrDefault();
                if (!string.IsNullOrEmpty(curId))
                {
                    var curText = _dm.CurrenciesGateway.SelectMany(result => result.Entries)
                        .Where(e => e.Id == curId).Select(e => e.Text)
                        .FirstOrDefault();
                    if (!string.IsNullOrEmpty(curText))
                    {
                        Type = curText;
                    }
                }
            }
        }
    }
}
