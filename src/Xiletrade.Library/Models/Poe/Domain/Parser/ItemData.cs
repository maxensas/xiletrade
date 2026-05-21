using System;
using System.Collections.Generic;
using System.Globalization;
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
        (Id, IdCurrency) = GetItemIds(Flag, Type);
        
        NameEn = GetParsedEnglishName(Flag, IsPoe2, header.Name);
        Name = Lang is Lang.English ? NameEn : GetTranslatedName(Flag, NameEn);

        Options = new();
        if (Flag.Parseable)
        {
            ModList = GetModList(Options, Flag, infoDesc);
        }
        Stats = new(_dm, Flag, ModList, Lang);
        State = new(_dm, Flag, ModList, Type);
        Damage = new(Flag, Stats, Options, Lang);
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

    internal Dictionary<StatPanel, MinMaxModel> GetMinMax()
    {
        var minMax = MinMaxModel.CreateDictionary();

        if (!IsPoe2 && (Flag.ItemSocketable || Flag.Jewellery))
        {
            minMax[StatPanel.CommonMemoryStrand].Min = Options.MemoryStrands;
        }

        if (Flag.SanctumResearch)
        {
            if (Options.Resolve is var resolve && resolve.Length is 2)
            {
                minMax[StatPanel.SanctumResolve].Min = resolve[0];
                minMax[StatPanel.SanctumMaxResolve].Max = resolve[1];
            }
            minMax[StatPanel.SanctumInspiration].Min = Options.Inspiration;
            minMax[StatPanel.SanctumAureus].Min = Options.Aureus;
        }

        var preferTier = _dm.Config.Options.AutoSelectMinTierValue && !Flag.Mirrored && !Flag.Corrupted;
        if (!Flag.Map && !Flag.Flask && Stats.Resistance)
        {
            var res = minMax[StatPanel.TotalElemResistance];
            res.Min = Stats.GetResistance(preferTier);
            if (_dm.Config.Options.AutoSelectRes
                && (res.Min.ToDoubleDefault() >= 36 || Flag.Jewel))
            {
                res.Selected = true;
            }
        }
        if (Stats.Life)
        {
            var life = minMax[StatPanel.TotalLife];
            life.Min = Stats.GetLife(preferTier);
            if (_dm.Config.Options.AutoSelectLife
                && (life.Min.ToDoubleDefault() >= 40 || Flag.Jewel))
            {
                life.Selected = true;
            }
        }
        if (Stats.EnergyShield)
        {
            var globalEs = minMax[StatPanel.TotalGlobalEs];
            globalEs.Min = !Flag.ArmourPiece ? Stats.GetEnergyShield(preferTier) : string.Empty;
            if (!Flag.ArmourPiece && (_dm.Config.Options.AutoSelectGlobalEs
                && (globalEs.Min.ToDoubleDefault() >= 38 || Flag.Jewel)))
            {
                globalEs.Selected = true;
            }
        }
        if (Stats.Attribute)
        {
            var attribute = minMax[StatPanel.TotalAttribute];
            attribute.Min = Stats.GetAttribute(preferTier);
            if (_dm.Config.Options.AutoSelectAttr && attribute.Min.ToDoubleDefault() >= 20)
            {
                attribute.Selected = true;
            }
        }

        if (Flag.ItemSocketable)
        {
            var socket = minMax[StatPanel.CommonSocket];
            if (socket.Min is "6" && State.ImmutableSockets)
            {
                socket.Selected = true;
            }
            var link = minMax[StatPanel.CommonLink];
            if (link.Min is "6")
            {
                link.Selected = true;
            }
        }

        var level = minMax[StatPanel.CommonItemLevel];

        if (Flag.UncutGem || Flag.Wombgift || Flag.UltimatumPoe2 || Flag.TrialCoins)
        {
            level.Min = Options.ItemLevel;
            level.Selected = true;
        }

        var qual = minMax[StatPanel.CommonQuality];
        if (!Flag.Unique && (Flag.Flask || Flag.Tincture || (Flag.Normal && IsPoe2)))
        {
            var iLvl = Options.ItemLevel;
            var baseLevelMin = IsPoe2 ? 79 : 84;
            if (int.TryParse(iLvl, out int result) && result >= baseLevelMin)
            {
                qual.Selected = Options.Quality.Length > 0
                    && int.Parse(Options.Quality, CultureInfo.InvariantCulture) > 14; // Glassblower is now valuable
            }
        }

        if (!State.ExchangeCurrency)
        {
            level.Min = Flag.Gems ? Options.Level : Options.ItemLevel;
            qual.Min = Options.Quality;

            if (Flag.ArmourPiece || Flag.Weapon || Flag.Jewellery || Flag.Flask || Flag.Charm)
            {
                var lv = Options.Level;
                var req = Options.Requires;
                minMax[StatPanel.CommonRequiresLevel].Min = lv.Length > 0 ? lv : req;
            }

            if (Flag.Map)
            {
                level.Min = level.Max = Options.MapTier;
                level.Text = Resources.Resources.Main094_lbTier;
                level.Selected = true;

                var mapQuant = minMax[StatPanel.MapQuantity];
                mapQuant.Min = Options.ItemQuantity;
                var mapRarity = minMax[StatPanel.MapRarity];
                mapRarity.Min = Options.ItemRarity;
                var mapPackSize = minMax[StatPanel.MapPackSize];
                mapPackSize.Min = Options.MonsterPackSize;
                var mapScarab = minMax[StatPanel.MapMoreScarab];
                mapScarab.Min = Options.MoreScarabs;
                var mapCurrency = minMax[StatPanel.MapMoreCurrency];
                mapCurrency.Min = Options.MoreCurrency;
                var mapDivCard = minMax[StatPanel.MapMoreDivCard];
                mapDivCard.Min = Options.MoreDiv;
                var mapMoreMap = minMax[StatPanel.MapMoreMap];
                mapMoreMap.Min = Options.MoreMaps;

                // new auto select behaviour
                if (mapQuant.Min.ToDoubleDefault() >= 100
                    && mapRarity.Min.ToDoubleDefault() >= 90
                    && mapPackSize.Min.ToDoubleDefault() >= 40)
                {
                    mapQuant.Selected = mapRarity.Selected = mapPackSize.Selected = true;
                    if (mapScarab.Min.ToDoubleDefault() >= 70)
                    {
                        mapScarab.Selected = true;
                    }
                    if (mapCurrency.Min.ToDoubleDefault() >= 70)
                    {
                        mapCurrency.Selected = true;
                    }
                    if (mapDivCard.Min.ToDoubleDefault() >= 70)
                    {
                        mapDivCard.Selected = true;
                    }
                    if (mapMoreMap.Min.ToDoubleDefault() >= 100)
                    {
                        mapMoreMap.Selected = true;
                    }
                }
            }
            else if (Flag.Waystones)
            {
                level.Min = level.Max = Options.WaystoneTier;
                level.Text = Resources.Resources.Main094_lbTier;
                level.Selected = true;

                minMax[StatPanel.MapQuantity].Min = Options.ItemQuantity;
                minMax[StatPanel.MapQuantity].Selected = true;
                minMax[StatPanel.MapRarity].Min = Options.ItemRarity;
                minMax[StatPanel.MapRarity].Selected = true;
                minMax[StatPanel.MapPackSize].Min = Options.MonsterPackSize;
                minMax[StatPanel.MapPackSize].Selected = true;
                minMax[StatPanel.MapMonsterRare].Min = Options.RareMonsters;
                minMax[StatPanel.MapMonsterRare].Selected = true;
                minMax[StatPanel.MapMonsterMagic].Min = Options.MagicMonsters;
            }
            else if (Flag.Gems)
            {
                level.Selected = true;
                minMax[StatPanel.CommonQuality].Selected = Options.Quality.Length > 0
                    && int.Parse(Options.Quality, CultureInfo.InvariantCulture) > 12;
            }
            else if (Flag.ByType && Flag.Normal)
            {
                level.Selected = level.Min.Length > 0
                    && int.Parse(level.Min, CultureInfo.InvariantCulture) > 82;
            }
            else if (!Flag.Unique && Flag.Cluster)
            {
                level.Selected = level.Min.Length > 0
                    && int.Parse(level.Min, CultureInfo.InvariantCulture) >= 78;
                if (level.Min.Length > 0)
                {
                    int minVal = int.Parse(level.Min, CultureInfo.InvariantCulture);
                    level.Min = minVal >= 84 ? "84" : minVal >= 78 ? "78" : level.Min;
                }
            }
        }

        if (Flag.Logbook || Flag.Corpses || Flag.SanctumResearch
            || Flag.Chronicle || Flag.MirroredTablet
            || Flag.TrialCoins || (Flag.Ultimatum && IsPoe2)
            || (Flag.Flask || Flag.Tincture) && !Flag.Unique)
        {
            level.Selected = true;
        }

        if (Flag.Chronicle || Flag.Ultimatum || Flag.MirroredTablet
            || Flag.SanctumResearch || Flag.TrialCoins || Flag.Logbook)
        {
            level.Text = Resources.Resources.General067_AreaLevel;
            var area = Options.AreaLevel;
            level.Min = area.Length > 0 ? area : Options.AreaLevelBis;
        }

        if (level.Text.Length is 0)
        {
            level.Text = Resources.Resources.Main065_tbiLevel;
        }

        if (Flag.ArmourPiece && !Flag.Unidentified)
        {
            var armour = Options.Armour;
            var energy = Options.Energy;
            var evasion = Options.Evasion;
            var ward = Options.Ward;

            if (armour.Length > 0)
            {
                var ar = minMax[StatPanel.DefenseArmour];
                if (_dm.Config.Options.AutoSelectArEsEva) ar.Selected = true;
                ar.Min = armour;
            }
            if (energy.Length > 0)
            {
                var es = minMax[StatPanel.DefenseEnergy];
                if (_dm.Config.Options.AutoSelectArEsEva) es.Selected = true;
                es.Min = energy;
            }
            if (evasion.Length > 0)
            {
                var eva = minMax[StatPanel.DefenseEvasion];
                if (_dm.Config.Options.AutoSelectArEsEva) eva.Selected = true;
                eva.Min = evasion;
            }
            if (ward.Length > 0)
            {
                var wrd = minMax[StatPanel.DefenseWard];
                if (_dm.Config.Options.AutoSelectArEsEva) wrd.Selected = true;
                wrd.Min = ward;
            }
        }

        if (Flag.Weapon && !Flag.Unidentified)
        {
            if (_dm.Config.Options.AutoSelectDps && Damage.Total > 100)
            {
                minMax[StatPanel.DamageTotal].Selected = true;
            }
            if (Damage.TotalMin.Length > 0)
            {
                minMax[StatPanel.DamageTotal].Min = Damage.TotalMin;
            }
            if (Damage.PysicalMin.Length > 0)
            {
                minMax[StatPanel.DamagePhysical].Min = Damage.PysicalMin;
            }
            if (Damage.ElementalMin.Length > 0)
            {
                minMax[StatPanel.DamageElemental].Min = Damage.ElementalMin;
            }
        }

        return minMax;
    }

    //private
    private (string Id, string IdCurrency) GetItemIds(ItemFlag flag, ReadOnlySpan<char> type)
    {
        if (flag.Currency || flag.Divcard || flag.MapFragment || (flag.SupportGems && IsPoe2))
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
        var typeEn = GetParsedType(flag, inpuType);
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

    private static string GetParsedEnglishName(ItemFlag flag, bool isPoe2, ReadOnlySpan<char> dataName)
    {
        if (flag.CapturedBeast || flag.Currency || flag.Divcard || flag.MapFragment
            || (flag.Gems && !(flag.Transfigured && flag.VaalSkillGems)))
            return string.Empty;

        if (!isPoe2 && flag.Unique)
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

    private string GetParsedType(ItemFlag flag, ReadOnlySpan<char> inputType)
    {
        var type = string.Empty;
        if (flag.Unidentified || flag.Normal || flag.Synthesised || flag.MapBlight || flag.MapBlightRavaged)
        {
            var rm = Resources.Resources.ResourceManager;
            if (flag.Unidentified || flag.Normal)
            {
                var higher = rm.GetEnglish(nameof(Resources.Resources.General030_Higher)).Split('/');
                var exceptional = rm.GetEnglish(nameof(Resources.Resources.General159_Exceptional)).Split('/');
                type = inputType.RemoveStringFromArrayDesc(higher).RemoveStringFromArrayDesc(exceptional);
            }
            if (flag.Synthesised)
            {
                var synth = rm.GetEnglish(nameof(Resources.Resources.General048_Synthesised)).Split('/');
                type = inputType.RemoveStringFromArrayDesc(synth);
            }
            if (flag.MapBlight)
            {
                var blight = rm.GetEnglish(nameof(Resources.Resources.General040_Blighted));
                type = inputType.StartWith(blight)
                    ? inputType[blight.Length..].Trim().ToString() : inputType.Trim().ToString();
            }
            if (flag.MapBlightRavaged)
            {
                var ravaged = rm.GetEnglish(nameof(Resources.Resources.General100_BlightRavaged));
                type = inputType.StartWith(ravaged)
                    ? inputType[ravaged.Length..].Trim().ToString() : inputType.Trim().ToString();
            }
        }
        if (!flag.Unidentified && !flag.Map && flag.Magic)
        {
            string longestName = _dm.Bases.GetLongestMatchingNameEn(inputType);
            if (!string.IsNullOrEmpty(longestName))
            {
                type = longestName;
            }
        }
        if ((flag.Map || flag.Waystones) && !flag.Unidentified && flag.Magic)
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

    private static bool FindContinuePoint(ItemFlag flag, ReadOnlySpan<char> data, bool BelowMaxMods)
    {
        if (flag.Gems)
        {
            return !flag.Imbued;
        }

        var cond = (flag.ItemLevel || flag.AreaLevel) && BelowMaxMods;
        if (!cond || SkipBetweenBrackets(data, flag.Ultimatum))
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

    private static bool SkipBetweenBrackets(ReadOnlySpan<char> data, bool ultimatum)
    {
        if (ultimatum)
        {
            return data.StartsWith('(') || data.EndsWith(')');
        }
        return data.StartsWith('(') && data.EndsWith(')');
    }

    private List<ModLine> GetModList(ItemOption options, ItemFlag flag, InfoDescription infoDesc)
    {
        var modList = new List<ModLine>();
        for (int idx = 1; idx < infoDesc.Item.Length; idx++)
        {
            if ((flag.Flask || flag.Charm) && idx is 1)
            {
                continue;
            }
            var data = GetDataAndParseSanctumDelirium(options, flag, infoDesc, idx);
            var lSubMods = GetModsFromData(options, flag, data);
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
            if (options.Update(affix.ParsedData) 
                || FindContinuePoint(flag, affix.ParsedData, lMods.Count < NB_MAX_MODS))
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

        if (flag.SanctumResearch && infoIndex == infoDesc.Item.Length - 1) // at the last loop
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

    private string GetTranslatedName(ItemFlag flag, ReadOnlySpan<char> nameEn)
    {
        if (nameEn.Length is 0)
        {
            return string.Empty;
        }
        if (_dm.Words.FindWordByNameEn(nameEn) is var word && word is not null)
        {
            return word.Name;
        }
        if (_dm.Bases.FindBaseByNameEn(nameEn) is var bases && bases is not null)
        {
            return bases.Name;
        }
        if (_dm.Gems.FindGemByNameEn(nameEn) is var gem && gem is not null)
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
            if (_dm.Words.FindWordByNameEn(nameEn[range]) is var part && part is not null)
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
