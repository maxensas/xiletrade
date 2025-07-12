using System;
using System.Diagnostics;
using System.Text;
using Xiletrade.Library.Models.Serializable;
using System.Linq;
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

    //not used
    internal static void CollectGarbage()
    {
        GC.Collect(); // find finalizable objects
        GC.WaitForPendingFinalizers(); // wait until finalizers executed
        GC.Collect(); // collect finalized objects
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

    /*
    private static T FindChild<T>(DependencyObject parent, string childName) where T : DependencyObject
    {
        // Confirm parent and childName are valid. 
        if (parent == null) return null;

        T foundChild = null;

        int childrenCount = System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < childrenCount; i++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
            // If the child is not of the request child type child
            T childType = child as T;
            if (childType == null)
            {
                // recursively drill down the tree
                foundChild = FindChild<T>(child, childName);

                // If the child is found, break so we do not overwrite the found child. 
                if (foundChild != null) break;
            }
            else if (!string.IsNullOrEmpty(childName))
            {
                var frameworkElement = child as FrameworkElement;
                // If the child's name is set for search
                if (frameworkElement != null && frameworkElement.Name == childName)
                {
                    // if the child's name is of the request name
                    foundChild = (T)child;
                    break;
                }
            }
            else
            {
                // child element found.
                foundChild = (T)child;
                break;
            }
        }

        return foundChild;
    }
    */
}
