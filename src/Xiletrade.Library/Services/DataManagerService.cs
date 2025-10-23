using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Models.Application.Serialization;
using Xiletrade.Library.Models.Ninja.Contract;
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

    internal Exception InitializeException { get; private set; } = null;

    internal JsonHelper Json { get; }

    internal ConfigData Config { get; private set; } // does not use StringCache

    internal FilterData Filter { get; private set; }
    internal FilterData FilterEn { get; private set; }
    internal ParserData Parser { get; private set; }
    internal LeagueData League { get; private set; }
    internal NinjaState NinjaState { get; private set; }
    
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
        Json = new(this);
        InitializeData();
    }

    /// <summary>
    /// Initialize all data settings and shutdown application if an error is encountered.
    /// </summary>
    internal void TryInit(IServiceProvider serviceProvider = null)
    {
        if (serviceProvider is not null)
        {
            _serviceProvider = serviceProvider;
        }
        if (!InitializeData())
        {
            var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            service.Show(Resources.Resources.Main118_Closing, Resources.Resources.Main187_Fatalerror, MessageStatus.Exclamation);
            _serviceProvider.GetRequiredService<INavigationService>().ShutDownXiletrade(1);
            return;
        }
    }

    public bool InitConfig()
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
                return false;
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

        return true;
    }
    
    public void InitLeague(int gateway)
    {
        string langGateway = "Lang\\" + Strings.Culture[gateway] + "\\";
        string streamLeagues = LoadConfiguration(langGateway + Strings.File.Leagues);
        League = Json.Deserialize<LeagueData>(streamLeagues);
    }

    private bool InitializeData()
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
            
            Json.ResetCache();

            DivTiers = LoadDivTiers(basePath + Strings.File.Divination);
            DustLevel = LoadDustLevel(basePath + Strings.File.DustLevel);

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
            InitializeException = ex;
            return false;
        }
        finally
        {
            GC.Collect();
        }
        return true;
    }

    private List<BaseResultData> LoadBaseResults(string filePath)
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

    private List<CurrencyResultData> LoadCurrencyResults(string filePath)
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

    private FilterData LoadFilter(string filePath, int gameVersion)
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

    private List<DivTiersResult> LoadDivTiers(string filePath)
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

    private List<DustLevel> LoadDustLevel(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException(filePath);

        var json = File.ReadAllText(filePath);
        var dustData = Json.Deserialize<DustData>(json);
        if (dustData is null || dustData.Level is null)
        {
            return new();
        }
        return [.. dustData.Level];
    }

    private List<WordResultData> LoadWordResults(string filePath)
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

    private List<GemResultData> LoadGemResults(string filePath)
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

    private ParserData LoadParser(string filePath)
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

    internal async Task LoadNinjaStateAsync()
    {
        try
        {
            var service = _serviceProvider.GetRequiredService<NetService>();
            var result = await service.SendHTTP(Strings.ApiNinjaLeague, Client.Ninja).ConfigureAwait(false);
            var ninjaState = Json.Deserialize<NinjaState>(result);
            NinjaState = ninjaState ?? GenerateCustomState();
        }
        catch (Exception ex)
        {
            var messageService = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            messageService.Show(ex.Message, "Can not load leagues list from poe.ninja", MessageStatus.Information);
            NinjaState ??= GenerateCustomState();
        }
    }

    private NinjaState GenerateCustomState()
    {
        if (League is null || League.Result is null)
        {
            return null;
        }
        string leagueKind = League.Result[0].Id;
        var eventLeague = League.Result.FirstOrDefault(x => x.Text.Contain('(')
            && x.Text.Contain(')') && x.Text.Contain("00")) is not null;
        NinjaState state = new()
        {
            Leagues = [
                new() { Name = leagueKind, DisplayName = leagueKind, Url = leagueKind.ToLowerInvariant(), Hardcore = false, Indexed = true },
                new() { Name = "Hardcore " + leagueKind, DisplayName = "Hardcore " + leagueKind, Url = leagueKind.ToLowerInvariant() + "hc", Hardcore = true, Indexed = false },
                new() { Name = "Standard", DisplayName = "Standard", Url = "standard", Hardcore = false, Indexed = false },
                new() { Name = "Hardcore", DisplayName = "Hardcore", Url = "hardcore", Hardcore = true, Indexed = false }
            ]
        };
        if (eventLeague)
        {
            state = new()
            {
                Leagues = [..state.Leagues,
                    new() { Name = "Event", DisplayName = "Event", Url = "event", Hardcore = false, Indexed = false },
                    new() { Name = "EventHC", DisplayName = "EventHC", Url = "eventhc", Hardcore = true, Indexed = false }
                ]
            };
        }
        return state;
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
