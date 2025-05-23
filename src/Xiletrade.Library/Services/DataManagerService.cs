﻿using System;
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
using Xiletrade.Library.Models.Collections;

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

    private bool InitSettings() // can be refactored
    {
        string path = Path.GetFullPath("Data\\");

        bool returnVal = true;
        FileStream fs = null;
        try
        {
            if (!InitConfig())
            {
                return false;
            }

            string lang = "Lang\\" + Strings.Culture[Config.Options.Language] + "\\";
            string langGateway = "Lang\\" + Strings.Culture[Config.Options.Gateway] + "\\";

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
                Filter = Json.Deserialize<FilterData>(json).ArrangeFilter(Config.Options.GameVersion);
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

            InitLeague(Config.Options.Gateway);

            if (Config.Options.Gateway != Config.Options.Language)
            {
                fs = new FileStream(path + langGateway + Strings.File.Bases, FileMode.Open);
                using (StreamReader reader = new(fs))
                {
                    fs = null;
                    string json = reader.ReadToEnd();
                    BaseData data = Json.Deserialize<BaseData>(json);
                    BasesGateway = new List<BaseResultData>();
                    BasesGateway.AddRange(data.Result[0].Data);
                }

                fs = new FileStream(path + langGateway + Strings.File.Words, FileMode.Open);
                using (StreamReader reader = new(fs))
                {
                    fs = null;
                    string json = reader.ReadToEnd();
                    WordData data = Json.Deserialize<WordData>(json);
                    WordsGateway = new List<WordResultData>();
                    WordsGateway.AddRange(data.Result[0].Data);
                }

                fs = new FileStream(path + langGateway + Strings.File.Currency, FileMode.Open);
                using (StreamReader reader = new(fs))
                {
                    fs = null;
                    string json = reader.ReadToEnd();
                    CurrencyResult cur = Json.Deserialize<CurrencyResult>(json);
                    CurrenciesGateway = new List<CurrencyResultData>();
                    CurrenciesGateway.AddRange(cur.Result);
                }
            }

            lang = "Lang\\" + Strings.Culture[0] + "\\"; // "en"
            fs = new FileStream(path + lang + Strings.File.Filters, FileMode.Open);
            using (StreamReader reader = new(fs))
            {
                fs = null;
                string json = reader.ReadToEnd();
                FilterEn = Json.Deserialize<FilterData>(json).ArrangeFilter(Config.Options.GameVersion);
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
        }
        finally
        {
            fs?.Dispose();
        }

        return returnVal;
    }

    internal string Load_Config(string configfile)
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

    internal bool Save_File(string json, string location)
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
