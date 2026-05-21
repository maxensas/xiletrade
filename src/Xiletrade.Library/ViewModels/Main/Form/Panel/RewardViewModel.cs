using CommunityToolkit.Mvvm.ComponentModel;
using System.Text;
using Xiletrade.Library.Models.Poe.Contract.Extension;
using Xiletrade.Library.Models.Poe.Domain.Parser;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.ViewModels.Main.Form.Panel;

public sealed partial class RewardViewModel : ViewModelBase
{
    [ObservableProperty]
    private string text = string.Empty;

    [ObservableProperty]
    private string tip = string.Empty;

    [ObservableProperty]
    private string fgColor = string.Empty;

    internal RewardViewModel(ItemOption options)
    {
        StringBuilder sbReward = new(options.Reward);
        if (sbReward.ToString().Length > 0)
        {
            sbReward.Replace(Resources.Resources.General125_Foil, string.Empty).Replace("(", string.Empty).Replace(")", string.Empty);
            text = new(sbReward.ToString().Trim());
            fgColor = Strings.Color.Peru;
            tip = Strings.Reward.FoilUnique;
        }
    }

    internal RewardViewModel(DataManagerService dm, ItemOption options)
    {
        bool cur = false, div = false;
        var seekCurrency = options.SacrificeItem;
        if (seekCurrency.Length > 0)
        {
            var (Entry, GroupId) = dm.Currencies.FindEntryAndGroupIdByType(seekCurrency, image: false);
            if (Entry is not null)
            {
                cur = GroupId is Strings.CurrencyTypePoe1.Currency;
                div = GroupId is Strings.CurrencyTypePoe1.Cards;
            }
        }
        bool condMirrored = options.Reward == Resources.Resources.General072_RewardMirrored;
        text = cur || div ? seekCurrency : options.Reward;
        fgColor = cur ? string.Empty : div ? Strings.Color.DeepSkyBlue
            : condMirrored ? Strings.Color.Gold : Strings.Color.Peru;
        tip = cur ? Strings.Reward.DoubleCurrency : div ? Strings.Reward.DoubleDivCards
            : condMirrored ? Strings.Reward.MirrorRare : Strings.Reward.ExchangeUnique;
    }
}
