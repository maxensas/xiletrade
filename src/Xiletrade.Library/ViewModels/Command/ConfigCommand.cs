using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using Xiletrade.Library.Models.Enums;
using Xiletrade.Library.Models.Serializable;
using Xiletrade.Library.Services;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Interop;
using Xiletrade.Library.ViewModels.Config;

namespace Xiletrade.Library.ViewModels.Command;

public sealed partial class ConfigCommand : ViewModelBase
{
    private static ConfigViewModel Vm { get; set; }
    private static IServiceProvider _serviceProvider;

    public ConfigCommand(ConfigViewModel vm, IServiceProvider serviceProvider)
    {
        Vm = vm;
        _serviceProvider = serviceProvider;
    }

    [RelayCommand]
    private static void SaveConfig(object commandParameter)
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
        CloseConfig(commandParameter);
    }

    [RelayCommand]
    private static void LoadDefaultConfig(object commandParameter)
    {
        string configDefault = DataManager.Load_Config(Strings.File.DefaultConfig); // parentWindow
        Vm.Config = Json.Deserialize<ConfigData>(configDefault);
        Vm.Initialize();
    }

    [RelayCommand]
    private static void CloseConfig(object commandParameter)
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

    [RelayCommand]
    private static void UpdateFilters(object commandParameter)
    {
        bool allLang = commandParameter is string cmd && cmd is "all";
        DataFilters.Update(cfgVm: Vm, allLanguages: allLang);
    }

    [RelayCommand]
    private static void OpenEditor(object commandParameter)
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
    private static void UpdateGameVersion(object commandParameter)
    {
        //TODO
    }
}
