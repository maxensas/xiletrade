using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xiletrade.Library.Models.Ninja.Contract;
using Xiletrade.Library.Models.Ninja.Contract.Exchange.Detail;
using Xiletrade.Library.Models.Ninja.Domain;
using Xiletrade.Library.Models.Poe.Contract.Extension;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.ViewModels.Main;

public sealed partial class NinjaViewModel : ViewModelBase
{
    private static IServiceProvider _serviceProvider;
    private readonly MainViewModel _vm;
    private readonly DataManagerService _dm;
    private readonly PoeNinjaService _ninja;

    [ObservableProperty]
    private string price;

    [ObservableProperty]
    private string imageName;

    [ObservableProperty]
    private NinjaDetail detail;

    private NinjaInfoBase NinjaInfoBase { get; set; }

    public NinjaViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _vm = _serviceProvider.GetRequiredService<MainViewModel>();
        _dm = _serviceProvider.GetRequiredService<DataManagerService>();
        _ninja = _serviceProvider.GetRequiredService<PoeNinjaService>();
    }

    /// <summary>
    /// Get the generated poeninja URL of the item.
    /// </summary>
    /// <returns></returns>
    internal string FullUrl => NinjaInfoBase.Link;

    /// <summary>
    /// Try to update poeninja price with the given parameter and refresh poeninja data cache.
    /// </summary>
    /// <param name="xiletradeItem"></param>
    internal async Task TryUpdateNinjaTask()
    {
        try
        {
            NinjaInfoBase = _vm.Item.IsPoe2 ? !_vm.Item.Flag.Rarity.Unique ? 
                new NinjaInfoExchangeTwo(_serviceProvider) : new NinjaInfoTwo(_serviceProvider) 
                : _vm.Item.State.ExchangeCurrency ? 
                new NinjaInfoExchange(_serviceProvider) : new NinjaInfo(_serviceProvider);

            if (NinjaInfoBase is null || !NinjaInfoBase.VerifiedLink)
                return;

            var ninja = await NinjaInfoBase.GetNinjaValueAsync();
            if (ninja is not null)
            {
                if (NinjaInfoBase is NinjaInfo info && info.Map)
                {
                    NinjaInfoBase.Link += ninja.Id;
                }
                if (ninja.Detail is not null)
                {
                    Detail = ninja.Detail;

                    if (!_vm.Form.Tab.HistoryEnable)
                    {
                        _vm.Form.Tab.HistoryEnable = _vm.Form.Tab.HistorySelected = true;
                        _vm.Form.Visible.Poeprices = false;
                        _vm.Form.Tab.QuickEnable = _vm.Form.Tab.DetailEnable = false;
                    }
                }

                _vm.Form.Visible.Ninja = true;

                double value = ninja.DivinePrice > 1 ? Math.Round(ninja.DivinePrice, 2)
                : _vm.Item.IsPoe2 ? Math.Round(ninja.ExaltPrice, 2) : Math.Round(ninja.ChaosPrice, 2);

                Price = value.ToString();
                ImageName = ninja.DivinePrice > 1 ? (_vm.Item.IsPoe2 ? "divine2" : "divine")
                    : (_vm.Item.IsPoe2 ? "exalt2" : "chaos");
            }

            if (_vm.Form.UnidentifiedUnique)
            {
                UpdateUniqueListIcons();
            }
        }
        catch(Exception ex)
        {
#if DEBUG
            var logger = _serviceProvider.GetRequiredService<ILogger<NinjaViewModel>>();
            logger.LogInformation("Exception raised : {Message}", ex.Message);
#endif
        }
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
        var api = isCurrency ? Strings.ApiNinjaExchangeOverview : Strings.ApiNinjaItem;
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
            or Strings.CurrencyTypePoe1.Exotic or Strings.CurrencyTypePoe1.Keepers ? Strings.NinjaTypeOne.Currency
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
            : cur is Strings.CurrencyTypePoe1.DjinnCoins ? Strings.NinjaTypeOne.DjinnCoin
            : cur is Strings.CurrencyTypePoe1.Ducats ? Strings.NinjaTypeOne.Ducat // TO TEST
            : cur is Strings.CurrencyTypePoe1.EnshroudingCrystals ? Strings.NinjaTypeOne.EnshroudingCrystal // TO TEST
            : cur is Strings.CurrencyTypePoe1.Ancestor ? 
            NameCur.StartWith("Omen") ? Strings.NinjaTypeOne.Omen : Strings.NinjaTypeOne.Tattoo
            //: cur is "Misc" ? null
            : null;
    }

    private void UpdateUniqueListIcons()
    {
        foreach (var item in _vm.Form.Unique)
        {
            item.Icon = _ninja.GetIcon(item.Name);
        }
    }
}
