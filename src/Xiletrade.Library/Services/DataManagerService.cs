using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Models.Application.Serialization;
using Xiletrade.Library.Models.Poe.Contract;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Collection;
using Xiletrade.Library.Shared.Enum;
using Xiletrade.Library.ViewModels;

namespace Xiletrade.Library.Services;

/// <summary>Service (singleton) handling all serializable data in memory for Xiletrade.</summary>
/// <remarks></remarks>
public sealed class DataManagerService
{
    private static IServiceProvider _serviceProvider;

    internal JsonHelper Json { get; private set; }

    internal ConfigData Config { get; private set; } // does not use StringCache

    internal FilterData Filter { get; private set; }
    internal FilterData FilterEn { get; private set; }
    internal ParserData Parser { get; private set; }
    internal LeagueData League { get; private set; }
    internal SearchPresetData SearchPreset { get; private set; }

    internal IEnumerable<BaseResultData> Bases { get; private set; } = null;
    internal IEnumerable<BaseResultData> Mods { get; private set; } = null;
    internal IEnumerable<WordResultData> Words { get; private set; } = null;
    internal IEnumerable<GemResultData> Gems { get; private set; }
    internal IEnumerable<BaseResultData> Monsters { get; private set; } = null;
    internal IEnumerable<CurrencyResultData> Currencies { get; private set; } = null;
    internal IEnumerable<CurrencyResultData> CurrenciesEn { get; private set; } = null;
    internal IEnumerable<DivTiersResult> DivTiers { get; private set; } = null;
    internal IEnumerable<DustLevel> DustLevel { get; private set; }

    //temp
    internal IEnumerable<WordResultData> WordsGateway { get; private set; } = null;
    internal IEnumerable<BaseResultData> BasesGateway { get; private set; } = null;
    internal IEnumerable<CurrencyResultData> CurrenciesGateway { get; private set; } = null;

    public DataManagerService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Initialize all data settings and shutdown application if an error is encountered.
    /// </summary>
    internal void TryInit()
    {
        try
        {
            Initialize();
        }
        catch (Exception ex) 
        {
            var ms = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            ms.Show(Resources.Resources.Main118_Closing + "\n" + ex.InnerException.Message
                , Resources.Resources.Main187_Fatalerror, MessageStatus.Exclamation);
            _serviceProvider.GetRequiredService<INavigationService>().ShutDownXiletrade(1);
        }
    }

    private void InitConfig()
    {
        string configJson;
        string path = Path.GetFullPath("Data\\");
        if (File.Exists(path + Strings.File.Config))
        {
            configJson = LoadConfiguration(Strings.File.Config);
        }
        else
        {
            if (!File.Exists(path + Strings.File.DefaultConfig))
            {
                throw new FileNotFoundException(path + Strings.File.DefaultConfig);
            }
            configJson = LoadConfiguration(Strings.File.DefaultConfig);
            SaveConfiguration(configJson);
        }

        Config = Json.Deserialize<ConfigData>(configJson);

        if (Config.Options.SearchFetchDetail > 80)
            Config.Options.SearchFetchDetail = 80;
        if (Config.Options.SearchFetchBulk > 80)
            Config.Options.SearchFetchBulk = 80;

        var isPoe2 = Config.Options.GameVersion is 1;
        var gateway = Config.Options.Gateway;
        Strings.Initialize(isPoe2, gateway);
    }
    
    private void InitLeague(int gateway)
    {
        string langGateway = "Lang\\" + Strings.Culture[gateway] + "\\";
        string streamLeagues = LoadConfiguration(langGateway + Strings.File.Leagues);
        League = Json.Deserialize<LeagueData>(streamLeagues);
    }

    private void Initialize()
    {
        try
        {
            if (Json is null)
            {
                Json = new(_serviceProvider);
            }
            else
            {
                Json.ResetCache();
            }
            InitConfig();

            string basePath = Path.GetFullPath("Data\\");
            string lang = $"Lang\\{Strings.Culture[Config.Options.Language]}\\";            
            string langEn = $"Lang\\{Strings.Culture[0]}\\";

            DivTiers = LoadDivTiers(basePath + Strings.File.Divination);
            DustLevel = LoadDustLevel(basePath + Strings.File.DustLevel);
            SearchPreset = LoadSearchPreset(basePath + Strings.File.SearchPreset);

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
        catch(Exception ex)
        {
            throw new ApplicationException("Can not initialize Data manager.", ex);
        }
        finally
        {
            GC.Collect();
        }
    }

    private List<BaseResultData> LoadBaseResults(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException(filePath);
        try
        {
            var json = File.ReadAllText(filePath);
            var baseData = Json.Deserialize<BaseData>(json);
            if (baseData is null || baseData.Result is null
                || baseData.Result.Length is 0 || baseData.Result[0].Data is null)
            {
                return new();
            }
            return [.. baseData.Result[0].Data];
        }
        catch (Exception ex)
        {
            throw new JsonException($"Can not load Base data.\nFile location: {filePath}" + ex.Message, ex);
        }
    }

    private List<CurrencyResultData> LoadCurrencyResults(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException(filePath);
        try
        {
            var json = File.ReadAllText(filePath);
            var currencyData = Json.Deserialize<CurrencyResult>(json);
            if (currencyData is null || currencyData.Result is null)
            {
                return new();
            }
            return [.. currencyData.Result];
        }
        catch (Exception ex)
        {
            throw new JsonException($"Can not load Currency data.\nFile location: {filePath}" + ex.Message, ex);
        }
    }

    private FilterData LoadFilter(string filePath, int gameVersion)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException(filePath);
        try
        {
            var json = File.ReadAllText(filePath);
            var filterData = Json.Deserialize<FilterData>(json);
            if (filterData is null)
            {
                return new();
            }
            return filterData.ArrangeFilter(gameVersion);
        }
        catch (Exception ex)
        {
            throw new JsonException($"Can not load Filter data.\nFile location: {filePath}" + ex.Message, ex);
        }
    }

    private List<DivTiersResult> LoadDivTiers(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException(filePath);
        try
        {
            var json = File.ReadAllText(filePath);
            var divData = Json.Deserialize<DivTiersData>(json);
            if (divData is null || divData.Result is null)
            {
                return new();
            }
            return [.. divData.Result];
        }
        catch (Exception ex)
        {
            throw new JsonException($"Can not load Divination cards data.\nFile location: {filePath}" + ex.Message, ex);
        }
    }

    private List<DustLevel> LoadDustLevel(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException(filePath);
        try
        {
            var json = File.ReadAllText(filePath);
            var dustData = Json.Deserialize<DustData>(json);
            if (dustData is null || dustData.Level is null)
            {
                return new();
            }
            return [.. dustData.Level];
        }
        catch(Exception ex)
        {
            throw new JsonException($"Can not load Dust level.\nFile location: {filePath}" + ex.Message, ex);
        }
    }

    private SearchPresetData LoadSearchPreset(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException(filePath);
        try
        {
            var json = File.ReadAllText(filePath);
            var searchPreset = Json.Deserialize<SearchPresetData>(json);
            return searchPreset is null ? new() : searchPreset;
        }
        catch (Exception ex)
        {
            throw new JsonException($"Can not load search presets.\nFile location: {filePath}" + ex.Message, ex);
        }
    }

    private List<WordResultData> LoadWordResults(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException(filePath);
        try
        {
            var json = File.ReadAllText(filePath);
            var wordData = Json.Deserialize<WordData>(json);
            if (wordData is null || wordData.Result is null
                || wordData.Result.Length is 0 || wordData.Result[0].Data is null)
            {
                return new();
            }
            return [.. wordData.Result[0].Data];
        }
        catch (Exception ex)
        {
            throw new JsonException($"Can not load Words data.\nFile location: {filePath}" + ex.Message, ex);
        }
    }

    private List<GemResultData> LoadGemResults(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException(filePath);
        try
        {
            var json = File.ReadAllText(filePath);
            var gemData = Json.Deserialize<GemData>(json);
            if (gemData is null || gemData.Result is null
                || gemData.Result.Length is 0 || gemData.Result[0].Data is null)
            {
                return new();
            }
            return [.. gemData.Result[0].Data];
        }
        catch (Exception ex)
        {
            throw new JsonException($"Can not load Gems data.\nFile location: {filePath}" + ex.Message, ex);
        }
    }

    private ParserData LoadParser(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException(filePath);
        try
        {
            var json = File.ReadAllText(filePath);
            var parserData = Json.Deserialize<ParserData>(json);
            if (parserData is null)
            {
                return new();
            }
            return parserData;
        }
        catch (Exception ex)
        {
            throw new JsonException($"Can not load Parsing rules data.\nFile location: {filePath}" + ex.Message, ex);
        }
    }

    internal string LoadConfiguration(string configfile)
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

    // will be moved
    internal void RefreshCurrentCulture(int culture = -1)
    {
        var indexCulture = culture < 0 ? Config.Options.Language : culture;
        CultureInfo cultureRefresh = CultureInfo.CreateSpecificCulture(Strings.Culture[indexCulture]);
        Thread.CurrentThread.CurrentCulture = cultureRefresh;
        Thread.CurrentThread.CurrentUICulture = cultureRefresh;
        TranslationViewModel.Instance.CurrentCulture = cultureRefresh;
    }

    internal async Task<bool> SaveFileAsync(string content, string filePath)
    {
        try
        {
            await File.WriteAllTextAsync(filePath, content, Encoding.UTF8);
            return true;
        }
        catch (Exception ex)
        {
            var messageService = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            messageService.Show(ex.Message, "Error: file cannot be saved", MessageStatus.Exclamation);
            return false;
        }
    }

    internal bool SaveConfiguration(ReadOnlySpan<char> configToSave)
    {
        string path = Path.GetFullPath("Data\\");
        string filePath = Path.Combine(path, Strings.File.Config);
        string configBackup = string.Empty;
        try
        {
            if (File.Exists(filePath))
            {
                configBackup = File.ReadAllText(filePath, Encoding.UTF8);
            }

            var newConfig = Json.Deserialize<ConfigData>(configToSave);
            var serialized = Json.Serialize<ConfigData>(newConfig);

            File.WriteAllText(filePath, serialized, Encoding.UTF8);
            Config = newConfig;

            return true;
        }
        catch (Exception ex)
        {
            try
            {
                if (configBackup.Length > 0)
                    File.WriteAllText(filePath, configBackup, Encoding.UTF8);
            }
            catch
            {
                //ignore
            }
            var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            service.Show(ex.Message, "Error while saving configuration", MessageStatus.Exclamation);
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
