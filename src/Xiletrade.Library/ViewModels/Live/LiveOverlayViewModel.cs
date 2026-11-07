using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Xiletrade.Library.Models.Application.LiveSearch;
using Xiletrade.Library.Services;

namespace Xiletrade.Library.ViewModels.Live;

/// <summary>
/// Buttons of the live searches over the game : pause (the results are skipped, the connection is kept) and stop.
/// </summary>
/// <remarks>
/// Own state of the live searches : the buttons stay while the forms change (the custom search can be closed,
/// a price check has no custom search).
/// </remarks>
public sealed partial class LiveOverlayViewModel : ViewModelBase
{
    private readonly LiveSearchService _live;

    /// <summary>A live search is listening : the buttons are displayed.</summary>
    [ObservableProperty]
    private bool isRunning;

    [ObservableProperty]
    private bool isPaused;

    public LiveOverlayViewModel(LiveSearchService live)
    {
        _live = live;
        IsRunning = live.IsRunning;
        IsPaused = live.IsPaused;
        WeakReferenceMessenger.Default.Register<LiveOverlayViewModel, LiveSearchStateMessage>(this,
            static (vm, state) => (vm.IsRunning, vm.IsPaused) = (state.IsRunning, state.IsPaused));
    }

    [RelayCommand]
    private void TogglePause(object commandParameter) => _live.SetPaused(!_live.IsPaused);

    [RelayCommand]
    private void Stop(object commandParameter) => _live.StopAll();
}
