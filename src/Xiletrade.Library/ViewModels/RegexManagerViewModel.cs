using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
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

    [ObservableProperty]
    private AsyncObservableCollection<RegexViewModel> regexList = new();

    // members
    public ConfigData Config { get; set; }
    public string ConfigBackup { get; set; }

    public RegexManagerViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;

        ConfigBackup = DataManager.Load_Config(Strings.File.Config);
        Config = Json.Deserialize<ConfigData>(ConfigBackup);

        // load regex list.
    }

    [RelayCommand]
    private static void AddRegex(object commandParameter)
    {

    }

    [RelayCommand]
    private static void CloseWindow(object commandParameter)
    {
        if (commandParameter is IViewBase view)
        {
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
