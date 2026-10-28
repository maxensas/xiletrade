using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Models.Application.Serialization;
using Xiletrade.Library.Models.Serialization.SourceGeneration;
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

    /// <summary>Folder of the data files, next to the application (whatever the current directory is).</summary>
    internal static string DataDirectory => Path.Combine(AppContext.BaseDirectory, "Data");

    internal JsonHelper Json { get; private set; }

    internal ConfigData Config { get; private set; } // does not use StringCache

    internal FilterData Filter { get; private set; }
    internal FilterData FilterEn { get; private set; }
    internal ParserData Parser { get; private set; }
    internal LeagueData League { get; private set; }
    internal SearchPresetData SearchPreset { get; private set; }

    // Deserialized files, kept as they are : the data below are read from them (no copy in memory).
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

    //temp
    internal WordResultData[] WordsGateway => WordDataGateway?.Result?[0].Data;
    internal BaseResultData[] BasesGateway => BaseDataGateway?.Result?[0].Data;
    internal CurrencyResultData[] CurrenciesGateway => CurrencyDataGateway?.Result;

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
        byte[] configJson;
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
    
    private void InitLeague(int gateway, SourceGenerationContext context)
    {
        string langGateway = Path.Combine("Lang", Strings.Culture[gateway]);
        var leagues = LoadConfiguration(Path.Combine(langGateway, Strings.File.Leagues));
        League = Json.Deserialize<LeagueData>(leagues, context: context);
    }

    private void Initialize(int forceGameVersion)
    {
        try
        {
            Json ??= new();
            InitConfig(forceGameVersion);

            // The strings of the files loaded together are shared through this cache :
            // the cache and its context are only used here, and released with the loading.
            var context = Json.CreateInterningContext(new StringCache());

            string basePath = DataDirectory;
            string lang = Path.Combine("Lang", Strings.Culture[Config.Options.Language]);
            string langEn = Path.Combine("Lang", Strings.Culture[0]);

            // The files are independent : they are read and deserialized together.
            var filterPath = Path.Combine(basePath, lang);
            var englishPath = Path.Combine(basePath, langEn);
            var arrangeVersion = Config.Options.GameVersion;
            var isEnglish = Config.Options.Language is 0;

            var divTiers = Load<DivTiersData>(Path.Combine(basePath, Strings.File.Divination), context);
            var dust = Load<DustData>(Path.Combine(basePath, Strings.File.DustLevel), context);
            var searchPreset = Load<SearchPresetData>(Path.Combine(basePath, Strings.File.SearchPreset), context);
            var bases = Load<BaseData>(Path.Combine(filterPath, Strings.File.Bases), context);
            var mods = Load<ModData>(Path.Combine(filterPath, Strings.File.Mods), context);
            var monsters = Load<BaseData>(Path.Combine(filterPath, Strings.File.Monsters), context);
            var words = Load<WordData>(Path.Combine(filterPath, Strings.File.Words), context);
            var gems = Load<GemData>(Path.Combine(filterPath, Strings.File.Gems), context);
            var parser = Load<ParserData>(Path.Combine(filterPath, Strings.File.ParsingRules), context);

            var filter = LoadFilter(Path.Combine(filterPath, Strings.File.Filters), arrangeVersion, context);
            var currency = Load<CurrencyResult>(Path.Combine(filterPath, Strings.File.Currency), context);
            var items = Load<ItemTypeResult>(Path.Combine(filterPath, Strings.File.Items), context);

            var filterEn = isEnglish ? filter : LoadFilter(Path.Combine(englishPath, Strings.File.Filters), arrangeVersion, context);
            var currencyEn = isEnglish ? currency : Load<CurrencyResult>(Path.Combine(englishPath, Strings.File.Currency), context);
            var itemsEn = isEnglish ? items : Load<ItemTypeResult>(Path.Combine(englishPath, Strings.File.Items), context);

            Task<BaseData> basesGateway = null;
            Task<WordData> wordsGateway = null;
            Task<CurrencyResult> currencyGateway = null;
            if (Config.Options.Gateway != Config.Options.Language)
            {
                var gatewayPath = Path.Combine(basePath, Path.Combine("Lang", Strings.Culture[Config.Options.Gateway]));
                basesGateway = Load<BaseData>(Path.Combine(gatewayPath, Strings.File.Bases), context);
                wordsGateway = Load<WordData>(Path.Combine(gatewayPath, Strings.File.Words), context);
                currencyGateway = Load<CurrencyResult>(Path.Combine(gatewayPath, Strings.File.Currency), context);
            }

            InitLeague(Config.Options.Gateway, context);

            WaitAll(divTiers, dust, searchPreset, bases, mods, monsters, currency, items, filter, words, gems,
                parser, filterEn, currencyEn, itemsEn, basesGateway, wordsGateway, currencyGateway);

            DivTiersData = divTiers.Result;
            DustData = dust.Result;
            SearchPreset = searchPreset.Result ?? new();
            BaseData = bases.Result;
            ModData = mods.Result;
            MonsterData = monsters.Result;
            WordData = words.Result;
            GemData = gems.Result;
            Parser = parser.Result ?? new();

            Filter = filter.Result ?? new();
            CurrencyData = currency.Result;
            ItemTypeData = items.Result;

            FilterEn = isEnglish ? Filter : filterEn.Result ?? new();
            CurrencyDataEn = currencyEn.Result;
            ItemTypeDataEn = itemsEn.Result;

            BaseDataGateway = basesGateway?.Result;
            WordDataGateway = wordsGateway?.Result;
            CurrencyDataGateway = currencyGateway?.Result;
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
    /// Waits for the tasks, and throws the exception of the first one (in the given order) that failed,
    /// as the sequential loading did, instead of an AggregateException.
    /// </summary>
    private static void WaitAll(params Task[] tasks)
    {
        tasks = [.. tasks.Where(t => t is not null)];
        try
        {
            Task.WaitAll(tasks);
        }
        catch (AggregateException)
        {
            foreach (var task in tasks)
            {
                if (task.IsFaulted)
                {
                    ExceptionDispatchInfo.Capture(task.Exception.InnerException).Throw();
                }
            }
            throw;
        }
    }

    /// <summary>
    /// Reads (asynchronously) and deserializes a data file, directly from its UTF-8 content.
    /// </summary>
    /// <exception cref="FileNotFoundException"></exception>
    /// <exception cref="JsonException">The file can not be read or deserialized.</exception>
    private async Task<T> Load<T>(string filePath, SourceGenerationContext context) where T : class
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException(filePath);
        try
        {
            // ConfigureAwait(false) : the deserialization goes on a thread pool thread, and never waits for the caller thread.
            var json = await File.ReadAllBytesAsync(filePath).ConfigureAwait(false);
            return Json.Deserialize<T>(json, context: context);
        }
        catch (Exception ex)
        {
            throw new JsonException($"Can not load {Path.GetFileName(filePath)}.\nFile location: {filePath}" + ex.Message, ex);
        }
    }

    private async Task<FilterData> LoadFilter(string filePath, int gameVersion, SourceGenerationContext context)
        => (await Load<FilterData>(filePath, context).ConfigureAwait(false))?.ArrangeFilter(gameVersion);

    internal byte[] LoadConfiguration(string configfile)
    {
        byte[] config;
        try
        {
            string pathFile = Path.Combine(DataDirectory, configfile);
            if (!File.Exists(pathFile))
                throw new FileNotFoundException(pathFile);

            config = File.ReadAllBytes(pathFile);
        }
        catch (Exception ex)
        {
            _message.Show(ex.GetFormated(), Resources.Resources.Main118_Closing, MessageStatus.Exclamation);
            _view.ShutDownNativeApp();
            return null;
        }
        return config;
    }

    private static readonly byte[] Utf8Preamble = Encoding.UTF8.GetPreamble();

    /// <summary>
    /// Writes a UTF-8 json file, with a byte order mark as the files saved by the application have always had.
    /// </summary>
    internal static void WriteJsonFile(string filePath, ReadOnlySpan<byte> json)
    {
        using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read);
        stream.Write(Encoding.UTF8.Preamble);
        stream.Write(json);
    }

    internal async Task<bool> SaveFileAsync(ReadOnlyMemory<byte> json, string filePath)
    {
        try
        {
            await using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read, 4096, useAsync: true);
            await stream.WriteAsync(Utf8Preamble);
            await stream.WriteAsync(json);
            return true;
        }
        catch (Exception ex)
        {
            _message.Show(ex.GetFormated(), Resources.Resources.Error024_XSaveFile, MessageStatus.Exclamation);
            return false;
        }
    }

    internal bool SaveConfiguration(ReadOnlySpan<byte> json)
    {
        string filePath = Path.Combine(DataDirectory, Strings.File.Config);
        byte[] configBackup = null;
        try
        {
            if (File.Exists(filePath))
            {
                configBackup = File.ReadAllBytes(filePath);
            }

            var newConfig = Json.Deserialize<ConfigData>(json);

            WriteJsonFile(filePath, Json.SerializeToUtf8<ConfigData>(newConfig));
            Config = newConfig;

            return true;
        }
        catch (Exception ex)
        {
            try
            {
                if (configBackup is { Length: > 0 })
                    File.WriteAllBytes(filePath, configBackup);
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
