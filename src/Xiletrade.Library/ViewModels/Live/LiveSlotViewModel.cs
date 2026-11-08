using CommunityToolkit.Mvvm.Input;
using Xiletrade.Library.Models.Application.LiveSearch;
using Xiletrade.Library.Services;

namespace Xiletrade.Library.ViewModels.Live;

/// <summary>
/// Live search listening, in the custom search : red dot (orange while paused), pause / resume and stop.
/// </summary>
/// <remarks>Created again with each state of the live searches (<see cref="LiveSearchStateMessage"/>).</remarks>
public sealed partial class LiveSlotViewModel : ViewModelBase
{
    private readonly LiveSearchService _live;

    public int Number { get; }

    /// <summary>Name of the preset or search text when it started, otherwise "Live" and its number (tooltip).</summary>
    public string Label { get; }

    public bool IsPaused { get; }

    internal LiveSlotViewModel(LiveSearchService live, LiveSearchInfo info)
    {
        _live = live;
        Number = info.Number;
        Label = info.Label;
        IsPaused = info.IsPaused;
    }

    [RelayCommand]
    private void TogglePause(object commandParameter) => _live.SetPaused(Number, !IsPaused);

    [RelayCommand]
    private void Stop(object commandParameter) => _live.Stop(Number);
}
