using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using Xiletrade.Library.Shared.Collection;
using Xiletrade.Library.ViewModels.Main.Form.Panel;

namespace Xiletrade.Library.ViewModels.Main.Form.Search;

/// <summary>
/// Section of the filters of the custom search, as on the trade site : type, weapon, map, miscellaneous...
/// </summary>
/// <remarks>The min / max are the ones of <see cref="CustomSearchViewModel.MinMaxList"/>, grouped by section.</remarks>
public sealed partial class FilterSectionViewModel : ViewModelBase
{
    public string Title { get; }

    /// <summary>The rarity of the form is displayed in this section (type filters).</summary>
    public bool ShowRarity { get; }

    /// <summary>The options of the form (corrupted, identified...) are displayed in this section (miscellaneous).</summary>
    public bool ShowOptions { get; }

    public AsyncObservableCollection<MinMaxViewModel> MinMax { get; } = new();

    /// <summary>Buyout price (trade filters) : currencies of the form and min / max, null in the other sections.</summary>
    public MinMaxViewModel Price { get; }

    public bool ShowPrice => Price is not null;

    /// <summary>The filters of the section are displayed.</summary>
    [ObservableProperty]
    private bool isExpanded;

    internal FilterSectionViewModel(string title, IEnumerable<MinMaxViewModel> minMax,
        bool isExpanded = false, bool showRarity = false, bool showOptions = false, MinMaxViewModel price = null)
    {
        Title = title;
        Price = price;
        ShowRarity = showRarity;
        ShowOptions = showOptions;
        this.isExpanded = isExpanded;
        foreach (var vm in minMax)
        {
            MinMax.Add(vm);
        }
    }
}
