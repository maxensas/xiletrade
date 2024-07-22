using System;
using System.Collections.Generic;
using System.Linq;
using Xiletrade.Library.ViewModels;
using Xiletrade.Library.Models.Serializable;
using Xiletrade.Library.Models.Enums;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Xiletrade.Library.Models.Logic;

/// <summary>Class containing logic used to handle and update main viewmodel.</summary>
internal sealed class MainLogic : MainUpdater
{
    //private static IServiceProvider _serviceProvider;
    private static MainViewModel Vm { get; set; }

    internal ItemBaseName CurrentItem { get; set; }
    internal TaskManager Task { get; private set; }

    internal MainLogic(MainViewModel vm, IServiceProvider serviceProvider) : base(vm, serviceProvider)
    {
        Vm = vm;
        //_serviceProvider = serviceProvider;
        Task = new(vm, serviceProvider);
    }

    // internal methods
    internal XiletradeItem GetItemFromViewModel()
    {
        XiletradeItem itemOption = new()
        {
            InfShaper = Vm.Form.Influence.Shaper,
            InfElder = Vm.Form.Influence.Elder,
            InfCrusader = Vm.Form.Influence.Crusader,
            InfRedeemer = Vm.Form.Influence.Redeemer,
            InfHunter = Vm.Form.Influence.Hunter,
            InfWarlord = Vm.Form.Influence.Warlord,

            //itemOption.Corrupt = (byte)cbCorrupt.SelectedIndex;
            Corrupted = Vm.Form.CorruptedIndex switch
            {
                1 => "false",
                2 => "true",
                _ => Strings.any,
            },
            SynthesisBlight = Vm.Form.Panel.SynthesisBlight,
            BlightRavaged = Vm.Form.Panel.BlighRavaged,
            Scourged = Vm.Form.Panel.Scourged,
            ChkSocket = Vm.Form.Panel.Common.Sockets.Selected,
            ChkQuality = Vm.Form.Panel.Common.Quality.Selected,
            ChkLv = Vm.Form.Panel.Common.ItemLevel.Selected,
            ByType = Vm.Form.ByBase != true,
            ChkArmour = Vm.Form.Panel.Defense.Armour.Selected,
            ChkEnergy = Vm.Form.Panel.Defense.Energy.Selected,
            ChkEvasion = Vm.Form.Panel.Defense.Evasion.Selected,
            ChkWard = Vm.Form.Panel.Defense.Ward.Selected,
            ChkDpsTotal = Vm.Form.Panel.Damage.Total.Selected,
            ChkDpsPhys = Vm.Form.Panel.Damage.Physical.Selected,
            ChkDpsElem = Vm.Form.Panel.Damage.Elemental.Selected,

            ChkResolve = Vm.Form.Panel.Sanctum.Resolve.Selected,
            ChkMaxResolve = Vm.Form.Panel.Sanctum.MaximumResolve.Selected,
            ChkInspiration = Vm.Form.Panel.Sanctum.Inspiration.Selected,
            ChkAureus = Vm.Form.Panel.Sanctum.Aureus.Selected,

            AlternateQuality = Vm.Form.Panel.AlternateGemIndex switch
            {
                0 => null,
                1 => "1",
                2 => "2",
                3 => "3",
                _ => null,
            },
            RewardType = Vm.Form.Panel.Reward.Tip.Length > 0 ? Vm.Form.Panel.Reward.Tip : null,
            Reward = Vm.Form.Panel.Reward.Text.Length > 0 ? Vm.Form.Panel.Reward.Text : null,
            ChaosDivOnly = Vm.ChaosDiv,

            SocketColors = Vm.Form.Condition.SocketColors,

            SocketRed = Common.StrToDouble(Vm.Form.Panel.Common.Sockets.RedColor, true),
            SocketGreen = Common.StrToDouble(Vm.Form.Panel.Common.Sockets.GreenColor, true),
            SocketBlue = Common.StrToDouble(Vm.Form.Panel.Common.Sockets.BlueColor, true),
            SocketWhite = Common.StrToDouble(Vm.Form.Panel.Common.Sockets.WhiteColor, true),

            SocketMin = Common.StrToDouble(Vm.Form.Panel.Common.Sockets.SocketMin, true),
            SocketMax = Common.StrToDouble(Vm.Form.Panel.Common.Sockets.SocketMax, true),
            LinkMin = Common.StrToDouble(Vm.Form.Panel.Common.Sockets.LinkMin, true),
            LinkMax = Common.StrToDouble(Vm.Form.Panel.Common.Sockets.LinkMax, true),
            QualityMin = Common.StrToDouble(Vm.Form.Panel.Common.Quality.Min, true),
            QualityMax = Common.StrToDouble(Vm.Form.Panel.Common.Quality.Max, true),
            LvMin = Common.StrToDouble(Vm.Form.Panel.Common.ItemLevel.Min, true),
            LvMax = Common.StrToDouble(Vm.Form.Panel.Common.ItemLevel.Max, true),
            ArmourMin = Common.StrToDouble(Vm.Form.Panel.Defense.Armour.Min, true),
            ArmourMax = Common.StrToDouble(Vm.Form.Panel.Defense.Armour.Max, true),
            EnergyMin = Common.StrToDouble(Vm.Form.Panel.Defense.Energy.Min, true),
            EnergyMax = Common.StrToDouble(Vm.Form.Panel.Defense.Energy.Max, true),
            EvasionMin = Common.StrToDouble(Vm.Form.Panel.Defense.Evasion.Min, true),
            EvasionMax = Common.StrToDouble(Vm.Form.Panel.Defense.Evasion.Max, true),
            WardMin = Common.StrToDouble(Vm.Form.Panel.Defense.Ward.Min, true),
            WardMax = Common.StrToDouble(Vm.Form.Panel.Defense.Ward.Max, true),
            DpsTotalMin = Common.StrToDouble(Vm.Form.Panel.Damage.Total.Min, true),
            DpsTotalMax = Common.StrToDouble(Vm.Form.Panel.Damage.Total.Max, true),
            DpsPhysMin = Common.StrToDouble(Vm.Form.Panel.Damage.Physical.Min, true),
            DpsPhysMax = Common.StrToDouble(Vm.Form.Panel.Damage.Physical.Max, true),
            DpsElemMin = Common.StrToDouble(Vm.Form.Panel.Damage.Elemental.Min, true),
            DpsElemMax = Common.StrToDouble(Vm.Form.Panel.Damage.Elemental.Max, true),
            FacetorExpMin = Common.StrToDouble(Vm.Form.Panel.FacetorMin, true),
            FacetorExpMax = Common.StrToDouble(Vm.Form.Panel.FacetorMax, true),

            ResolveMin = Common.StrToDouble(Vm.Form.Panel.Sanctum.Resolve.Min, true),
            ResolveMax = Common.StrToDouble(Vm.Form.Panel.Sanctum.Resolve.Max, true),
            MaxResolveMin = Common.StrToDouble(Vm.Form.Panel.Sanctum.MaximumResolve.Min, true),
            MaxResolveMax = Common.StrToDouble(Vm.Form.Panel.Sanctum.MaximumResolve.Max, true),
            InspirationMin = Common.StrToDouble(Vm.Form.Panel.Sanctum.Inspiration.Min, true),
            InspirationMax = Common.StrToDouble(Vm.Form.Panel.Sanctum.Inspiration.Max, true),
            AureusMin = Common.StrToDouble(Vm.Form.Panel.Sanctum.Aureus.Min, true),
            AureusMax = Common.StrToDouble(Vm.Form.Panel.Sanctum.Aureus.Max, true),
            Rarity = Vm.Form.Rarity.Index >= 0 && Vm.Form.Rarity.Index < Vm.Form.Rarity.ComboBox.Count ?
                Vm.Form.Rarity.ComboBox[Vm.Form.Rarity.Index] : Vm.Form.Rarity.Item,

            PriceMin = 0 // not used
        };
        //itemOption.PriceMin = tbPriceFilterMin.Text.Length == 0 ? 0 : Common.StrToDouble(tbPriceFilterMin.Text, 99999);

        //int total_res_idx = -1;
        if (Vm.Form.ModLine.Count > 0)
        {
            int modLimit = 1;
            foreach (ModLineViewModel mod in Vm.Form.ModLine)
            {
                ItemFilter itemFilter = new();
                if (mod.Affix.Count > 0)
                {
                    double minValue = Common.StrToDouble(mod.Min, true);
                    double maxValue = Common.StrToDouble(mod.Max, true);
                    if (mod.Affix[mod.AffixIndex].Name.Equals(Resources.Resources.General018_Monster, StringComparison.InvariantCultureIgnoreCase))
                    {
                        mod.Min = Vm.Logic.MetamorphMods[mod.Min.Trim()];
                    }
                    itemFilter.Text = mod.Mod.Trim();
                    itemFilter.Disabled = mod.Selected != true;
                    itemFilter.Min = minValue;
                    itemFilter.Max = maxValue;

                    itemFilter.Id = mod.Affix[mod.AffixIndex].ID;
                    if (mod.OptionVisible)
                    {
                        itemFilter.Option = mod.OptionID[mod.OptionIndex];
                        itemFilter.Min = Modifier.EMPTYFIELD;
                    }
                    itemOption.ItemFilters.Add(itemFilter);
                    if (modLimit >= Modifier.NB_MAX_MODS)
                    {
                        break;
                    }
                    modLimit++;
                }
            }
        }

        if (Vm.Form.Panel.Total.Resistance.Selected) //  && (!tbTotalResMin.Text.Equals("") || !tbTotalResMax.Text.Equals(""))
        {
            ItemFilter itemFilterRes = new("pseudo.pseudo_total_resistance");
            FilterResultEntrie filterResultEntry = DataManager.Filter.Result[0].Entries.FirstOrDefault(x => x.ID == itemFilterRes.Id);
            if (filterResultEntry is not null)
            {
                itemFilterRes.Disabled = false;
                itemFilterRes.Text = filterResultEntry.Text; // +#% total Resistance
                itemFilterRes.Min = Common.StrToDouble(Vm.Form.Panel.Total.Resistance.Min, true);
                itemFilterRes.Max = Common.StrToDouble(Vm.Form.Panel.Total.Resistance.Max, true);

                itemOption.ItemFilters.Add(itemFilterRes);
            }
        }
        if (Vm.Form.Panel.Total.Life.Selected)
        {
            ItemFilter itemFilterLife = new("pseudo.pseudo_total_life");
            FilterResultEntrie filterResultEntry = DataManager.Filter.Result[0].Entries.FirstOrDefault(x => x.ID == itemFilterLife.Id);
            if (filterResultEntry is not null)
            {
                itemFilterLife.Disabled = false;
                itemFilterLife.Text = filterResultEntry.Text; // +# total maximum Life
                itemFilterLife.Min = Common.StrToDouble(Vm.Form.Panel.Total.Life.Min, true);
                itemFilterLife.Max = Common.StrToDouble(Vm.Form.Panel.Total.Life.Max, true);

                itemOption.ItemFilters.Add(itemFilterLife);
            }
        }
        if (Vm.Form.Panel.Total.GlobalEs.Selected)
        {
            ItemFilter itemFilterEs = new("pseudo.pseudo_total_energy_shield");
            FilterResultEntrie filterResultEntry = DataManager.Filter.Result[0].Entries.FirstOrDefault(x => x.ID == itemFilterEs.Id);
            if (filterResultEntry is not null)
            {
                itemFilterEs.Text = filterResultEntry.Text; // # to maximum Energy Shield
                itemFilterEs.Disabled = false;
                itemFilterEs.Min = Common.StrToDouble(Vm.Form.Panel.Total.GlobalEs.Min, true);
                itemFilterEs.Max = Common.StrToDouble(Vm.Form.Panel.Total.GlobalEs.Max, true);

                itemOption.ItemFilters.Add(itemFilterEs);
            }
        }

        if (Vm.Form.Condition.FreePrefix)
        {
            ItemFilter itemFilter = new("pseudo.pseudo_number_of_empty_prefix_mods");
            FilterResultEntrie filterResultEntry = DataManager.Filter.Result[0].Entries.FirstOrDefault(x => x.ID == itemFilter.Id);
            if (filterResultEntry is not null)
            {
                itemFilter.Disabled = false;
                itemFilter.Text = filterResultEntry.Text; // # Empty Prefix Modifiers
                itemFilter.Min = 1;
                itemFilter.Max = Modifier.EMPTYFIELD; //3

                itemOption.ItemFilters.Add(itemFilter);
            }
        }

        if (Vm.Form.Condition.FreeSuffix)
        {
            ItemFilter itemFilter = new("pseudo.pseudo_number_of_empty_suffix_mods");
            FilterResultEntrie filterResultEntry = DataManager.Filter.Result[0].Entries.FirstOrDefault(x => x.ID == itemFilter.Id);
            if (filterResultEntry is not null)
            {
                itemFilter.Disabled = false;
                itemFilter.Text = filterResultEntry.Text; // # Empty Suffix Modifiers
                itemFilter.Min = 1;
                itemFilter.Max = Modifier.EMPTYFIELD; //3

                itemOption.ItemFilters.Add(itemFilter);
            }
        }

        List<string> influences = new();

        if (Vm.Form.Influence.Shaper)
        {
            influences.Add(Strings.Stat.Influence.Shaper);
        }
        if (Vm.Form.Influence.Elder)
        {
            influences.Add(Strings.Stat.Influence.Elder);
        }
        if (Vm.Form.Influence.Crusader)
        {
            influences.Add(Strings.Stat.Influence.Crusader);
        }
        if (Vm.Form.Influence.Redeemer)
        {
            influences.Add(Strings.Stat.Influence.Redeemer);
        }
        if (Vm.Form.Influence.Hunter)
        {
            influences.Add(Strings.Stat.Influence.Hunter);
        }
        if (Vm.Form.Influence.Warlord)
        {
            influences.Add(Strings.Stat.Influence.Warlord);
        }

        if (influences.Count > 0)
        {
            foreach (string stat_influence in influences)
            {
                ItemFilter itemFilter = new("pseudo." + stat_influence);
                FilterResultEntrie filterResultEntry = DataManager.Filter.Result[0].Entries.FirstOrDefault(x => x.ID == itemFilter.Id);
                if (filterResultEntry is not null)
                {
                    itemFilter.Disabled = false;
                    itemFilter.Text = filterResultEntry.Text;
                    itemFilter.Min = Modifier.EMPTYFIELD;
                    itemFilter.Max = Modifier.EMPTYFIELD;
                    itemOption.ItemFilters.Add(itemFilter);
                }
            }
        }

        return itemOption;
    }

    internal Dictionary<string, bool> GetInfluenceSate()
    {
        Dictionary<string, bool> dicInflu = new()
        {
            { Vm.Form.Influence.ShaperText, Vm.Form.Influence.Shaper },
            { Vm.Form.Influence.ElderText, Vm.Form.Influence.Elder },
            { Vm.Form.Influence.CrusaderText, Vm.Form.Influence.Crusader },
            { Vm.Form.Influence.RedeemerText, Vm.Form.Influence.Redeemer },
            { Vm.Form.Influence.WarlordText, Vm.Form.Influence.Warlord },
            { Vm.Form.Influence.HunterText, Vm.Form.Influence.Hunter }
        };
        return dicInflu;
    }

    internal string GetExchangeCurrencyTag(Exchange exchange) // get: true, pay: false
    {
        var exVm = exchange is Exchange.Get ? Vm.Form.Bulk.Get :
            exchange is Exchange.Pay ? Vm.Form.Bulk.Pay :
            exchange is Exchange.Shop ? Vm.Form.Shop.Exchange : null;
        if (exVm is null)
        {
            return string.Empty;
        }

        string category = exVm.Category.Count > 0 && exVm.CategoryIndex > -1 ?
                exVm.Category[exVm.CategoryIndex] : string.Empty;
        string currency = exVm.Currency.Count > 0 && exVm.CurrencyIndex > -1 ?
            exVm.Currency[exVm.CurrencyIndex] : string.Empty;
        string tier = exVm.Tier.Count > 0 && exVm.TierIndex > -1 ?
            exVm.Tier[exVm.TierIndex] : string.Empty;

        foreach (CurrencyResultData resultDat in DataManager.Currencies)
        {
            bool runLoop = true;
            /*
            if (category is Strings.Maps)
            {
                string tierVal = tier.Replace("T", string.Empty);
                tierVal = tierVal is Strings.Blight or Strings.Ravaged ? Strings.CurrencyType.MapsBlighted 
                    : Strings.CurrencyType.MapsTier + tierVal;
                if (resultDat.ID != tierVal)
                {
                    runLoop = false;
                }
            }
            */
            if (category is Strings.Maps)
            {
                string mapKind = tier.Replace("T", string.Empty);
                mapKind = mapKind is Strings.Blight or Strings.Ravaged ?
                    Strings.CurrencyType.MapsBlighted : Strings.CurrencyType.Maps;
                if (resultDat.ID != mapKind)
                {
                    runLoop = false;
                }
            }

            if (runLoop)
            {
                foreach (CurrencyEntrie entrieDat in resultDat.Entries)
                {
                    if (entrieDat.Text == currency)
                    {
                        return entrieDat.ID;
                    }
                }
            }
        }
        return null;
    }
}
