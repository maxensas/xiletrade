using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Xiletrade.Library.Models.GitHub.Contract;
using Xiletrade.Library.Models.Poe.Contract;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Views;
using Xiletrade.UI.Avalonia.Views;

namespace Xiletrade.UI.Avalonia.Services;

/// <summary>
/// MIGRATION TO FINISH <br/><br/>
/// Provides window management services for the Xiletrade application, including showing or closing views and handling keyboard input.
/// </summary>
/// <remarks>
/// Avalonia UI Framework implementation of the INavigationService interface. 
/// </remarks>
public class ViewManager : IViewManager
{
    private readonly IServiceProvider _sp;
    private readonly ILogger<ViewManager> _logger;
    private readonly IUIService _ui;

    private MainView MainView => _sp.GetRequiredService<MainView>();
    private ConfigView ConfigView => _sp.GetRequiredService<ConfigView>();
    private StartView StartView => _sp.GetRequiredService<StartView>();
    private RegexView RegexView => _sp.GetRequiredService<RegexView>();
    private EditorView EditorView => _sp.GetRequiredService<EditorView>();
    private PopView PopView => _sp.GetRequiredService<PopView>();

    public nint MainHandle { get; set; }

    public ViewManager(IServiceProvider sp, ILogger<ViewManager> logger, IUIService ui)
    {
        _sp = sp;
        _logger = logger;
        _ui = ui;

#if DEBUG
        logger.LogInformation("Service launched");
#endif
    }

    public void CloseMainView()
    {
        var win = MainView;
        if (win.IsVisible)
        {
            win.Close();
        }
    }

    public void InstantiateMainView()
    {
        var appLifetime = (IClassicDesktopStyleApplicationLifetime)Application.Current!.ApplicationLifetime!;
        appLifetime.MainWindow = MainView;
    }

    public bool IsVisibleMainView() => MainView.IsVisible;

    public void SetMainHandle(object view)
    {
        if (view is Window win)
        {
            var platformImpl = (win.PlatformImpl as IPlatformHandle);
            IntPtr hwnd = platformImpl?.Handle ?? IntPtr.Zero;
            MainHandle = hwnd;
        }
    }

    public void ShowConfigView() => PopView.Show();

    public void ShowEditorView() => EditorView.Show();

    public void ShowMainView()
    {
        Action showMainWindow = new(() =>
        {
            try
            {
                var win = MainView;
                win.WindowStartupLocation = WindowStartupLocation.Manual;
                win.Show();
            }
            catch (Exception)
            {
                //nothing
            }
        });
        _ui.Invoke(showMainWindow);
    }

    public void ShowPopupView(string imgName)
    {
        _ = new PopView(imgName); // viewmodel not used.
    }

    public void ShowRegexView() => RegexView.Show();

    public async Task ShowStartView()
    {
        await CreateDialog<StartView>().ConfigureAwait(false);
    }

    public void ShowUpdateView(GitHubRelease release)
    {
        throw new NotImplementedException();
    }

    public void ShowWhisperView(Tuple<FetchDataListing, OfferInfo> data)
    {
        throw new NotImplementedException();
    }

    /*
    public async Task ShowStartView()
    {
        await CreateDialog<StartView>(new StartViewModel(_dm, _localization)).ConfigureAwait(false);
    }

    public void ShowUpdateView(GitHubRelease release)
    {
        Action showUpdateWindow = new(() =>
        {
            var view = _sp.GetRequiredService<UpdateView>();
            view.DataContext = new UpdateViewModel(_updater, _message, _ui, release);
            view.ShowDialog(null);
        });
        _ui.Invoke(showUpdateWindow);
    }

    public void ShowWhisperView(Tuple<FetchDataListing, OfferInfo> data) 
        => CreateWindow<WhisperListView>(new WhisperViewModel(_dm, _clipboard, data), false);
    */
    private static void CreateWindow<T>(object dataContext, bool show) where T : IViewBase, new()
    {
        var window = GetNewWindow<T>(dataContext);
        if (show)
            window.Show();
    }

    private static Task CreateDialog<T>(object dataContext) where T : IViewBase, new()
    {
        var window = GetNewWindow<T>(dataContext);
        var tcs = new TaskCompletionSource<T>();
        window.Closed += (_, __) =>
        {
            if (window.DataContext is T result)
                tcs.TrySetResult(result);
            else
                tcs.TrySetResult(default);
        };
        window.Show();

        return tcs.Task;
    }

    private Task CreateDialog<T>() where T : IViewBase, new()
    {
        var view = _sp.GetRequiredService<T>();
        var tcs = new TaskCompletionSource<T>();

        if (view is not Window window)
        {
            return Task.CompletedTask;
        }

        window.Closed += (_, __) =>
        {
            if (window.DataContext is T result)
                tcs.TrySetResult(result);
            else
                tcs.TrySetResult(default);
        };
        window.Show();

        return tcs.Task;
    }

    private static Window GetNewWindow<T>(object dataContext) where T : IViewBase, new()
    {
        var instance = new T();
        if (instance is not Window window)
            throw new InvalidOperationException("Type must be a Window.");

        window.DataContext = dataContext;
        return window;
    }

    public void ClearFocus()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var window = desktop.MainWindow;
            window?.Focus();
        }
    }

    public void ShutDownNativeApp(int exitCode = 0)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown(exitCode);
        }
    }

    /*
    public void DelegateActionToUiThread(Action action)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            action();
            return;
        }
        Dispatcher.UIThread.Post(action, DispatcherPriority.Normal); //previous DispatcherPriority.Background
    }

    public TResult DelegateFuncToUiThread<TResult>(Func<TResult> func)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            return func();
        }
        return Dispatcher.UIThread.Invoke(func, DispatcherPriority.Normal);
    }

    public async Task<TResult> DelegateActionToUiThreadAsync<TResult>(Func<Task<TResult>> asyncFunc)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            return await asyncFunc();
        }

        var tcs = new TaskCompletionSource<TResult>();

        Dispatcher.UIThread.Post(async () =>
        {
            try
            {
                var result = await asyncFunc();
                tcs.SetResult(result);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        }, DispatcherPriority.Normal);

        return await tcs.Task;
    }

    public static async Task<TResult> DelegateFuncToUiThreadAsync<TResult>(Func<Task<TResult>> asyncFunc)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            return await asyncFunc();
        }

        var tcs = new TaskCompletionSource<TResult>();
        Dispatcher.UIThread.Post(async () =>
        {
            try
            {
                var result = await asyncFunc();
                tcs.SetResult(result);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        }, DispatcherPriority.Normal);

        return await tcs.Task;
    }
    */
}
