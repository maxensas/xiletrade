using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xiletrade.Library.Models.Application.Configuration.Domain;
using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Services;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Collection;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.ViewModels.Start;

public sealed partial class StartViewModel : ViewModelBase
{
    private static IServiceProvider _serviceProvider;

    //property
    [ObservableProperty]
    private AsyncObservableCollection<Language> language;

    [ObservableProperty]
    private int languageIndex;

    [ObservableProperty]
    private int gameIndex;

    [ObservableProperty]
    private double viewScale;

    //member
    private ConfigData Config { get; set; }
    private string ConfigBackup { get; set; }

    public StartViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        var dm = _serviceProvider.GetRequiredService<DataManagerService>();
        viewScale = dm.Config.Options.Scale;
        ConfigBackup = dm.LoadConfiguration(Strings.File.Config);
        Config = dm.Json.Deserialize<ConfigData>(ConfigBackup);

        language = new()
        {
            new(Lang.English, "English"),
            new(Lang.Korean, "한국어"),
            new(Lang.French, "Français"),
            new(Lang.Spanish, "Castellano"),
            new(Lang.German, "Deutsch"),
            new(Lang.Portuguese, "Português"),
            new(Lang.Russian, "Русский"),
            new(Lang.Thai, "ภาษาไทย"),
            new(Lang.Taiwanese, "正體中文"),
            new(Lang.Chinese, "简体中文"),
            new(Lang.Japanese, "日本語")
        };
        languageIndex = Config.Options.Language;
        gameIndex = Config.Options.GameVersion;

        _serviceProvider.GetRequiredService<LocalizationService>().RefreshCurrentCulture(init: true);
    }

    [RelayCommand]
    private void CloseStart(object commandParameter)
    {
        if (commandParameter is IViewBase view)
        {
            UpdateLanguage(null);
            view.Close();
        }
    }

    [RelayCommand]
    private void UpdateLanguage(object commandParameter)
    {
        if (commandParameter is bool updateGateway && updateGateway)
        {
            Config.Options.Gateway = LanguageIndex;
        }
        Config.Options.Language = LanguageIndex;

        _serviceProvider.GetRequiredService<LocalizationService>().RefreshCurrentCulture(LanguageIndex);
        UpdateConfig();
    }

    [RelayCommand]
    private void UpdateGameVersion(object commandParameter)
    {
        Config.Options.GameVersion = GameIndex;
        UpdateConfig();
    }

    private void UpdateConfig()
    {
        var dm = _serviceProvider.GetRequiredService<DataManagerService>();
        var configToSave = dm.Json.Serialize<ConfigData>(Config);
        dm.SaveConfiguration(configToSave);
        dm.TryInit();
    }
}
