using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
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
    internal TotalStats Stats { get; } = new();
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

        /*
        StringBuilder sbMods = new(lOptions[Resources.Resources.General118_SanctumMajorBoons]);
        sbMods.AppendJoin(',', lOptions[Resources.Resources.General120_SanctumMajorAfflictions])
            .AppendJoin(',', lOptions[Resources.Resources.General123_SanctumPacts])
            .AppendJoin(',', lOptions[Resources.Resources.General121_RewardsFloorCompletion])
            .AppendJoin(',', lOptions[Resources.Resources.General122_RewardsSanctumCompletion]);
        var test = sbMods.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries);
        */

        if (lEntrie.Count > 0)
        {
            foreach (string mod in lEntrie)
            {
                var modEntry =
                    from result in _dm.Filter.Result
                    from filt in result.Entries
                    where filt.Text.Contain(mod) && filt.Type is "sanctum"
                    select filt.Text;
                if (modEntry.Any())
                {
                    var modTxt = modEntry.First();
                    if (modTxt.Length > 0)
                    {
                        lMods.Add(modTxt);
                    }
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

                var modEntry =
                    from result in _dm.Filter.Result
                    from filt in result.Entries
                    where filt.Text.Contain(modKind) && filt.ID.StartWith("sanctum.sanctum_floor_reward")
                    select filt.Text;
                if (modEntry.Any())
                {
                    var modTxt = modEntry.First();
                    if (modTxt.Length > 0)
                    {
                        if (match.Count is 1)
                        {
                            modTxt = modTxt.Replace("#", match[0].Value);
                        }

                        lMods.Add(modTxt);
                    }
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

                var modEntry =
                    from result in _dm.Filter.Result
                    from filt in result.Entries
                    where filt.Text.Contain(modKind) && filt.ID.StartWith("sanctum.sanctum_final_reward")
                    select filt.Text;
                if (modEntry.Any())
                {
                    var modTxt = modEntry.First();
                    if (modTxt.Length > 0)
                    {
                        if (match.Count is 1)
                        {
                            modTxt = modTxt.Replace("#", match[0].Value);
                        }

                        lMods.Add(modTxt);
                    }
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
            var cur =
                    from result in _dm.Currencies
                    from Entrie in result.Entries
                    where Entrie.Text.Contain(Type)
                        && Entrie.Id.EndWith(Strings.tierPrefix + tier)
                    select Entrie.Text;
            if (cur.Any())
            {
                IsExchangeCurrency = true;
                MapName = cur.First();
            }
        }
        if (!Flag.Unidentified)
        {
            if (Flag.Map && Flag.Unique && Name.Length > 0)
            {
                var cur =
                    from result in _dm.Currencies
                    from Entrie in result.Entries
                    where Entrie.Text.Contain(Name)
                        && Entrie.Id.EndWith(Strings.tierPrefix + tier)
                    select Entrie.Text;
                if (cur.Any())
                {
                    IsExchangeCurrency = true;
                    MapName = cur.First();
                }
            }
            else
            {
                var cur =
                    from result in _dm.Currencies
                    from Entrie in result.Entries
                    where Entrie.Text == Type
                    select true;
                if (cur.Any() && cur.First())
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
                var enCur =
                    from result in _dm.CurrenciesEn
                    from Entrie in result.Entries
                    where Entrie.Id == Id
                    select Entrie.Text;
                if (enCur.Any())
                {
                    TypeEn = enCur.First();
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

            if ((Flag.Unidentified || Flag.Normal) && Type.Contain(Resources.Resources.General030_Higher))
            {
                if (Lang is Lang.French)
                {
                    Type = Type.Replace(Resources.Resources.General030_Higher + "es", string.Empty).Trim();
                    Type = Type.Replace(Resources.Resources.General030_Higher + "e", string.Empty).Trim();
                }
                if (Lang is Lang.Spanish)
                {
                    Type = Type.Replace(Resources.Resources.General030_Higher + "es", string.Empty).Trim();
                }
                Type = Type.Replace(Resources.Resources.General030_Higher, string.Empty).Trim();
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
                if (Type.Contain(Resources.Resources.General048_Synthesised))
                {
                    if (Lang is Lang.French)
                    {
                        Type = Type.Replace(Resources.Resources.General048_Synthesised + "e", string.Empty).Trim();
                    }
                    if (Lang is Lang.German)
                    {
                        StringBuilder iType = new(Type);
                        iType.Replace(Resources.Resources.General048_Synthesised + "s", string.Empty)
                            .Replace(Resources.Resources.General048_Synthesised + "r", string.Empty);
                        Type = iType.ToString().Trim();
                    }
                    if (Lang is Lang.Russian)
                    {
                        StringBuilder iType = new(Type);
                        iType.Replace(Resources.Resources.General048_Synthesised + "ый", string.Empty)
                            .Replace(Resources.Resources.General048_Synthesised + "ое", string.Empty)
                            .Replace(Resources.Resources.General048_Synthesised + "ая", string.Empty);
                        Type = iType.ToString().Trim();
                    }
                    Type = Type.Replace(Resources.Resources.General048_Synthesised, string.Empty).Trim();
                }
            }

            if (!Flag.Unidentified && !Flag.Map && Flag.Magic)
            {
                var resultName =
                    from result in _dm.Bases
                    where result.Name.Length > 0 && Type.Contain(result.Name)
                    && !result.Id.StartWith("Gems")
                    select result.Name;
                if (resultName.Any())
                {
                    //itemType = resultName.First();
                    string longestName = string.Empty;
                    foreach (var result in resultName)
                    {
                        if (result.Length > longestName.Length)
                        {
                            longestName = result;
                        }
                    }
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
                    var affixes =
                        from result in _dm.Mods
                        from names in result.Name.Split('/')
                        where names.Length > 0 && Type.Contain(names)
                        select names;
                    if (affixes.Any())
                    {
                        foreach (string str in affixes)
                        {
                            Type = Type.Replace(str, string.Empty).Trim();
                        }
                    }
                }

                string mapKind = IsBlightMap || IsBlightRavagedMap ? Strings.CurrencyTypePoe1.MapsBlighted :
                    Flag.Unique ? Strings.CurrencyTypePoe1.MapsUnique : Strings.CurrencyTypePoe1.Maps;

                var mapId =
                    from result in _dm.Currencies
                    from Entrie in result.Entries
                    where (result.Id == mapKind || result.Id == Strings.CurrencyTypePoe2.Waystones)
                    && (Entrie.Text.StartWith(Type)
                    || Entrie.Text.EndWith(Type))
                    select Entrie.Id;
                if (mapId.Any())
                {
                    Id = mapId.First();
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
        if (modFilter.ID.Contain(Strings.Stat.IncPhys) && valMin > 0 && valMin < 9999)
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

        var resultName =
                    from result in _dm.Bases
                    where result.NameEn.Length > 0
                    && type.Contain(result.NameEn)
                    && !result.Id.StartWith("Gems")
                    select result.NameEn;
        if (resultName.Any())
        {
            string longestName = string.Empty;
            foreach (var result in resultName)
            {
                if (result.Length > longestName.Length)
                {
                    longestName = result;
                }
            }
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

            var enCur =
                    from result in _dm.CurrenciesEn
                    from Entrie in result.Entries
                    where isMap ? Entrie.Text.Contain(type) : Entrie.Text == type
                    select Entrie.Id;
            if (enCur.Any())
            {
                var cur = from result in _dm.Currencies
                          from Entrie in result.Entries
                          where Entrie.Id == enCur.First()
                          select Entrie.Text;
                if (cur.Any())
                {
                    type = cur.First();
                    if (isMap)
                    {
                        type = type.Substring(0, type.IndexOf('(')).Trim();
                    }
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
                string curId = string.Empty;
                var curIdList = from result in _dm.Currencies
                                from Entrie in result.Entries
                                where Entrie.Text == Type
                                select Entrie.Id;
                if (curIdList.Any())
                {
                    curId = curIdList.FirstOrDefault();
                }
                if (curId.Length > 0)
                {
                    var curList = from result in _dm.CurrenciesGateway
                                  from Entrie in result.Entries
                                  where Entrie.Id == curId
                                  select Entrie.Text;
                    if (curList.Any())
                    {
                        Type = curList.FirstOrDefault();
                    }
                }
            }
        }
    }
}
