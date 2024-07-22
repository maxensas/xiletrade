using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Xiletrade.Library.ViewModels;
using Xiletrade.Library.Models.Serializable;
using Xiletrade.Library.Shared;
using Microsoft.Extensions.DependencyInjection;
using Xiletrade.Library.Models.Enums;
using Xiletrade.Library.Services.Interface;

namespace Xiletrade.Library.Services;

/// <summary>Singleton handling all JSON data in memory for Xiletrade.</summary>
/// <remarks></remarks>
internal sealed class DataManager
{
    private static DataManager instance = null;
    private static readonly object instancelock = new();
    private static IServiceProvider _serviceProvider;

    internal static ConfigData Config { get; private set; }
    internal static FilterData Filter { get; private set; }
    internal static FilterData FilterEn { get; private set; }
    internal static ParserData Parser { get; private set; }
    internal static LeagueData League { get; private set; }

    internal static List<BaseResultData> Bases { get; private set; } = null;
    internal static List<BaseResultData> Mods { get; private set; } = null;
    internal static List<WordResultData> Words { get; private set; } = null;
    internal static List<GemResultData> Gems { get; private set; }
    internal static List<BaseResultData> Monsters { get; private set; } = null;
    internal static List<CurrencyResultData> Currencies { get; private set; } = null;
    internal static List<CurrencyResultData> CurrenciesEn { get; private set; } = null;
    internal static List<DivTiersResult> DivTiers { get; private set; } = null;

    private DataManager()
    {
    }

    internal static DataManager Instance
    {
        get
        {
            if (instance is null)
            {
                lock (instancelock)
                {
                    instance ??= new DataManager();
                }
            }
            return instance;
        }
    }

    /// <summary>
    /// Will initialize all data settings and shutdown application if an error is encountered.
    /// </summary>
    internal static void TryInit(IServiceProvider serviceProvider = null)
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

    private static bool InitSettings() // can be refactored
    {
        var init = Instance;
        string path = Path.GetFullPath("Data\\");

        bool returnVal = true;
        FileStream fs = null;
        try
        {
            //string config = Load_Config("Config.json");
            string configJson = null, configName = Strings.File.Config;
            if (ExistFile(configName))
            {
                configJson = Load_Config(configName);
            }
            else
            {
                configName = Strings.File.DefaultConfig;
                if (!ExistFile(configName))
                {
                    return false;
                }
                configJson = Load_Config(configName);
                Save_Config(configJson, "cfg");
            }

            Config = Json.Deserialize<ConfigData>(configJson);

            if (Config.Options.SearchFetchDetail > 80)
                Config.Options.SearchFetchDetail = 80;
            if (Config.Options.SearchFetchBulk > 80)
                Config.Options.SearchFetchBulk = 80;

            string lang = "Lang\\" + Strings.Culture[Config.Options.Language] + "\\";

            System.Globalization.CultureInfo cultureRefresh = System.Globalization.CultureInfo.CreateSpecificCulture(Strings.Culture[Config.Options.Language]);
            Thread.CurrentThread.CurrentUICulture = cultureRefresh;
            TranslationViewModel.Instance.CurrentCulture = cultureRefresh;

            fs = new FileStream(path + lang + Strings.File.Bases, FileMode.Open);
            using (StreamReader reader = new(fs))
            {
                fs = null;
                string json = reader.ReadToEnd();
                BaseData data = Json.Deserialize<BaseData>(json);
                Bases = new List<BaseResultData>();
                Bases.AddRange(data.Result[0].Data);
            }

            fs = new FileStream(path + lang + Strings.File.Mods, FileMode.Open);
            using (StreamReader reader = new(fs))
            {
                fs = null;
                string json = reader.ReadToEnd();
                BaseData data = Json.Deserialize<BaseData>(json);
                Mods = new List<BaseResultData>();
                Mods.AddRange(data.Result[0].Data);
            }
            /*
            fs = new FileStream(path + lang + Strings.File.PROPHECIES, FileMode.Open);
            using (StreamReader reader = new(fs))
            {
                fs = null;
                string json = reader.ReadToEnd();
                BaseData data = Json.Deserialize<BaseData>(json);
                Prophecies = new List<BaseResultData>();
                Prophecies.AddRange(data.Result[0].Data);
            }
            */
            fs = new FileStream(path + lang + Strings.File.Monsters, FileMode.Open);
            using (StreamReader reader = new(fs))
            {
                fs = null;
                string json = reader.ReadToEnd();
                BaseData data = Json.Deserialize<BaseData>(json);
                Monsters = new List<BaseResultData>();
                Monsters.AddRange(data.Result[0].Data);
            }

            fs = new FileStream(path + lang + Strings.File.Currency, FileMode.Open);
            using (StreamReader reader = new(fs))
            {
                fs = null;
                string json = reader.ReadToEnd();
                CurrencyResult cur = Json.Deserialize<CurrencyResult>(json);
                Currencies = new List<CurrencyResultData>();
                Currencies.AddRange(cur.Result);
            }

            fs = new FileStream(path + lang + Strings.File.Filters, FileMode.Open);
            using (StreamReader reader = new(fs))
            {
                fs = null;
                string json = reader.ReadToEnd();
                Filter = Json.Deserialize<FilterData>(json);
            }

            fs = new FileStream(path + Strings.File.Divination, FileMode.Open);
            using (StreamReader reader = new(fs))
            {
                fs = null;
                string json = reader.ReadToEnd();
                DivTiersData data = Json.Deserialize<DivTiersData>(json);
                DivTiers = new List<DivTiersResult>();
                DivTiers.AddRange(data.Result);
            }
            /*
            fs = new FileStream(path + "Uniques.json", FileMode.Open);
            using (StreamReader reader = new StreamReader(fs))
            {
                fs = null;
                string json = reader.ReadToEnd();
                Uniques = Json.Deserialize<UniquesData>(json);
            }
            */
            fs = new FileStream(path + lang + Strings.File.Words, FileMode.Open);
            using (StreamReader reader = new(fs))
            {
                fs = null;
                string json = reader.ReadToEnd();
                WordData data = Json.Deserialize<WordData>(json);
                Words = new List<WordResultData>();
                Words.AddRange(data.Result[0].Data);
            }

            fs = new FileStream(path + lang + Strings.File.Gems, FileMode.Open);
            using (StreamReader reader = new(fs))
            {
                fs = null;
                string json = reader.ReadToEnd();
                GemData data = Json.Deserialize<GemData>(json);
                Gems = new List<GemResultData>();
                Gems.AddRange(data.Result[0].Data);
            }

            fs = new FileStream(path + lang + Strings.File.ParsingRules, FileMode.Open);
            using (StreamReader reader = new(fs))
            {
                fs = null;
                string json = reader.ReadToEnd();
                Parser = Json.Deserialize<ParserData>(json);
            }

            string streamLeagues = Load_Config(lang + Strings.File.Leagues);
            League = Json.Deserialize<LeagueData>(streamLeagues);

            lang = "Lang\\" + Strings.Culture[0] + "\\"; // "en"
            fs = new FileStream(path + lang + Strings.File.Filters, FileMode.Open);
            using (StreamReader reader = new(fs))
            {
                fs = null;
                string json = reader.ReadToEnd();
                FilterEn = Json.Deserialize<FilterData>(json);
            }
            fs = new FileStream(path + lang + Strings.File.Currency, FileMode.Open);
            using (StreamReader reader = new(fs))
            {
                fs = null;
                string json = reader.ReadToEnd();
                CurrencyResult cur = Json.Deserialize<CurrencyResult>(json);
                CurrenciesEn = new List<CurrencyResultData>();
                CurrenciesEn.AddRange(cur.Result);
            }
        }/*
            catch (Exception ex)
            {

            }*/
        finally
        {
            fs?.Dispose();
        }

        return returnVal;
    }

    internal static string Load_Config(string configfile)
    {
        string path = Path.GetFullPath("Data\\");

        FileStream fs = null;
        string config = null;
        try
        {
            fs = new FileStream(path + configfile, FileMode.Open);

            using StreamReader reader = new(fs);
            fs = null;
            config = reader.ReadToEnd();
        }
        catch (Exception ex)
        {
            var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            service.Show(ex.Message, Resources.Resources.Main118_Closing, MessageStatus.Exclamation);
            _serviceProvider.GetRequiredService<INavigationService>().ShutDownXiletrade();
            return null;
        }
        finally
        {
            fs?.Dispose();
        }
        return config;
    }

    private static bool ExistFile(string file)
    {
        string path = Path.GetFullPath("Data\\");
        return File.Exists(path + file);
    }

    internal static bool Save_File(string json, string location)
    {
        /*
        string path = Path.GetFullPath("Data\\");
        string lang = "Lang\\" + Strings.Culture[Config.Options.Language] + "\\";
        string name = string.Empty;
        */

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

    internal static bool Save_Config(string configToSave, string type)
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
}
