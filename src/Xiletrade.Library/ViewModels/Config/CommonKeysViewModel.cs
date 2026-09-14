using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using Xiletrade.Library.Services.Interface;

namespace Xiletrade.Library.ViewModels.Config;

public sealed partial class CommonKeysViewModel : ViewModelBase
{
    [ObservableProperty]
    private HotkeyViewModel priceCheck;

    [ObservableProperty]
    private HotkeyViewModel openBulk;

    [ObservableProperty]
    private HotkeyViewModel openConfig;

    [ObservableProperty]
    private HotkeyViewModel closeWindow;

    [ObservableProperty]
    private HotkeyViewModel openSyndicate;

    [ObservableProperty]
    private HotkeyViewModel openIncursion;

    [ObservableProperty]
    private HotkeyViewModel tcpLogout;

    [ObservableProperty]
    private HotkeyViewModel openWiki;

    [ObservableProperty]
    private HotkeyViewModel openNinja;

    [ObservableProperty]
    private HotkeyViewModel openPoeLab;

    [ObservableProperty]
    private HotkeyViewModel openPoeDb;

    [ObservableProperty]
    private HotkeyViewModel openCoe;

    [ObservableProperty]
    private HotkeyViewModel openCustomFirst;

    [ObservableProperty]
    private HotkeyViewModel openCustomSecond;

    [ObservableProperty]
    private HotkeyViewModel openRegexManager;

    public CommonKeysViewModel(INavigationService nav, IMessageAdapterService message, ConfigViewModel vm)
    {
        priceCheck = new(nav, message, vm, Resources.Resources.Config077_lbPrice, Resources.Resources.Config092_lbPriceTip);
        openBulk = new(nav, message, vm, Resources.Resources.Config078_lbBulkEx, Resources.Resources.Config093_lbBulkExTip);
        openConfig = new(nav, message, vm, Resources.Resources.Config079_lbSettingsWin, Resources.Resources.Config094_lbSettingsWinTip);
        closeWindow = new(nav, message, vm, Resources.Resources.Config080_lbCloseWin, Resources.Resources.Config095_lbCloseWinTip);
        openSyndicate = new(nav, message, vm, Resources.Resources.Config081_lbSyndicate, Resources.Resources.Config096_lbSyndicateTip);
        openIncursion = new(nav, message, vm, Resources.Resources.Config082_lbIncursion, Resources.Resources.Config097_lbIncursionTip);
        tcpLogout = new(nav, message, vm, Resources.Resources.Config084_lbTcp, Resources.Resources.Config098_lbTcpTip);
        openWiki = new(nav, message, vm, Resources.Resources.Config086_lbWiki, Resources.Resources.Config099_lbWikiTip);
        openNinja = new(nav, message, vm, Resources.Resources.Config087_lbNinja, Resources.Resources.Config100_lbNinjaTip);
        openPoeLab = new(nav, message, vm, Resources.Resources.Config088_lbLab, Resources.Resources.Config101_lbLabTip);
        openPoeDb = new(nav, message, vm, Resources.Resources.Config089_lbData, Resources.Resources.Config102_lbDataTip);
        openCoe = new(nav, message, vm, Resources.Resources.Config172_coe, Resources.Resources.Config173_coeTip);
        openCustomFirst = new(nav, message, vm, Resources.Resources.Config090_lbCustom1, Resources.Resources.Config103_lbCustom1Tip);
        openCustomSecond = new(nav, message, vm, Resources.Resources.Config091_lbCustom2, Resources.Resources.Config104_lbCustom2Tip);
        openRegexManager = new(nav, message, vm, Resources.Resources.Config157_lbRegex, Resources.Resources.Config158_lbRegexTip);
    }

    internal IEnumerable<HotkeyViewModel> GetListHotkey()
    {
        return [PriceCheck, OpenBulk, OpenConfig, CloseWindow, OpenSyndicate
            , OpenIncursion, TcpLogout, OpenWiki, OpenNinja, OpenPoeLab, OpenPoeDb, OpenCoe
            , OpenCustomFirst, OpenCustomSecond, OpenRegexManager];
    }
}
