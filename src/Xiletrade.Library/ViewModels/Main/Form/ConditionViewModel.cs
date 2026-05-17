using CommunityToolkit.Mvvm.ComponentModel;
using Xiletrade.Library.Models.Poe.Domain.Parser;
using Xiletrade.Library.ViewModels.Main.Form.Panel;

namespace Xiletrade.Library.ViewModels.Main.Form;

public sealed partial class ConditionViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool freePrefix;

    [ObservableProperty]
    private bool freeSuffix;

    [ObservableProperty]
    private bool socketColors;

    [ObservableProperty]
    private string freePrefixText = Resources.Resources.Main174_cbFreePrefix;

    [ObservableProperty]
    private string freeSuffixText = Resources.Resources.Main176_cbFreeSuffix;

    [ObservableProperty]
    private string socketColorsText = Resources.Resources.Main209_cbSocketColors;

    [ObservableProperty]
    private string freePrefixToolTip = Resources.Resources.Main175_cbFreePrefixTip;

    [ObservableProperty]
    private string freeSuffixToolTip = Resources.Resources.Main177_cbFreeSuffixTip;

    [ObservableProperty]
    private string socketColorsToolTip = Resources.Resources.Main210_cbSocketColorsTip;

    internal ConditionViewModel(ItemData item, SocketViewModel sVm)
    {
        if (sVm is null)
        {
            return;
        }
        if (!item.IsPoe2)
        {
            socketColorsToolTip = sVm.GetSocketColors();
        }
        if (item.State.ImmutableSockets || sVm.WhiteColor is "6")
        {
            socketColors = true;
        }
    }
}
