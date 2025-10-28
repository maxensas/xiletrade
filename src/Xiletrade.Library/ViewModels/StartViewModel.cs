using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Globalization;
using Xiletrade.Library.Models.Application.Configuration.Domain;
using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Services;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Collection;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.ViewModels;

public sealed partial class StartViewModel : ViewModelBase
{
    private static IServiceProvider _serviceProvider;

    private readonly DataManagerService _dm;

    //property
    [ObservableProperty]
    private AsyncObservableCollection<Language> language = new();

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
        _dm = _serviceProvider.GetRequiredService<DataManagerService>();
        ViewScale = _dm.Config.Options.Scale;
        ConfigBackup = _dm.LoadConfiguration(Strings.File.Config);
        Config = _dm.Json.Deserialize<ConfigData>(ConfigBackup);

        Language = new()
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
        LanguageIndex = Config.Options.Language;
        GameIndex = Config.Options.GameVersion;

        // Init with InstalledUICulture on purpose.
        System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InstalledUICulture;
        System.Threading.Thread.CurrentThread.CurrentUICulture = CultureInfo.InstalledUICulture;
        TranslationViewModel.Instance.CurrentCulture = CultureInfo.InstalledUICulture;
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

        _serviceProvider.GetRequiredService<DataManagerService>().RefreshCurrentCulture(LanguageIndex);
        
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
        var configToSave = _dm.Json.Serialize<ConfigData>(Config);
        var dm = _serviceProvider.GetRequiredService<DataManagerService>();
        dm.SaveConfiguration(configToSave);
        dm.TryInit();
    }
}
