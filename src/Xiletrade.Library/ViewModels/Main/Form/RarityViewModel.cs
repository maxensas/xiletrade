using CommunityToolkit.Mvvm.ComponentModel;
using Xiletrade.Library.Models.Collections;

namespace Xiletrade.Library.ViewModels.Main.Form;

public sealed partial class RarityViewModel : ViewModelBase
{
    [ObservableProperty]
    private string item;

    [ObservableProperty]
    private int index;

    [ObservableProperty]
    private AsyncObservableCollection<string> comboBox = new();
}
