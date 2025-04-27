using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Xiletrade.Library.Models.Serializable;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Parser;

//WIP
internal sealed class ItemData
{
    internal ItemFlag Flag { get; }
    internal ItemBase Base { get; } = new();
    internal Dictionary<string, string> Option { get; } = InitListOption();

    internal string[] Data { get; }
    internal string Class { get; }
    internal string Rarity { get; }
    internal string Quality => RegexUtil.NumericalPattern().Replace(Option[Resources.Resources.General035_Quality].Trim(), string.Empty);

    internal string Name { get; private set; }
    internal string Type { get; private set; }
    internal string Inherits { get; private set; } = string.Empty;
    internal string Id { get; private set; } = string.Empty;
    internal string MapName { get; private set; } = string.Empty;
    internal string GemName { get; private set; } = string.Empty;

    internal ItemData(string[] clipData)
    {
        Data = clipData[0].Trim().Split(Strings.CRLF, StringSplitOptions.None);
        Class = Data[0].Split(':')[1].Trim();
        var rarityPrefix = Data[1].Split(':');
        Rarity = rarityPrefix.Length > 1 ? rarityPrefix[1].Trim() : string.Empty;

        Name = Data.Length > 3 && Data[2].Length > 0 ? Data[2] ?? string.Empty : string.Empty;
        Type = Data.Length > 3 && Data[3].Length > 0 ? Data[3] ?? string.Empty
            : Data.Length > 2 && Data[2].Length > 0 ? Data[2] ?? string.Empty
            : Data.Length > 1 && Data[1].Length > 0 ? Data[1] ?? string.Empty
            : string.Empty;

        if (DataManager.Config.Options.DevMode && DataManager.Config.Options.Language is not 0)
        {
            var tuple = GetTranslatedItemNameAndType(Name, Type);
            Name = tuple.Item1;
            Type = tuple.Item2;
        }

        Flag = new ItemFlag(clipData, Rarity, Type, Class);
        if (Flag.ScourgedMap)
        {
            Type = Type.Replace(Resources.Resources.General103_Scourged, string.Empty).Trim();
        }
    }

    /// <summary>
    /// Update Option and Flag member.
    /// </summary>
    /// <remarks>
    /// Return 'true' to skip mod parsing or 'false' to proceed.
    /// </remarks>
    /// <param name="data"></param>
    /// <param name="BelowMaxMods"></param>
    /// <returns></returns>
    internal bool UpdateOptionAndFlag(string data, bool BelowMaxMods)
    {
        var splitData = data.Split(':', StringSplitOptions.TrimEntries);
        if (splitData[0].Contain(Resources.Resources.General110_FoilUnique))
        {
            splitData[0] = Resources.Resources.General110_FoilUnique; // Ignore Foil Variation 
        }

        if (Option.TryGetValue(splitData[0], out string value))
        {
            if (value.Length is 0)
            {
                Option[splitData[0]] = splitData.Length > 1 ? splitData[1] : Strings.TrueOption;
                Flag.ItemLevel = Option[Resources.Resources.General032_ItemLv].Length > 0
                    || Option[Resources.Resources.General143_WaystoneTier].Length > 0;
                Flag.AreaLevel = Option[Resources.Resources.General067_AreaLevel].Length > 0;
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

    internal void UpdateOption(ModFilter modFilter, double valMin)
    {
        if (modFilter.ID.Contain(Strings.Stat.IncAs) && valMin > 0 && valMin < 999)
        {
            var val = Option[Strings.Stat.IncAs].ToDoubleDefault();
            Option[Strings.Stat.IncAs] = (val + valMin).ToString();
            return;
        }
        if (modFilter.ID.Contain(Strings.Stat.IncPhys) && valMin > 0 && valMin < 9999)
        {
            var val = Option[Strings.Stat.IncPhys].ToDoubleDefault();
            Option[Strings.Stat.IncPhys] = (val + valMin).ToString();
        }
    }

    internal void InitOptionSecondStep()
    {
        Flag.Unidentified = Option[Resources.Resources.General039_Unidentify] == Strings.TrueOption;
        Flag.Corrupted = Option[Resources.Resources.General037_Corrupt] == Strings.TrueOption;
        Flag.Mirrored = Option[Resources.Resources.General109_Mirrored] == Strings.TrueOption;
        Flag.FoilVariant = Option[Resources.Resources.General110_FoilUnique] == Strings.TrueOption;
        Flag.ScourgedItem = Option[Resources.Resources.General099_ScourgedItem] == Strings.TrueOption;
        Flag.MapCategory = Option[Resources.Resources.General034_MaTier].Length > 0 && !Flag.Divcard;
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
                    from result in DataManager.Filter.Result
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
                    from result in DataManager.Filter.Result
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
                    from result in DataManager.Filter.Result
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

    internal void UpdateMapFlag(string tier)
    {
        if (Flag.MapCategory && !Flag.Unique && Type.Length > 0)
        {
            var cur =
                    from result in DataManager.Currencies
                    from Entrie in result.Entries
                    where Entrie.Text.Contain(Type)
                        && Entrie.Id.EndWith(Strings.tierPrefix + tier)
                    select Entrie.Text;
            if (cur.Any())
            {
                Flag.ExchangeCurrency = true;
                MapName = cur.First();
            }
        }
        if (!Flag.Unidentified)
        {
            if (Flag.MapCategory && Flag.Unique && Name.Length > 0)
            {
                var cur =
                    from result in DataManager.Currencies
                    from Entrie in result.Entries
                    where Entrie.Text.Contain(Name)
                        && Entrie.Id.EndWith(Strings.tierPrefix + tier)
                    select Entrie.Text;
                if (cur.Any())
                {
                    Flag.ExchangeCurrency = true;
                    MapName = cur.First();
                }
            }
            else
            {
                var cur =
                    from result in DataManager.Currencies
                    from Entrie in result.Entries
                    where Entrie.Text == Type
                    select true;
                if (cur.Any() && cur.First())
                {
                    Flag.ExchangeCurrency = true;
                }
            }
        }
    }

    internal void UpdateBaseName(bool showDetail, int idLang)
    {
        if (showDetail)
        {
            var tmpBaseType = DataManager.Bases.FirstOrDefault(x => x.Name == Type);

            Base.Type = tmpBaseType is null ? Type : tmpBaseType.Name;
            Base.TypeEn = tmpBaseType is null ? string.Empty : tmpBaseType.NameEn;
        }
        if (!showDetail)
        {
            BaseResultData baseResult = null;
            if (Flag.CapturedBeast)
            {
                baseResult = DataManager.Monsters.FirstOrDefault(x => x.Name.Contain(Type));
                Base.Type = baseResult is null ? Type : baseResult.Name.Replace("\"", string.Empty);
                Base.TypeEn = baseResult is null ? string.Empty : baseResult.NameEn.Replace("\"", string.Empty);
                Name = string.Empty;
            }
            else
            {
                var cultureEn = new CultureInfo(Strings.Culture[0]);
                var rm = new System.Resources.ResourceManager(typeof(Resources.Resources));
                baseResult = DataManager.Bases.FirstOrDefault(x => x.Name == Type);
                Base.Type = baseResult is null ? Type : baseResult.Name;
                Base.TypeEn = baseResult is null ? string.Empty : baseResult.NameEn;
                if (Flag.BlightMap)
                {
                    Base.Type = Base.Type.Replace(Resources.Resources.General040_Blighted, string.Empty).Trim();
                    Base.TypeEn = Base.TypeEn.Replace(rm.GetString("General040_Blighted", cultureEn), string.Empty).Trim();
                }
                else if (Flag.BlightRavagedMap)
                {
                    Base.Type = Base.Type.Replace(Resources.Resources.General100_BlightRavaged, string.Empty).Trim();
                    Base.TypeEn = Base.TypeEn.Replace(rm.GetString("General100_BlightRavaged", cultureEn), string.Empty).Trim();
                }
            }
        }

        if (Base.TypeEn.Length is 0) //!item.Is.CapturedBeast
        {
            if (idLang is 0) // en
            {
                Base.TypeEn = Base.Type;
            }
            else
            {
                var enCur =
                    from result in DataManager.CurrenciesEn
                    from Entrie in result.Entries
                    where Entrie.Id == Id
                    select Entrie.Text;
                if (enCur.Any())
                {
                    Base.TypeEn = enCur.First();
                }
            }
        }

        Base.Name = Name;
        Base.NameEn = string.Empty;
        if (idLang is 0) //en
        {
            Base.NameEn = Base.Name;
        }
        else if (Name.Length > 0)
        {
            var wordRes = DataManager.Words.FirstOrDefault(x => x.Name == Name);
            if (wordRes is not null)
            {
                Base.NameEn = wordRes.NameEn;
            }
        }
    }

    internal void UpdateItemData(string[] clipData,int idLang)
    {
        if (Flag.CapturedBeast)
        {
            var tmpBaseType = DataManager.Monsters.FirstOrDefault(x => x.Name.Contain(Type));
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
                StringBuilder sbType = new(Type);
                sbType.Replace(Resources.Resources.General001_Anomalous, string.Empty)
                    .Replace(Resources.Resources.General002_Divergent, string.Empty)
                    .Replace(Resources.Resources.General003_Phantasmal, string.Empty).Replace("()", string.Empty);
                Type = sbType.ToString().Trim();
                if (Type.StartsWith(':'))
                {
                    Type = Type[1..].Trim();
                }

                if (Option[Resources.Resources.General037_Corrupt] is Strings.TrueOption
                    && Option[Resources.Resources.General038_Vaal] is Strings.TrueOption)
                {
                    for (int i = 3; i < clipData.Length; i++)
                    {
                        string seekVaal = clipData[i].Replace(Strings.CRLF, string.Empty).Trim();
                        var tmpBaseType = DataManager.Bases.FirstOrDefault(x => x.Name == seekVaal);
                        if (tmpBaseType is not null)
                        {
                            GemName = Type;
                            Type = tmpBaseType.Name;
                            break;
                        }
                    }
                }
            }

            if ((Flag.Unidentified || Flag.Normal) && Type.Contain(Resources.Resources.General030_Higher))
            {
                if (idLang is 2) // fr
                {
                    Type = Type.Replace(Resources.Resources.General030_Higher + "es", string.Empty).Trim();
                    Type = Type.Replace(Resources.Resources.General030_Higher + "e", string.Empty).Trim();
                }
                if (idLang is 3) // es
                {
                    Type = Type.Replace(Resources.Resources.General030_Higher + "es", string.Empty).Trim();
                }
                Type = Type.Replace(Resources.Resources.General030_Higher, string.Empty).Trim();
            }

            if (Flag.MapCategory && Type.Length > 5)
            {
                if (Type.Contain(Resources.Resources.General040_Blighted))
                {
                    Flag.BlightMap = true;
                }
                else if (Type.Contain(Resources.Resources.General100_BlightRavaged))
                {
                    Flag.BlightRavagedMap = true;
                }
            }
            else if (Option[Resources.Resources.General047_Synthesis] is Strings.TrueOption)
            {
                if (Type.Contain(Resources.Resources.General048_Synthesised))
                {
                    if (idLang is 2)
                    {
                        Type = Type.Replace(Resources.Resources.General048_Synthesised + "e", string.Empty).Trim(); // french female item name
                    }
                    if (idLang is 4)
                    {
                        StringBuilder iType = new(Type);
                        iType.Replace(Resources.Resources.General048_Synthesised + "s", string.Empty) // german
                            .Replace(Resources.Resources.General048_Synthesised + "r", string.Empty); // german
                        Type = iType.ToString().Trim();
                    }
                    if (idLang is 6)
                    {
                        StringBuilder iType = new(Type);
                        iType.Replace(Resources.Resources.General048_Synthesised + "ый", string.Empty) // russian
                            .Replace(Resources.Resources.General048_Synthesised + "ое", string.Empty) // russian
                            .Replace(Resources.Resources.General048_Synthesised + "ая", string.Empty); // russian
                        Type = iType.ToString().Trim();
                    }
                    Type = Type.Replace(Resources.Resources.General048_Synthesised, string.Empty).Trim();
                }
            }

            if (!Flag.Unidentified && !Flag.MapCategory && Flag.Magic)
            {
                var resultName =
                    from result in DataManager.Bases
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
            var tmpBaseType2 = DataManager.Bases.FirstOrDefault(x => x.Name == Type);
            if (tmpBaseType2 is not null)
            {
                // 3.14 : to remove and replace by itemClass
                //Strings.lpublicID.TryGetValue(tmpBaseType2.NameEn, out publicID);
                Flag.SpecialBase = Strings.lSpecialBases.Contains(tmpBaseType2.NameEn);
            }
        }

        if (Inherits.Length is 0)
        {
            if (Flag.MapCategory || Flag.Waystones)
            {
                //bool isGuardian = IsGuardianMap(itemType, out string guardName);
                if (!Flag.Unidentified && Flag.Magic)
                {
                    var affixes =
                        from result in DataManager.Mods
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

                string mapKind = Flag.BlightMap || Flag.BlightRavagedMap ? Strings.CurrencyTypePoe1.MapsBlighted :
                    Flag.Unique ? Strings.CurrencyTypePoe1.MapsUnique : Strings.CurrencyTypePoe1.Maps;

                var mapId =
                    from result in DataManager.Currencies
                    from Entrie in result.Entries
                    where (result.Id == mapKind || result.Id == Strings.CurrencyTypePoe2.Waystones)
                    && (Entrie.Text.StartWith(Type)
                    || Entrie.Text.EndWith(Type))
                    select Entrie.Id;
                if (mapId.Any())
                {
                    Id = mapId.First();
                }

                Inherits = Flag.MapCategory ? "Maps/AbstractMap" : "Waystones";
            }
            else if (Flag.Currency || Flag.Divcard || Flag.MapFragment)
            {
                var curResult =
                    from resultDat in DataManager.Currencies
                    from Entrie in resultDat.Entries
                    where Entrie.Text == Type
                    select (Entrie.Id, resultDat.Id);
                if (curResult.Any())
                {
                    Id = curResult.FirstOrDefault().Item1;
                    string cur = curResult.FirstOrDefault().Item2;

                    Inherits = cur is Strings.CurrencyTypePoe1.Cards ? "DivinationCards/DivinationCardsCurrency"
                        : cur is Strings.CurrencyTypePoe1.DelveResonators ? "Delve/DelveSocketableCurrency"
                        : cur is Strings.CurrencyTypePoe1.Fragments && Id != "ritual-vessel"
                        && Id != "valdos-puzzle-box" ? "MapFragments/AbstractMapFragment"
                        : cur is Strings.CurrencyTypePoe1.Incubators ? "Legion/Incubator"
                        : "Currency/StackableCurrency";
                }
            }
            else if (Flag.Gems)
            {
                var findGem = DataManager.Gems.FirstOrDefault(x => x.Name == Type);
                if (findGem is not null)
                {
                    if (GemName.Length is 0 && findGem.Type != findGem.Name) // transfigured normal gem
                    {
                        Type = findGem.Type;
                        Inherits = Strings.Inherit.Gems + '/' + findGem.Disc;
                    }
                    if (GemName.Length > 0 && findGem.Type == findGem.Name)
                    {
                        var findGem2 = DataManager.Gems.FirstOrDefault(x => x.Name == GemName);
                        if (findGem2 is not null) // transfigured vaal gem
                        {
                            Inherits = Strings.Inherit.Gems + '/' + findGem2.Disc;
                        }
                    }
                }
            }

            if (Inherits.Length is 0)
            {
                var tmpBaseType = DataManager.Bases.FirstOrDefault(x => x.Name == Type);
                if (tmpBaseType is not null)
                {
                    Id = tmpBaseType.Id;
                    Inherits = tmpBaseType.InheritsFrom;
                }
            }
        }

        Base.Inherits = Inherits.Split('/')[0] is Strings.Inherit.Jewels or Strings.Inherit.Armours or Strings.Inherit.Weapons ? Id.Split('/') : Inherits.Split('/');
        if (Flag.Chronicle || Flag.Ultimatum || Flag.MirroredTablet || Flag.SanctumResearch) Base.Inherits[1] = "Area";
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
    private static Tuple<string, string> GetTranslatedItemNameAndType(string itemName, string itemType)
    {
        string name = itemName;
        string type = itemType;

        if (name.Length > 0)
        {
            var word = DataManager.Words.FirstOrDefault(x => x.NameEn == name);
            if (word is not null && !word.Name.Contain('/'))
            {
                name = word.Name;
            }
        }

        var resultName =
                    from result in DataManager.Bases
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

        var baseType = DataManager.Bases.FirstOrDefault(x => x.NameEn == type);
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
                    from result in DataManager.CurrenciesEn
                    from Entrie in result.Entries
                    where isMap ? Entrie.Text.Contain(type) : Entrie.Text == type
                    select Entrie.Id;
            if (enCur.Any())
            {
                var cur = from result in DataManager.Currencies
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
            var findGem = DataManager.Gems.FirstOrDefault(x => x.NameEn == type);
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
            { Resources.Resources.General037_Corrupt, string.Empty },
            { Resources.Resources.General109_Mirrored, string.Empty },
            { Resources.Resources.General110_FoilUnique, string.Empty },
            { Resources.Resources.General039_Unidentify, string.Empty },
            { Resources.Resources.General038_Vaal, string.Empty },
            { Strings.AlternateGem, string.Empty },
            { Strings.Stat.IncPhys, string.Empty },
            { Strings.Stat.IncAs, string.Empty },
            { Resources.Resources.Main154_tbFacetor, string.Empty },
            { Resources.Resources.General070_ReqSacrifice, string.Empty },
            { Resources.Resources.General071_Reward, string.Empty },
            { Resources.Resources.General099_ScourgedItem, string.Empty },
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
            { Resources.Resources.General149_FireDamage, string.Empty }
        };
    }
}
