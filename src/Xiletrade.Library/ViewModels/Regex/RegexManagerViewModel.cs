using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Services;
using Xiletrade.Library.Services.Extension;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Services.Interface.View;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Collection;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.ViewModels.Regex;

public sealed partial class RegexManagerViewModel : ViewModelBase
{
    private readonly IServiceProvider _sp;
    private readonly IMessageAdapterService _message;
    private readonly DataManagerService _dm;

    private RegexViewModel GetNewRegex => _sp.CreateInstance<RegexViewModel>();
    private const int MAX_REGEX = 20;

    [ObservableProperty]
    private AsyncObservableCollection<RegexViewModel> regexList = new();

    [ObservableProperty]
    private double viewScale;

    [ObservableProperty]
    private int idLang;

    // members
    private ConfigData Config { get; set; }

    public RegexManagerViewModel(IServiceProvider sp, DataManagerService dm, 
        IMessageAdapterService message)
    {
        _sp = sp;
        _dm = dm;
        _message = message;

        viewScale = _dm.Config.Options.Scale;
        idLang = _dm.Config.Options.Language;
        var cfg = _dm.LoadConfiguration(Strings.File.Config);
        Config = _dm.Json.Deserialize<ConfigData>(cfg);

        foreach (var regex in Config.RegularExpressions)
        {
            RegexViewModel vm = GetNewRegex;
            vm.Id = regex.Id;
            vm.Name = regex.Name;
            vm.Regex = regex.Regex;

            regexList.Add(vm);
        }
    }

    [RelayCommand]
    private void AddRegex(object commandParameter)
    {
        if (RegexList.Count <= MAX_REGEX)
        {
            RegexViewModel vm = GetNewRegex;
            vm.Id = RegexList.Count - 1;
            vm.Name = string.Empty;
            vm.Regex = string.Empty;
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
            var configToSave = _dm.Json.Serialize<ConfigData>(Config);
            _dm.SaveConfiguration(configToSave);

            view.Close();
        }
    }

    [RelayCommand]
    private void OpenPoeRegex(object commandParameter)
    {
        var url = IdLang is 1 ? Strings.UrlPoeRegexKr 
            : IdLang is 6 ? Strings.UrlPoeRegexRu
            : IdLang is 10 ? Strings.UrlPoeRegexJp
            : Strings.UrlPoeRegex;
        try
        {
            Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
        }
        catch (Exception ex)
        {
            _message.Show(ex.GetFormated(), "Failed to redirect to Poe Regex website", MessageStatus.Warning);
        }
    }
}
