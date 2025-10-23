using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Xiletrade.Library.Models.Poe.Contract;
using Xiletrade.Library.Services;

namespace Xiletrade.Library.Shared;

/// <summary>Static class containing all common methods used by Xiletrade.</summary>
internal static class Common
{
    internal static string GetInnerExceptionMessages(Exception exp)
    {
        //string message = string.Empty;
        StringBuilder sbMessage = new();
        Exception innerException = exp;
        int watchdog = 0;
        do
        {
            if (!string.IsNullOrEmpty(innerException.Message)
                && !sbMessage.ToString().Contain(innerException.Message))
            {
                sbMessage.AppendLine().Append(innerException.Message);
            }
            innerException = innerException.InnerException;
            watchdog++;
        }
        while (innerException is not null && watchdog <= 20);

        return sbMessage.ToString();
    }

    internal static string GetAppName()
    {
        string exeName = AppDomain.CurrentDomain.FriendlyName;
        return exeName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
            ? Path.GetFileNameWithoutExtension(exeName.Replace(".vshost", ""))
            : exeName;
    }

    internal static string GetHash(string valString)
    {
        var bytes = Encoding.UTF8.GetBytes(valString);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash)[..16].ToLower(); // 16 hex chars
    }

    internal static string TranslateCurrency(DataManagerService dm, string currency)
    {
        if (dm.Config.Options.Language > 0)
        {
            var cur = dm.Currencies.SelectMany(result => result.Entries)
                .FirstOrDefault(e => e.Text == currency);
            if (cur is not null)
            {
                var cur2 = dm.CurrenciesEn.SelectMany(result => result.Entries)
                    .FirstOrDefault(e => e.Id == cur.Id);
                if (cur2 is not null)
                {
                    return cur2.Text;
                }
            }
        }
        return currency;
    }

    //not used
    /*
    internal static bool IsAdministrator()
    {
#if Windows
        var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
#endif
    }
    */

    internal static void CollectGarbage()
    {
        GC.Collect(); // find finalizable objects
        GC.WaitForPendingFinalizers(); // wait until finalizers executed
        GC.Collect(); // collect finalized objects

        GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
        GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
    }

    internal static string GetFileVersion()
    {
        return FileVersionInfo.GetVersionInfo(Environment.ProcessPath).FileVersion;
    }

    internal static Uri GetCurrencyImageUri(DataManagerService dm, string curName, string tier)
    {
        string id = string.Empty;
        foreach (var resDat in dm.Currencies)
        {
            if (resDat.Label is null)
            {
                continue;
            }
            foreach (var entrie in resDat.Entries)
            {
                if (entrie.Text == curName) id = resDat.Id;
                if (entrie.Text == curName && entrie.Img?.ToString().Length > 0)
                {
                    return new Uri(Strings.Cdn.Url + entrie.Img.ToString());
                }
            }
        }
        if (id.Length > 0)
        {
            if (id is Strings.CurrencyTypePoe1.Cards)
            {
                return new Uri(Strings.Cdn.Cards);
            }
            if (id is Strings.CurrencyTypePoe1.Prophecies)
            {
                return new Uri(Strings.Cdn.Prophecies);
            }
            if (id is Strings.CurrencyTypePoe1.MapsUnique)
            {
                return new Uri(Strings.Cdn.MapsUnique);
            }
            if (id is Strings.CurrencyTypePoe1.Beasts)
            {
                return new Uri(Strings.Cdn.Beasts);
            }
            if (id is Strings.CurrencyTypePoe1.Heist)
            {
                return new Uri(Strings.Cdn.Heist);
            }
            if (id is Strings.CurrencyTypePoe1.Sanctum)
            {
                return new Uri(Strings.Cdn.Sanctum);
            }
            if (id is Strings.CurrencyTypePoe1.ScoutingReport)
            {
                return new Uri(Strings.Cdn.ScoutingReport);
            }
            if (id.Contain(Strings.Maps)
                && int.TryParse(tier, out int valTier))
            {
                string url;
                if (id is Strings.CurrencyTypePoe1.MapsBlighted)
                {
                    url = valTier <= 5 ? Strings.Cdn.MapsBlightedWhite
                        : valTier <= 10 ? Strings.Cdn.MapsBlightedYellow
                        : Strings.Cdn.MapsBlightedRed;
                    return new Uri(url);
                }
                url = valTier <= 5 ? Strings.Cdn.MapsWhite
                        : valTier <= 10 ? Strings.Cdn.MapsYellow
                        : Strings.Cdn.MapsRed;
                return new Uri(url);
            }
        }
        return null;
    }

    internal static Uri GetCurrencyImageUri(DataManagerService dm, string curId)
    {
        foreach (CurrencyResultData resDat in dm.Currencies)
        {
            if (resDat.Label is null)
            {
                continue;
            }
            foreach (CurrencyEntrie entrie in resDat.Entries)
            {
                if (entrie.Id == curId && entrie.Img?.ToString().Length > 0)
                {
                    return new Uri(Strings.Cdn.Url + entrie.Img.ToString());
                }
            }
        }
        return null;
    }

    internal static void Retry(Action action, ushort attempts, ushort delayMs)
    {
        for (int i = 0; i < attempts; i++)
        {
            try
            {
                action();
                return;
            }
            catch (ExternalException)
            {
                Thread.Sleep(delayMs);
            }
        }

        throw new Exception("Operation failed after multiple attempts.");
    }

    internal static T Retry<T>(Func<T> func, ushort attempts, ushort delayMs)
    {
        for (int i = 0; i < attempts; i++)
        {
            try
            {
                return func();
            }
            catch (ExternalException)
            {
                Thread.Sleep(delayMs);
            }
        }

        throw new Exception("Operation failed after multiple attempts.");
    }
}
