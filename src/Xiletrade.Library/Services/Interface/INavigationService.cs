using System;
using System.Threading.Tasks;
using Xiletrade.Library.Models.GitHub.Contract;
using Xiletrade.Library.Models.Poe.Contract;

namespace Xiletrade.Library.Services.Interface;

public interface INavigationService
{
    void InstantiateMainView();
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
    void DelegateActionToUiThread(Action action);
    TResult DelegateFuncToUiThread<TResult>(Func<TResult> func);
    Task<TResult> DelegateActionToUiThreadAsync<TResult>(Func<Task<TResult>> asyncFunc);
    void ShutDownXiletrade(int code = 0);

    //move next to other service
    string GetKeyPressed(EventArgs e);
    int GetModifierCode(string textMod);
    string GetModifierText(int modifier);
    void ClearKeyboardFocus();    
}