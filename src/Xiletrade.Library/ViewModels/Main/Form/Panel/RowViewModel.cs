using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using Xiletrade.Library.Models.Poe.Domain;
using Xiletrade.Library.Shared.Collection;

namespace Xiletrade.Library.ViewModels.Main.Form.Panel;

public sealed partial class RowViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool useBorderThickness;

    [ObservableProperty]
    private AsyncObservableCollection<MinMaxViewModel> firstRow = new();

    [ObservableProperty]
    private AsyncObservableCollection<MinMaxViewModel> secondRow = new();

    [ObservableProperty]
    private AsyncObservableCollection<MinMaxViewModel> thirdRow = new();

    [ObservableProperty]
    private AsyncObservableCollection<MinMaxViewModel> fourthRow = new();

    internal void FillBottomFormLists(IEnumerable<MinMaxModel> minMaxList)
    {
        foreach (var minMax in minMaxList)
        {
            if (minMax.Min == string.Empty && minMax.Max == string.Empty)
            {
                continue;
            }
            if (FirstRow.Count < 3)
            {
                FirstRow.Add(new(minMax));
                continue;
            }
            if (SecondRow.Count < 3)
            {
                SecondRow.Add(new(minMax));
                continue;
            }
            if (ThirdRow.Count < 3)
            {
                ThirdRow.Add(new(minMax));
                continue;
            }
            if (FourthRow.Count < 3)
            {
                FourthRow.Add(new(minMax));
                continue;
            }
        }
    }
}
