using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Xiletrade.Library.Models;
using Xiletrade.Library.Models.Collections;
using Xiletrade.Library.Models.Serializable;
using Xiletrade.Library.Services;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.ViewModels;

public sealed partial class StartViewModel : ViewModelBase
{
    //property
    [ObservableProperty]
    private AsyncObservableCollection<Language> language = new();

    [ObservableProperty]
    private int languageIndex;

    [ObservableProperty]
    private int gameIndex;

    //member
    private ConfigData Config { get; set; }
    private string ConfigBackup { get; set; }

    public StartViewModel()
    {
        ConfigBackup = DataManager.Load_Config(Strings.File.Config);
        Config = Json.Deserialize<ConfigData>(ConfigBackup);

        Language = new()
        {
            new(0, "English"),
            new(1, "한국어"),
            new(2, "Français"),
            new(3, "Castellano"),
            new(4, "Deutsch"),
            new(5, "Português"),
            new(6, "Русский"),
            new(7, "ภาษาไทย"),
            new(8, "正體中文"),
            new(9, "简体中文"),
            new(10, "日本語")
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
        DataManager.Save_Config(configToSave, "cfg");
        DataManager.InitConfig();
    }
}
