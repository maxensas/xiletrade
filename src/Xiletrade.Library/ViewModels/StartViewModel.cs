using System.Windows.Input;
using Xiletrade.Library.Models.Collections;
using Xiletrade.Library.Models.Serializable;
using Xiletrade.Library.Services;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.ViewModels.Command;

namespace Xiletrade.Library.ViewModels;

public sealed class StartViewModel : BaseViewModel
{
    //property
    private AsyncObservableCollection<string> language = new();
    private int languageIndex;
    public AsyncObservableCollection<string> Language { get => language; set => SetProperty(ref language, value); }
    public int LanguageIndex { get => languageIndex; set => SetProperty(ref languageIndex, value); }

    //command
    private readonly DelegateCommand closeStart;
    public ICommand CloseStart => closeStart;

    //member
    private ConfigData Config { get; set; }
    private string ConfigBackup { get; set; }

    public StartViewModel()
    {
        closeStart = new(OnCloseStart, CanCloseStart);

        ConfigBackup = DataManager.Load_Config(Strings.File.Config);
        Config = Json.Deserialize<ConfigData>(ConfigBackup);

        Language = new()
        {
            "English",
            "한국어",
            "Français",
            "Castellano",
            "Deutsch",
            "Português",
            "Русский",
            "ภาษาไทย",
            "正體中文",
            "简体中文",
            "日本語"
        };
        LanguageIndex = Config.Options.Language;

        System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.InstalledUICulture;
        TranslationViewModel.Instance.CurrentCulture = System.Globalization.CultureInfo.InstalledUICulture;
    }

    private bool CanCloseStart(object commandParameter)
    {
        return true;
    }

    private void OnCloseStart(object commandParameter)
    {
        if (commandParameter is IViewBase view)
        {
            Config.Options.Language = LanguageIndex;

            System.Globalization.CultureInfo cultureRefresh = System.Globalization.CultureInfo.CreateSpecificCulture(Strings.Culture[Config.Options.Language]);
            System.Threading.Thread.CurrentThread.CurrentUICulture = cultureRefresh;
            TranslationViewModel.Instance.CurrentCulture = cultureRefresh;

            string configToSave = Json.Serialize<ConfigData>(Config);

            DataManager.Save_Config(configToSave, "cfg");
            DataManager.InitConfig();

            view.Close();
        }
    }
}
