using System;
using System.Collections.Generic;
using System.Linq;
using Xiletrade.Library.Models.Application.Configuration.DTO.Extension;
using Xiletrade.Library.Models.Poe.Contract.Extension;
using Xiletrade.Library.Models.Poe.Domain.Extension;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.Models.Poe.Domain.Parser;

internal sealed class ItemData
{
    // immutable, init with constructor
    private readonly DataManagerService _dm;

    /// <summary>Maximum number of mods to store.</summary>
    internal const int NB_MAX_MODS = 30;

    internal List<ModLine> ModList { get; }
    internal ItemFlag Flag { get; }
    internal ItemState State { get; }
    internal ItemOption Options { get; }
    internal TotalStats Stats { get; }

    internal Lang Lang { get; }

    internal bool IsPoe2 { get; }

    internal string Rarity { get; }
    internal string Name { get; }
    internal string Type { get; }
    internal string NameEn { get; }
    internal string TypeEn { get; }
    internal string Id { get; }
    internal string IdCurrency { get; }

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
            if (Name.Length > 0 && NameEn.Length > 0 && _dm.WordsGateway.FindWordByNameEn(NameEn) is var word 
                && word is not null && word.Name.Length > 0 && word.Name.IndexOf('/') is -1)
            {
                return word.Name;
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
            if (_dm.BasesGateway.FindBaseByNameEn(TypeEn) is var findBase && findBase is not null)
            {
                return findBase.Name.Length > 0 ? findBase.Name : Type;
            }
            if (_dm.Currencies.FindEntryByType(Type) is var cur && cur is not null
                && !string.IsNullOrEmpty(cur.Id))
            {
                if (_dm.CurrenciesGateway.FindEntryById(cur.Id) is var curGateway && curGateway is not null
                    && !string.IsNullOrEmpty(curGateway.Text))
                {
                    return curGateway.Text;
                }
            }
            return Type;
        }
    }

    public ItemData(DataManagerService dm, InfoDescription infoDesc)
    {
        _dm = dm;
        Lang = (Lang)_dm.Config.Options.Language;
        IsPoe2 = _dm.Config.Options.GameVersion is 1;

        var header = new ItemHeader(infoDesc);
        Rarity = header.Rarity;
        Flag = new ItemFlag(infoDesc, header);
        (Type, TypeEn) = GetTypes(Flag, infoDesc, header.Type);
        (Id, IdCurrency) = GetItemIds(Type);
        
        NameEn = GetParsedEnglishName(Flag, header.Name);
        Name = Lang is Lang.English ? NameEn : GetTranslatedName(dm, Flag, NameEn);

        Options = new();
        if (Flag.Parseable)
        {
            ModList = GetModList(infoDesc);
            Stats = new(_dm.FilterEn, Lang, Flag, ModList);
        }
        State = new(_dm, ModList, Flag, Type);
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

    //private
    private (string Id, string IdCurrency) GetItemIds(ReadOnlySpan<char> type)
    {
        if (Flag.Currency || Flag.Divcard || Flag.MapFragment || (Flag.SupportGems && IsPoe2))
        {
            var (Entry, GroupId) = _dm.Currencies.FindEntryAndGroupIdByType(type, image: false);
            if (Entry is not null)
            {
                return (Entry.Id, GroupId);
            }
        }
        return (_dm.Bases.FindBaseByName(type) is var findBase && findBase is not null ? 
            findBase.Id : string.Empty, string.Empty);
    }

    private (string Type, string TypeEn) GetTypes(ItemFlag flag, InfoDescription infoDesc, ReadOnlySpan<char> inpuType)
    {
        var typeEn = GetParsedType(inpuType);
        var type = string.Empty;
        if (flag.ShowDetail || flag.Waystones)
        {
            if (flag.Currency || flag.Divcard || flag.MapFragment || flag.Waystones)
            {
                if (_dm.CurrenciesEn.FindEntryByType(typeEn) is var typeEng && typeEng is not null
                    && !string.IsNullOrEmpty(typeEng.Id))
                {
                    if (_dm.Currencies.FindEntryById(typeEng.Id) is var typeLang && typeLang is not null
                    && !string.IsNullOrEmpty(typeLang.Text))
                    {
                        type = typeLang.Text;
                    }
                }
            }
            if (flag.VaalSkillGems)
            {
                var vaalName = GetVaalGemName(infoDesc);
                if (vaalName.Length > 0)
                {
                    type = vaalName;
                    if (_dm.Bases.FindBaseByName(type) is var vaalGem && vaalGem is not null)
                    {
                        typeEn = vaalGem.NameEn;
                    }
                }
            }
            if (type.Length is 0 && flag.Transfigured
                && _dm.Gems.FindGemByNameEn(typeEn) is var findGem
                && findGem is not null && !string.IsNullOrEmpty(findGem.Name))
            {
                type = findGem.Name;
            }
        }
        if (flag.CapturedBeast)
        {
            if (_dm.Monsters.FindMonsterByNameEn(typeEn, nospirit: true) is var findMonster
                && findMonster is not null && !string.IsNullOrEmpty(findMonster.Name))
            {
                type = findMonster.Name.Replace("\"", string.Empty);
            }
        }
        if (type.Length is 0 && _dm.Bases.FindBaseByNameEn(typeEn) is var findBase
            && findBase is not null && !string.IsNullOrEmpty(findBase.Name))
        {
            type = findBase.Name;
        }
        // item type for special cases here
        if (flag.Facetor)
        {
            type = Resources.Resources.General064_FacetorLens;
        }
        return (type, typeEn);
    }

    private string GetVaalGemName(InfoDescription infoDesc)
    {
        for (int i = 3; i < infoDesc.Item.Length; i++)
        {
            string seekVaal = infoDesc.Item[i].Replace(Strings.CRLF, string.Empty).Trim();
            if (_dm.Bases.FindBaseByNameEn(seekVaal) is var findBase && findBase is not null)
            {
                return findBase.Name;
            }
        }
        return string.Empty;
    }

    private string GetParsedEnglishName(ItemFlag flag, ReadOnlySpan<char> dataName)
    {
        if (flag.CapturedBeast || flag.Currency || flag.Divcard || flag.MapFragment
            || (flag.Gems && !(flag.Transfigured && flag.VaalSkillGems)))
            return string.Empty;

        if (!IsPoe2 && flag.Unique)
        {
            var rm = Resources.Resources.ResourceManager;
            var foulborn = rm.GetEnglish(nameof(Resources.Resources.General166_Foulborn)).AsSpan();
            int index = dataName.IdxOf(foulborn);
            if (index >= 0)
            {
                return string.Concat(dataName[..index], dataName[(index + foulborn.Length)..]).Trim();
            }
        }
        return dataName.ToString();
    }

    private string GetParsedType(ReadOnlySpan<char> inputType)
    {
        var type = string.Empty;
        if (Flag.Unidentified || Flag.Normal || Flag.Synthesised || Flag.MapBlight || Flag.MapBlightRavaged)
        {
            var rm = Resources.Resources.ResourceManager;
            if (Flag.Unidentified || Flag.Normal)
            {
                var higher = rm.GetEnglish(nameof(Resources.Resources.General030_Higher)).Split('/');
                var exceptional = rm.GetEnglish(nameof(Resources.Resources.General159_Exceptional)).Split('/');
                type = inputType.RemoveStringFromArrayDesc(higher).RemoveStringFromArrayDesc(exceptional);
            }
            if (Flag.Synthesised)
            {
                var synth = rm.GetEnglish(nameof(Resources.Resources.General048_Synthesised)).Split('/');
                type = inputType.RemoveStringFromArrayDesc(synth);
            }
            if (Flag.MapBlight)
            {
                var blight = rm.GetEnglish(nameof(Resources.Resources.General040_Blighted));
                type = inputType.StartWith(blight)
                    ? inputType[blight.Length..].Trim().ToString() : inputType.Trim().ToString();
            }
            if (Flag.MapBlightRavaged)
            {
                var ravaged = rm.GetEnglish(nameof(Resources.Resources.General100_BlightRavaged));
                type = inputType.StartWith(ravaged)
                    ? inputType[ravaged.Length..].Trim().ToString() : inputType.Trim().ToString();
            }
        }
        if (!Flag.Unidentified && !Flag.Map && Flag.Magic)
        {
            string longestName = _dm.Bases.GetLongestMatchingNameEn(inputType);
            if (!string.IsNullOrEmpty(longestName))
            {
                type = longestName;
            }
        }
        if ((Flag.Map || Flag.Waystones) && !Flag.Unidentified && Flag.Magic)
        {
            var affixes = _dm.Mods.GetMatchingAffixesEnList(inputType);
            if (affixes.Count > 0)
            {
                type = inputType.ToString();
                foreach (var affix in affixes.OrderByDescending(x => x.Length))
                {
                    type = type.Replace(affix, string.Empty).Trim();
                }
                type = RegexUtil.MultipleSpace().Replace(type, " ");
            }
        }
        return type.Length > 0 ? type : inputType.ToString();
    }

    private bool FindContinuePoint(ReadOnlySpan<char> data, bool BelowMaxMods)
    {
        if (Flag.Gems)
        {
            return !Flag.Imbued;
        }

        var cond = (Flag.ItemLevel || Flag.AreaLevel) && BelowMaxMods;
        if (!cond || SkipBetweenBrackets(data, Flag.Ultimatum))
        {
            return true;
        }

        return false;
    }

    private string[] GetSanctumMods()
    {
        List<string> lMods = new(), lEntrie = new();

        if (Options.MajorBoons is var majBoons && majBoons[0].Length > 0)
        {
            lEntrie.AddRange(majBoons);
        }
        if (Options.MajorAfflictions is var majAfflictions && majAfflictions[0].Length > 0)
        {
            lEntrie.AddRange(majAfflictions);
        }
        if (Options.Pacts is var pacts && pacts[0].Length > 0)
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
        if (Options.RewardsFloor is var floorRewards && floorRewards[0].Length > 0)
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
        if (Options.RewardsSanctum is var sanctumRewards && sanctumRewards[0].Length > 0)
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

    private List<ModLine> GetModList(InfoDescription infoDesc)
    {
        var modList = new List<ModLine>();
        for (int idx = 1; idx < infoDesc.Item.Length; idx++)
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
            if (Options.Update(affix.ParsedData) 
                || FindContinuePoint(affix.ParsedData, lMods.Count < NB_MAX_MODS))
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

    private static string GetTranslatedName(DataManagerService dm, ItemFlag flag, ReadOnlySpan<char> nameEn)
    {
        if (nameEn.Length is 0)
        {
            return string.Empty;
        }
        if (dm.Words.FindWordByNameEn(nameEn) is var word && word is not null)
        {
            return word.Name;
        }
        if (dm.Bases.FindBaseByNameEn(nameEn) is var bases && bases is not null)
        {
            return bases.Name;
        }
        if (dm.Gems.FindGemByNameEn(nameEn) is var gem && gem is not null)
        {
            return gem.Name;
        }

        // Handle magic
        if (!flag.Unidentified && flag.Magic)
        {
            // TODO with dm.Mods & dm.Bases
        }
        // Handle rares
        int wordCount = 0;
        var wordList = new List<string>();
        foreach (Range range in nameEn.Split(' '))
        {
            wordCount++;
            if (dm.Words.FindWordByNameEn(nameEn[range]) is var part && part is not null)
            {
                wordList.Add(part.Name.Split('/')[0]);
                continue;
            }
            //TODO
        }
        if (wordCount > 0 && wordCount == wordList.Count)
        {
            // TO DO : reorder words from wordList per lang and item conditions (MS,FS,NS,MP,FP,NP)
            return string.Join(' ', wordList.OrderBy(s => char.IsLower(s[0])).ThenBy(s => s));
        }
        return string.Empty;
    }
}
