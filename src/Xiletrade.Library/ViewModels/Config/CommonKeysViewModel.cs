using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;

namespace Xiletrade.Library.ViewModels.Config;

public sealed partial class CommonKeysViewModel(IServiceProvider sp) : ViewModelBase
{
    [ObservableProperty]
    private HotkeyViewModel priceCheck = new(sp, Resources.Resources.Config077_lbPrice, Resources.Resources.Config092_lbPriceTip);

    [ObservableProperty]
    private HotkeyViewModel openBulk = new(sp, Resources.Resources.Config078_lbBulkEx, Resources.Resources.Config093_lbBulkExTip);

    [ObservableProperty]
    private HotkeyViewModel openConfig = new(sp, Resources.Resources.Config079_lbSettingsWin, Resources.Resources.Config094_lbSettingsWinTip);

    [ObservableProperty]
    private HotkeyViewModel closeWindow = new(sp, Resources.Resources.Config080_lbCloseWin, Resources.Resources.Config095_lbCloseWinTip);

    [ObservableProperty]
    private HotkeyViewModel openSyndicate = new(sp, Resources.Resources.Config081_lbSyndicate, Resources.Resources.Config096_lbSyndicateTip);

    [ObservableProperty]
    private HotkeyViewModel openIncursion = new(sp, Resources.Resources.Config082_lbIncursion, Resources.Resources.Config097_lbIncursionTip);

    [ObservableProperty]
    private HotkeyViewModel tcpLogout = new(sp, Resources.Resources.Config084_lbTcp, Resources.Resources.Config098_lbTcpTip);

    [ObservableProperty]
    private HotkeyViewModel openWiki = new(sp, Resources.Resources.Config086_lbWiki, Resources.Resources.Config099_lbWikiTip);

    [ObservableProperty]
    private HotkeyViewModel openNinja = new(sp, Resources.Resources.Config087_lbNinja, Resources.Resources.Config100_lbNinjaTip);

    [ObservableProperty]
    private HotkeyViewModel openPoeLab = new(sp, Resources.Resources.Config088_lbLab, Resources.Resources.Config101_lbLabTip);

    [ObservableProperty]
    private HotkeyViewModel openPoeDb = new(sp, Resources.Resources.Config089_lbData, Resources.Resources.Config102_lbDataTip);

    [ObservableProperty]
    private HotkeyViewModel openCoe = new(sp, Resources.Resources.Config172_coe, Resources.Resources.Config173_coeTip);

    [ObservableProperty]
    private HotkeyViewModel openCustomFirst = new(sp, Resources.Resources.Config090_lbCustom1, Resources.Resources.Config103_lbCustom1Tip);

    [ObservableProperty]
    private HotkeyViewModel openCustomSecond = new(sp, Resources.Resources.Config091_lbCustom2, Resources.Resources.Config104_lbCustom2Tip);

    [ObservableProperty]
    private HotkeyViewModel openRegexManager = new(sp, Resources.Resources.Config157_lbRegex, Resources.Resources.Config158_lbRegexTip);

    internal IEnumerable<HotkeyViewModel> GetListHotkey()
    {
        return [PriceCheck, OpenBulk, OpenConfig, CloseWindow, OpenSyndicate
            , OpenIncursion, TcpLogout, OpenWiki, OpenNinja, OpenPoeLab, OpenPoeDb, OpenCoe
            , OpenCustomFirst, OpenCustomSecond, OpenRegexManager];
    }
}
