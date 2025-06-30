using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xiletrade.Library.Models.Serializable;
using Xiletrade.Library.Models.Enums;
using Xiletrade.Library.Services.Interface;

namespace Xiletrade.Library.ViewModels;

public sealed partial class UpdateViewModel : ViewModelBase
{
    private static IServiceProvider _serviceProvider;
    private readonly GitHubRelease _release;
    private readonly IUpdateDownloader _downloader;
    private readonly IMessageAdapterService _message;

    [ObservableProperty]
    private string releaseName = "";

    [ObservableProperty]
    private string releaseNotes = "";

    [ObservableProperty]
    private string releaseNotesUrl;

    [ObservableProperty]
    private DownloadStatusViewModel status = new();

    public UpdateViewModel(GitHubRelease release, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _downloader = _serviceProvider.GetRequiredService<IUpdateDownloader>();
        _message = _serviceProvider.GetRequiredService<IMessageAdapterService>();
        _release = release;

        releaseName = $"{Resources.Resources.Update001_NewVersion} : {_release.TagName}"; //: {_release.Name}
        releaseNotes = _release.Body; // can be used if webview2 ask too much ressources
        releaseNotesUrl = release.HtmlUrl ?? "https://github.com"; // fallback
    }

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
            _message.Show($"Failed to update: {ex.Message}", "Error", MessageStatus.Error);
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
            _serviceProvider.GetRequiredService<INavigationService>().ShutDownXiletrade();
        }
    }
}
