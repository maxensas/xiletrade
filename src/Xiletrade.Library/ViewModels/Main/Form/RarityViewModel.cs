using CommunityToolkit.Mvvm.ComponentModel;
using Xiletrade.Library.Models.Collections;

namespace Xiletrade.Library.ViewModels.Main.Form;

public sealed partial class RarityViewModel : ViewModelBase
{
    [ObservableProperty]
    private string item;

    [ObservableProperty]
    private int index = 0;

    [ObservableProperty]
    private AsyncObservableCollection<string> comboBox = new() { Resources.Resources.General005_Any, Resources.Resources.General009_Normal, Resources.Resources.General008_Magic,
        Resources.Resources.General007_Rare, Resources.Resources.General006_Unique, Resources.Resources.General110_FoilUnique, Resources.Resources.General010_AnyNU };
}
