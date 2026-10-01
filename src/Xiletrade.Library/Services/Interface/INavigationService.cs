using System;
using System.Threading.Tasks;
using Xiletrade.Library.Models.GitHub.Contract;
using Xiletrade.Library.Models.Poe.Contract;

namespace Xiletrade.Library.Services.Interface;

/// <summary>
/// Provides window management services for the application, including showing or closing views.
/// </summary>
public interface INavigationService
{
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
    void SetMainHandle(object view);
    void ShutDownNativeApp(int exitCode = 0);
}