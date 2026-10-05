using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Xiletrade.Library.Models.GitHub.Contract;
using Xiletrade.Library.Models.Poe.Contract;
using Xiletrade.Library.Services.Extension;
using Xiletrade.Library.Services.Interface;
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

    private IViewBase MainView => _sp.GetRequiredService<IMainView>();
    private IViewBase ConfigView => _sp.GetRequiredService<IConfigView>();
    private IViewBase StartView => _sp.GetRequiredService<IStartView>();
    private IViewBase RegexView => _sp.GetRequiredService<IRegexView>();
    private IViewBase EditorView => _sp.GetRequiredService<IEditorView>();

    public nint MainHandle { get; private set; }

    public ViewManager(IServiceProvider sp, ILogger<ViewManager> logger)
    {
        _sp = sp;

#if DEBUG
        logger.LogInformation("Service launched");
#endif
    }

    public void CloseMainView() => MainView.Close();

    public bool IsVisibleMainView() => MainView.IsVisible;

    public void SetMainHandle(object view)
    {
        if (view is Window win)
        {
            MainHandle = win.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
        }
    }

    public void ShowConfigView() => ConfigView.Show();

    public void ShowEditorView() => EditorView.Show();

    public void ShowMainView() => MainView.Show();

    public void ShowRegexView() => RegexView.Show();

    public void ShowPopupView(string imgName)
    {
        _ = new PopView(imgName); // viewmodel not used.
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
}
