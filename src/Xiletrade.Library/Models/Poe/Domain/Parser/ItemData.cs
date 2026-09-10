using System;
using System.Collections.Generic;
using System.Linq;
using Xiletrade.Library.Models.Application.Configuration.DTO.Extension;
using Xiletrade.Library.Models.Poe.Contract.Extension;
using Xiletrade.Library.Models.Poe.Domain.Extension;
using Xiletrade.Library.Models.Poe.Domain.Parser.Flag;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.Models.Poe.Domain.Parser;

/// <summary>
/// Representative model of a fully parsed PoE 1/2 item.
/// </summary>
internal sealed class ItemData
{
    // immutable, init with constructor
    private readonly DataManagerService _dm;

    /// <summary>Maximum number of mods to store.</summary>
    internal const int NB_MAX_MODS = 30;

    internal List<ModLine> ModList { get; }
    internal ItemFlag Flag { get; }
    internal ItemRule Rule { get; }
    internal ItemState State { get; }
    internal ItemOption Options { get; }
    internal ItemDamage Damage { get; }
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
            if (Flag.Tag.Unidentified)
            {
                return string.Empty;
            }
            if (Flag.Map.Chart && _dm.Items.FindEntryByText(Name) is var item && !string.IsNullOrEmpty(item?.Type))
            {
                return item.Type;
            }
            if (_dm.Config.Options.Gateway == _dm.Config.Options.Language)
            {
                return Name;
            }
            if (Name.Length > 0 && NameEn.Length > 0 && _dm.WordsGateway.FindWordByNameEn(NameEn) is var word
                && !string.IsNullOrEmpty(word?.Name) && word.Name.IndexOf('/') is -1)
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

    /// <summary>
    /// Representative model of a fully parsed PoE 1/2 item.
    /// </summary>
    /// <param name="dm"></param>
    /// <param name="infoDesc"></param>
    public ItemData(DataManagerService dm, InfoDescription infoDesc)
    {
        _dm = dm;
        Lang = (Lang)_dm.Config.Options.Language;
        IsPoe2 = _dm.Config.Options.GameVersion is 1;

        var header = new ItemHeader(infoDesc);
        Rarity = header.Rarity;
        Flag = new(infoDesc, header);
        Rule = new(Flag);
        (Type, TypeEn) = GetTypes(Flag, Rule, infoDesc, header.Type);
        (Id, IdCurrency) = GetItemIds(Flag, Type);

        Options = new();
        if (Rule.Parseable)
        {
            ModList = GetModList(Options, Flag, infoDesc);
        }

        Name = GetName(Options, Flag, infoDesc, header.Name, IsPoe2);
        NameEn = Lang is Lang.English ? Name : GetEnglishdName(Flag, Name);

        Stats = new(_dm, Flag, Rule, ModList, Lang, IsPoe2);
        State = new(_dm, Flag, Rule, ModList, Type);
        Damage = new(Flag, Rule, Stats, Options, Lang);
    }

    internal string GetDetails(InfoDescription infodesc)
    {
        string details;
        if (Flag.Incubator || Flag.Gem.IsGem || Flag.Pieces) // || is_essences
        {
            int i = Flag.Gem.IsGem ? 3 : 1;
            details = infodesc.Item.Length > 2 ? (Flag.Gem.IsGem ?
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
                if (Flag.Area.SanctumResearch && infodesc.Item.Length >= 5)
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
    private (string Id, string IdCurrency) GetItemIds(ItemFlag flag, ReadOnlySpan<char> type)
    {
        if (flag.Currency || flag.Divcard || flag.Map.Fragment || flag.Breachstone || (flag.Gem.Support && IsPoe2))
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

    private (string Type, string TypeEn) GetTypes(ItemFlag flag, ItemRule rule, InfoDescription infoDesc, ReadOnlySpan<char> inputType)
    {
        var type = string.Empty;
        var typeEn = string.Empty;
        if (flag.Tag.Unidentified || flag.Rarity.Normal || flag.Tag.Synthesised || flag.Map.Blight || flag.Map.BlightRavaged)
        {
            var rm = Resources.Resources.ResourceManager;
            if (flag.Tag.Unidentified || flag.Rarity.Normal)
            {
                var higher = Resources.Resources.General030_Higher.Split('/');
                var exceptional = Resources.Resources.General159_Exceptional.Split('/');
                type = inputType.RemoveStringFromArrayDesc(higher).RemoveStringFromArrayDesc(exceptional);
                typeEn = _dm.Bases.FindBaseByName(type)?.NameEn ?? typeEn;
            }
            if (flag.Tag.Synthesised)
            {
                var synth = rm.GetEnglish(nameof(Resources.Resources.General048_Synthesised)).Split('/');
                type = inputType.RemoveStringFromArrayDesc(synth);
            }
            if (flag.Map.Blight)
            {
                var blight = rm.GetEnglish(nameof(Resources.Resources.General040_Blighted));
                type = inputType.StartWith(blight)
                    ? inputType[blight.Length..].Trim().ToString() : inputType.Trim().ToString();
            }
            if (flag.Map.BlightRavaged)
            {
                var ravaged = rm.GetEnglish(nameof(Resources.Resources.General100_BlightRavaged));
                type = inputType.StartWith(ravaged)
                    ? inputType[ravaged.Length..].Trim().ToString() : inputType.Trim().ToString();
            }
        }
        if (!flag.Tag.Unidentified && !flag.Map.IsMap && flag.Rarity.Magic)
        {
            string longestName = _dm.Bases.GetLongestMatchingName(inputType);
            if (!string.IsNullOrEmpty(longestName))
            {
                type = longestName;
                typeEn = _dm.Bases.FindBaseByName(type)?.NameEn ?? typeEn;
            }
        }
        if ((flag.Map.IsMap || flag.Waystones) && !flag.Tag.Unidentified && flag.Rarity.Magic)
        {
            var affixes = _dm.Mods.GetMatchingAffixesList(inputType);
            if (affixes.Count > 0)
            {
                type = inputType.ToString();
                foreach (var affix in affixes.OrderByDescending(x => x.Length))
                {
                    type = type.Replace(affix, string.Empty).Trim();
                }
                type = RegexUtil.MultipleSpace().Replace(type, " ");
                typeEn = _dm.Bases.FindBaseByName(type)?.NameEn ?? typeEn;
            }
        }
        
        if (string.IsNullOrEmpty(type))
        {
            var checkVestigial = !IsPoe2 && (flag.Rarity.Unique || (flag.Rarity.Rare && flag.Tag.Corrupted));
            type = !checkVestigial ? inputType.ToString()
                : inputType.RemoveStringFromArrayDesc(Resources.Resources.General213_Vestigial.Split('/'));
        }

        if (flag.Tag.CapturedBeast)
        {
            var monster = _dm.Monsters.FindMonsterByName(type, nospirit: true);
            if (!string.IsNullOrEmpty(monster?.Name))
            {
                typeEn = monster.NameEn.Replace("\"", string.Empty);
            }
        }

        if (string.IsNullOrEmpty(typeEn))
        {
            typeEn = _dm.Bases.FindBaseByName(type)?.NameEn ?? typeEn;
        }

        if (rule.ShowDetail || flag.Waystones)
        {
            if (flag.Currency || flag.Breachstone || flag.Divcard || flag.Map.Fragment || flag.Waystones)
            {
                var currency = _dm.Currencies.FindEntryByType(type);
                if (!string.IsNullOrEmpty(currency?.Id))
                {
                    typeEn = _dm.CurrenciesEn.FindEntryById(currency.Id)?.Text ?? typeEn;
                }
            }
            if (flag.Tag.VaalSkillGems)
            {
                var vaalName = GetVaalGemName(infoDesc);
                if (!string.IsNullOrEmpty(vaalName))
                {
                    type = vaalName;
                    typeEn = _dm.Bases.FindBaseByName(type)?.NameEn ?? typeEn;
                }
            }
            if (flag.Tag.Transfigured && string.IsNullOrEmpty(type))
            {
                type = _dm.Gems.FindGemByNameEn(typeEn)?.Name ?? type;
            }
            if (flag.ScryingOrb)
            {
                typeEn = _dm.Config.Options.Language is 0 ? type : Resources.Resources.ResourceManager
                    .GetEnglish(nameof(Resources.Resources.General245_ScryingOrb));
            }
        }

        if (string.IsNullOrEmpty(type))
        {
            type = _dm.Bases.FindBaseByNameEn(typeEn)?.Name ?? type;
        }
        if (IsPoe2 && string.IsNullOrEmpty(typeEn)) // temp
        {
            typeEn = _dm.Bases.FindBaseByName(type)?.Name ?? typeEn;
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

    private string GetName(ItemOption options, ItemFlag flag, InfoDescription infoDesc, ReadOnlySpan<char> dataName, bool isPoe2)
    {
        if (flag.Tag.CapturedBeast || (flag.Currency && !flag.ScryingOrb) || flag.Divcard 
            || (flag.Map.Fragment && !flag.MercenaryWarrant)
            || (flag.Gem.IsGem && !(flag.Tag.Transfigured && flag.Tag.VaalSkillGems)))
            return string.Empty;

        if (!isPoe2 && flag.Rarity.Unique)
        {
            return dataName.RemoveStringFromArrayDesc(Resources.Resources.General166_Foulborn.Split('/'));
        }
        if (flag.Map.Chart || flag.MercenaryWarrant || flag.ScryingOrb)
        {
            var variant = flag.Map.Chart ? infoDesc.SecondHeader 
                : flag.ScryingOrb ? options.MapArea
                : options.MercenaryBuild;
            if (!string.IsNullOrEmpty(variant))
            {
                return _dm.Items.FindEntryByText(variant)?.Text ?? dataName.ToString();
            }
        }
        return dataName.ToString();
    }

    private static bool FindContinuePoint(ItemFlag flag, ReadOnlySpan<char> data, bool BelowMaxMods)
    {
        if (flag.Gem.IsGem)
        {
            return !flag.Tag.Imbued;
        }

        var cond = (flag.Tag.ItemLevel || flag.Tag.AreaLevel) && BelowMaxMods;
        if (!cond || flag.Corpses || SkipBetweenParenthesis(data, flag))
        {
            return true;
        }

        return false;
    }

    private string[] GetSanctumMods(ItemOption options)
    {
        List<string> lMods = new(), lEntrie = new();

        if (options.MajorBoons is var majBoons && majBoons[0].Length > 0)
        {
            lEntrie.AddRange(majBoons);
        }
        if (options.MajorAfflictions is var majAfflictions && majAfflictions[0].Length > 0)
        {
            lEntrie.AddRange(majAfflictions);
        }
        if (options.Pacts is var pacts && pacts[0].Length > 0)
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
        if (options.RewardsFloor is var floorRewards && floorRewards[0].Length > 0)
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
        if (options.RewardsSanctum is var sanctumRewards && sanctumRewards[0].Length > 0)
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

    private static bool SkipBetweenParenthesis(ReadOnlySpan<char> data, ItemFlag flag)
    {
        if (flag.Area.Ultimatum)
        {
            return data.StartsWith('(') || data.EndsWith(')');
        }
        // disabled for now : incompatibility with timeless jewel
        /*if (flag.Unique && RegexUtil.TextParenthesisPattern().IsMatch(data))
        {
            return true;
        }*/
        return data.StartsWith('(') && data.EndsWith(')');
    }

    private List<ModLine> GetModList(ItemOption options, ItemFlag flag, InfoDescription infoDesc)
    {
        var modList = new List<ModLine>();
        for (int idx = 1; idx < infoDesc.Item.Length; idx++)
        {
            var data = GetDataAndParseSanctumDelirium(options, flag, infoDesc, idx);
            var lSubMods = GetModsFromData(options, flag, data);
            if ((flag.Slot.Flask || flag.Slot.Charm) && idx is 1)
            {
                continue;
            }
            if (lSubMods.Count > 0)
            {
                modList.AddRange(lSubMods);
            }
        }
        return modList;
    }

    private List<ModLine> GetModsFromData(ItemOption options, ItemFlag flag, ReadOnlyMemory<string> dataMemory)
    {
        var lMods = new List<ModLine>();
        ModDescription pendingDesc = null;
        var data = dataMemory.Span;
        bool skip = false;

        for (int i = 0; i < data.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(data[i]) || skip)
            {
                skip = false;
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
            if (options.Update(flag, affix.ParsedData) 
                || FindContinuePoint(flag, affix.ParsedData, lMods.Count < NB_MAX_MODS))
            {
                continue;
            }

            var modifier = new ItemModifier(_dm, this, affix, GetNextMod(data, i));
            if (modifier.SkipNextLine)
            {
                skip = true;
            }
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
        return lMods.HandleDuplicates(_dm.Config.Options.AutoSelectMinTierValue);
    }

    private static string GetNextMod(ReadOnlySpan<string> data, int index)
    {
        int next = index + 1;

        if (next >= data.Length)
            return string.Empty;

        var value = data[next];
        return string.IsNullOrEmpty(value) ? string.Empty : new AffixFlag(value).ParsedData;
    }

    private string[] GetDataAndParseSanctumDelirium(ItemOption options, ItemFlag flag, InfoDescription infoDesc, int infoIndex)
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

        if (flag.Area.SanctumResearch && infoIndex == infoDesc.Item.Length - 1) // at the last loop
        {
            var sanctumMods = GetSanctumMods(options);
            if (sanctumMods.Length > 0)
            {
                Array.Resize(ref data, data.Length + sanctumMods.Length);
                Array.Copy(sanctumMods, 0, data, data.Length - sanctumMods.Length, sanctumMods.Length);
            }
        }

        return data;
    }

    private string GetEnglishdName(ItemFlag flag, ReadOnlySpan<char> name)
    {
        if (name.Length is 0)
        {
            return string.Empty;
        }

        if (flag.Map.Chart || flag.ScryingOrb)
        {
            var entry = _dm.Items.FindEntryByText(name);
            if (!string.IsNullOrEmpty(entry?.Type)) 
            {
                return _dm.ItemsEn.FindEntryByType(entry.Type)?.Text ?? string.Empty;
            }
        }

        if (_dm.Words.FindWordByName(name) is var word && word is not null)
        {
            return word.NameEn;
        }
        if (_dm.Bases.FindBaseByName(name) is var bases && bases is not null)
        {
            return bases.NameEn;
        }
        if (_dm.Gems.FindGemByName(name) is var gem && gem is not null)
        {
            return gem.NameEn;
        }

        // Handle magic
        if (!flag.Tag.Unidentified && flag.Rarity.Magic)
        {
            // TODO with dm.Mods & dm.Bases
        }
        // Handle rares
        int wordCount = 0;
        var wordList = new List<string>();
        foreach (Range range in name.Split(' '))
        {
            wordCount++;
            if (_dm.Words.FindWordByName(name[range]) is var part && part is not null)
            {
                wordList.Add(part.NameEn.Split('/')[0]);
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
