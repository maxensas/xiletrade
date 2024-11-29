using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xiletrade.Library.Models.Collections;
using Xiletrade.Library.Models.Enums;
using Xiletrade.Library.Models.Serializable;
using Xiletrade.Library.Services;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.ViewModels;

public sealed partial class RegexManagerViewModel : ViewModelBase
{
    private static IServiceProvider _serviceProvider;
    private const int MAX_REGEX = 20;

    [ObservableProperty]
    private AsyncObservableCollection<RegexViewModel> regexList = new();

    // members
    private ConfigData Config { get; set; }

    public RegexManagerViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;

        var cfg = DataManager.Load_Config(Strings.File.Config);
        Config = Json.Deserialize<ConfigData>(cfg);

        foreach (var regex in Config.RegularExpressions)
        {
            RegexViewModel vm = new() { Id = regex.Id, Name = regex.Name, Regex = regex.Regex };
            RegexList.Add(vm);
        }
    }

    [RelayCommand]
    private void AddRegex(object commandParameter)
    {
        if (RegexList.Count <= MAX_REGEX)
        {
            RegexViewModel vm = new() { Id = RegexList.Count - 1, Name = string.Empty, Regex = string.Empty };
            RegexList.Add(vm);
        }
    }

    [RelayCommand]
    private void CloseWindow(object commandParameter)
    {
        if (commandParameter is IViewBase view)
        {
            List<ConfigRegex> listCfgReg = new();
            foreach (var reg in RegexList)
            {
                ConfigRegex cfgReg = new() { Id = reg.Id, Name = reg.Name, Regex = reg.Regex };
                listCfgReg.Add(cfgReg);
            }
            Config.RegularExpressions = [..listCfgReg];
            string configToSave = Json.Serialize<ConfigData>(Config);
            DataManager.Save_Config(configToSave, "cfg");

            view.Close();
        }
    }

    [RelayCommand]
    private static void OpenPoeRegex(object commandParameter)
    {
        string url = Strings.UrlPoeRegex;
        try
        {
            Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
        }
        catch (Exception)
        {
            var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            service.Show("Failed to redirect to Poe Regex website.", "Error", MessageStatus.Warning);
        }
    }
}
