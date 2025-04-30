using CommunityToolkit.Mvvm.ComponentModel;
using Xiletrade.Library.Models;
using Xiletrade.Library.Models.Collections;
using Xiletrade.Library.Models.Enums;
using Xiletrade.Library.Models.Parser;
using Xiletrade.Library.ViewModels.Main;

namespace Xiletrade.Library.ViewModels.Mock;

public sealed partial class MockFormViewModel : ViewModelBase
{
    [ObservableProperty]
    private AsyncObservableCollection<ModLineViewModel> modList = new();

    //TODO
    MockFormViewModel()
    {
        /*
        string data = string.Empty;
        string nextMod = string.Empty;
        string[] clipData = [];

        var item = new ItemData(clipData, Lang.English, false);
        var affix = new AffixFlag(data);
        var desc = new ModDescription(affix.ParsedData, false);
        var mod = new ItemModifier(affix.ParsedData, nextMod, desc.Name, item);
        var modFilter = new ModFilter(mod, item);

        var modLine = new ModLineViewModel(modFilter, affix, desc);
        ModList.Add(modLine);
        */
    }
}
