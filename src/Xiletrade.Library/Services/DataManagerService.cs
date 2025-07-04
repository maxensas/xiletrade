using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Xiletrade.Library.Models.Collections;
using Xiletrade.Library.Models.Enums;
using Xiletrade.Library.Models.Serializable;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.ViewModels;

namespace Xiletrade.Library.Services;

/// <summary>Service (singleton) handling all serializable data in memory for Xiletrade.</summary>
/// <remarks></remarks>
public sealed class DataManagerService
{
    private static IServiceProvider _serviceProvider;

    internal ConfigData Config { get; private set; }
    internal FilterData Filter { get; private set; }
    internal FilterData FilterEn { get; private set; }
    internal ParserData Parser { get; private set; }
    internal LeagueData League { get; private set; }

    internal List<BaseResultData> Bases { get; private set; } = null;
    internal List<BaseResultData> Mods { get; private set; } = null;
    internal List<WordResultData> Words { get; private set; } = null;
    internal List<GemResultData> Gems { get; private set; }
    internal List<BaseResultData> Monsters { get; private set; } = null;
    internal List<CurrencyResultData> Currencies { get; private set; } = null;
    internal List<CurrencyResultData> CurrenciesEn { get; private set; } = null;
    internal List<DivTiersResult> DivTiers { get; private set; } = null;

    //temp
    internal List<WordResultData> WordsGateway { get; private set; } = null;
    internal List<BaseResultData> BasesGateway { get; private set; } = null;
    internal List<CurrencyResultData> CurrenciesGateway { get; private set; } = null;

    public DataManagerService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        TryInit();
    }

    /// <summary>
    /// Will initialize all data settings and shutdown application if an error is encountered.
    /// </summary>
    internal void TryInit(IServiceProvider serviceProvider = null)
    {
        if (serviceProvider is not null)
        {
            _serviceProvider = serviceProvider;
        }
        if (!InitSettings())
        {
            var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            service.Show(Resources.Resources.Main118_Closing, Resources.Resources.Main187_Fatalerror, MessageStatus.Exclamation);
            _serviceProvider.GetRequiredService<INavigationService>().ShutDownXiletrade();
            return;
        }
    }

    public bool InitConfig()
    {
        string configJson;
        string path = Path.GetFullPath("Data\\");
        if (File.Exists(path + Strings.File.Config))
        {
            configJson = Load_Config(Strings.File.Config);
        }
        else
        {
            if (!File.Exists(path + Strings.File.DefaultConfig))
            {
                return false;
            }
            configJson = Load_Config(Strings.File.DefaultConfig);
            Save_Config(configJson, "cfg");
        }

        Config = Json.Deserialize<ConfigData>(configJson);

        if (Config.Options.SearchFetchDetail > 80)
            Config.Options.SearchFetchDetail = 80;
        if (Config.Options.SearchFetchBulk > 80)
            Config.Options.SearchFetchBulk = 80;

        var isPoe2 = Config.Options.GameVersion is 1;
        var gateway = Config.Options.Gateway;
        Strings.Initialize(isPoe2, gateway);

        return true;
    }
    
    public void InitLeague(int gateway)
    {
        string langGateway = "Lang\\" + Strings.Culture[gateway] + "\\";
        string streamLeagues = Load_Config(langGateway + Strings.File.Leagues);
        League = Json.Deserialize<LeagueData>(streamLeagues);
    }

    private bool InitSettings()
    {
        if (!InitConfig())
            return false;

        try
        {
            string basePath = Path.GetFullPath("Data\\");
            string lang = $"Lang\\{Strings.Culture[Config.Options.Language]}\\";            
            string langEn = $"Lang\\{Strings.Culture[0]}\\";

            var culture = System.Globalization.CultureInfo.CreateSpecificCulture(Strings.Culture[Config.Options.Language]);
            Thread.CurrentThread.CurrentUICulture = culture;
            TranslationViewModel.Instance.CurrentCulture = culture;

            DivTiers = LoadDivTiers(basePath + Strings.File.Divination);

            var filterPath = basePath + lang;
            Bases = LoadBaseResults(filterPath + Strings.File.Bases);
            Mods = LoadBaseResults(filterPath + Strings.File.Mods);
            Monsters = LoadBaseResults(filterPath + Strings.File.Monsters);
            Currencies = LoadCurrencyResults(filterPath + Strings.File.Currency);
            Filter = LoadFilter(filterPath + Strings.File.Filters, Config.Options.GameVersion);            
            Words = LoadWordResults(filterPath + Strings.File.Words);
            Gems = LoadGemResults(filterPath + Strings.File.Gems);
            Parser = LoadParser(filterPath + Strings.File.ParsingRules);

            InitLeague(Config.Options.Gateway);

            if (Config.Options.Gateway != Config.Options.Language)
            {
                var langGateway = $"Lang\\{Strings.Culture[Config.Options.Gateway]}\\";
                var gatewayPath = basePath + langGateway;
                BasesGateway = LoadBaseResults(gatewayPath + Strings.File.Bases);
                WordsGateway = LoadWordResults(gatewayPath + Strings.File.Words);
                CurrenciesGateway = LoadCurrencyResults(gatewayPath + Strings.File.Currency);
            }
            var englishPath = basePath + langEn;
            FilterEn = LoadFilter(englishPath + Strings.File.Filters, Config.Options.GameVersion);
            CurrenciesEn = LoadCurrencyResults(englishPath + Strings.File.Currency);
        }
        catch
        {
            return false;
        }

        return true;
    }

    private static List<BaseResultData> LoadBaseResults(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException(filePath);

        var json = File.ReadAllText(filePath);
        var baseData = Json.Deserialize<BaseData>(json);
        if (baseData is null || baseData.Result is null
            || baseData.Result.Length is 0 || baseData.Result[0].Data is null)
        {
            return new();
        }
        return [.. baseData.Result[0].Data];
    }

    private static List<CurrencyResultData> LoadCurrencyResults(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException(filePath);

        var json = File.ReadAllText(filePath);
        var currencyData = Json.Deserialize<CurrencyResult>(json);
        if (currencyData is null || currencyData.Result is null)
        {
            return new();
        }
        return [.. currencyData.Result];
    }

    private static FilterData LoadFilter(string filePath, int gameVersion)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException(filePath);

        var json = File.ReadAllText(filePath);
        var filterData = Json.Deserialize<FilterData>(json);
        if (filterData is null)
        {
            return new();
        }
        return filterData.ArrangeFilter(gameVersion);
    }

    private static List<DivTiersResult> LoadDivTiers(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException(filePath);

        var json = File.ReadAllText(filePath);
        var divData = Json.Deserialize<DivTiersData>(json);
        if (divData is null || divData.Result is null)
        {
            return new();
        }
        return [.. divData.Result];
    }

    private static List<WordResultData> LoadWordResults(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException(filePath);

        var json = File.ReadAllText(filePath);
        var wordData = Json.Deserialize<WordData>(json);
        if (wordData is null || wordData.Result is null
            || wordData.Result.Length is 0 || wordData.Result[0].Data is null)
        {
            return new();
        }
        return [.. wordData.Result[0].Data];
    }

    private static List<GemResultData> LoadGemResults(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException(filePath);

        var json = File.ReadAllText(filePath);
        var gemData = Json.Deserialize<GemData>(json);
        if (gemData is null || gemData.Result is null
            || gemData.Result.Length is 0 || gemData.Result[0].Data is null)
        {
            return new();
        }
        return [.. gemData.Result[0].Data];
    }

    private static ParserData LoadParser(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException(filePath);

        var json = File.ReadAllText(filePath);
        var parserData = Json.Deserialize<ParserData>(json);
        if (parserData is null)
        {
            return new();
        }
        return parserData;
    }

    internal string Load_Config(string configfile)
    {
        string config;
        try
        {
            string pathFile = Path.GetFullPath("Data\\") + configfile;
            if (!File.Exists(pathFile))
                throw new FileNotFoundException(pathFile);

            config = File.ReadAllText(pathFile);
        }
        catch (Exception ex)
        {
            var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            service.Show(ex.Message, Resources.Resources.Main118_Closing, MessageStatus.Exclamation);
            _serviceProvider.GetRequiredService<INavigationService>().ShutDownXiletrade();
            return null;
        }
        return config;
    }

    internal bool Save_File(string json, string location)
    {
        using StreamWriter writer = new(location, false, Encoding.UTF8);
        try
        {
            writer.Write(json); // Saving new json
        }
        catch (Exception ex)
        {
            var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            service.Show(ex.Message, "Error: new json file can not be saved", MessageStatus.Exclamation);
            return false;
        }
        return true;
    }

    internal bool Save_Config(string configToSave, string type)
    {
        if (type is "cfg" or "patron")
        {
            string path = Path.GetFullPath("Data\\");
            string name = type is "cfg" ? Strings.File.Config : type is "patron" ? "Patron.json" : string.Empty;

            FileStream fs = null;
            try
            {
                fs = new FileStream(path + name, FileMode.OpenOrCreate);

                string configBackup = string.Empty;
                string configNew = string.Empty;
                using (StreamReader reader = new(fs))
                {
                    fs = null;
                    configBackup = reader.ReadToEnd();
                }

                using StreamWriter writer = new(path + name, false, Encoding.UTF8);
                try
                {
                    if (type is "cfg")
                    {
                        ConfigData newConfigData = Json.Deserialize<ConfigData>(configToSave);
                        configNew = Json.Serialize<ConfigData>(newConfigData);
                        writer.Write(configNew); // Saving new config
                        Config = Json.Deserialize<ConfigData>(configToSave);
                        return true;
                    }
                    if (type is "patron")
                    {
                        LicenceData newConfigData = Json.Deserialize<LicenceData>(configToSave);
                        configNew = Json.Serialize<LicenceData>(newConfigData);
                        writer.Write(configNew); // Saving new config
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    writer.Write(configBackup); // Backup
                    var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
                    service.Show(ex.Message, "Error: new file can not be serialized", MessageStatus.Exclamation);
                }
            }
            catch (Exception ex)
            {
                var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
                service.Show(ex.Message, "Error while saving new file", MessageStatus.Exclamation);
            }
            finally
            {
                fs?.Dispose();
            }
        }
        return false;
    }

    internal AsyncObservableCollection<string> GetLeagueAsyncCollection()
    {
        AsyncObservableCollection<string> listLeague = new();

        if (League.Result.Length >= 2)
        {
            foreach (var league in League.Result)
            {
                listLeague.Add(league.Id);
            }
        }

        return listLeague;
    }

    internal int GetDefaultLeagueIndex()
    {
        var tempLeague = GetLeagueAsyncCollection();
        int idx = tempLeague.IndexOf(Config.Options.League);
        return idx > -1 ? idx : 0;
    }
}
