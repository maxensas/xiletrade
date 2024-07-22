using System;
using System.Diagnostics;
using System.Globalization;
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
                && !sbMessage.ToString().Contains(innerException.Message, StringComparison.Ordinal))
            {
                sbMessage.AppendLine().Append(innerException.Message);
            }
            innerException = innerException.InnerException;
            watchdog++;
        }
        while (innerException is not null && watchdog <= 20);

        return sbMessage.ToString();
    }

    internal static string TranslateCurrency(string currency)
    {
        if (DataManager.Config.Options.Language > 0)
        {
            var cur =
            from result in DataManager.Currencies
            from Entrie in result.Entries
            where Entrie.Text == currency
            select Entrie.ID;
            if (cur.Any())
            {
                string id = cur.First();
                var cur2 =
                from result in DataManager.CurrenciesEn
                from Entrie in result.Entries
                where Entrie.ID == cur.First()
                select Entrie.Text;
                if (cur2.Any())
                {
                    return cur2.First();
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
        //string old = Process.GetCurrentProcess().MainModule.FileName;
        FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(Environment.ProcessPath);
        return fvi.FileVersion;
    }

    internal static double StrToDouble(string str, bool useEmptyfield = false)
    {
        double value = useEmptyfield ? Modifier.EMPTYFIELD : 0;
        if (str?.Length > 0)
        {
            try
            {
                value = double.Parse(str, CultureInfo.InvariantCulture); // correction
            }
            catch (Exception)
            {
                //Helper.Debug.Trace("Exception using double parsing : " + ex.Message);
            }
        }
        return value;
    }

    internal static Uri GetCurrencyImageUri(string curName, string tier)
    {
        string id = string.Empty;
        foreach (CurrencyResultData resDat in DataManager.Currencies)
        {
            if (resDat.Label is null)
            {
                continue;
            }
            foreach (CurrencyEntrie entrie in resDat.Entries)
            {
                if (entrie.Text == curName) id = resDat.ID;
                if (entrie.Text == curName && entrie.Img?.ToString().Length > 0)
                {
                    return new Uri(Strings.Cdn.Url + entrie.Img.ToString());
                }
            }
        }
        if (id.Length > 0)
        {
            if (id is Strings.CurrencyType.Cards)
            {
                return new Uri(Strings.Cdn.Cards);
            }
            if (id is Strings.CurrencyType.Prophecies)
            {
                return new Uri(Strings.Cdn.Prophecies);
            }
            if (id is Strings.CurrencyType.MapsUnique)
            {
                return new Uri(Strings.Cdn.MapsUnique);
            }
            if (id is Strings.CurrencyType.Beasts)
            {
                return new Uri(Strings.Cdn.Beasts);
            }
            if (id is Strings.CurrencyType.Heist)
            {
                return new Uri(Strings.Cdn.Heist);
            }
            if (id is Strings.CurrencyType.Sanctum)
            {
                return new Uri(Strings.Cdn.Sanctum);
            }
            if (id is Strings.CurrencyType.ScoutingReport)
            {
                return new Uri(Strings.Cdn.ScoutingReport);
            }
            if (id.Contains(Strings.Maps, StringComparison.Ordinal)
                && int.TryParse(tier, out int valTier))
            {
                string url;
                if (id is Strings.CurrencyType.MapsBlighted)
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

    internal static string GetPoeChatCommand(int index)
    {
        if (index >= 0 && index < Strings.Feature.ChatCommands.Count)
        {
            return "/" + Strings.Feature.ChatCommands[index];
        }
        return string.Empty;
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
