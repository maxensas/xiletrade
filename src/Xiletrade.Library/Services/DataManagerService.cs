using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Models.Application.Serialization;
using Xiletrade.Library.Models.Poe.Contract;
using Xiletrade.Library.Models.Poe.Contract.Extension;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Collection;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.Services;

/// <summary>Service handling all serializable data in memory for Xiletrade.</summary>
/// <remarks></remarks>
public sealed class DataManagerService
{
    private readonly IMessageAdapterService _message;
    private readonly IViewManager _view;

    // Deserialized files, kept as they are : the data below are read from them.
    private BaseData BaseData { get; set; }
    private BaseData MonsterData { get; set; }
    private ModData ModData { get; set; }
    private WordData WordData { get; set; }
    private GemData GemData { get; set; }
    private CurrencyResult CurrencyData { get; set; }
    private CurrencyResult CurrencyDataEn { get; set; }
    private ItemTypeResult ItemTypeData { get; set; }
    private ItemTypeResult ItemTypeDataEn { get; set; }
    private DivTiersData DivTiersData { get; set; }
    private DustData DustData { get; set; }
    private BaseData BaseDataGateway { get; set; }
    private WordData WordDataGateway { get; set; }
    private CurrencyResult CurrencyDataGateway { get; set; }

    internal BaseResultData[] Bases => BaseData?.Result?[0].Data;
    internal ModResultData[] Mods => ModData?.Result?[0].Data;
    internal WordResultData[] Words => WordData?.Result?[0].Data;
    internal GemResultData[] Gems => GemData?.Result?[0].Data;
    internal BaseResultData[] Monsters => MonsterData?.Result?[0].Data;
    internal CurrencyResultData[] Currencies => CurrencyData?.Result;
    internal CurrencyResultData[] CurrenciesEn => CurrencyDataEn?.Result;
    internal ItemTypeResultData[] Items => ItemTypeData?.Result;
    internal ItemTypeResultData[] ItemsEn => ItemTypeDataEn?.Result;
    internal DivTiersResult[] DivTiers => DivTiersData?.Result;
    internal DustLevel[] DustLevel => DustData?.Level;

    //gateway
    internal WordResultData[] WordsGateway => WordDataGateway?.Result?[0].Data;
    internal BaseResultData[] BasesGateway => BaseDataGateway?.Result?[0].Data;
    internal CurrencyResultData[] CurrenciesGateway => CurrencyDataGateway?.Result;

    internal FilterData Filter { get; private set; }
    internal FilterData FilterEn { get; private set; }
    internal ParserData Parser { get; private set; }
    internal LeagueData League { get; private set; }
    internal SearchPresetData SearchPreset { get; private set; }

    internal ConfigData Config { get; private set; } // does not use StringCache
    internal JsonHelper Json { get; private set; }

    /// <summary>Folder of the data files, next to the application (whatever the current directory is).</summary>
    internal static string DataDirectory => Path.Combine(AppContext.BaseDirectory, "Data");

    public DataManagerService(ILogger<DataManagerService> logger,
        IMessageAdapterService message, IViewManager view)
    {
        _message = message;
        _view = view;

        TryInit();
#if DEBUG
        logger.LogInformation("Service launched");
#endif
    }

    /// <summary>
    /// Initialize all data settings and shutdown application if an error is encountered.
    /// </summary>
    internal void TryInit(int forceGameVersion = 0)
    {
        try
        {
            Initialize(forceGameVersion);
        }
        catch (Exception ex) 
        {
            _message.Show(Resources.Resources.Main118_Closing + "\n" + ex.InnerException.Message
                , Resources.Resources.Main187_Fatalerror, MessageStatus.Exclamation);
            _view.ShutDownNativeApp(1);
        }
    }

    private void InitConfig(int forceGameVersion)
    {
        string configJson;
        string path = DataDirectory;
        if (File.Exists(Path.Combine(path, Strings.File.Config)))
        {
            configJson = LoadConfiguration(Strings.File.Config);
        }
        else
        {
            if (!File.Exists(Path.Combine(path, Strings.File.DefaultConfig)))
            {
                throw new FileNotFoundException(Path.Combine(path, Strings.File.DefaultConfig));
            }
            configJson = LoadConfiguration(Strings.File.DefaultConfig);
            SaveConfiguration(configJson);
        }

        Config = Json.Deserialize<ConfigData>(configJson);

        if (Config.Options.SearchFetchDetail > 80)
            Config.Options.SearchFetchDetail = 80;
        if (Config.Options.SearchFetchBulk > 80)
            Config.Options.SearchFetchBulk = 80;

        if (forceGameVersion is 1 or 2)
        {
            Config.Options.GameVersion = forceGameVersion - 1;
        }
        var isPoe2 = Config.Options.GameVersion is 1;
        var gateway = Config.Options.Gateway;
        Strings.InitializeGameContext(isPoe2, gateway);
    }
    
    private void InitLeague(int gateway)
    {
        string langGateway = Path.Combine("Lang", Strings.Culture[gateway]);
        string streamLeagues = LoadConfiguration(Path.Combine(langGateway, Strings.File.Leagues));
        League = Json.Deserialize<LeagueData>(streamLeagues);
    }

    private void Initialize(int forceGameVersion)
    {
        try
        {
            if (Json is null)
            {
                Json = new(useCache: true);
            }
            else
            {
                Json.ResetCache();
            }
            InitConfig(forceGameVersion);

            string basePath = DataDirectory;
            string lang = Path.Combine("Lang", Strings.Culture[Config.Options.Language]);
            string langEn = Path.Combine("Lang", Strings.Culture[0]);

            DivTiersData = Load<DivTiersData>(Path.Combine(basePath, Strings.File.Divination));
            DustData = Load<DustData>(Path.Combine(basePath, Strings.File.DustLevel));
            SearchPreset = Load<SearchPresetData>(Path.Combine(basePath, Strings.File.SearchPreset)) ?? new();

            var filterPath = Path.Combine(basePath, lang);
            BaseData = Load<BaseData>(Path.Combine(filterPath, Strings.File.Bases));
            ModData = Load<ModData>(Path.Combine(filterPath, Strings.File.Mods));
            MonsterData = Load<BaseData>(Path.Combine(filterPath, Strings.File.Monsters));
            CurrencyData = Load<CurrencyResult>(Path.Combine(filterPath, Strings.File.Currency));
            ItemTypeData = Load<ItemTypeResult>(Path.Combine(filterPath, Strings.File.Items));
            Filter = Load<FilterData>(Path.Combine(filterPath, Strings.File.Filters))?.ArrangeFilter(Config.Options.GameVersion) ?? new();            
            WordData = Load<WordData>(Path.Combine(filterPath, Strings.File.Words));
            GemData = Load<GemData>(Path.Combine(filterPath, Strings.File.Gems));
            Parser = Load<ParserData>(Path.Combine(filterPath, Strings.File.ParsingRules)) ?? new();

            InitLeague(Config.Options.Gateway);

            if (Config.Options.Gateway != Config.Options.Language)
            {
                var langGateway = Path.Combine("Lang", Strings.Culture[Config.Options.Gateway]);
                var gatewayPath = Path.Combine(basePath, langGateway);
                BaseDataGateway = Load<BaseData>(Path.Combine(gatewayPath, Strings.File.Bases));
                WordDataGateway = Load<WordData>(Path.Combine(gatewayPath, Strings.File.Words));
                CurrencyDataGateway = Load<CurrencyResult>(Path.Combine(gatewayPath, Strings.File.Currency));
            }
            var englishPath = Path.Combine(basePath, langEn);
            FilterEn = Load<FilterData>(Path.Combine(englishPath, Strings.File.Filters))?.ArrangeFilter(Config.Options.GameVersion) ?? new FilterData();
            CurrencyDataEn = Load<CurrencyResult>(Path.Combine(englishPath, Strings.File.Currency));
            ItemTypeDataEn = Load<ItemTypeResult>(Path.Combine(englishPath, Strings.File.Items));
        }
        catch(Exception ex)
        {
            throw new ApplicationException("Can not initialize Data manager.", ex);
        }
        finally
        {
            Common.CollectGarbage();
        }
    }

    /// <summary>
    /// Reads and deserializes a data file, directly from its UTF-8 content.
    /// </summary>
    /// <exception cref="FileNotFoundException"></exception>
    /// <exception cref="JsonException">The file can not be read or deserialized.</exception>
    private T Load<T>(string filePath) where T : class
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException(filePath);
        try
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return Json.Deserialize<T>(stream);
        }
        catch (Exception ex)
        {
            throw new JsonException($"Can not load {Path.GetFileName(filePath)}.\nFile location: {filePath}" + ex.Message, ex);
        }
    }

    internal string LoadConfiguration(string configfile)
    {
        string config;
        try
        {
            string pathFile = Path.Combine(DataDirectory, configfile);
            if (!File.Exists(pathFile))
                throw new FileNotFoundException(pathFile);

            config = File.ReadAllText(pathFile);
        }
        catch (Exception ex)
        {
            _message.Show(ex.GetFormated(), Resources.Resources.Main118_Closing, MessageStatus.Exclamation);
            _view.ShutDownNativeApp();
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
            _message.Show(ex.GetFormated(), Resources.Resources.Error024_XSaveFile, MessageStatus.Exclamation);
            return false;
        }
    }

    internal bool SaveConfiguration(ReadOnlySpan<char> configToSave)
    {
        string filePath = Path.Combine(DataDirectory, Strings.File.Config);
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
            _message.Show(ex.GetFormated(), Resources.Resources.Error007_XSaveConfig, MessageStatus.Exclamation);
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
