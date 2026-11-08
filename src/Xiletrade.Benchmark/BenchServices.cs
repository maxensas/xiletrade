using System;
using System.Threading.Tasks;
using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Models.GitHub.Contract;
using Xiletrade.Library.Models.Poe.Contract;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Benchmark;

/// <summary>
/// Message service without UI : a message is only shown on errors (data loading), the benchmark stops with it.
/// </summary>
internal sealed class BenchMessageAdapterService : IMessageAdapterService
{
    public void Show(string message, string caption, MessageStatus status)
        => throw new InvalidOperationException($"{caption} : {message}");

    public bool ShowResult(string message, string caption, MessageStatus status, bool yesNo = false)
        => throw new InvalidOperationException($"{caption} : {message}");

    public Task<bool> ShowResultAsync(string message, string caption, MessageStatus status, bool yesNo = false)
        => throw new InvalidOperationException($"{caption} : {message}");
}

/// <summary>
/// View manager without UI : nothing to display in a benchmark.
/// </summary>
internal sealed class BenchViewManager : IViewManager
{
    public nint MainHandle => 0;

    public void ShowMainView() { }
    public bool IsVisibleMainView() => false;
    public void CloseMainView() { }
    public void ShowConfigView() { }
    public void ShowEditorView() { }
    public void ShowRegexView() { }
    public void ShowPopupView(string imgName) { }
    public Task ShowStartView() => Task.CompletedTask;
    public void ShowUpdateView(GitHubRelease release) { }
    public void ShowWhisperView(Tuple<FetchDataListing, OfferInfo> data) { }
    public void CloseOpenedView(ConfigShortcut shortcut) { }
    public PoeState GetPoeWindowState() => default;
    public void ClearFocus() { }
    public Task<string> ShowOpenFileViewAsync(string title, string filterName, string extension) => Task.FromResult<string>(null);
    public Task<string> ShowSaveFileViewAsync(string title, string filterName, string extension, string fileName) => Task.FromResult<string>(null);
    public void SetMainHandle(object view) { }
    public void ShutDownNativeApp(int exitCode = 0)
        => throw new InvalidOperationException($"Application shutdown requested (exit code {exitCode})");
}
