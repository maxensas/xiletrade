using CommunityToolkit.Mvvm.ComponentModel;
using System;
using Xiletrade.Library.Services;
using System.Linq;
using Xiletrade.Library.Shared;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

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
                var match = dm.Currencies
                    .SelectMany(result => result.Entries, (result, entry) => new { CurrencyType = result.Id, EntryText = entry.Text })
                    .FirstOrDefault(x => x.EntryText == seekCurrency 
                        && (x.CurrencyType == Strings.CurrencyTypePoe1.Currency
                        || x.CurrencyType == Strings.CurrencyTypePoe1.Cards));
                if (match is not null)
                {
                    cur = match.CurrencyType == Strings.CurrencyTypePoe1.Currency;
                    div = match.CurrencyType == Strings.CurrencyTypePoe1.Cards;
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
