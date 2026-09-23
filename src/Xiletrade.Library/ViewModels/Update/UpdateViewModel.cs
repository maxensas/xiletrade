using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xiletrade.Library.Models.GitHub.Contract;
using Xiletrade.Library.Services;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Services.Interface.View;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.ViewModels.Update;

public sealed partial class UpdateViewModel(IUpdateDownloader downloader, 
    IMessageAdapterService message, UIService ui, GitHubRelease release) : ViewModelBase
{
    private readonly IUpdateDownloader _downloader = downloader;
    private readonly IMessageAdapterService _message = message;
    private readonly GitHubRelease _release = release;
    private readonly UIService _ui = ui;

    [ObservableProperty]
    private string releaseName = $"{Resources.Resources.Update001_NewVersion} : {release.TagName}";

    [ObservableProperty]
    private string releaseNotes = release.Body;

    [ObservableProperty]
    private string releaseNotesUrl = release.HtmlUrl ?? "https://github.com";

    [ObservableProperty]
    private DownloadStatusViewModel status = new();

    [RelayCommand]
    private static void Skip(object commandParameter)
    {
        if (commandParameter is IViewBase view)
        {
            view.Close();
        }
    }

    [RelayCommand]
    private async Task DownloadAsync(object commandParameter)
    {
        try
        {
            Status.DownloadStarted = true;
            var archiveFile = await _downloader.DownloadAndExtractUpdateAsync(_release, Status);
            //var archiveFile = _downloader.ExtractUpdate(_release); //test without DL
            ExtractAndLaunchUpdater(commandParameter, archiveFile);
        }
        catch (Exception ex)
        {
            _message.Show(ex.GetFormated(), "Failed to update", MessageStatus.Error);
        }
    }

    private void ExtractAndLaunchUpdater(object commandParameter, string archiveFile)
    {
        if (_downloader.DownloadPath is null || archiveFile is null)
        {
            _message.Show("No update asset found.", "Update", MessageStatus.Error);
            return;
        }
        var exe = Directory.GetFiles(_downloader.DownloadPath, _downloader.ListUpdaterFiles[0], SearchOption.AllDirectories).FirstOrDefault();
        if (exe is null)
        {
            _message.Show("Update extracted, but no .exe found.", "Update", MessageStatus.Warning);
            return;
        }

        //string installerPath = Path.Combine(Path.GetDirectoryName(archiveFile) ?? throw new InvalidOperationException(), _downloader.ListUpdaterFiles[0]);
        string executablePath = Process.GetCurrentProcess().MainModule?.FileName;
        string extractionPath = Path.GetDirectoryName(executablePath);

        if (!string.IsNullOrEmpty(_downloader.InstallationPath) &&
            Directory.Exists(_downloader.InstallationPath))
        {
            extractionPath = _downloader.InstallationPath;
        }

        if (extractionPath.EndsWith('\\')) // for windows root case
        {
            extractionPath = extractionPath.TrimEnd('\\');
        }

        StringBuilder arguments =
        new($"\"{archiveFile}\" \"{extractionPath}\" \"{executablePath}\"");
        string[] args = Environment.GetCommandLineArgs();

        for (int i = 1; i < args.Length; i++)
        {
            if (i is 1)
            {
                arguments.Append(" \"");
            }
            arguments.Append(args[i]);
            arguments.Append(i < (args.Length - 1) ? ' ' : '\"');
        }

        var processStartInfo = new ProcessStartInfo
        {
            FileName = exe,
            UseShellExecute = true,
            Arguments = arguments.ToString(),
            Verb = "runas" // RunUpdateAsAdmin
        };

        try
        {
            Process.Start(processStartInfo);
        }
        catch (Win32Exception)
        {
            throw;
        }
        finally
        {
            if (commandParameter is IViewBase view)
            {
                view.Close();
            }
            _ui.ShutDownXiletrade();
        }
    }
}
