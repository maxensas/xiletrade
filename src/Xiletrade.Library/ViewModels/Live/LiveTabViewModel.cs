using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using Xiletrade.Library.Models.Application.LiveSearch;
using Xiletrade.Library.Models.Poe.Domain;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared.Collection;
using Xiletrade.Library.ViewModels.Main.Result;

namespace Xiletrade.Library.ViewModels.Live;

/// <summary>
/// Tab of a live search, in the custom search : its last results, listed as the results of a price check with the time
/// they appeared, and its controls above them (green dot, orange while paused, pause / resume and stop).
/// A click on the dot gives its criteria back to the custom search.
/// </summary>
/// <remarks>
/// One tab by number of live search, kept and updated with each state of the live searches
/// (<see cref="LiveSearchStateMessage"/>) : displayed while its live search is listening.
/// </remarks>
public sealed partial class LiveTabViewModel : ViewModelBase
{
    private readonly LiveSearchService _live;
    private readonly Action<LiveSearchCriteria> _restore;
    private LiveSearchCriteria _criteria;

    /// <summary>Number of its live search, from 1 to the maximum.</summary>
    public int Number { get; }

    /// <summary>"Live" and its number, as on its toasts.</summary>
    public string Header { get; }

    /// <summary>Its live search is listening : the tab is displayed.</summary>
    [ObservableProperty]
    private bool isActive;

    [ObservableProperty]
    private bool isSelected;

    /// <summary>Name of the preset or search text when it started, otherwise "Live" and its number.</summary>
    [ObservableProperty]
    private string label;

    [ObservableProperty]
    private bool isPaused;

    /// <summary>Last results, newest first.</summary>
    [ObservableProperty]
    private AsyncObservableCollection<ResultListItemViewModel> results;

    /// <summary>Page of the live search on the trade site, null while it is not listening.</summary>
    internal string Url { get; private set; }

    /// <param name="restore">Criteria given back to the custom search.</param>
    internal LiveTabViewModel(LiveSearchService live, int number, Action<LiveSearchCriteria> restore)
    {
        _live = live;
        _restore = restore;
        Number = number;
        Header = string.Concat(Resources.Resources.Main297_Live, " ", number.ToString());
    }

    /// <param name="info">Its live search, null : not listening.</param>
    internal void Update(LiveSearchInfo info)
    {
        _criteria = info?.Criteria;
        Url = info?.Url;
        Label = info?.Label;
        IsPaused = info?.IsPaused ?? false;
        Results = info?.History;
        IsActive = info is not null;
    }

    [RelayCommand]
    private void TogglePause(object commandParameter) => _live.SetPaused(Number, !IsPaused);

    [RelayCommand]
    private void Stop(object commandParameter) => _live.Stop(Number);

    [RelayCommand]
    private void Restore(object commandParameter)
    {
        if (_criteria is not null)
        {
            _restore?.Invoke(_criteria);
        }
    }

    [RelayCommand]
    private async Task TravelToHideout(object commandParameter)
    {
        if (commandParameter is ResultListItemViewModel { Info: SaleInfo saleInfo } line
            && await _live.TravelToHideoutAsync(saleInfo.HideoutToken, Url))
        {
            line.IsVisited = true; // darker line : the travel was sent
        }
    }
}
