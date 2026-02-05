using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xiletrade.Library.Models.Ninja.Contract;
using Xiletrade.Library.Models.Ninja.Contract.Two;
using Xiletrade.Library.Models.Ninja.Domain;
using Xiletrade.Library.Models.Ninja.Domain.Two;
using Xiletrade.Library.Models.Poe.Contract.Extension;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.ViewModels.Main;

public sealed partial class NinjaViewModel : ViewModelBase
{
    private static IServiceProvider _serviceProvider;
    private readonly MainViewModel _vm;
    private readonly DataManagerService _dm;
    private readonly PoeNinjaService _ninja;

    private bool IsPoe2 { get; }

    [ObservableProperty]
    private string price;

    [ObservableProperty]
    private double valWidth = 0;

    [ObservableProperty]
    private double btnWidth = 0;

    [ObservableProperty]
    private string imageName;

    [ObservableProperty]
    private string imgLeftRightMargin;

    public NinjaViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _vm = _serviceProvider.GetRequiredService<MainViewModel>();
        _dm = _serviceProvider.GetRequiredService<DataManagerService>();
        _ninja = _serviceProvider.GetRequiredService<PoeNinjaService>();
        IsPoe2 = _dm.Config.Options.GameVersion is 1;
    }

    /// <summary>
    /// Get the generated poeninja URL of the item.
    /// </summary>
    /// <returns></returns>
    internal string GetFullUrl() => IsPoe2 ? GetNinjaInfoTwo().Link : GetNinjaInfo().Link;

    /// <summary>
    /// Try to update poeninja price with the given parameter and refresh poeninja data cache.
    /// </summary>
    /// <param name="xiletradeItem"></param>
    internal Task TryUpdatePriceTask()
    {
        return Task.Run(async () =>
        {
            try
            {
                NinjaValue ninja = null;
                //TO REFACTOR, poe2.ninja use temp models for now.
                if (IsPoe2)
                {
                    var ninjaInfoTwo = GetNinjaInfoTwo();
                    if (!ninjaInfoTwo.VerifiedLink)
                    {
                        return;
                    }
                    ninja = await GetNinjaValueAsync(ninjaInfoTwo);
                }
                else
                {
                    var ninjaInfo = GetNinjaInfo();
                    if (!ninjaInfo.VerifiedLink)
                    {
                        return;
                    }
                    ninja = await GetNinjaValueAsync(ninjaInfo);
                }
                UpdateViewModels(ninja);
            }
            catch//(WebException ex)
            {
                // we can't know if task goes wrong.
            }
        });
    }

    private async Task<NinjaValue> GetNinjaValueAsync(NinjaInfo ninjaInfo)
    {
        if (ninjaInfo.UseItemApi)
        {
            var jsonItem = await _ninja.GetNinjaItem<NinjaItemContract>(ninjaInfo);
            if (jsonItem is null)
            {
                return null;
            }
            var firstLine = jsonItem.Lines?.FirstOrDefault();
            if (ninjaInfo.Map && firstLine?.Id.Length > 0)
            {
                UpdateMapGeneration(firstLine.Id);
            }

            var line = jsonItem.Lines.FirstOrDefault(
                x => ninjaInfo.IsAllFlame || ninjaInfo.Map ? 
                x.Id.StartWith(ninjaInfo.SubType) : x.Id == ninjaInfo.SubType);
            return line is null ? null : new()
            {
                Id = line.Id,
                Name = line.Name,
                ChaosPrice = line.ChaosPrice,
                ExaltPrice = line.ExaltPrice,
                DivinePrice = line.DivinePrice
            };
        }

        var jsonCurrency = await _ninja.GetNinjaItem<NinjaCurrencyContract>(ninjaInfo);
        var lineCur = jsonCurrency?.Lines.FirstOrDefault(x => x.Id == ninjaInfo.SubType);
        return lineCur is null ? null : new()
        {
            Id = lineCur.Id,
            Name = lineCur.Name,
            ChaosPrice = lineCur.ChaosPrice
        };
    }

    private async Task<NinjaValue> GetNinjaValueAsync(NinjaInfoTwo ninjaInfoTwo)
    {
        var jsonItem = await _ninja.GetNinjaItem<NinjaItemTwoContract>(ninjaInfoTwo);
        if (jsonItem is null)
        {
            return null;
        }
        var line = jsonItem.Line.FirstOrDefault(x => x.Id == ninjaInfoTwo.Id);
        if (line is null)
        {
            return null;
        }

        var divinePrice = jsonItem.Core.Primary is "divine" ? line.PrimaryValue : 0;
        var isDivinePrimary = divinePrice > 0;
        var chaosPrice = jsonItem.Core.Primary is "chaos" ? line.PrimaryValue : 0;
        var isChaosPrimary = chaosPrice > 0;
        var exaltedPrice = jsonItem.Core.Primary is "exalted" ? line.PrimaryValue : 0;
        var isExaltedPrimary = exaltedPrice > 0;
        if (isDivinePrimary)
        {
            chaosPrice = divinePrice * jsonItem.Core.Rates.Chaos.Value;
            exaltedPrice = divinePrice * jsonItem.Core.Rates.Exalted.Value;
        }
        if (isChaosPrimary)
        {
            divinePrice = chaosPrice * jsonItem.Core.Rates.Divine.Value;
            exaltedPrice = chaosPrice * jsonItem.Core.Rates.Exalted.Value;
        }
        if (isExaltedPrimary)
        {
            divinePrice = exaltedPrice * jsonItem.Core.Rates.Divine.Value;
            chaosPrice = exaltedPrice * jsonItem.Core.Rates.Chaos.Value;
        }

        return new()
        {
            Id = line.Id,
            Name = jsonItem.Items.FirstOrDefault(x => x.Id == line.Id)?.Name,
            ChaosPrice = chaosPrice,
            ExaltPrice = exaltedPrice,
            DivinePrice = divinePrice
        };
    }

    private void UpdateMapGeneration(string mapId)
    {
        var gen = mapId.Split("-gen-");
        if (gen.Length > 1)
        {
            var currentGen = "gen-" + gen[1];
            // not saved
            if (_dm.Config.Options.NinjaMapGeneration != currentGen)
            {
                _dm.Config.Options.NinjaMapGeneration = currentGen;
            }
        }
    }

    private void UpdateViewModels(NinjaValue ninja)
    {
        if (ninja is null)
        {
            _vm.Form.Visible.Ninja = false;
            return;
        }
        _vm.Form.Visible.Ninja = true;

        double value = ninja.DivinePrice > 1 ? Math.Round(ninja.DivinePrice, 2) 
            : IsPoe2 ? Math.Round(ninja.ExaltPrice, 2) : Math.Round(ninja.ChaosPrice, 2);
        ImageName = ninja.DivinePrice > 1 ? (IsPoe2 ? "divine2" : "divine")
            : (IsPoe2 ? "exalt2" : "chaos");

        string valueString = value.ToString();
        Price = valueString;

        //TODO : update view to handle this behaviour
        double nbDigit = valueString.Length - 1;
        double charLength = 6;
        double leftPad = 63 + nbDigit * charLength;
        double rightPad = 38 - nbDigit * charLength;
        ImgLeftRightMargin = leftPad + "." + rightPad;
        ValWidth = 76 + nbDigit * charLength;
        BtnWidth = 90 + nbDigit * charLength;
    }

    /// <summary>
    /// Get the chaos equivalent of the currency item.
    /// </summary>
    /// <param name="league"></param>
    /// <param name="NameCur"></param>
    /// <param name="tier"></param>
    /// <returns></returns>
    internal async Task<double> GetChaosEqAsync(string league, string NameCur, string tier)
    {
        double error = -1;
        var type = GetNinjaType(NameCur);
        if (type is null)
        {
            return error;
        }

        var isCurrency = type is Strings.NinjaTypeOne.Currency or Strings.NinjaTypeOne.Fragment;
        var api = isCurrency ? Strings.ApiNinjaCur : Strings.ApiNinjaItem;
        var urlNinja = api + league + "&type=" + type;

        if (isCurrency)
        {
            var currency = await _ninja.GetNinjaItem<NinjaCurrencyContract>(league, type, urlNinja);
            if (currency is not null)
            {
                var line = currency.Lines.FirstOrDefault(x => x.Name == NameCur);
                return line is not null ? line.ChaosPrice : error;
            }
            return error;
        }

        var item = await _ninja.GetNinjaItem<NinjaItemContract>(league, type, urlNinja);
        if (item is null)
        {
            return error;
        }
        if (type is Strings.NinjaTypeOne.Map && tier is not null)
        {
            var line = item.Lines.FirstOrDefault(x => x.Name == NameCur && x.Id.Contain("-" + tier + "-"));
            return line is not null ? line.ChaosPrice : error;
        }
        if (type is Strings.NinjaTypeOne.UniqueMap)
        {
            var split = NameCur.Split('(');
            if (split.Length is 2)
            {
                string mapName = split[0].Trim();
                string tierUnique = "-t" + split[1].Replace("Tier ", string.Empty).Replace(")", string.Empty).Trim();
                var line = item.Lines.FirstOrDefault(x => x.Name == mapName && x.Id.EndWith(tierUnique));
                return line is not null ? line.ChaosPrice : error;
            }
        }
        var lineDef = item.Lines.FirstOrDefault(x => x.Name == NameCur);
        return lineDef is not null ? lineDef.ChaosPrice : error;
    }

    private NinjaInfo GetNinjaInfo()
    {
        string level = string.Empty, quality = string.Empty;
        var influences = _vm.Form.Influence.GetSate("/");
        if (influences.Length is 0) influences = Resources.Resources.Main036_None;
        var iLvl = _vm.Form.Panel.Row.FirstRow.FirstOrDefault(x => x.Id is StatPanel.CommonItemLevel);
        if (iLvl?.Min.Length > 0)
        {
            level = iLvl.Min.Trim();
        }
        var qual = _vm.Form.Panel.Row.FirstRow.FirstOrDefault(x => x.Id is StatPanel.CommonQuality);
        if (qual?.Min.Length > 0)
        {
            quality = qual.Min.Trim();
        }
        return new(_dm, _ninja, _vm.Form.GetXiletradeItem(), _vm.Item
            , _vm.Form.League[_vm.Form.LeagueIndex]
            , level, quality, influences);
    }

    private NinjaInfoTwo GetNinjaInfoTwo()
    {
        return new(_dm, _ninja, _vm.Form.League[_vm.Form.LeagueIndex], _vm.Item);
    }

    private string GetNinjaType(ReadOnlySpan<char> NameCur)
    {
        var curEn = _dm.CurrenciesEn.FindEntryByType(NameCur);
        if (curEn is null)
        {
            return null;
        }
        if (curEn.Id.Contain(Strings.CurrencyTypePoe1.Maps))
        {
            return Strings.NinjaTypeOne.Map;
        }

        var cur = curEn.Id;
        return cur is Strings.CurrencyTypePoe1.Currency or Strings.CurrencyTypePoe1.Catalysts
            or Strings.CurrencyTypePoe1.Exotic ? Strings.NinjaTypeOne.Currency
            : cur is Strings.CurrencyTypePoe1.Splinters
            or Strings.CurrencyTypePoe1.Fragments ? Strings.NinjaTypeOne.Fragment
            : cur is Strings.CurrencyTypePoe1.DeliriumOrbs ? Strings.NinjaTypeOne.DeliriumOrb
            : cur is Strings.CurrencyTypePoe1.Oils ? Strings.NinjaTypeOne.Oil
            : cur is Strings.CurrencyTypePoe1.Incubators ? Strings.NinjaTypeOne.Incubator
            : cur is Strings.CurrencyTypePoe1.Scarabs ? Strings.NinjaTypeOne.Scarab
            : cur is Strings.CurrencyTypePoe1.Delve ? 
            NameCur.EndWith("Fossil") ? Strings.NinjaTypeOne.Fossil : Strings.NinjaTypeOne.Resonator
            : cur is Strings.CurrencyTypePoe1.Essences ? Strings.NinjaTypeOne.Essence
            : cur is Strings.CurrencyTypePoe1.Cards ? Strings.NinjaTypeOne.DivinationCard
            : cur is Strings.CurrencyTypePoe1.Prophecies ? Strings.NinjaTypeOne.Prophecy
            : cur is Strings.CurrencyTypePoe1.MapsUnique ? Strings.NinjaTypeOne.UniqueMap
            : cur is Strings.CurrencyTypePoe1.MapsBlighted ? Strings.NinjaTypeOne.BlightedMap
            : cur is Strings.CurrencyTypePoe1.Runegrafts ? Strings.NinjaTypeOne.Runegraft
            //: cur is "MemoryLine" ? Strings.NinjaTypeOne.Memory
            : cur is Strings.CurrencyTypePoe1.Expedition ? Strings.NinjaTypeOne.Artifact
            : cur is Strings.CurrencyTypePoe1.Ancestor ? 
            NameCur.StartWith("Omen") ? Strings.NinjaTypeOne.Omen : Strings.NinjaTypeOne.Tattoo
            //: cur is "Misc" ? null
            : null;
    }
}
