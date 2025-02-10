using CommunityToolkit.Mvvm.ComponentModel;

namespace Xiletrade.Library.ViewModels.Main.Result;

public sealed partial class ResultBarViewModel : ViewModelBase
{
    public ResultBarViewModel(string price, string total)
    {
        RightString = price;
        Total = total;
    }

    [ObservableProperty]
    private string rightString = string.Empty;

    [ObservableProperty]
    private string leftString = string.Empty;

    [ObservableProperty]
    private string total;
}
