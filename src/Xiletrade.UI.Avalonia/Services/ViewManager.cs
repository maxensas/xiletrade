using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Xiletrade.Library.Models.GitHub.Contract;
using Xiletrade.Library.Models.Poe.Contract;
using Xiletrade.Library.Services;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Services.Interface.View;
using Xiletrade.Library.ViewModels.Start;
using Xiletrade.Library.ViewModels.Update;
using Xiletrade.Library.ViewModels.Whisper;
using Xiletrade.UI.Avalonia.Views;

namespace Xiletrade.UI.Avalonia.Services;

/// <summary>
/// Provides window management services for the Xiletrade application, including showing or closing views and handling keyboard input.
/// </summary>
/// <remarks>
/// Avalonia UI Framework implementation of the INavigationService interface. 
/// </remarks>
public class ViewManager : IViewManager
{
    private readonly IServiceProvider _sp;
    private readonly IMessageAdapterService _message;
    private readonly IUpdateDownloader _updater;
    private readonly LocalizationService _localization;
    private readonly DataManagerService _dm;
    private readonly ClipboardService _clipboard;
    private readonly UIService _ui;

    public ViewManager(IServiceProvider sp, ILogger<ViewManager> logger,
        IMessageAdapterService message, 
        IUpdateDownloader updater, LocalizationService localization,
        ClipboardService clipboard, DataManagerService dm, UIService ui
        // Instantiate singletons :
        /*,MainView main, TaskbarIcon taskbarIcon*/)
    {
        _sp = sp;
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

    public void ClearKeyboardFocus()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var window = desktop.MainWindow;
            window?.Focus();
        }
    }

    public void CloseMainView()
    {
        var win = _sp.GetRequiredService<MainView>();
        if (win.IsVisible)
        {
            win.Close();
        }
    }

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

    

    public void InstantiateMainView()
    {
        var appLifetime = (IClassicDesktopStyleApplicationLifetime)Application.Current!.ApplicationLifetime!;
        var win = _sp.GetRequiredService<MainView>();
        appLifetime.MainWindow = win;
    }

    public bool IsVisibleMainView() => _sp.GetRequiredService<MainView>().IsVisible;

    public void SetMainHandle(object view)
    {
        if (view is Window win)
        {
            var platformImpl = (win.PlatformImpl as IPlatformHandle);
            IntPtr hwnd = platformImpl?.Handle ?? IntPtr.Zero;
            _ui.MainWindowHandle = hwnd;
        }
    }

    public void ShowConfigView() => _sp.GetRequiredService<PopView>().Show();

    public void ShowEditorView() => _sp.GetRequiredService<EditorView>().Show();

    public void ShowMainView()
    {
        Action showMainWindow = new(() =>
        {
            try
            {
                var win = _sp.GetRequiredService<MainView>();
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

    public void ShowRegexView() => _sp.GetRequiredService<RegexView>().Show();

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

    public void ShutDownXiletrade(int code = 0)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown(code);
        }
    }

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
        throw new NotImplementedException();
    }

    public void ShutDownNativeApp(int exitCode = 0)
    {
        throw new NotImplementedException();
    }
}
