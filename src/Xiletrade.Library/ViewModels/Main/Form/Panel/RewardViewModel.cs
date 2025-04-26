using CommunityToolkit.Mvvm.ComponentModel;
using System;
using Xiletrade.Library.Services;
using System.Linq;
using Xiletrade.Library.Shared;
using System.Collections.Generic;

namespace Xiletrade.Library.ViewModels.Main.Form.Panel;

public sealed partial class RewardViewModel : ViewModelBase
{
    [ObservableProperty]
    private string text = string.Empty;

    [ObservableProperty]
    private string tip = string.Empty;

    [ObservableProperty]
    private string fgColor = string.Empty;

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
                var isCur =
                    from result in DataManager.Currencies
                    from Entrie in result.Entries
                    where result.Id == Strings.CurrencyTypePoe1.Currency && Entrie.Text == seekCurrency
                    select true;
                if (isCur.Any() && isCur.First())
                {
                    cur = true;
                }
                if (!cur)
                {
                    var isDiv =
                        from result in DataManager.Currencies
                        from Entrie in result.Entries
                        where result.Id == Strings.CurrencyTypePoe1.Cards && Entrie.Text == seekCurrency
                        select true;
                    if (isDiv.Any() && isDiv.First())
                    {
                        div = true;
                    }
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
