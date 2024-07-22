using System;
using Xiletrade.Library.Models.Serializable;

namespace Xiletrade.Library.Services.Interface;

public interface INavigationService
{
    void InstantiateMainView();
    void ShowMainView();
    bool IsVisibleMainView();
    void CloseMainView();
    void ShowConfigView();
    void ShowEditorView();
    void ShowPopupView(string imgName);
    void ShowStartView();
    void ShowWhisperView(Tuple<FetchDataListing, OfferInfo> data);
    void SetMainHandle(object view);
    void DelegateActionToUiThread(Action action);
    void ShutDownXiletrade();

    //move next to other service
    void UpdateControlValue(object obj, double value = 0);
    string GetKeyPressed(EventArgs e);
    int GetModifierCode(string textMod);
    string GetModifierText(int modifier);
    void ClearKeyboardFocus();
}