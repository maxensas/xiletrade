using System;
using System.Diagnostics;
using System.IO;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Xiletrade.Library.Models.Poe.Contract.Extension;
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
            var cur = dm.Currencies.FindEntryByType(currency);
            if (cur is not null)
            {
                var cur2 = dm.CurrenciesEn.FindEntryById(cur.Id);
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
        var (Entry, GroupId) = dm.Currencies.FindEntryAndGroupIdByType(curName);
        if (GroupId.Length is 0)
        {
            return null;
        }
        if (Entry is not null)
        {
            return new(Strings.Cdn.Url + Entry.Img);
        }

        if (Strings.dicCurrencyCdnById.TryGetValue(GroupId, out string cdn))
            return new(cdn);

        if (GroupId.Contain(Strings.Maps)
            && int.TryParse(tier, out int valTier))
        {
            string url;
            if (GroupId is Strings.CurrencyTypePoe1.MapsBlighted)
            {
                url = valTier <= 5 ? Strings.Cdn.MapsBlightedWhite
                    : valTier <= 10 ? Strings.Cdn.MapsBlightedYellow
                    : Strings.Cdn.MapsBlightedRed;
                return new(url);
            }
            url = valTier <= 5 ? Strings.Cdn.MapsWhite
                    : valTier <= 10 ? Strings.Cdn.MapsYellow
                    : Strings.Cdn.MapsRed;
            return new(url);
        }
        return null;
    }

    internal static Uri GetCurrencyImageUri(DataManagerService dm, string curId)
    {
        var (Entry, _) = dm.Currencies.FindEntryAndGroupIdByCurId(curId);
        return Entry is null ? null : new(Strings.Cdn.Url + Entry.Img);
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
