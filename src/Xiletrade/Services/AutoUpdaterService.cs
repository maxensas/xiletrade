using AutoUpdaterDotNET;
using System;
using System.IO;
using System.Net;
using System.Windows;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;

namespace Xiletrade.Services;

public sealed class AutoUpdaterService : IAutoUpdaterService
{
    private static bool firstCheck = true;
    private static bool eventSet = false;

    public AutoUpdaterService()
    {
        AutoUpdater.DownloadPath = Environment.CurrentDirectory;
        // AutoUpdater.RunUpdateAsAdmin = false;

        // Application folder should be used in archive : 'Xiletrade' atm
        var currentDirectory = new DirectoryInfo(Environment.CurrentDirectory);
        if (currentDirectory.Parent is not null) AutoUpdater.InstallationPath = currentDirectory.Parent.FullName;
    }

    public void CheckUpdate()
    {
        if (firstCheck)
        {
            firstCheck = false;
            AutoUpdater.Start(Strings.UrlGithubVersion);
            return;
        }
        if (!eventSet) // will not show original window (form) again.
        {
            AutoUpdater.CheckForUpdateEvent += AutoUpdaterOnCheckForUpdateEvent;
            eventSet = true;
        }
        AutoUpdater.Start(Strings.UrlGithubVersion);
    }

    private static void AutoUpdaterOnCheckForUpdateEvent(UpdateInfoEventArgs args)
    {
        if (args.Error is null)
        {
            if (args.IsUpdateAvailable)
            {
                MessageBoxResult dialogResult;
                if (args.Mandatory.Value)
                {
                    dialogResult =
                        MessageBox.Show($@"{Library.Resources.Resources.Update001_NewVersion} {args.CurrentVersion} {Library.Resources.Resources.Update002_Available} {args.InstalledVersion}. {Library.Resources.Resources.Update003_Required}",
                            $@"{Library.Resources.Resources.Update007_TitleAvailable}",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    dialogResult =
                        MessageBox.Show($@"{Library.Resources.Resources.Update001_NewVersion} {args.CurrentVersion} {Library.Resources.Resources.Update002_Available} {args.InstalledVersion}. {Library.Resources.Resources.Update004_Want}",
                            $@"{Library.Resources.Resources.Update007_TitleAvailable}",
                            MessageBoxButton.YesNo, MessageBoxImage.Information);
                }

                // Uncomment the following line if you want to show standard update dialog instead.
                // AutoUpdater.ShowUpdateForm(args);

                if (dialogResult.Equals(MessageBoxResult.Yes) || dialogResult.Equals(MessageBoxResult.OK))
                {
                    try
                    {
                        if (AutoUpdater.DownloadUpdate(args))
                        {
                            Application.Current.Shutdown();
                        }
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show(exception.Message,
                            exception.GetType().ToString(),
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show($@"{Library.Resources.Resources.Update005_NoUpdate}",
                    $@"{Library.Resources.Resources.Update008_TitleNoUpdate}",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        else
        {
            if (args.Error is WebException)
            {
                MessageBox.Show($@"{Library.Resources.Resources.Update006_Error}",
                    $@"{Library.Resources.Resources.Update009_TitleError}",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                MessageBox.Show(args.Error.Message,
                    args.Error.GetType().ToString(),
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
