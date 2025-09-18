using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Services;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Enum;
using Xiletrade.Library.Shared.Interop;
using Xiletrade.Library.ViewModels.Config;

namespace Xiletrade.Library.ViewModels.Command;

public sealed partial class ConfigCommand : ViewModelBase
{
    private static IServiceProvider _serviceProvider;
    private readonly ConfigViewModel _vm;
    private readonly DataManagerService _dm;

    public ConfigCommand(ConfigViewModel vm, IServiceProvider serviceProvider)
    {
        _vm = vm;
        _serviceProvider = serviceProvider;
        _dm = _serviceProvider.GetRequiredService<DataManagerService>();
    }

    [RelayCommand]
    private void SaveConfig(object commandParameter)
    {
        int idxLangOld = _vm.Config.Options.Language;
        int timeoutOld = _vm.Config.Options.TimeoutTradeApi;

        _vm.SaveConfigForm();
        if (_vm.General.LanguageIndex != idxLangOld)
        {
            _dm.TryInit();
        }
        if (timeoutOld != _vm.Config.Options.TimeoutTradeApi)
        {
            var service = _serviceProvider.GetRequiredService<NetService>();
            service.InitTradeClient(_vm.Config.Options.TimeoutTradeApi);
        }
        CloseConfig(commandParameter);
    }

    [RelayCommand]
    private void LoadDefaultConfig(object commandParameter)
    {
        string configDefault = _dm.LoadConfiguration(Strings.File.DefaultConfig);
        _vm.Config = Json.Deserialize<ConfigData>(configDefault);
        _vm.Initialize(false);

        var fullList = _vm.CommonKeys.GetListHotkey()
            .Concat(_vm.AdditionalKeys.GetListHotkey());
        foreach (var hk in fullList)
        {
            hk.IsInConflict = false;
        }
        _vm.CanSave = true;
    }

    [RelayCommand]
    private void CloseConfig(object commandParameter)
    {
        if (commandParameter is IViewBase view)
        {
            RefreshLanguageUi();
            view.Close();
            return;
        }
    }

    [RelayCommand]
    private void CenterView(object commandParameter)
    {
        /*
        if (commandParameter is IViewBase view)
        {
            view.Center(_vm.ViewScale);
            return;
        }
        */
    }

    [RelayCommand]
    private void UpdateFilters(object commandParameter)
    {
        bool allLang = commandParameter is string cmd && cmd is "all";
        _serviceProvider.GetRequiredService<DataUpdaterService>()
            .Update(cfgVm: _vm, allLanguages: allLang);
    }

    [RelayCommand]
    private void OpenEditor(object commandParameter)
    {
        IntPtr pHwnd = Native.FindWindow(null, Strings.WindowName.Editor);
        if (pHwnd.ToInt32() > 0)
        {
            Native.SendMessage(pHwnd, Native.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
        }
        _serviceProvider.GetRequiredService<INavigationService>().ShowEditorView();
        CloseConfig(null);
    }

    [RelayCommand]
    private static void OpenChatCommandsList(object commandParameter)
    {
        string url = Strings.UrlPoeWiki + "Chat_console#Commands";
        try
        {
            Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
        }
        catch (Exception)
        {
            var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            service.Show("Failed to redirect to Poe Wiki website.", "Error", MessageStatus.Warning);
        }
    }

    [RelayCommand]
    private static void OpenGitHubIssue(object commandParameter)
    {
        string url = "https://github.com/maxensas/xiletrade/issues";
        try
        {
            Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
        }
        catch (Exception)
        {
            var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            service.Show("Failed to redirect to Github website.", "Error", MessageStatus.Warning);
        }
    }

    [RelayCommand]
    private static void OpenPaypal(object commandParameter)
    {
        //string url = "https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=9TEA8EMSSB846";
        string url = "https://www.paypal.me/maxensas";
        try
        {
            Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
        }
        catch (Exception)
        {
            var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            service.Show("Failed to redirect to Paypal website.", "Error", MessageStatus.Warning);
        }
    }

    [RelayCommand]
    private static void OpenDiscord(object commandParameter)
    {
        string url = "https://discord.gg/AXP5VntYgA";
        try
        {
            Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
        }
        catch (Exception)
        {
            var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            service.Show("Failed to redirect to Discord.gg website.", "Error", MessageStatus.Warning);
        }
    }

    [RelayCommand]
    private static void OpenLiberapay(object commandParameter)
    {
        string url = "https://liberapay.com/Xiletrade/donate";
        try
        {
            Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
        }
        catch (Exception)
        {
            var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            service.Show("Failed to redirect to Liberapay website.", "Error", MessageStatus.Warning);
        }
    }

    [RelayCommand]
    private static void CheckHotkey(object commandParameter)
    {
        if (commandParameter is CompositeCommandParameter composite
            && composite.Parameter is HotkeyViewModel vm)
        {
            var keyPressed = _serviceProvider.GetRequiredService<INavigationService>().GetKeyPressed(composite.EventArgs);
            if (keyPressed.Length > 0)
            {
                vm.Hotkey = keyPressed;
            }
        }
    }

    [RelayCommand]
    private void UpdateGameVersion(object commandParameter)
    {
        _vm.SaveConfigForm();
        _dm.TryInit();
        _vm.InitLeagueList();
    }

    [RelayCommand]
    private void UpdateLanguage(object commandParameter)
    {
        RefreshLanguageUi(false);
        _vm.General.GatewayIndex = _vm.General.LanguageIndex;
        _vm.InitShortcuts();
    }

    [RelayCommand]
    private void UpdateGateway(object commandParameter)
    {
        _vm.InitLeagueList();
    }

    private void RefreshLanguageUi(bool reset = true)
    {
        CultureInfo cultureRefresh = CultureInfo.CreateSpecificCulture(Strings.Culture[reset ? _dm.Config.Options.Language : _vm.General.LanguageIndex]);
        Thread.CurrentThread.CurrentUICulture = cultureRefresh;
        TranslationViewModel.Instance.CurrentCulture = cultureRefresh;
    }
}
