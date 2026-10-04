using System;
using System.Threading.Tasks;
using Xiletrade.Library.Models.GitHub.Contract;
using Xiletrade.Library.Models.Poe.Contract;

namespace Xiletrade.Library.Services.Interface;

/// <summary>
/// Handles application view creation, display, and global UI operations.
/// </summary>
/// <remarks>
/// Implementations may target different UI frameworks.
/// </remarks>
public interface IViewManager
{
    nint MainHandle { get; }

    void ShowMainView();
    bool IsVisibleMainView();
    void CloseMainView();
    void ShowConfigView();
    void ShowEditorView();
    void ShowRegexView();
    void ShowPopupView(string imgName);
    Task ShowStartView();
    void ShowUpdateView(GitHubRelease release);
    void ShowWhisperView(Tuple<FetchDataListing, OfferInfo> data);
    void ClearFocus();
    void SetMainHandle(object view);
    void ShutDownNativeApp(int exitCode = 0);
}