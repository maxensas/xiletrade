using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Models.Poe.Contract;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Enum;
using Xiletrade.Library.ViewModels.Config;

namespace Xiletrade.Library.Services;

/// <summary>Service used to update all JSON local data files used by Xiletrade.</summary>
/// <remarks>Retrieve source data from Xiletrade Github repository and GGG server.</remarks>
public sealed class DataUpdaterService
{
    private static IServiceProvider _serviceProvider;
    private static string ErrorMsg { get; set; } = string.Empty;

    private readonly DataManagerService _dm;

    public DataUpdaterService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _dm = _serviceProvider.GetRequiredService<DataManagerService>();
    }

    internal async Task UpdateAsync(ConfigViewModel cfgVm = null, bool allLanguages = false, bool updateGenerated = true)
    {
        List<Task> taskList = new();

        if (cfgVm is not null)
        {
            cfgVm.General.BtnUpdateEnable = false;
        }

        var dm = _serviceProvider.GetRequiredService<DataManagerService>();
        var cult = cfgVm is null || allLanguages ? string.Empty : " (" + Strings.Culture[cfgVm.General.LanguageIndex] + ")";
        var cultStart = cfgVm is not null ? allLanguages ? 0 : cfgVm.General.LanguageIndex : dm.Config.Options.Language;
        var cultStop = allLanguages ? Strings.Culture.Length : (cfgVm is not null ? cfgVm.General.LanguageIndex : dm.Config.Options.Language) + 1;
        var timeoutSecond = allLanguages ? 50 : 3;

        bool aborted = false;
        try
        {
            for (int i = cultStart; i < cultStop; i++)
            {
                if (i < 0 || i >= Strings.Culture.Length) continue;

                taskList.Add(LeaguesUpdate(i));
                taskList.Add(FilterDataUpdates(i));
                taskList.Add(CurrencyUpdate(i));
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

            var completedTask = await Task.WhenAny(allTasks, timeoutTask).ConfigureAwait(false);

            if (completedTask == timeoutTask)
            {
                aborted = true;
            }
            else
            {
                aborted = false;
                await allTasks.ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            ErrorMsg += $"Exception while executing tasks : {ex}";
        }

        if (cfgVm is not null)
        {
            cfgVm.General.BtnUpdateEnable = true;
        }

        bool isNoError = ErrorMsg.Length is 0;
        if (isNoError)
        {
            dm.TryInit();
            cfgVm?.InitLeagueList();
        }
        var title = isNoError ? "Xiletrade : " + (aborted ? Resources.Resources.Main193_DownloadKo : Resources.Resources.Main192_DownloadOk)
        : "Xiletrade : " + Resources.Resources.Main193_DownloadKo;
        var msg = isNoError ? aborted ? Resources.Resources.Main191_FiltersKo : Resources.Resources.Main190_FiltersOk
        : cult + Resources.Resources.Main191_FiltersKo + Strings.LF + ErrorMsg;
        var type = isNoError ? aborted ? Notify.Ko : Notify.Ok : Notify.Ko;
        ErrorMsg = string.Empty;

        var action = new Action(() => { _serviceProvider.GetRequiredService<INotificationService>().Send(title, msg, type); });
        _serviceProvider.GetRequiredService<INavigationService>().DelegateActionToUiThread(action);
    }

    // all private methods
    private async Task FilterDataUpdates(int idxLang)
    {
        string urlStats = Strings.GetUpdateApi(idxLang) + "stats";
        try
        {
            string path = Path.GetFullPath("Data\\Lang\\");
            if (Uri.TryCreate(Strings.GetUpdateApi(idxLang), UriKind.Absolute, out Uri res)) // res not used
            {
                var service = _serviceProvider.GetRequiredService<NetService>();
                string sResult = await service.SendHTTP(urlStats, Client.Update);

                if (sResult.Length > 0)
                {
                    if (idxLang is 5) // portuguese
                    {
                        sResult = sResult.Replace("\\u000b", string.Empty); // remove line tabulation
                    }
                    FilterData filterInfo = _dm.Json.Deserialize<FilterData>(sResult);
                    if (filterInfo is not null)
                    {
                        if (filterInfo.Result.Length > 0) // needed for files comparison following leagues updates.
                        {
                            for (int i = 0; i < filterInfo.Result.Length; i++)
                            {
                                filterInfo.Result[i].Entries = [.. filterInfo.Result[i].Entries.OrderBy(x => x.ID)];
                            }
                        }

                        WriteFile(path, Strings.File.Filters, useLanguageFolder: true, idxLang, filterInfo);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMsg += ex.InnerException is not HttpRequestException ? Strings.LF + "[FilterData Update] Exception with Json writting : " + Common.GetInnerExceptionMessages(ex) + Strings.LF
                : idxLang != 9 ? Strings.LF + GetPoeServer(idxLang) + " Server : FilterData Update" + Strings.LF + urlStats + " : " + Common.GetInnerExceptionMessages(ex) + Strings.LF : string.Empty;
        }
    }

    private async Task LeaguesUpdate(int idxLang)
    {
        string urlStats = Strings.GetUpdateApi(idxLang) + "leagues";
        try
        {
            string path = Path.GetFullPath("Data\\Lang\\");
            if (Uri.TryCreate(Strings.GetUpdateApi(idxLang), UriKind.Absolute, out Uri res)) // res not used
            {
                var service = _serviceProvider.GetRequiredService<NetService>();
                string sResult = await service.SendHTTP(urlStats, Client.Update);

                if (sResult.Length > 0)
                {
                    LeagueData leaguesInfo = _dm.Json.Deserialize<LeagueData>(sResult);
                    if (leaguesInfo is not null)
                    {
                        List<LeagueResult> leagueList = new();
                        foreach (var league in leaguesInfo.Result)
                        {
                            var checkPresence = leagueList.Where(x => x.Id == league.Id);
                            if (!checkPresence.Any())
                            {
                                leagueList.Add(league);
                            }
                        }

                        if (leagueList.Count > 0)
                        {
                            leaguesInfo.Result = [.. leagueList];

                            WriteFile(path, Strings.File.Leagues, useLanguageFolder: true, idxLang, leaguesInfo);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMsg += ex.InnerException is not HttpRequestException ? Strings.LF + "[Leagues Update] Exception with Json writting : " + Common.GetInnerExceptionMessages(ex) + Strings.LF
                : idxLang != 9 ? Strings.LF + GetPoeServer(idxLang) + " Server : Leagues Update" + Strings.LF + urlStats + " : " + Common.GetInnerExceptionMessages(ex) + Strings.LF : string.Empty;
        }
    }

    private async Task CurrencyUpdate(int idxLang)
    {
        string urlStats = Strings.GetUpdateApi(idxLang) + "static";
        try
        {
            string path = Path.GetFullPath("Data\\Lang\\");
            if (Uri.TryCreate(Strings.GetUpdateApi(idxLang), UriKind.Absolute, out Uri res)) // res not used
            {
                var service = _serviceProvider.GetRequiredService<NetService>();
                string sResult = await service.SendHTTP(urlStats, Client.Update);

                SaveJsonToFile<CurrencyResult>(sResult, path, Strings.File.Currency, useLanguageFolder: true, idxLang: idxLang);
            }
        }
        catch (Exception ex)
        {
            ErrorMsg += ex.InnerException is not HttpRequestException ? Strings.LF + "[Currency Update] Exception with Json writting : " + Common.GetInnerExceptionMessages(ex) + Strings.LF
                : idxLang != 9 ? Strings.LF + GetPoeServer(idxLang) + " Server : Currency Update" + Strings.LF + urlStats + " : " + Common.GetInnerExceptionMessages(ex) + Strings.LF : string.Empty;
        }
    }

    private async Task GithubUpdate<T>(string fileName, int idxLang = -1) where T : class
    {
        string urlStats = Strings.UrlGithubData + 
            (idxLang < 0 ? string.Empty : "Lang\\" + Strings.Culture[idxLang] + "\\") + fileName;
        try
        {
            string path = Path.GetFullPath("Data\\");
            if (Uri.TryCreate(Strings.UrlGithubData, UriKind.Absolute, out _))
            {
                var service = _serviceProvider.GetRequiredService<NetService>();
                string json = await service.SendHTTP(urlStats, Client.GitHub);

                SaveJsonToFile<T>(json, path, fileName);
            }
        }
        catch (Exception ex)
        {
            if (ex.InnerException is not HttpRequestException)
            {
                ErrorMsg += $"{Strings.LF}[{fileName} Update] Exception with Json writting : {Common.GetInnerExceptionMessages(ex)}{Strings.LF}";
            }
        }
    }

    private static string GetPoeServer(int idxLang)
    {
        return idxLang is 9 ? "TENCENT" : idxLang is 8 ? "GARENA" : "GLOBAL";
    }

    private void SaveJsonToFile<T>(ReadOnlySpan<char> json, string path, string fileName, 
        bool useLanguageFolder = false, int idxLang = -1) where T : class
    {
        if (json.IsEmpty)
            return;

        var data = _dm.Json.Deserialize<T>(json);
        if (data is null)
            return;

        WriteFile(path, fileName, useLanguageFolder, idxLang, data);
    }

    private void WriteFile<T>(string path, string fileName, bool useLanguageFolder, int idxLang, T data) where T : class
    {
        // Create the main directory
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        // Create the language subdirectory if necessary
        if (useLanguageFolder && idxLang >= 0 && idxLang < Strings.Culture.Length)
        {
            path = Path.Combine(path, Strings.Culture[idxLang]);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        string filePath = Path.Combine(path, fileName);

        // Serialize and write the file
        using StreamWriter writer = new(filePath, false, Encoding.UTF8);
        writer.Write(_dm.Json.Serialize<T>(data));
    }
}
