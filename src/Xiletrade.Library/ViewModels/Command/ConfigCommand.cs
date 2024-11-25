using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Windows.Input;
using Xiletrade.Library.Models.Enums;
using Xiletrade.Library.Models.Serializable;
using Xiletrade.Library.Services;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Interop;

namespace Xiletrade.Library.ViewModels.Command;

public sealed class ConfigCommand
{
    private static ConfigViewModel Vm { get; set; }
    private static IServiceProvider _serviceProvider;

    private readonly DelegateCommand saveConfig;
    private readonly DelegateCommand loadDefaultConfig;
    private readonly DelegateCommand closeConfig;
    private readonly DelegateCommand updateFilters;
    private readonly DelegateCommand openEditor;
    private readonly DelegateCommand openChatCommandsList;
    private readonly DelegateCommand openGitHubIssue;
    private readonly DelegateCommand openPaypal;
    private readonly DelegateCommand openLiberapay;
    private readonly DelegateCommand checkHotkey;
    private readonly DelegateCommand openDiscord;

    public ICommand SaveConfig => saveConfig;
    public ICommand LoadDefaultConfig => loadDefaultConfig;
    public ICommand CloseConfig => closeConfig;
    public ICommand UpdateFilters => updateFilters;
    public ICommand OpenEditor => openEditor;
    public ICommand OpenChatCommandsList => openChatCommandsList;
    public ICommand OpenGitHubIssue => openGitHubIssue;
    public ICommand OpenPaypal => openPaypal;
    public ICommand OpenLiberapay => openLiberapay;
    public ICommand CheckHotkey => checkHotkey;
    public ICommand OpenDiscord => openDiscord;

    public ConfigCommand(ConfigViewModel vm, IServiceProvider serviceProvider)
    {
        Vm = vm;
        _serviceProvider = serviceProvider;
        saveConfig = new(OnSaveConfig, CanSaveConfig);
        loadDefaultConfig = new(OnLoadDefaultConfig, CanLoadDefaultConfig);
        closeConfig = new(OnCloseConfig, CanCloseConfig);
        updateFilters = new(OnUpdateFilters, CanUpdateFilters);
        openEditor = new(OnOpenEditor, CanOpenEditor);
        openChatCommandsList = new(OnOpenChatCommandsList, CanOpenChatCommandsList);
        openGitHubIssue = new(OnOpenGitHubIssue, CanOpenGitHubIssue);
        openPaypal = new(OnOpenPaypal, CanOpenPaypal);
        openLiberapay = new(OnOpenLiberapay, CanOpenLiberapay);
        checkHotkey = new(OnCheckHotkey, CanCheckHotkey);
        openDiscord = new(OnOpenDiscord, CanOpenDiscord);
    }

    private static bool CanSaveConfig(object commandParameter)
    {
        return true;
    }

    private static void OnSaveConfig(object commandParameter)
    {
        int idxLangOld = Vm.Config.Options.Language;
        int timeoutOld = Vm.Config.Options.TimeoutTradeApi;

        Vm.SaveConfigForm();
        if (Vm.General.LanguageIndex != idxLangOld)
        {
            DataManager.TryInit();
        }
        if (timeoutOld != Vm.Config.Options.TimeoutTradeApi)
        {
            var service = _serviceProvider.GetRequiredService<NetService>();
            service.InitTradeClient(Vm.Config.Options.TimeoutTradeApi);
        }
        if (CanCloseConfig(commandParameter)) OnCloseConfig(commandParameter);
    }

    private static bool CanLoadDefaultConfig(object commandParameter)
    {
        return true;
    }

    private static void OnLoadDefaultConfig(object commandParameter)
    {
        string configDefault = DataManager.Load_Config(Strings.File.DefaultConfig); // parentWindow
        Vm.Config = Json.Deserialize<ConfigData>(configDefault);
        Vm.Initialize();
    }

    private static bool CanCloseConfig(object commandParameter)
    {
        return true;
    }

    private static void OnCloseConfig(object commandParameter)
    {
        if (commandParameter is IViewBase view)
        {
            view.Close();
            return;
        }
        /*
        IntPtr pHwnd = Native.FindWindow(null, Strings.WindowTitle.Config);
        if (pHwnd.ToInt32() > 0)
        {
            Native.SendMessage(pHwnd, Native.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
        }*/
    }

    private static bool CanUpdateFilters(object commandParameter)
    {
        return true;
    }

    private static void OnUpdateFilters(object commandParameter)
    {
        bool allLang = commandParameter is string cmd && cmd is "all";
        DataFilters.Update(cfgVm: Vm, allLanguages: allLang);
    }

    private static bool CanOpenEditor(object commandParameter)
    {
        return true;
    }

    private static void OnOpenEditor(object commandParameter)
    {
        IntPtr pHwnd = Native.FindWindow(null, Strings.WindowName.Editor);
        if (pHwnd.ToInt32() > 0)
        {
            Native.SendMessage(pHwnd, Native.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
        }
        _serviceProvider.GetRequiredService<INavigationService>().ShowEditorView();
        if (CanCloseConfig(null)) OnCloseConfig(null);
    }

    private static bool CanOpenChatCommandsList(object commandParameter)
    {
        return true;
    }

    private static void OnOpenChatCommandsList(object commandParameter)
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

    private static bool CanOpenGitHubIssue(object commandParameter)
    {
        return true;
    }

    private static void OnOpenGitHubIssue(object commandParameter)
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

    private static bool CanOpenPaypal(object commandParameter)
    {
        return true;
    }

    private static void OnOpenPaypal(object commandParameter)
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

    private static bool CanOpenDiscord(object commandParameter)
    {
        return true;
    }

    private static void OnOpenDiscord(object commandParameter)
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

    private static bool CanOpenLiberapay(object commandParameter)
    {
        return true;
    }

    private static void OnOpenLiberapay(object commandParameter)
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

    private static bool CanCheckHotkey(object commandParameter)
    {
        return true;
    }

    private static void OnCheckHotkey(object commandParameter)
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
}
