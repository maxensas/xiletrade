using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Windows;
using Xiletrade.Library.Models.GitHub.Contract;
using Xiletrade.Library.Models.Poe.Contract;
using Xiletrade.Library.Services.Extension;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.ViewModels.Update;
using Xiletrade.Library.ViewModels.Whisper;
using Xiletrade.Library.Views;
using Xiletrade.UI.WPF.Views;

namespace Xiletrade.UI.WPF.Services;

/// <summary>
/// WPF UI Framework implementation of the IViewManager interface. 
/// </summary>
public class ViewManager : IViewManager
{
    private readonly IServiceProvider _sp;
    private readonly ILogger<ViewManager> _logger;
    private readonly IUIService _ui;
    private readonly Action _showMainView;

    private IViewBase ConfigView => _sp.GetRequiredService<IConfigView>();
    private IViewBase StartView => _sp.GetRequiredService<IStartView>();
    private IViewBase RegexView => _sp.GetRequiredService<IRegexView>();
    private IViewBase EditorView => _sp.GetRequiredService<IEditorView>();

    private static Window MainWindow => Application.Current.MainWindow;

    public nint MainHandle { get; private set; }

    public ViewManager(IServiceProvider sp, ILogger<ViewManager> logger, IUIService ui)
    {
        _sp = sp;
        _logger = logger;
        _ui = ui;

        _showMainView = new(() =>
        {
            try
            {
                MainWindow.Show();
                MainWindow.ShowActivated = false;
            }
            catch (Exception ex)
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug("Exception raised : {Message}", ex.Message);
            }
        });
#if DEBUG
        logger.LogInformation("Service launched");
#endif
    }

    public void ShowMainView() => _ui.Invoke(_showMainView);

    public bool IsVisibleMainView() => MainWindow is not null && MainWindow.IsVisible;

    public void CloseMainView()
    {
        if (IsVisibleMainView()) MainWindow.Close();
    }

    public void ShowEditorView() => EditorView.Show();

    public void ShowConfigView() => ConfigView.Show();

    public Task ShowStartView()
    {
        StartView.ShowDialog();
        return Task.CompletedTask;
    }

    public void ShowWhisperView(Tuple<FetchDataListing, OfferInfo> data)
    {
        var window = new WhisperListView(_sp.CreateInstance<WhisperViewModel>(data));
        window.Show();
    }

    public void ShowPopupView(string imgName)
    {
        _ = new PopView(imgName); // viewmodel not used.
    }

    public void SetMainHandle(object view)
    {
        if (view is not Window win)
        {
            throw new ArgumentException("The provided view must be a WPF Window.", nameof(view));
        }
        MainHandle = new System.Windows.Interop.WindowInteropHelper(win).Handle;
    }

    public void ShutDownNativeApp(int code = 0) => Application.Current.Shutdown(code);

    public void ShowRegexView() => RegexView.Show();

    public void ShowUpdateView(GitHubRelease release)
    {
        var view = new UpdateView(_sp.CreateInstance<UpdateViewModel>(release));
        view.ShowDialog();
    }

    public void ClearFocus() => System.Windows.Input.Keyboard.ClearFocus(); // UI responsibility, contrary to what code say.
}
