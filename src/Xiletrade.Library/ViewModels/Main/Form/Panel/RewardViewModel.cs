using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xiletrade.Library.Models.Poe.Contract.Extension;
using Xiletrade.Library.Models.Poe.Domain.Parser;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.ViewModels.Main.Form.Panel;

public sealed partial class RewardViewModel : ViewModelBase
{
    private static IServiceProvider _serviceProvider;

    [ObservableProperty]
    private string text = string.Empty;

    [ObservableProperty]
    private string tip = string.Empty;

    [ObservableProperty]
    private string fgColor = string.Empty;

    public RewardViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    internal void UpdateReward(ItemOption options)
    {
        bool cur = false, div = false;
        var seekCurrency = options.SacrificeItem;
        if (seekCurrency.Length > 0)
        {
            var dm = _serviceProvider.GetRequiredService<DataManagerService>();
            var (Entry, GroupId) = dm.Currencies.FindEntryAndGroupIdByType(seekCurrency, image: false);
            if (Entry is not null)
            {
                cur = GroupId is Strings.CurrencyTypePoe1.Currency;
                div = GroupId is Strings.CurrencyTypePoe1.Cards;
            }
        }
        bool condMirrored = options.Reward == Resources.Resources.General072_RewardMirrored;
        Text = cur || div ? seekCurrency : options.Reward;
        FgColor = cur ? string.Empty : div ? Strings.Color.DeepSkyBlue
            : condMirrored ? Strings.Color.Gold : Strings.Color.Peru;
        Tip = cur ? Strings.Reward.DoubleCurrency : div ? Strings.Reward.DoubleDivCards
            : condMirrored ? Strings.Reward.MirrorRare : Strings.Reward.ExchangeUnique;
    }
}
