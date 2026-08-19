using System.Collections.Generic;
using System.Linq;
using Xiletrade.Library.Models.Poe.Domain.Parser;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Enum;
using Xiletrade.Library.ViewModels.Main.Form;

namespace Xiletrade.Library.Models.Poe.Domain;

/// <summary>
/// Transition model used to extract parameters from the viewmodel associated to the price check feature. 
/// </summary>
internal record class XiletradeItem
{
    internal bool SynthesisBlight { get; }
    internal bool BlightRavaged { get; }
    internal bool ChaosDivOnly { get; }
    internal bool ExaltOnly { get; }
    internal bool ChaosOnly { get; }
    internal bool ByType { get; }
    internal bool SocketColors { get; }

    internal bool InfShaper { get; }
    internal bool InfElder { get; }
    internal bool InfCrusader { get; }
    internal bool InfRedeemer { get; }
    internal bool InfHunter { get; }
    internal bool InfWarlord { get; }

    internal bool IsInfluenced => InfShaper || InfElder || InfCrusader 
        || InfRedeemer || InfHunter || InfWarlord;

    internal XiletradeOption Socket { get; } = new();
    internal XiletradeOption Link { get; } = new();
    internal XiletradeOption Quality { get; } = new();
    internal XiletradeOption Lvl { get; } = new();
    internal XiletradeOption DpsTotal { get; } = new();
    internal XiletradeOption DpsPhys { get; } = new();
    internal XiletradeOption DpsElem { get; } = new();
    internal XiletradeOption Armour { get; } = new();
    internal XiletradeOption Energy { get; } = new();
    internal XiletradeOption Evasion { get; } = new();
    internal XiletradeOption Ward { get; } = new();
    internal XiletradeOption MapIiq { get; } = new();
    internal XiletradeOption MapIir { get; } = new();
    internal XiletradeOption MapPack { get; } = new();
    internal XiletradeOption MapScarab { get; } = new();
    internal XiletradeOption MapCurrency { get; } = new();
    internal XiletradeOption MapDivCard { get; } = new();
    internal XiletradeOption Resolve { get; } = new();
    internal XiletradeOption MaxResolve { get; } = new();
    internal XiletradeOption Inspiration { get; } = new();
    internal XiletradeOption Aureus { get; } = new();
    internal XiletradeOption RuneSockets { get; } = new();
    internal XiletradeOption GemSockets { get; } = new();
    internal XiletradeOption ReqLevel { get; } = new();
    internal XiletradeOption MemoryStrand { get; } = new();
    internal XiletradeOption ItemRarity { get; } = new();
    internal XiletradeOption MonsterRarity { get; } = new();
    internal XiletradeOption Effectiveness { get; } = new();
    internal XiletradeOption PackSize { get; } = new();
    internal XiletradeOption WaystoneDrop { get; } = new();
    internal XiletradeOption Revives { get; } = new();
    internal XiletradeOption GoldFound { get; } = new();
    internal XiletradeOption DeadSulphur { get; } = new();
    internal XiletradeOption FacetorExp { get; } = new();
    internal XiletradeOption Intangibility { get; } = new();
    internal XiletradeOption HeistRevealedWings { get; } = new();
    internal XiletradeOption HeistRevealedEscapeRoutes { get; } = new();
    internal XiletradeOption HeistRevealedRewardRooms { get; } = new();
    internal XiletradeOption HeistTotalWings { get; } = new();
    internal XiletradeOption HeistTotalEscapeRoutes { get; } = new();
    internal XiletradeOption HeistTotalRewardRooms { get; } = new();
    internal XiletradeOption HeistLockpicking { get; } = new();
    internal XiletradeOption HeistDemolition { get; } = new();
    internal XiletradeOption HeistCounterThaumaturgy { get; } = new();
    internal XiletradeOption HeistTrapDisarmament { get; } = new();
    internal XiletradeOption HeistAgility { get; } = new();
    internal XiletradeOption HeistEngineering { get; } = new();
    internal XiletradeOption HeistBruteForce { get; } = new();
    internal XiletradeOption HeistPerception { get; } = new();
    internal XiletradeOption HeistDeception { get; } = new();

    internal DefaultOption Corrupted { get; }
    internal DefaultOption TwiceCorrupted { get; }
    internal DefaultOption Identified { get; }
    internal DefaultOption Fractured { get; }
    internal DefaultOption Mirrored { get; }
    internal DefaultOption Split { get; }
    internal DefaultOption Crafted { get; }
    internal DefaultOption Mutated { get; }
    internal DefaultOption Veiled { get; } // Unrevealed
    internal DefaultOption Desecrated { get; }
    internal DefaultOption Sanctified { get; }

    internal string RewardType { get; }
    internal string Reward { get; }
    internal string Rarity { get; }
    internal string UniqueName { get; }

    internal double SocketRed { get; }
    internal double SocketGreen { get; }
    internal double SocketBlue { get; }
    internal double SocketWhite { get; }
    internal double PriceMin { get; }

    internal List<ItemFilter> ItemFilters { get; } = new();

    /// <summary>
    /// Transition model used to extract parameters from the viewmodel associated to the price check feature. 
    /// </summary>
    internal XiletradeItem(DataManagerService dm, FormViewModel form, bool customSearch = false)
    {
        var listPanel = customSearch ? form.CustomSearch.MinMaxList : form.Panel.StatList;

        var noSockets = form.Panel?.Sockets is null;
        var panel = form.Panel is not null;
        var influence = form.Influence is not null;

        InfShaper = influence && form.Influence.Shaper;
        InfElder = influence && form.Influence.Elder;
        InfCrusader = influence && form.Influence.Crusader;
        InfRedeemer = influence && form.Influence.Redeemer;
        InfHunter = influence && form.Influence.Hunter;
        InfWarlord = influence && form.Influence.Warlord;

        Corrupted = GetOption(form.CorruptedIndex);
        TwiceCorrupted = GetOption(form.DoubleCorruptedIndex);
        Identified = GetOption(form.IdentifiedIndex);
        Mirrored = GetOption(form.MirroredIndex);
        Fractured = GetOption(form.FracturedIndex);
        Split = GetOption(form.SplitIndex);
        Crafted = GetOption(form.CraftedIndex);
        Mutated = GetOption(form.MutatedIndex);
        Desecrated = GetOption(form.DesecratedIndex);
        Veiled = GetOption(form.VeiledIndex);
        Sanctified = GetOption(form.SanctifiedIndex);
        SynthesisBlight = panel && form.Panel.SynthesisBlight;
        BlightRavaged = panel && form.Panel.BlighRavaged;
        ByType = form.ByBase != true;

        RewardType = form.Panel?.Reward?.Tip.Length > 0 ? form.Panel.Reward.Tip : null;
        Reward = form.Panel?.Reward?.Text.Length > 0 ? form.Panel.Reward.Text : null;
        ChaosDivOnly = form.ChaosDiv;
        ExaltOnly = form.Exalt;
        ChaosOnly = form.Chaos;
        Rarity = form.Rarity.Index >= 0 && form.Rarity.Index < form.Rarity.ComboBox.Count ?
            form.Rarity.ComboBox[form.Rarity.Index] : form.Rarity.Item;
        UniqueName = form.GetUniqueName();

        PriceMin = 0; // not used

        SocketColors = form.Condition is not null && form.Condition.SocketColors;

        SocketRed = noSockets ? ModFilter.EMPTYFIELD : form.Panel.Sockets.RedColor.ToDoubleEmptyField();
        SocketGreen = noSockets ? ModFilter.EMPTYFIELD : form.Panel.Sockets.GreenColor.ToDoubleEmptyField();
        SocketBlue = noSockets ? ModFilter.EMPTYFIELD : form.Panel.Sockets.BlueColor.ToDoubleEmptyField();
        SocketWhite = noSockets ? ModFilter.EMPTYFIELD : form.Panel.Sockets.WhiteColor.ToDoubleEmptyField();

        FacetorExp.Min = !panel ? ModFilter.EMPTYFIELD : form.Panel.FacetorMin.ToDoubleEmptyField();
        FacetorExp.Max = !panel ? ModFilter.EMPTYFIELD : form.Panel.FacetorMax.ToDoubleEmptyField();
        FacetorExp.Enable = (FacetorExp.Min?.IsNotEmpty() ?? FacetorExp.Max?.IsNotEmpty()) ?? false;

        // add item filters
        if (form.ModList?.Count > 0)
        {
            int modLimit = 1;
            foreach (var mod in form.ModList)
            {
                var itemFilter = new ItemFilter();
                if (mod.Affix.Count is 0)
                {
                    continue;
                }

                double minValue = mod.PreferMinMax ? mod.Min.ToDoubleEmptyField()
                        : !mod.IsSlideReversed ? mod.SlideValue : mod.Max.ToDoubleEmptyField();
                double maxValue = mod.PreferMinMax ? mod.Max.ToDoubleEmptyField()
                    : mod.IsSlideReversed ? mod.SlideValue : mod.Max.ToDoubleEmptyField();

                itemFilter.Text = mod.Mod.Trim();
                itemFilter.Type = mod.Affix[mod.AffixIndex].Type;
                itemFilter.Disabled = mod.Selected != true;
                itemFilter.Min = minValue;
                itemFilter.Max = maxValue;

                itemFilter.Id = mod.Affix[mod.AffixIndex].ID;
                if (mod.OptionVisible)
                {
                    itemFilter.Option = mod.OptionID[mod.OptionIndex];
                    itemFilter.Min = ModFilter.EMPTYFIELD;
                }
                ItemFilters.Add(itemFilter);
                if (modLimit >= ItemData.NB_MAX_MODS)
                {
                    break;
                }
                modLimit++;
            }
        }

        var stats = new[]
        {
            (StatPanel.CommonItemLevel, Lvl),
            (StatPanel.CommonQuality, Quality),
            (StatPanel.CommonSocket, Socket),
            (StatPanel.CommonLink, Link),
            (StatPanel.CommonSocketRune, RuneSockets),
            (StatPanel.CommonSocketGem, GemSockets),
            (StatPanel.CommonRequiresLevel, ReqLevel),
            (StatPanel.CommonMemoryStrand, MemoryStrand),
            (StatPanel.CommonIntangibility, Intangibility),
            (StatPanel.DamageElemental, DpsElem),
            (StatPanel.DamagePhysical, DpsPhys),
            (StatPanel.DamageTotal, DpsTotal),
            (StatPanel.DefenseArmour, Armour),
            (StatPanel.DefenseEnergy, Energy),
            (StatPanel.DefenseEvasion, Evasion),
            (StatPanel.DefenseWard, Ward),
            (StatPanel.DefenseRunicWard, Ward),
            (StatPanel.MapPackSize, MapPack),
            (StatPanel.MapQuantity, MapIiq),
            (StatPanel.MapRarity, MapIir),
            (StatPanel.GoldFound, GoldFound),
            (StatPanel.DeadSulphur, DeadSulphur),
            (StatPanel.SanctumAureus, Aureus),
            (StatPanel.SanctumInspiration, Inspiration),
            (StatPanel.SanctumMaxResolve, MaxResolve),
            (StatPanel.SanctumResolve, Resolve),
            (StatPanel.WaystoneRarity, ItemRarity),
            (StatPanel.WaystoneMonsterRarity, MonsterRarity),
            (StatPanel.WaystoneMonsterEffectiveness, Effectiveness),
            (StatPanel.WaystonePackSize, PackSize),
            (StatPanel.WaystoneDrop, WaystoneDrop),
            (StatPanel.WaystoneRevives, Revives),
            (StatPanel.HeistRevealedWings, HeistRevealedWings),
            (StatPanel.HeistRevealedEscapeRoutes, HeistRevealedEscapeRoutes),
            (StatPanel.HeistRevealedRewardRooms, HeistRevealedRewardRooms),
            (StatPanel.HeistTotalWings, HeistTotalWings),
            (StatPanel.HeistTotalEscapeRoutes, HeistTotalEscapeRoutes),
            (StatPanel.HeistTotalRewardRooms, HeistTotalRewardRooms),
            (StatPanel.HeistLockpicking, HeistLockpicking),
            (StatPanel.HeistDemolition, HeistDemolition),
            (StatPanel.HeistCounterThaumaturgy, HeistCounterThaumaturgy),
            (StatPanel.HeistTrapDisarmament, HeistTrapDisarmament),
            (StatPanel.HeistAgility, HeistAgility),
            (StatPanel.HeistEngineering, HeistEngineering),
            (StatPanel.HeistBruteForce, HeistBruteForce),
            (StatPanel.HeistPerception, HeistPerception),
            (StatPanel.HeistDeception, HeistDeception)
        };

        foreach ((var stat, var option) in stats)
        {
            var vm = listPanel?.FirstOrDefault(x => x.Id == stat);
            if (vm is not null)
            {
                option.Enable = vm.Selected;
                option.Min = vm.ItemMin;
                option.Max = vm.ItemMax;
            }
        }

        var pseudos = new[]
        {
            (StatPanel.TotalElemResistance, Strings.Stat.Pseudo.TotalElemResistance),
            (StatPanel.TotalLife, Strings.Stat.Pseudo.TotalLife),
            (StatPanel.TotalAttribute, Strings.Stat.Pseudo.TotalAttribute),
            (StatPanel.TotalGlobalEs, Strings.Stat.Pseudo.TotalEs),
            (StatPanel.MapMoreScarab, Strings.Stat.Pseudo.MoreScarab),
            (StatPanel.MapMoreCurrency, Strings.Stat.Pseudo.MoreCurrency),
            (StatPanel.MapMoreDivCard, Strings.Stat.Pseudo.MoreDivCard)
            //(StatPanel.MapMoreMap, "pseudo.pseudo_map_more_map_drops")
        };

        foreach ((var stat, var pseudo) in pseudos)
        {
            var vm = listPanel?.FirstOrDefault(x => x.Id == stat);

            if (vm is null || !vm.Selected)
                continue;

            var filter = vm.SlideValue is not ModFilter.EMPTYFIELD
                ? new ItemFilter(dm.Filter, pseudo, vm.SlideValue, vm.Max)
                : new ItemFilter(dm.Filter, pseudo, vm.Min, vm.Max);

            if (!string.IsNullOrEmpty(filter.Id))
                ItemFilters.Add(filter);
        }

        if (form.Condition is not null)
        {
            if (form.Condition.FreePrefix)
            {
                var filter = new ItemFilter(dm.Filter, Strings.Stat.Pseudo.EmmptyPrefix, 1, ModFilter.EMPTYFIELD);
                if (filter.Id.Length > 0)
                {
                    ItemFilters.Add(filter);
                }
            }

            if (form.Condition.FreeSuffix)
            {
                var filter = new ItemFilter(dm.Filter, Strings.Stat.Pseudo.EmptySuffix, 1, ModFilter.EMPTYFIELD);
                if (filter.Id.Length > 0)
                {
                    ItemFilters.Add(filter);
                }
            }
        }

        List<string> listInfluence = new();

        if (InfShaper)
        {
            listInfluence.Add(Strings.Stat.Influence.Shaper);
        }
        if (InfElder)
        {
            listInfluence.Add(Strings.Stat.Influence.Elder);
        }
        if (InfCrusader)
        {
            listInfluence.Add(Strings.Stat.Influence.Crusader);
        }
        if (InfRedeemer)
        {
            listInfluence.Add(Strings.Stat.Influence.Redeemer);
        }
        if (InfHunter)
        {
            listInfluence.Add(Strings.Stat.Influence.Hunter);
        }
        if (InfWarlord)
        {
            listInfluence.Add(Strings.Stat.Influence.Warlord);
        }

        if (listInfluence.Count > 0)
        {
            foreach (var influ in listInfluence)
            {
                var filter = new ItemFilter(dm.Filter, "pseudo." + influ, ModFilter.EMPTYFIELD, ModFilter.EMPTYFIELD);
                if (filter.Id.Length > 0)
                {
                    ItemFilters.Add(filter);
                }
            }
        }
    }

    private static DefaultOption GetOption(int index)
    {
        return index switch
        {
            1 => DefaultOption.False,
            2 => DefaultOption.True,
            _ => DefaultOption.Any,
        };
    }
}
