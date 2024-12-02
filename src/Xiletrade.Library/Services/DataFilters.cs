using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Linq;
using System.Text;
using Xiletrade.Library.Models.Serializable;
using Xiletrade.Library.ViewModels.Config;
using System.IO;
using Xiletrade.Library.Models.Enums;
using Xiletrade.Library.Shared;
using Microsoft.Extensions.DependencyInjection;
using Xiletrade.Library.Services.Interface;

namespace Xiletrade.Library.Services;

/// <summary>Static helper class used to update all JSON local data files used by Xiletrade.</summary>
/// <remarks>Retrieve source data from Xiletrade Github repository and GGG server.</remarks>
internal static class DataFilters
{
    private static IServiceProvider _serviceProvider;
    private static string ErrorMsg { get; set; } = string.Empty;

    internal static void Initialize(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    internal static void Update(ConfigViewModel cfgVm = null, bool allLanguages = false, bool updateGenerated = true)
    {
        Task taskMess = new(() =>
        {
            List<Task> taskList = new();
            Stopwatch stopWatch = new();
            stopWatch.Start();
            TimeSpan ts;

            if (cfgVm is not null)
            {
                cfgVm.General.BtnUpdateText = "Loading ...";
                cfgVm.General.BtnUpdateEnable = false;
            }

            var cult = cfgVm is null || allLanguages ? string.Empty : " (" + Strings.Culture[cfgVm.General.LanguageIndex] + ")";
            var cultStart = cfgVm is not null ? allLanguages ? 0 : cfgVm.General.LanguageIndex : DataManager.Config.Options.Language;
            var cultStop = allLanguages ? Strings.Culture.Length : (cfgVm is not null ? cfgVm.General.LanguageIndex : DataManager.Config.Options.Language) + 1;

            for (int i = cultStart; i < cultStop; i++)
            {
                taskList.Add(LeaguesUpdate(i));
                taskList.Add(FilterDataUpdates(i));
                taskList.Add(CurrencyUpdate(i));
                taskList.Add(RuleUpdate(i));
                if (updateGenerated)
                {
                    taskList.Add(BaseUpdate(i, Strings.File.Bases));
                    taskList.Add(BaseUpdate(i, Strings.File.Mods));
                    taskList.Add(BaseUpdate(i, Strings.File.Monsters));
                    taskList.Add(WordUpdate(i));
                    taskList.Add(GemUpdate(i));
                }
            }
            taskList.Add(DivinationUpdate());

            bool alive;
            bool aborted = false;

            do
            {
                alive = false;
                foreach (Task task in taskList)
                {
                    if (!task.IsFaulted && !task.IsCompleted && !task.IsCanceled && !task.IsCompletedSuccessfully)
                    {
                        alive = true;
                        break;
                    }
                }
                ts = stopWatch.Elapsed;
                if (ts.Minutes >= 1)
                {
                    stopWatch.Stop();
                    aborted = true;
                    alive = false; //break;
                }
            } while (alive);

            if (cfgVm is not null)
            {
                cfgVm.General.BtnUpdateText = Resources.Resources.Config010_btnUpdate;
                cfgVm.General.BtnUpdateEnable = true;
            }

            bool isNoError = ErrorMsg.Length is 0;
            if (isNoError)
            {
                DataManager.TryInit();
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
        });
        taskMess.Start();
    }

    // all private methods
    private static Task FilterDataUpdates(int idxLang)
    {
        string path = Path.GetFullPath("Data\\Lang\\");
        string urlStats = Strings.UpdateApi[idxLang] + "stats";
        Task task = null;
        if (idxLang >= 0 && idxLang < Strings.Culture.Length)
        {
            task = new Task(() =>
            {
                try
                {
                    if (Uri.TryCreate(Strings.UpdateApi[idxLang], UriKind.Absolute, out Uri res)) // res not used
                    {
                        Thread.Sleep(200);
                        var service = _serviceProvider.GetRequiredService<NetService>();
                        string sResult = service.SendHTTP(null, urlStats, Client.Update).Result;

                        if (sResult.Length > 0)
                        {
                            if (idxLang == 5) // portuguese
                            {
                                sResult = sResult.Replace("\\u000b", string.Empty); // remove line tabulation
                            }
                            FilterData filterInfo = Json.Deserialize<FilterData>(sResult);
                            if (filterInfo is not null)
                            {
                                if (filterInfo.Result.Length > 0) // needed for files comparison following leagues updates.
                                {
                                    for (int i = 0; i < filterInfo.Result.Length; i++)
                                    {
                                        filterInfo.Result[i].Entries = [.. filterInfo.Result[i].Entries.OrderBy(x => x.ID)];
                                    }
                                }

                                if (!Directory.Exists(path))
                                {
                                    Directory.CreateDirectory(path);
                                }
                                string pathLang = path + Strings.Culture[idxLang] + "\\";
                                if (!Directory.Exists(pathLang))
                                {
                                    Directory.CreateDirectory(pathLang);
                                }
                                using StreamWriter writer = new(pathLang + Strings.File.Filters, false, Encoding.UTF8);
                                writer.Write(Json.Serialize<FilterData>(filterInfo));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorMsg += ex.InnerException is not HttpRequestException ? Strings.LF + "[FilterData Update] Exception with Json writting : " + Common.GetInnerExceptionMessages(ex) + Strings.LF
                        : idxLang != 9 ? Strings.LF + GetPoeServer(idxLang) + " Server : FilterData Update" + Strings.LF + urlStats + " : " + Common.GetInnerExceptionMessages(ex) + Strings.LF : string.Empty;
                }
            });

            task.Start();
        }

        return task;
    }

    private static Task LeaguesUpdate(int idxLang)
    {
        string path = Path.GetFullPath("Data\\Lang\\");
        string urlStats = Strings.UpdateApi[idxLang] + "leagues";
        Task task = null;

        if (idxLang >= 0 && idxLang < Strings.Culture.Length)
        {
            task = new Task(() =>
            {
                try
                {
                    if (Uri.TryCreate(Strings.UpdateApi[idxLang], UriKind.Absolute, out Uri res)) // res not used
                    {
                        Thread.Sleep(200);
                        var service = _serviceProvider.GetRequiredService<NetService>();
                        string sResult = service.SendHTTP(null, urlStats, Client.Update).Result;

                        if (sResult.Length > 0)
                        {
                            LeagueData leaguesInfo = Json.Deserialize<LeagueData>(sResult);
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


                                    if (!Directory.Exists(path))
                                    {
                                        Directory.CreateDirectory(path);
                                    }

                                    string pathLang = path + Strings.Culture[idxLang] + "\\";
                                    if (!Directory.Exists(pathLang))
                                    {
                                        Directory.CreateDirectory(pathLang);
                                    }
                                    using StreamWriter writer = new(pathLang + Strings.File.Leagues, false, Encoding.UTF8);
                                    writer.Write(Json.Serialize<LeagueData>(leaguesInfo));
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

            });

            task.Start();
        }

        return task;
    }

    private static Task CurrencyUpdate(int idxLang)
    {
        string path = Path.GetFullPath("Data\\Lang\\");
        string urlStats = Strings.UpdateApi[idxLang] + "static";
        Task task = null;

        if (idxLang >= 0 && idxLang < Strings.Culture.Length)
        {
            task = new Task(() =>
            {
                try
                {
                    if (Uri.TryCreate(Strings.UpdateApi[idxLang], UriKind.Absolute, out Uri res)) // res not used
                    {
                        Thread.Sleep(200);
                        var service = _serviceProvider.GetRequiredService<NetService>();
                        string sResult = service.SendHTTP(null, urlStats, Client.Update).Result;

                        if (sResult.Length > 0)
                        {
                            CurrencyResult CurrencyInfo = Json.Deserialize<CurrencyResult>(sResult);
                            if (CurrencyInfo is not null)
                            {
                                if (!Directory.Exists(path))
                                {
                                    Directory.CreateDirectory(path);
                                }

                                string pathLang = path + Strings.Culture[idxLang] + "\\";
                                if (!Directory.Exists(pathLang))
                                {
                                    Directory.CreateDirectory(pathLang);
                                }
                                using StreamWriter writer = new(pathLang + Strings.File.Currency, false, Encoding.UTF8);
                                writer.Write(Json.Serialize<CurrencyResult>(CurrencyInfo));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorMsg += ex.InnerException is not HttpRequestException ? Strings.LF + "[Currency Update] Exception with Json writting : " + Common.GetInnerExceptionMessages(ex) + Strings.LF
                        : idxLang != 9 ? Strings.LF + GetPoeServer(idxLang) + " Server : Currency Update" + Strings.LF + urlStats + " : " + Common.GetInnerExceptionMessages(ex) + Strings.LF : string.Empty;
                }
            });
            task.Start();
        }

        return task;
    }

    private static Task BaseUpdate(int idxLang, string jsonName)
    {
        string path = Path.GetFullPath("Data\\Lang\\");
        string urlStats = Strings.UrlGithubData + "Lang\\" + Strings.Culture[idxLang] + "\\" + jsonName;
        Task task = null;

        if (idxLang >= 0 && idxLang < Strings.Culture.Length)
        {
            task = new Task(() =>
            {
                try
                {
                    if (Uri.TryCreate(Strings.UrlGithubData, UriKind.Absolute, out Uri res)) // res not used
                    {
                        Thread.Sleep(200);
                        var service = _serviceProvider.GetRequiredService<NetService>();
                        string sResult = service.SendHTTP(null, urlStats, Client.Update).Result;

                        if (sResult.Length > 0)
                        {
                            BaseData baseInfo = Json.Deserialize<BaseData>(sResult);
                            if (baseInfo is not null)
                            {
                                if (!Directory.Exists(path))
                                {
                                    Directory.CreateDirectory(path);
                                }

                                string pathLang = path + Strings.Culture[idxLang] + "\\";
                                if (!Directory.Exists(pathLang))
                                {
                                    Directory.CreateDirectory(pathLang);
                                }
                                using StreamWriter writer = new(pathLang + jsonName, false, Encoding.UTF8);
                                writer.Write(Json.Serialize<BaseData>(baseInfo));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorMsg += ex.InnerException is not HttpRequestException ? Strings.LF + "[Base Update] Exception with Json writting : " + Common.GetInnerExceptionMessages(ex) + Strings.LF
                        : idxLang != 9 ? Strings.LF + "GITHUB : Base Update" + Strings.LF + urlStats + " : " + Common.GetInnerExceptionMessages(ex) + Strings.LF : string.Empty;
                }
            });
            task.Start();
        }

        return task;
    }

    private static Task WordUpdate(int idxLang)
    {
        string path = Path.GetFullPath("Data\\Lang\\");
        string urlStats = Strings.UrlGithubData + "Lang\\" + Strings.Culture[idxLang] + "\\" + Strings.File.Words;
        Task task = null;

        if (idxLang >= 0 && idxLang < Strings.Culture.Length)
        {
            task = new Task(() =>
            {
                try
                {
                    if (Uri.TryCreate(Strings.UrlGithubData, UriKind.Absolute, out Uri res)) // res not used
                    {
                        Thread.Sleep(200);
                        var service = _serviceProvider.GetRequiredService<NetService>();
                        string sResult = service.SendHTTP(null, urlStats, Client.Update).Result;

                        if (sResult.Length > 0)
                        {
                            WordData modInfo = Json.Deserialize<WordData>(sResult);
                            if (modInfo is not null)
                            {
                                if (!Directory.Exists(path))
                                {
                                    Directory.CreateDirectory(path);
                                }

                                string pathLang = path + Strings.Culture[idxLang] + "\\";
                                if (!Directory.Exists(pathLang))
                                {
                                    Directory.CreateDirectory(pathLang);
                                }
                                using StreamWriter writer = new(pathLang + Strings.File.Words, false, Encoding.UTF8);
                                writer.Write(Json.Serialize<WordData>(modInfo));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorMsg += ex.InnerException is not HttpRequestException ? Strings.LF + "[Word Update] Exception with Json writting : " + Common.GetInnerExceptionMessages(ex) + Strings.LF
                        : idxLang != 9 ? Strings.LF + "GITHUB : Word Update" + Strings.LF + urlStats + " : " + Common.GetInnerExceptionMessages(ex) + Strings.LF : string.Empty;
                }
            });
            task.Start();
        }

        return task;
    }

    private static Task GemUpdate(int idxLang)
    {
        string path = Path.GetFullPath("Data\\Lang\\");
        string urlStats = Strings.UrlGithubData + "Lang\\" + Strings.Culture[idxLang] + "\\" + Strings.File.Gems;
        Task task = null;

        if (idxLang >= 0 && idxLang < Strings.Culture.Length)
        {
            task = new Task(() =>
            {
                try
                {
                    if (Uri.TryCreate(Strings.UrlGithubData, UriKind.Absolute, out Uri res)) // res not used
                    {
                        Thread.Sleep(200);
                        var service = _serviceProvider.GetRequiredService<NetService>();
                        string sResult = service.SendHTTP(null, urlStats, Client.Update).Result;

                        if (sResult.Length > 0)
                        {
                            GemData modInfo = Json.Deserialize<GemData>(sResult);
                            if (modInfo is not null)
                            {
                                if (!Directory.Exists(path))
                                {
                                    Directory.CreateDirectory(path);
                                }

                                string pathLang = path + Strings.Culture[idxLang] + "\\";
                                if (!Directory.Exists(pathLang))
                                {
                                    Directory.CreateDirectory(pathLang);
                                }
                                using StreamWriter writer = new(pathLang + Strings.File.Gems, false, Encoding.UTF8);
                                writer.Write(Json.Serialize<GemData>(modInfo));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorMsg += ex.InnerException is not HttpRequestException ? Strings.LF + "[Gem Update] Exception with Json writting : " + Common.GetInnerExceptionMessages(ex) + Strings.LF
                        : idxLang != 9 ? Strings.LF + "GITHUB : Gem Update" + Strings.LF + urlStats + " : " + Common.GetInnerExceptionMessages(ex) + Strings.LF : string.Empty;
                }
            });
            task.Start();
        }

        return task;
    }

    private static Task RuleUpdate(int idxLang)
    {
        string path = Path.GetFullPath("Data\\Lang\\");
        string urlStats = Strings.UrlGithubData + "Lang\\" + Strings.Culture[idxLang] + "\\" + Strings.File.ParsingRules;
        Task task = null;

        if (idxLang >= 0 && idxLang < Strings.Culture.Length)
        {
            task = new Task(() =>
            {
                try
                {
                    if (Uri.TryCreate(Strings.UrlGithubData, UriKind.Absolute, out Uri res)) // res not used
                    {
                        Thread.Sleep(200);
                        var service = _serviceProvider.GetRequiredService<NetService>();
                        string sResult = service.SendHTTP(null, urlStats, Client.Update).Result;

                        if (sResult.Length > 0)
                        {
                            ParserData rules = Json.Deserialize<ParserData>(sResult);
                            if (rules is not null)
                            {
                                if (!Directory.Exists(path))
                                {
                                    Directory.CreateDirectory(path);
                                }

                                string pathLang = path + Strings.Culture[idxLang] + "\\";
                                if (!Directory.Exists(pathLang))
                                {
                                    Directory.CreateDirectory(pathLang);
                                }
                                using StreamWriter writer = new(pathLang + Strings.File.ParsingRules, false, Encoding.UTF8);
                                writer.Write(Json.Serialize<ParserData>(rules));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorMsg += ex.InnerException is not HttpRequestException ? Strings.LF + "[Rule Update] Exception with Json writting : " + Common.GetInnerExceptionMessages(ex) + Strings.LF
                        : idxLang != 9 ? Strings.LF + "GITHUB : Rule Update" + Strings.LF + urlStats + " : " + Common.GetInnerExceptionMessages(ex) + Strings.LF : string.Empty;
                }
            });
            task.Start();
        }

        return task;
    }

    private static Task DivinationUpdate()
    {
        string path = Path.GetFullPath("Data\\");
        string urlStats = Strings.UrlGithubData + Strings.File.Divination;
        Task task = new(() =>
        {
            try
            {
                if (Uri.TryCreate(Strings.UrlGithubData, UriKind.Absolute, out Uri res)) // res not used
                {
                    Thread.Sleep(200);
                    var service = _serviceProvider.GetRequiredService<NetService>();
                    string sResult = service.SendHTTP(null, urlStats, Client.Update).Result;

                    if (sResult.Length > 0)
                    {
                        DivTiersData divInfo = Json.Deserialize<DivTiersData>(sResult);
                        if (divInfo is not null)
                        {
                            if (!Directory.Exists(path))
                            {
                                Directory.CreateDirectory(path);
                            }

                            using StreamWriter writer = new(path + Strings.File.Divination, false, Encoding.UTF8);
                            writer.Write(Json.Serialize<DivTiersData>(divInfo));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException is not HttpRequestException)
                {
                    ErrorMsg += Strings.LF + "[Divination Update] Exception with Json writting : " + Common.GetInnerExceptionMessages(ex) + Strings.LF;
                }
            }
        });
        task.Start();

        return task;
    }

    private static string GetPoeServer(int idxLang)
    {
        return idxLang is 9 ? "TENCENT" : idxLang is 8 ? "GARENA" : "GLOBAL";
    }
}
