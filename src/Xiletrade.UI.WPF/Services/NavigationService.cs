using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Windows;
using Xiletrade.Library.Models.GitHub.Contract;
using Xiletrade.Library.Models.Poe.Contract;
using Xiletrade.Library.Services;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Services.Interface.View;
using Xiletrade.Library.ViewModels.Start;
using Xiletrade.Library.ViewModels.Update;
using Xiletrade.Library.ViewModels.Whisper;
using Xiletrade.UI.WPF.Views;

namespace Xiletrade.UI.WPF.Services;

/// <summary>
/// Provides window management services for the Xiletrade application, including showing or closing views and handling keyboard input.
/// </summary>
/// <remarks>
///  WPF UI Framework implementation of the INavigationService interface. 
/// </remarks>
public class NavigationService : INavigationService
{
    private readonly IServiceProvider _sp;
    private readonly ILogger<NavigationService> _logger;
    private readonly IMessageAdapterService _message;
    private readonly IUpdateDownloader _updater;
    private readonly LocalizationService _localization;
    private readonly DataManagerService _dm;
    private readonly ClipboardService _clipboard;
    private readonly IUIService _ui;
    
    private IViewBase ConfigView => _sp.GetRequiredService<IConfigView>();
    private IViewBase UpdateView => _sp.GetRequiredService<IUpdateView>();
    private IViewBase RegexView => _sp.GetRequiredService<IRegexView>();
    private IViewBase EditorView => _sp.GetRequiredService<IEditorView>();

    public NavigationService(IServiceProvider sp, ILogger<NavigationService> logger,
        IMessageAdapterService message, IUpdateDownloader updater, LocalizationService localization,
        ClipboardService clipboard, DataManagerService dm, IUIService ui)
    {
        _sp = sp;
        _logger = logger;
        _message = message;
        _updater = updater;
        _localization = localization;
        _clipboard = clipboard;
        _dm = dm;
        _ui = ui;

#if DEBUG
        logger.LogInformation("Service launched");
#endif
    }

    public void ShowMainView()
    {
        Action showMainView = new(() =>
        {
            try
            {
                Application.Current.MainWindow.Show();
                Application.Current.MainWindow.ShowActivated = false;
            }
            catch (Exception ex)
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug("Exception raised : {Message}", ex.Message);
            }
        });
        _ui.Invoke(showMainView);
    }

    public bool IsVisibleMainView()
    {
        return Application.Current.MainWindow is not null 
            && Application.Current.MainWindow.IsVisible;
    }

    public void CloseMainView()
    {
        if (Application.Current.MainWindow is not null 
            && Application.Current.MainWindow.IsVisible)
        {
            Application.Current.MainWindow.Close();
        }
    }

    public void ShowEditorView() => EditorView.Show();

    public void ShowConfigView() => ConfigView.Show();

    public async Task ShowStartView() => await CreateDialog<StartView>
        (new StartViewModel(_dm, _localization)).ConfigureAwait(false);

    public void ShowWhisperView(Tuple<FetchDataListing, OfferInfo> data) 
        => CreateWindow<WhisperListView>(new WhisperViewModel(_dm, _clipboard, data), false);

    public void ShowPopupView(string imgName)
    {
        PopView Popup = new(imgName); // viewmodel not used.
    }

    public void SetMainHandle(object view)
    {
        if (view is not Window win)
        {
            throw new ArgumentException("The provided view must be a WPF Window.", nameof(view));
        }
        _ui.MainWindowHandle = new System.Windows.Interop.WindowInteropHelper(win).Handle;
    }

    public void ShutDownNativeApp(int code = 0) => Application.Current.Shutdown(code);

    public void ShowRegexView() => RegexView.Show();

    //.ShowDialog();
    public void ShowUpdateView(GitHubRelease release)
    {
        Action showUpdateWindow = new(() =>
        {
            var view = UpdateView;
            view.DataContext = new UpdateViewModel(_updater, _message, _ui, release);
            view.ShowDialog();
        });
        _ui.Invoke(showUpdateWindow);
    }

    private static void CreateWindow<T>(object dataContext, bool show) where T : IViewBase, new()
    {
        if (Activator.CreateInstance<T>() is not Window window)
            throw new InvalidOperationException("T must be a Window.");

        window.DataContext = dataContext;

        if (show)
            window.Show();
    }

    private static Task CreateDialog<T>(object dataContext) where T : IViewBase, new()
    {
        if (Activator.CreateInstance<T>() is not Window window)
            throw new InvalidOperationException("T must be a Window.");

        window.DataContext = dataContext;
        window.ShowDialog();
        return Task.CompletedTask;
    }
}
