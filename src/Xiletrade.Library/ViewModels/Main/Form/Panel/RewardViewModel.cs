using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Xiletrade.Library.Models.Poe.Contract.Extension;
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

    public void UpdateReward(Dictionary<string, string> option)
    {
        bool cur = false, div = false;
        string seekCurrency = string.Empty;
        int idxCur = option[Resources.Resources.General070_ReqSacrifice].IndexOf(" x", StringComparison.Ordinal);
        if (idxCur > -1)
        {
            seekCurrency = option[Resources.Resources.General070_ReqSacrifice].AsSpan(0, idxCur).ToString();
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
        }
        bool condMirrored = option[Resources.Resources.General071_Reward] == Resources.Resources.General072_RewardMirrored;
        Text = cur || div ? seekCurrency : option[Resources.Resources.General071_Reward];
        FgColor = cur ? string.Empty : div ? Strings.Color.DeepSkyBlue
            : condMirrored ? Strings.Color.Gold : Strings.Color.Peru;
        Tip = cur ? Strings.Reward.DoubleCurrency : div ? Strings.Reward.DoubleDivCards
            : condMirrored ? Strings.Reward.MirrorRare : Strings.Reward.ExchangeUnique;
    }
}
