using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xiletrade.Library.Models;
using Xiletrade.Library.Models.Collections;
using Xiletrade.Library.Models.Enums;
using Xiletrade.Library.Models.Serializable;
using Xiletrade.Library.Services;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.ViewModels;

public sealed partial class StartViewModel : ViewModelBase
{
    private static IServiceProvider _serviceProvider;
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
        var dm = _serviceProvider.GetRequiredService<DataManagerService>();
        ViewScale = dm.Config.Options.Scale;
        ConfigBackup = dm.Load_Config(Strings.File.Config);
        Config = Json.Deserialize<ConfigData>(ConfigBackup);

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

        System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.InstalledUICulture;
        TranslationViewModel.Instance.CurrentCulture = System.Globalization.CultureInfo.InstalledUICulture;
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
        Config.Options.Language = LanguageIndex;

        System.Globalization.CultureInfo cultureRefresh = System.Globalization.CultureInfo.CreateSpecificCulture(Strings.Culture[Config.Options.Language]);
        System.Threading.Thread.CurrentThread.CurrentUICulture = cultureRefresh;
        TranslationViewModel.Instance.CurrentCulture = cultureRefresh;

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
        string configToSave = Json.Serialize<ConfigData>(Config);
        var dm = _serviceProvider.GetRequiredService<DataManagerService>();
        dm.Save_Config(configToSave, "cfg");
        dm.TryInit();
    }
}
