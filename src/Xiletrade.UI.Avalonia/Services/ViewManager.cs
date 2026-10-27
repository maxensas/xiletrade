using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Models.GitHub.Contract;
using Xiletrade.Library.Models.Poe.Contract;
using Xiletrade.Library.Services.Extension;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Services.Linux;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Enum;
using Xiletrade.Library.Shared.Interop.Windows;
using Xiletrade.Library.ViewModels.Update;
using Xiletrade.Library.ViewModels.Whisper;
using Xiletrade.Library.Views;
using Xiletrade.UI.Avalonia.Views;

namespace Xiletrade.UI.Avalonia.Services;

/// <remarks>
/// Avalonia UI Framework implementation of the IViewManager interface. 
/// </remarks>
public class ViewManager : IViewManager
{
    private readonly IServiceProvider _sp;
    private readonly IUIService _ui;

    // View content cleared upon closing
    private IViewBase _configView;
    private IViewBase _regexView;
    private IViewBase _editorView;
    private PopView _popView;

    private IViewBase MainView => _sp.GetRequiredService<IMainView>();
    private IViewBase ConfigView => _sp.GetRequiredService<IConfigView>();
    private IViewBase StartView => _sp.GetRequiredService<IStartView>();
    private IViewBase RegexView => _sp.GetRequiredService<IRegexView>();
    private IViewBase EditorView => _sp.GetRequiredService<IEditorView>();

    public nint MainHandle { get; private set; }

    public ViewManager(IServiceProvider sp, IUIService ui, ILogger<ViewManager> logger)
    {
        _sp = sp;
        _ui = ui;

#if DEBUG
        logger.LogInformation("Service launched");
#endif
    }

    public void CloseMainView() => MainView.Close();

    public bool IsVisibleMainView() => MainView.IsVisible;

    public void SetMainHandle(object view)
    {
        if (view is not Window win)
        {
            throw new ArgumentException("The provided view must be an Avalonia window.", nameof(view));
        }
        MainHandle = win.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
    }

    public void ShowConfigView()
    {
        CloseMainView();
        _configView?.Close();
        _configView = ConfigView;
        _configView.Show();
    }

    public void ShowEditorView()
    {
        _configView?.Close();
        _editorView?.Close();
        _editorView = EditorView;
        _editorView.Show();
    }

    public void ShowMainView()
    {
        var view = MainView;
        _ui.Invoke(view.Show);
    }

    public void ShowRegexView()
    {
        CloseMainView();
        _regexView?.Close();
        _regexView = RegexView;
        _regexView.Show();
    }

    public void ShowPopupView(string imgName)
    {
        CloseMainView();
        _popView?.Close();
        _popView = new PopView(imgName); // viewmodel not used.
    }

    public Task ShowStartView()
    {
        ShowAndWait(StartView);
        return Task.CompletedTask;
    }

    public void ShowUpdateView(GitHubRelease release)
    {
        var window = new UpdateView(_sp.CreateInstance<UpdateViewModel>(release));
        ShowAndWait(window);
    }

    public void ShowWhisperView(Tuple<FetchDataListing, OfferInfo> data)
    {
        var window = new WhisperListView(_sp.CreateInstance<WhisperViewModel>(data));
        window.Show();
    }

    public void ClearFocus()
    {
        if (MainView is Window win)
        {
            win?.Focus();
        }
    }

    public void ShutDownNativeApp(int exitCode = 0)
    {
        if (Application.Current?.ApplicationLifetime 
            is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown(exitCode);
        }
    }

    // private

    /// <summary>
    /// Works like a dialog, blocks code execution and waits for the view to be closed.
    /// </summary>
    /// <param name="view"></param>
    private static void ShowAndWait(IViewBase view)
    {
        if (view is not Window win)
        {
            return;
        }

        var frame = new DispatcherFrame();
        win.Closed += (_, _) => frame.Continue = false;
        win.Show();
        Dispatcher.UIThread.PushFrame(frame); // blocks here, but the UI keeps running
    }

    public PoeState GetPoeWindowState()
    {
        if (OperatingSystem.IsWindows())
        {
            return GetWinPoeState();
        }
        if (OperatingSystem.IsLinux())
        {
            return GetLinuxPoeState();
        }
        return PoeState.NotLaunched;
    }

    public void CloseOpenedView(ConfigShortcut shortcut)
    {
        if (OperatingSystem.IsWindows())
        {
            CloseWinOpenedView(shortcut);
        }
        if (OperatingSystem.IsLinux())
        {
            CloseLinuxOpenedView();
        }
    }

    [SupportedOSPlatform("linux")]
    private PoeState GetLinuxPoeState() => _sp.GetRequiredService<LinuxDesktopService>().GetPoeState();

    /// <summary>
    /// Closes the first opened window of Xiletrade (config, editor, whisper, popup, regex), or the main view.
    /// </summary>
    /// <remarks>
    /// The windows are found through Avalonia and not through their title on the screen : it also works without X11.
    /// <br/>There is no key relay to the game like on Windows : the key would come back to the grabbed shortcut.
    /// </remarks>
    [SupportedOSPlatform("linux")]
    private void CloseLinuxOpenedView()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            foreach (var win in desktop.Windows)
            {
                if (win.Name is not null && Array.IndexOf(Strings.WindowName.XiletradeWindowList, win.Name) >= 0)
                {
                    win.Close();
                    return;
                }
            }
        }
        if (IsVisibleMainView())
        {
            CloseMainView();
        }
    }

    [SupportedOSPlatform("windows")]
    private static PoeState GetWinPoeState()
    {
        nint findPoeHwnd = Native.FindWindow(Strings.PoeClass, Strings.PoeCaption);
        bool poeLaunched = findPoeHwnd.ToInt32() > 0;
        bool poeFocused = Native.GetForegroundWindow().Equals(findPoeHwnd);

        return poeFocused ? PoeState.LaunchedAndFocused :
            poeLaunched ? PoeState.Launched : PoeState.NotLaunched;
    }

    [SupportedOSPlatform("windows")]
    private void CloseWinOpenedView(ConfigShortcut shortcut)
    {
        foreach (var win in Strings.WindowName.XiletradeWindowList)
        {
            var findHwnd = Native.FindWindow(null, win);
            if (findHwnd != nint.Zero)
            {
                Native.SendMessage(findHwnd, Native.WM_CLOSE, nint.Zero, nint.Zero);
                return;
            }
        }
        if (IsVisibleMainView())
        {
            CloseMainView();
            return;
        }

        // In case the shortcut is not correctly unregistered.
        // Acts as a relay: passes the intercepted event back to the game,
        // ensuring the keypress retains its effect in PoE when the app is not involved.
        nint findPoeHwnd = Native.FindWindow(Strings.PoeClass, Strings.PoeCaption);
        if (findPoeHwnd != nint.Zero)
        {
            Native.SendMessage(findPoeHwnd, Native.WM_KEYUP, new nint(shortcut.Keycode), nint.Zero);
        }
    }
}
