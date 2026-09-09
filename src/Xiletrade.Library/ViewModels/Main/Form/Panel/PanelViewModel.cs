using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using Xiletrade.Library.Models.Poe.Domain;
using Xiletrade.Library.Models.Poe.Domain.Parser;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared.Collection;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.ViewModels.Main.Form.Panel;

public sealed partial class PanelViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool synthesisBlight;

    [ObservableProperty]
    private bool blighRavaged;

    [ObservableProperty]
    private bool tierSelection;

    [ObservableProperty]
    private string synthesisBlightLabel = "Synthblight";// = string.Empty;

    [ObservableProperty]
    private string blighRavagedtLabel = "Ravaged";// = string.Empty;

    [ObservableProperty]
    private string facetorMin = string.Empty;

    [ObservableProperty]
    private string facetorMax = string.Empty;

    [ObservableProperty]
    private SocketViewModel sockets;

    [ObservableProperty]
    private RewardViewModel reward;

    [ObservableProperty]
    private AsyncObservableCollection<MinMaxViewModel> statList;

    internal PanelViewModel(DataManagerService dm, ItemData item, Dictionary<StatPanel, MinMaxModel> minMax)
    {
        var flag = item.Flag;
        synthesisBlight = flag.Map.Blight || flag.Tag.Synthesised;
        blighRavaged = flag.Map.BlightRavaged;
        tierSelection = dm.Config.Options.AutoSelectMinTierValue
            && !item.Flag.Rarity.Unique && !item.Flag.Tag.Mirrored && !item.Flag.Tag.Corrupted;

        if (flag.Facetor)
        {
            facetorMin = item.Options.StoredExperience;
        }

        if (flag.Area.Ultimatum && !item.IsPoe2)
        {
            reward = new(dm, item.Options);
        }

        if (flag.Map.Valdo)
        {
            reward = new(item.Options);
        }

        if (flag.Contracts)
        {
            reward = new(item.Options.HeistTarget);
        }

        if (flag.Map.IsMap)
        {
            synthesisBlightLabel = "Blighted";
        }

        if (item.Options.Socket.Length > 0)
        {
            sockets = new(item, minMax);
        }

        var minMaxVm = new AsyncObservableCollection<MinMaxViewModel>();
        foreach ((var id, var model) in minMax)
        {
            if (model.Min.Length is 0 && model.Max.Length is 0)
            {
                continue;
            }
            minMaxVm.Add(new(id, model));
        }
        statList = minMaxVm;
    }
}
