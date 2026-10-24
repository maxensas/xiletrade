using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Models.Application.Notification;
using Xiletrade.Library.Models.Poe.Contract;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Enum;
using Xiletrade.Library.ViewModels.Config;

namespace Xiletrade.Library.Services;

/// <summary>Service used to update all JSON local data files used by Xiletrade.</summary>
/// <remarks>Retrieve source data from Xiletrade Github repository and GGG server.</remarks>
public sealed class DataUpdaterService : IDataUpdaterService
{
    private readonly INotificationService _notif;
    private readonly IMessageAdapterService _message;
    private readonly DataManagerService _dm;
    private readonly INetService _net;

    public DataUpdaterService(ILogger<DataUpdaterService> logger, DataManagerService dm,
        IMessageAdapterService message, INetService net,
        // external service :
        INotificationService notif)
    {
        _notif = notif;
        _message = message;
        _dm = dm;
        _net = net;

#if DEBUG
        logger.LogInformation("Service launched");
#endif
    }

    private static string ErrorMsg { get; set; } = string.Empty;

    public async Task UpdateAsync(GeneralViewModel cfgVm = null, bool allLanguages = false, bool updateGenerated = true)
    {
        List<Task> taskList = new();
        
        if (cfgVm is not null)
        {
            cfgVm.BtnUpdateEnable = false;
        }
        
        var cult = cfgVm is null || allLanguages ? string.Empty : " (" + Strings.Culture[cfgVm.LanguageIndex] + ")";
        var cultStart = cfgVm is not null ? allLanguages ? 0 : cfgVm.LanguageIndex : _dm.Config.Options.Language;
        var cultStop = allLanguages ? Strings.Culture.Length : (cfgVm is not null ? cfgVm.LanguageIndex : _dm.Config.Options.Language) + 1;
        var timeoutSecond = allLanguages ? 50 : 3;
        
        bool aborted = false;
        try
        {
            for (int i = cultStart; i < cultStop; i++)
            {
                if (i < 0 || i >= Strings.Culture.Length) continue;

                taskList.Add(ApiUpdate<LeagueData>("leagues", Strings.File.Leagues, "Leagues", i,
                    arrange: RemoveDuplicateLeagues));
                taskList.Add(ApiUpdate<FilterData>("stats", Strings.File.Filters, "FilterData", i,
                    prepare: FixFilterJson, arrange: SortFilterEntries));
                taskList.Add(ApiUpdate<CurrencyResult>("static", Strings.File.Currency, "Currency", i));
                taskList.Add(ApiUpdate<ItemTypeResult>("items", Strings.File.Items, "Item type", i));
                taskList.Add(GithubUpdate<ParserData>(Strings.File.ParsingRules, i));

                if (updateGenerated)
                {
                    taskList.Add(GithubUpdate<BaseData>(Strings.File.Bases, i));
                    taskList.Add(GithubUpdate<BaseData>(Strings.File.Mods, i));
                    taskList.Add(GithubUpdate<BaseData>(Strings.File.Monsters, i));
                    taskList.Add(GithubUpdate<WordData>(Strings.File.Words, i));
                    taskList.Add(GithubUpdate<GemData>(Strings.File.Gems, i));
                }
            }

            taskList.Add(GithubUpdate<DivTiersData>(Strings.File.Divination));
            taskList.Add(GithubUpdate<DustData>(Strings.File.DustLevel));

            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(timeoutSecond));
            var allTasks = Task.WhenAll(taskList);

            var completedTask = await Task.WhenAny(allTasks, timeoutTask);

            if (completedTask == timeoutTask)
            {
                aborted = true;
            }
            else
            {
                aborted = false;
                await allTasks;
            }
        }
        catch (Exception ex)
        {
            ErrorMsg += $"Exception while executing tasks : {ex}";
        }
        
        if (cfgVm is not null)
        {
            cfgVm.BtnUpdateEnable = true;
        }
        
        bool isNoError = ErrorMsg.Length is 0;
        if (isNoError)
        {
            _dm.TryInit();
            cfgVm?.InitLeagueList();
        }
        var title = isNoError ? "Xiletrade : " + (aborted ? Resources.Resources.Main193_DownloadKo : Resources.Resources.Main192_DownloadOk)
        : "Xiletrade : " + Resources.Resources.Main193_DownloadKo;
        var msg = isNoError ? aborted ? Resources.Resources.Main191_FiltersKo : Resources.Resources.Main190_FiltersOk
        : cult + Resources.Resources.Main191_FiltersKo + Strings.LF + ErrorMsg;
        ErrorMsg = string.Empty;
        var type = !isNoError || aborted ? NotificationStatus.Error : NotificationStatus.Success;
        var message = new ToastNotification(title: title, message: msg, status: type);
        _notif.Show(message);
    }

    public async Task<SettingsData> GetAppSettings()
    {
        try
        {
            if (Uri.TryCreate(Strings.Url.GithubData, UriKind.Absolute, out _))
            {
                var json = await _net.SendHTTPBytes(Strings.Url.GithubData + Strings.File.AppSettings, Client.GitHub);
                var settings = _dm.Json.Deserialize<SettingsData>(json);
                if (settings is not null)
                {
                    return settings;
                }
                _message.Show(Resources.Resources.Error003_EmptySettings, Resources.Resources.Error028_XLoadSettings, MessageStatus.Information);
            }
        }
        catch (Exception ex)
        {
            _message.Show(ex.GetFormated(), Resources.Resources.Error028_XLoadSettings, MessageStatus.Information);
        }
        return null;
    }

    // all private methods
    /// <summary>Fixes the downloaded json, returns null when there is nothing to fix.</summary>
    private delegate byte[] PrepareJson(ReadOnlySpan<byte> json, int idxLang);

    /// <summary>
    /// Downloads a data file from the game server (api), and saves it in the language folder.
    /// </summary>
    /// <param name="endpoint">Last part of the api url.</param>
    /// <param name="fileName">Name of the saved file.</param>
    /// <param name="label">Name of the data, used in the error messages.</param>
    /// <param name="idxLang">Index of the language (and of the server).</param>
    /// <param name="prepare">Fixes the downloaded text before it is deserialized.</param>
    /// <param name="arrange">Arranges the deserialized data before it is saved, returns null to save nothing.</param>
    private async Task ApiUpdate<T>(string endpoint, string fileName, string label, int idxLang,
        PrepareJson prepare = null, Func<T, T> arrange = null) where T : class
    {
        string url = Strings.Api.GetUpdate(idxLang) + endpoint;
        try
        {
            string path = Path.Combine(DataManagerService.DataDirectory, "Lang");
            if (Uri.TryCreate(Strings.Api.GetUpdate(idxLang), UriKind.Absolute, out _))
            {
                var json = await _net.SendHTTPBytes(url, Client.Update);
                if (prepare is not null && json.Length > 0)
                {
                    json = prepare(json, idxLang) ?? json;
                }
                SaveJsonToFile<T>(json, path, fileName, idxLang, arrange);
            }
        }
        catch (Exception ex)
        {
            ErrorMsg += ex.InnerException is not HttpRequestException ? Strings.LF + $"[{label} Update] Exception with Json writting : " + Common.GetInnerExceptionMessages(ex) + Strings.LF
                : idxLang != 9 ? Strings.LF + GetPoeServer(idxLang) + $" Server : {label} Update" + Strings.LF + url + " : " + Common.GetInnerExceptionMessages(ex) + Strings.LF : string.Empty;
        }
    }

    private static byte[] FixFilterJson(ReadOnlySpan<byte> json, int idxLang)
    {
        // remove line tabulation (portuguese)
        if (idxLang is not 5)
        {
            return null;
        }

        ReadOnlySpan<byte> tabulation = "\\u000b"u8;
        int index = json.IndexOf(tabulation);
        if (index < 0)
        {
            return null;
        }

        var result = new byte[json.Length];
        int length = 0;
        while (index >= 0)
        {
            json[..index].CopyTo(result.AsSpan(length));
            length += index;
            json = json[(index + tabulation.Length)..];
            index = json.IndexOf(tabulation);
        }
        json.CopyTo(result.AsSpan(length));
        length += json.Length;
        return result.AsSpan(0, length).ToArray();
    }

    // needed for files comparison following leagues updates.
    private static FilterData SortFilterEntries(FilterData filter)
    {
        if (filter.Result.Length > 0)
        {
            for (int i = 0; i < filter.Result.Length; i++)
            {
                filter.Result[i].Entries = [.. filter.Result[i].Entries.OrderBy(x => x.ID)];
            }
        }
        return filter;
    }

    private static LeagueData RemoveDuplicateLeagues(LeagueData leagues)
    {
        List<LeagueResult> leagueList = new();
        foreach (var league in leagues.Result)
        {
            if (!leagueList.Exists(x => x.Id == league.Id))
            {
                leagueList.Add(league);
            }
        }
        if (leagueList.Count is 0)
        {
            return null;
        }
        leagues.Result = [.. leagueList];
        return leagues;
    }

    private async Task GithubUpdate<T>(string fileName, int idxLang = -1) where T : class
    {
        string preffix = idxLang < 0 ? string.Empty : "Lang/" + Strings.Culture[idxLang] + "/";
        string urlStats = Strings.Url.GithubData + preffix + fileName;
        try
        {
            string path = idxLang < 0 ? DataManagerService.DataDirectory : Path.Combine(DataManagerService.DataDirectory, "Lang");
            if (Uri.TryCreate(Strings.Url.GithubData, UriKind.Absolute, out _))
            {
                var json = await _net.SendHTTPBytes(urlStats, Client.GitHub);
                SaveJsonToFile<T>(json, path, fileName, idxLang);
            }
        }
        catch (Exception ex)
        {
            if (ex.InnerException is not HttpRequestException)
            {
                var lang = Strings.Culture[idxLang].Split('-').Last();
                ErrorMsg += $"{Strings.LF}[{lang} : {fileName} Update] Exception with Json writting : {Common.GetInnerExceptionMessages(ex)}{Strings.LF}";
            }
        }
    }

    private static string GetPoeServer(int idxLang)
    {
        return idxLang is 9 ? "TENCENT" : idxLang is 8 ? "GARENA" : "GLOBAL";
    }

    private void SaveJsonToFile<T>(ReadOnlySpan<byte> json, string path, string fileName, 
        int idxLang = -1, Func<T, T> arrange = null) where T : class
    {
        if (json.IsEmpty)
            return;

        var data = _dm.Json.Deserialize<T>(json);
        if (data is not null && arrange is not null)
        {
            data = arrange(data);
        }
        if (data is null)
            return;

        WriteFile(path, fileName, data, idxLang);
    }

    private void WriteFile<T>(string path, string fileName, T data, int idxLang = -1) where T : class
    {
        // Create the main directory (Data or Lang)
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        // Create the language subdirectory if necessary
        if (idxLang >= 0 && idxLang < Strings.Culture.Length)
        {
            path = Path.Combine(path, Strings.Culture[idxLang]);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        string filePath = Path.Combine(path, fileName);

        // Serialize and write the file
        DataManagerService.WriteJsonFile(filePath, _dm.Json.SerializeToUtf8<T>(data));
    }
}
