using CommunityToolkit.Mvvm.Input;
using System;
using Xiletrade.Library.Models.Application.LiveSearch;
using Xiletrade.Library.Services;

namespace Xiletrade.Library.ViewModels.Live;

/// <summary>
/// Live search listening, in the custom search : green dot (orange while paused), pause / resume and stop.
/// A click on the dot gives its criteria back to the custom search.
/// </summary>
/// <remarks>Created again with each state of the live searches (<see cref="LiveSearchStateMessage"/>).</remarks>
public sealed partial class LiveSlotViewModel : ViewModelBase
{
    private readonly LiveSearchService _live;
    private readonly LiveSearchCriteria _criteria;
    private readonly Action<LiveSearchCriteria> _restore;

    public int Number { get; }

    /// <summary>Name of the preset or search text when it started, otherwise "Live" and its number (tooltip).</summary>
    public string Label { get; }

    public bool IsPaused { get; }

    /// <param name="restore">Criteria given back to the custom search.</param>
    internal LiveSlotViewModel(LiveSearchService live, LiveSearchInfo info, Action<LiveSearchCriteria> restore)
    {
        _live = live;
        _criteria = info.Criteria;
        _restore = restore;
        Number = info.Number;
        Label = info.Label;
        IsPaused = info.IsPaused;
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
}
