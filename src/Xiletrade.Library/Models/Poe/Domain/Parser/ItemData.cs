using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Xiletrade.Library.Models.Application.Configuration.DTO.Extension;
using Xiletrade.Library.Models.Poe.Contract.Extension;
using Xiletrade.Library.Models.Poe.Contract.One;
using Xiletrade.Library.Models.Poe.Domain.Extension;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.Models.Poe.Domain.Parser;

//WIP
internal sealed class ItemData
{
    // immutable, init with constructor
    private readonly DataManagerService _dm;

    /// <summary>Maximum number of mods to display.</summary>
    internal const int NB_MAX_MODS = 30;

    internal List<ModLine> ModList { get; }
    internal ItemFlag Flag { get; }
    internal ItemState State { get; }
    internal TotalStats Stats { get; }

    internal Lang Lang { get; }

    internal bool IsPoe2 { get; }

    internal string Class { get; }
    internal string Rarity { get; }
    internal string Name { get; }
    internal string Type { get; }
    internal string NameEn { get; }
    internal string TypeEn { get; }
    internal string Id { get; }
    internal string IdCurrency { get; }

    // non-immutable
    internal Dictionary<string, string> Option { get; } = InitListOption(); //TODO: remove this legacy parsing & move to item flag level ?

    internal string Quality => RegexUtil.NumericalPattern().
        Replace(Option[Resources.Resources.General035_Quality].Trim(), string.Empty);
    internal string MapTier => Option[Resources.Resources.General034_MaTier].Replace(" ", string.Empty);

    /// <summary>
    /// Translate item name in the correct language used by the trade gateway
    /// </summary>
    /// <param name="item"></param>
    internal string NameGateway
    {
        get
        {
            if (_dm.Config.Options.Gateway == _dm.Config.Options.Language)
            {
                return Name;
            }

            if (Name.Length > 0 && NameEn.Length > 0)
            {
                var word = _dm.WordsGateway.FindWordByNameEn(NameEn);
                if (word is not null && word.Name.Length > 0 && word.Name.IndexOf('/') is -1)
                {
                    return word.Name;
                }
            }
            return Name;
        }
    }

    /// <summary>
    /// Translate item type in the correct language used by the trade gateway
    /// </summary>
    /// <param name="item"></param>
    internal string TypeGateway
    {
        get
        {
            if (_dm.Config.Options.Gateway == _dm.Config.Options.Language
                || Type.Length is 0 || TypeEn.Length is 0)
            {
                return Type;
            }

            var bases = _dm.BasesGateway.FindBaseByNameEn(TypeEn);
            if (bases is not null)
            {
                return bases.Name.Length > 0 ? bases.Name : Type;
            }

            var cur = _dm.Currencies.FindEntryByType(Type);
            if (cur is not null && !string.IsNullOrEmpty(cur.Id))
            {
                var curGateway = _dm.CurrenciesGateway.FindEntryById(cur.Id);
                if (curGateway is not null && !string.IsNullOrEmpty(curGateway.Text))
                {
                    return curGateway.Text;
                }
            }

            return Type;
        }
    }

    internal ItemData(DataManagerService dm, InfoDescription infoDesc)
    {
        _dm = dm;
        Lang = (Lang)_dm.Config.Options.Language;
        IsPoe2 = _dm.Config.Options.GameVersion is 1;

        ReadOnlySpan<char> itemHeader = infoDesc.Item[0].AsSpan().Trim();
        int lineCount = itemHeader.CountOccurrences(Strings.CRLF);
        Span<Range> lineRanges = lineCount <= 4 ? stackalloc Range[4] : new Range[lineCount];
        lineRanges.SplitAndValidate(itemHeader, Strings.CRLF, lineCount); // Fill range values

        Class = lineCount > 0 ? itemHeader[lineRanges[0]].GetTrimAfterSeparator(':') : string.Empty;
        Rarity = lineCount > 1 ? itemHeader[lineRanges[1]].GetTrimAfterSeparator(':') : string.Empty;
        
        var name = lineCount > 2 && !itemHeader[lineRanges[2]].IsEmpty ? itemHeader[lineRanges[2]].Trim().ToString() : string.Empty;
        var type = GetItemType(itemHeader, lineRanges, lineCount);

        Flag = new ItemFlag(infoDesc, Rarity, type, Class);

        Name = GetParsedName(name);
        NameEn = GetEnglishName(Name, Lang);
        (Type, TypeEn) = GetTypes(Lang, Id, infoDesc, type);
        (Id, IdCurrency) = GetItemIds(Type);

        if (Flag.Parseable)
        {
            ModList = GetModList(infoDesc);
            Stats = new(_dm.FilterEn, Lang, Flag, ModList);
        }
        State = new(_dm, ModList, Flag, Type);
    }

    private (string Id, string IdCurrency) GetItemIds(ReadOnlySpan<char> type)
    {
        if (Flag.Currency || Flag.Divcard || Flag.MapFragment)
        {
            var (Entry, GroupId) = _dm.Currencies.FindEntryAndGroupIdByType(type, image: false);
            if (Entry is not null)
            {
                return (Entry.Id, GroupId);
            }
        }
        var findBase = _dm.Bases.FindBaseByName(type);
        return (findBase is not null ? findBase.Id : string.Empty, string.Empty);
    }

    private (string Type, string TypeEn) GetTypes(Lang lang, ReadOnlySpan<char> itemId, InfoDescription infoDesc, string type)
    {
        type = GetParsedType(type);
        var typeEn = string.Empty;
        if (Flag.ShowDetail)
        {
            if (Flag.VaalSkillGems)
            {
                var vaalName = GetVaalGemName(infoDesc);
                if (vaalName.Length > 0)
                {
                    type = vaalName;
                }
            }
            var findBase = _dm.Bases.FindBaseByName(type);
            type = findBase is null ? type : findBase.Name;
            typeEn = findBase is null ? string.Empty : findBase.NameEn;
        }
        if (!Flag.ShowDetail)
        {
            if (Flag.CapturedBeast)
            {
                var findMonster = _dm.Monsters.FindMonsterByName(type, nospirit: true);
                type = findMonster is null ? type : findMonster.Name.Replace("\"", string.Empty);
                typeEn = findMonster is null ? string.Empty : findMonster.NameEn.Replace("\"", string.Empty);
            }
            else
            {
                var rm = Resources.Resources.ResourceManager;
                var cult = CultureInfo.InvariantCulture;

                var findBase = _dm.Bases.FindBaseByName(type);
                type = findBase is null ? type : findBase.Name;
                typeEn = findBase is null ? string.Empty : findBase.NameEn;
                if (Flag.MapBlight)
                {
                    type = type.Replace(Resources.Resources.General040_Blighted, string.Empty).Trim();
                    typeEn = typeEn.Replace(rm.GetString("General040_Blighted", cult), string.Empty).Trim();
                }
                else if (Flag.MapBlightRavaged)
                {
                    type = type.Replace(Resources.Resources.General100_BlightRavaged, string.Empty).Trim();
                    typeEn = typeEn.Replace(rm.GetString("General100_BlightRavaged", cult), string.Empty).Trim();
                }
            }
        }
        if (typeEn.Length is 0) //!item.Is.CapturedBeast
        {
            if (lang is Lang.English)
            {
                typeEn = type;
            }
            else
            {
                var typeEnglish = _dm.CurrenciesEn.FindEntryById(itemId);
                if (typeEnglish is not null && !string.IsNullOrEmpty(typeEnglish.Text))
                {
                    typeEn = typeEnglish.Text;
                }
            }
        }
        return (type, typeEn);
    }

    private string GetVaalGemName(InfoDescription infoDesc)
    {
        for (int i = 3; i < infoDesc.Item.Length; i++)
        {
            string seekVaal = infoDesc.Item[i].Replace(Strings.CRLF, string.Empty).Trim();
            var findBase = _dm.Bases.FindBaseByName(seekVaal);
            if (findBase is not null)
            {
                return findBase.Name;
            }
        }
        return string.Empty;
    }

    private string GetEnglishName(ReadOnlySpan<char> name, Lang lang)
    {
        if (lang is Lang.English)
        {
            return name.ToString();
        }
        if (name.Length > 0)
        {
            var wordRes = _dm.Words.FindWordByName(name);
            if (wordRes is not null)
            {
                return wordRes.NameEn;
            }
        }
        return string.Empty;
    }

    private string GetParsedName(ReadOnlySpan<char> name)
    {
        if (Flag.CapturedBeast || (Flag.Transfigured && !Flag.VaalSkillGems))
            return string.Empty;

        if (!IsPoe2 && Flag.Unique)
        {
            var foulborn = Resources.Resources.General166_Foulborn.AsSpan();
            int index = name.IdxOf(foulborn);
            if (index >= 0)
            {
                return string.Concat(name[..index], name[(index + foulborn.Length)..]).Trim();
            }
        }
        return name.ToString();
    }

    private string GetParsedType(string type)
    {
        if (Flag.Unidentified || Flag.Normal)
        {
            type = type.RemoveStringFromArrayDesc(Resources.Resources.General030_Higher.Split('/'))
                .RemoveStringFromArrayDesc(Resources.Resources.General159_Exceptional.Split('/'));
        }
        if (Flag.Synthesised)
        {
            type = type.RemoveStringFromArrayDesc(Resources.Resources.General048_Synthesised.Split('/'));
        }
        if (!Flag.Unidentified && !Flag.Map && Flag.Magic)
        {
            string longestName = _dm.Bases.GetLongestMatchingName(type);
            if (!string.IsNullOrEmpty(longestName))
            {
                type = longestName;
            }
        }
        if ((Flag.Map || Flag.Waystones) && !Flag.Unidentified && Flag.Magic)
        {
            var affixes = _dm.Mods.GetMatchingAffixesList(Type);
            if (affixes.Count > 0)
            {
                foreach (var affix in affixes)
                {
                    type = type.Replace(affix, string.Empty).Trim();
                }
            }
        }
        return type;
    }

    private static string GetItemType(ReadOnlySpan<char> data, Span<Range> ranges, int lineCount)
    {
        if (lineCount <= 1)
            return string.Empty;

        int max = lineCount > 3 ? 3 : lineCount - 1;

        for (int i = max; i >= 1; i--)
        {
            var span = data[ranges[i]];

            if (!span.IsEmpty)
                return span.Trim().ToString();
        }

        return string.Empty;
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
                var isSocket = keyStr == Resources.Resources.General036_Socket;
                var minLength = isSocket ? 1 : 2;
                Option[keyStr] = valueSpan.Length >= minLength ? valueSpan.ToString() : Strings.TrueOption;
            }
            return true;
        }

        if (Flag.Gems)
        {
            return !Flag.Imbued;
        }

        var cond = (Flag.ItemLevel || Flag.AreaLevel || Flag.FilledCoffin) && BelowMaxMods;
        if (!cond || SkipBetweenBrackets(data, Flag.Ultimatum))
        {
            return true;
        }

        return false;
    }

    private string[] GetSanctumMods()
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

    private static bool SkipBetweenBrackets(ReadOnlySpan<char> data, bool ultimatum)
    {
        if (ultimatum)
        {
            return data.StartsWith('(') || data.EndsWith(')');
        }
        return data.StartsWith('(') && data.EndsWith(')');
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

    internal string GetDetails(InfoDescription infodesc)
    {
        string details;
        if (Flag.Incubator || Flag.Gems || Flag.Pieces) // || is_essences
        {
            int i = Flag.Gems ? 3 : 1;
            details = infodesc.Item.Length > 2 ? (Flag.Gems ?
                infodesc.Item[i] : string.Empty) + infodesc.Item[i + 1] : string.Empty;
        }
        else
        {
            int i = Flag.Divcard || Flag.StackableCurrency ? 2 : 1;
            details = infodesc.Item.Length > i + 1 ? infodesc.Item[i] + infodesc.Item[i + 1] : infodesc.Item[^1];

            if (infodesc.Item.Length > i + 1)
            {
                int v = infodesc.Item[i - 1].TrimStart().IndexOf("Apply: ", StringComparison.Ordinal);
                details += v > -1 ? string.Empty + Strings.LF + Strings.LF + infodesc.Item[i - 1].TrimStart().Split(Strings.LF)[v == 0 ? 0 : 1].TrimEnd() : string.Empty;
                if (Flag.SanctumResearch && infodesc.Item.Length >= 5)
                {
                    details += infodesc.Item[3] + infodesc.Item[4];
                }
            }
        }

        if (Lang is Lang.English)
        {
            details = details.Replace(Resources.Resources.General097_SClickSplitItem, string.Empty);
            details = RegexUtil.DetailPattern().Replace(details, string.Empty);
        }

        return details;
    }

    private List<ModLine> GetModList(InfoDescription infoDesc)
    {
        var modList = new List<ModLine>();
        var startIndexParsing = Flag.Imbued ? 5 : 1;
        for (int idx = startIndexParsing; idx < infoDesc.Item.Length; idx++)
        {
            if ((Flag.Flask || Flag.Charm) && idx is 1)
            {
                continue;
            }
            var data = GetDataAndParseSanctumDelirium(infoDesc, idx);
            var lSubMods = GetModsFromData(data);
            if (lSubMods.Count > 0)
            {
                modList.AddRange(lSubMods);
            }
        }
        return modList;
    }

    private List<ModLine> GetModsFromData(ReadOnlyMemory<string> dataMemory)
    {
        var lMods = new List<ModLine>();
        ModDescription pendingDesc = null;
        var data = dataMemory.Span;

        for (int i = 0; i < data.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(data[i]))
            {
                continue;
            }

            var desc = new ModDescription(_dm, data[i]);
            if (desc.IsParsed)
            {
                pendingDesc = desc;
                continue;
            }

            // pendingDesc can be used for more than one mod
            var affix = new AffixFlag(data[i], pendingDesc);
            if (UpdateOption(affix.ParsedData, lMods.Count < NB_MAX_MODS))
            {
                continue;
            }

            var modifier = new ItemModifier(_dm, this, affix, GetNextMod(data, i));
            if (modifier.IsBreakpointMod)
            {
                break;
            }

            var modFilter = new ModFilter(_dm, modifier, this);
            if (!modFilter.IsFetched)
            {
                continue;
            }

            lMods.Add(new(_dm, this, modFilter));
        }
        return lMods.HandleDuplicates();
    }

    private static string GetNextMod(ReadOnlySpan<string> data, int index)
    {
        int next = index + 1;

        if (next >= data.Length)
            return string.Empty;

        var value = data[next];
        return string.IsNullOrEmpty(value) ? string.Empty : new AffixFlag(value).ParsedData;
    }

    private string[] GetDataAndParseSanctumDelirium(InfoDescription infoDesc, int infoIndex)
    {
        var data = infoDesc.Item[infoIndex].Trim().Split(Strings.CRLF, StringSplitOptions.None);

        bool sameReward = false;
        for (int i = 0; i < data.Length; i++)
        {
            if (data[i].StartWith(Resources.Resources.General098_DeliriumReward))
            {
                sameReward = true;
                break;
            }
        }
        if (sameReward)
        {
            data = [.. data.Distinct()];
        }

        if (Flag.SanctumResearch && infoIndex == infoDesc.Item.Length - 1) // at the last loop
        {
            var sanctumMods = GetSanctumMods();
            if (sanctumMods.Length > 0)
            {
                Array.Resize(ref data, data.Length + sanctumMods.Length);
                Array.Copy(sanctumMods, 0, data, data.Length - sanctumMods.Length, sanctumMods.Length);
            }
        }

        return data;
    }

    // TODO MOVE to JSON factory
    private GemTransfigured GetTransfiguredGem(ReadOnlySpan<char> vaalGemName, string type)
    {
        var alt = string.Empty;
        var findGem = _dm.Gems.FindGemByName(type);
        bool isVaal = vaalGemName.Length > 0;
        if (findGem is not null)
        {
            if (!isVaal && findGem.Type != findGem.Name) // transfigured normal gem
            {
                type = findGem.Type;
                alt = findGem.Disc;
            }
            if (isVaal && findGem.Type == findGem.Name)
            {
                var findGem2 = _dm.Gems.FindGemByName(vaalGemName);
                if (findGem2 is not null) // transfigured vaal gem
                {
                    alt = findGem2.Disc;
                }
            }
        }
        return new(type, alt);
    }
}
