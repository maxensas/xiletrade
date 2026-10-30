using System.Collections.Generic;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using SharpCompress.Readers;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Common;
using Avalonia.Controls.ApplicationLifetimes;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading.Tasks;
using System.Linq;
using Xiletrade.Updater.Util;

namespace Xiletrade.Updater.ViewModels;

// TO REFACTOR
public partial class MainViewModel : ViewModelBase
{
    private static byte[]? _configFile;
    private static string? _configFilePath;
    private static CancellationTokenSource? _extractTokenSource;
    // the update is not done by this process (relaunched as administrator, or cancelled for lack of rights)
    private static bool _stopped;
    // current version moved aside during the update : restored if the update fails
    private const string BACKUP_FOLDER = ".update_backup";
    private static readonly List<(string Source, string Target)> _movedFiles = new();
    private static readonly List<string> _removedFolders = new();
    private static readonly StringBuilder _logBuilder = new();
    private static readonly string _extracting = "Extracting {0}";
    // Run from %LOCALAPPDATA% (see Util/Relocation.cs) : no installed file is locked, none is skipped.
    // Run in place (relocation failed) : its own files are locked, they are skipped as before.
    private static readonly List<string> _listFilesToSkip = Relocation.IsRelocated
        ? new()
        : new() { "Update.exe", "av_libglesv2.dll", "libHarfBuzzSharp.dll", "libSkiaSharp.dll" };

    [ObservableProperty]
    private string installText = "Extracting files...";

    [ObservableProperty]
    private string currentFile = string.Empty;

    [ObservableProperty]
    private int progressB = 0;

    [RelayCommand]
    private void Launch()
    {
        _ = ExtractFiles();
    }

    [RelayCommand]
    private static void Close()
    {
        _extractTokenSource?.Cancel();
        _logBuilder.AppendLine();
        File.AppendAllText(Path.Combine(Directory.GetCurrentDirectory(), "Update.log"), _logBuilder.ToString());
    }

    private async Task ExtractFiles()
    {
        _extractTokenSource = new();
        CancellationToken token = _extractTokenSource.Token;
        _extractTokenSource.CancelAfter(10000);
        token.ThrowIfCancellationRequested();

        _logBuilder.AppendLine(DateTime.Now.ToString("F"));
        _logBuilder.AppendLine();
        _logBuilder.AppendLine("Xiletrade updater started with following command line arguments.");

        string[] args = Environment.GetCommandLineArgs();
        for (var index = 0; index < args.Length; index++)
        {
            var arg = args[index];
            _logBuilder.AppendLine($"[{index}] {arg}");
        }

        if (args.Length < 4)
        {
            InstallText = "No files extracted.";
            ProgressB = 100;
            var box = MessageBoxManager
                .GetMessageBoxStandard("Application launched with missing arguments",
                _logBuilder.ToString(), ButtonEnum.Ok, Icon.Info);
            var result = await box.ShowAsync();
            CloseApplication();
            return;
        }

        _logBuilder.AppendLine();
        string executablePath = args[3];

        await Task.Run(async () =>
        {
            foreach (var process in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(executablePath)))
            {
                try
                {
                    if (process.MainModule is { FileName: { } } && process.MainModule.FileName.Equals(executablePath))
                    {
                        _logBuilder.AppendLine("Waiting for application process to exit...");

                        ProgressB = 0;
                        CurrentFile = "Waiting for application to exit...";
                        process.WaitForExit();
                    }
                }
                catch (Exception exception)
                {
                    Debug.WriteLine(exception.Message);
                }
            }

            var path = args[2];

            // Ensures that the last character on the extraction path
            // is the directory separator char.
            // Without this, a malicious zip file could try to traverse outside of the expected
            // extraction path.
            if (!path.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
            {
                path += Path.DirectorySeparatorChar;
            }

            var dirName = new DirectoryInfo(Directory.GetCurrentDirectory()).Name;
            if (!dirName.Equals("Xiletrade"))
            {
                var box = MessageBoxManager
                .GetMessageBoxStandard("Bad installation folder name",
                "The installation folder must be named 'Xiletrade' in order to update the application properly", ButtonEnum.Ok, Icon.Info);
                var result = await box.ShowAsync();
                return;
            }

            if (args[1].ToLower().EndsWith(".7z"))
            {
                IReader? reader = null;

                ReaderOptions opts = args.Length >= 4 ? new() { Password = args[3] } : new();
                var archive = SevenZipArchive.OpenArchive(args[1], opts);
                var entries = archive.Entries;
                _logBuilder.AppendLine($"Found total of {entries.Count()} files and folders inside the 7z archive.");

                var folder = Directory.GetCurrentDirectory();
                var backup = Path.Combine(folder, BACKUP_FOLDER);
                var extracting = false;
                var extracted = false;
                try
                {
                    LoadConfig();
                    CurrentFile = "Moving the current version aside...";
                    if (!BackupInstallFolder(folder, backup))
                    {
                        await HandleMissingRightsAsync(executablePath, args);
                        return;
                    }

                    extracting = true;
                    reader = archive.ExtractAllEntries();

                    int count = archive.Entries.Count();
                    int index = 0;
                    ProgressB = 0;
                    CurrentFile = string.Empty;

                    while (reader.MoveToNextEntry())
                    {
                        if (!reader.Entry.IsDirectory)
                        {
                            if (token.IsCancellationRequested)
                            {
                                return;
                            }
                            var readerStr = reader.Entry.ToString();
                            CurrentFile = string.Format(_extracting, readerStr);
                            //_listFilesToSkip.Contains(file.Name)
                            // if (readerStr is not null && Path.GetFileName(readerStr).Equals(Path.GetFileName(args[0])))
                            if (readerStr is not null && _listFilesToSkip.Contains(Path.GetFileName(readerStr)))
                            {
                                ProgressB = (index + 1) * 100 / count;
                                index++;
                                _logBuilder.AppendLine($"Skipping {reader.Entry} [{ProgressB}%]");
                                continue;
                            }

                            reader.WriteEntryToDirectory(path, new ExtractionOptions()
                            {
                                ExtractFullPath = true,
                                Overwrite = true
                            });

                            ProgressB = (index + 1) * 100 / count;
                            _logBuilder.AppendLine($"{CurrentFile} [{ProgressB}%]");

                            index++;
                        }
                    }
                    extracted = true;
                }
                catch (Exception exception)
                {
                    _logBuilder.AppendLine();
                    _logBuilder.AppendLine(exception.ToString());

                    var box = MessageBoxManager.GetMessageBoxStandard(exception.GetType().ToString(), exception.Message, ButtonEnum.Ok, Icon.Error);
                    var result = await box.ShowAsync();
                }
                finally
                {
                    archive.Dispose();
                    if (extracted)
                    {
                        DeleteBackup(backup);
                    }
                    else
                    {
                        RollbackUpdate(folder, backup, extracting); // nothing to do when the backup failed and was restored
                    }
                }
            }
        }, token);

        if (_stopped)
        {
            CloseApplication();
            return;
        }

        try
        {
            if (!token.IsCancellationRequested)
            {
                InstallText = @"Finished";
                ProgressB = 100;
                try
                {
                    _logBuilder.AppendLine("Application files extracted successfully.");

                    if (File.Exists(args[1]))
                    {
                        File.Delete(args[1]);
                        _logBuilder.AppendLine("Updater binary file deleted successfully : " + args[1]);
                    }

                    SaveBackupConfig();

                    string[] files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.tmp");

                    if (files != null)
                    {
                        for (int i = 0; i < files.Length; i++)
                        {
                            File.Delete(files[i]);
                            _logBuilder.AppendLine("Temp file deleted successfully : " + files[i]);
                        }
                    }

                    _logBuilder.AppendLine("Starting process from path : " + executablePath);
                    ProcessStartInfo processStartInfo = new(executablePath);
                    if (args.Length > 4)
                    {
                        processStartInfo.Arguments = args[4];
                    }

                    Process.Start(processStartInfo);

                    _logBuilder.AppendLine("Successfully launched the updated application.");
                }
                catch (Win32Exception exception)
                {
                    if (exception.NativeErrorCode != 1223)
                    {
                        throw;
                    }
                }
                catch (Exception exception)
                {
                    _logBuilder.AppendLine();
                    _logBuilder.AppendLine(exception.ToString());

                    var box = MessageBoxManager.GetMessageBoxStandard(exception.GetType().ToString(), exception.Message, ButtonEnum.Ok, Icon.Error);
                    var result = await box.ShowAsync();
                }
            }
        }
        catch (Exception exception)
        {
            _logBuilder.AppendLine();
            _logBuilder.AppendLine(exception.ToString());

            var box = MessageBoxManager.GetMessageBoxStandard(exception.GetType().ToString(), exception.Message, ButtonEnum.Ok, Icon.Error);
            var result = await box.ShowAsync();
        }
        finally
        {
            _logBuilder.AppendLine();
        }
        CloseApplication();
    }

    private static void LoadConfig()
    {
        string[] files = Directory.GetFiles(Directory.GetCurrentDirectory() + "\\Data", "Config.json");

        if (files != null)
        {
            for (int i = 0; i < files.Length; i++) // only Config.json , not DefaultConfig.json
            {
                _configFile = File.ReadAllBytes(files[i]);
                _configFilePath = files[i];
            }
        }
    }

    private static void SaveBackupConfig()
    {
        if (_configFile?.Length > 0 && _configFilePath?.Length > 0)
        {
            if (!File.Exists(_configFilePath))
            {
                File.WriteAllBytes(_configFilePath, _configFile);
                _logBuilder.AppendLine("Config file saved successfully : " + _configFilePath);
            }

            string suffix = ".bak";
            File.WriteAllBytes(_configFilePath + suffix, _configFile);
            _logBuilder.AppendLine("Config file backup saved successfully : " + _configFilePath + suffix);
        }
    }

    /// <summary>
    /// Moves the current version into the backup folder (renames only : same volume). Every file is moved one by one :
    /// moving requires the same rights as deleting, so a lack of rights is detected before anything is lost.
    /// </summary>
    /// <returns>False when rights are missing : everything has been moved back.</returns>
    private static bool BackupInstallFolder(string folder, string backup)
    {
        _logBuilder.AppendLine("Moving current version to : " + backup);
        try
        {
            if (Directory.Exists(backup))
            {
                Directory.Delete(backup, true); // left by a previous update
            }
            Directory.CreateDirectory(backup);
            foreach (var file in Directory.GetFiles(folder))
            {
                var name = Path.GetFileName(file);
                if (name.Contains(".7z") || _listFilesToSkip.Contains(name))
                {
                    _logBuilder.AppendLine("Skipped : " + name);
                    continue;
                }
                MoveFile(file, Path.Combine(backup, name));
            }
            foreach (var directory in Directory.GetDirectories(folder))
            {
                if (string.Equals(directory, backup, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                BackupDirectory(directory, Path.Combine(backup, Path.GetFileName(directory)));
            }
            _logBuilder.AppendLine("Current version moved successfully.");
            return true;
        }
        catch (UnauthorizedAccessException exception)
        {
            _logBuilder.AppendLine("Insufficient rights : " + exception.Message);
            RestoreInstallFolder();
            return false;
        }
        catch (Exception)
        {
            RestoreInstallFolder();
            throw;
        }
    }

    private static void BackupDirectory(string source, string target)
    {
        Directory.CreateDirectory(target);
        foreach (var file in Directory.GetFiles(source))
        {
            MoveFile(file, Path.Combine(target, Path.GetFileName(file)));
        }
        foreach (var directory in Directory.GetDirectories(source))
        {
            BackupDirectory(directory, Path.Combine(target, Path.GetFileName(directory)));
        }
        Directory.Delete(source); // empty now
        _removedFolders.Add(source);
    }

    private static void MoveFile(string source, string target)
    {
        File.Move(source, target);
        _movedFiles.Add((source, target));
    }

    /// <summary>
    /// Moves back every file of the backup folder to its original place.
    /// </summary>
    private static void RestoreInstallFolder()
    {
        if (_movedFiles.Count is 0 && _removedFolders.Count is 0)
        {
            return;
        }
        _logBuilder.AppendLine("Restoring current version...");
        foreach (var directory in _removedFolders)
        {
            Directory.CreateDirectory(directory);
        }
        // removed from the list once moved back : a restore that fails can be done again from where it stopped
        for (int i = _movedFiles.Count - 1; i >= 0; i--)
        {
            File.Move(_movedFiles[i].Target, _movedFiles[i].Source, true);
            _movedFiles.RemoveAt(i);
        }
        _removedFolders.Clear();
        _logBuilder.AppendLine("Current version restored.");
    }

    /// <summary>
    /// Update failed or cancelled after the backup : the extracted files are removed, then the current version is restored.
    /// </summary>
    /// <param name="extracting">False : the extraction has not started, the folder only contains files of the current version.</param>
    private static void RollbackUpdate(string folder, string backup, bool extracting)
    {
        if (_movedFiles.Count is 0 && _removedFolders.Count is 0)
        {
            return;
        }
        try
        {
            if (extracting)
            {
                CleanInstallFolder(folder, backup);
            }
            RestoreInstallFolder();
            DeleteBackup(backup);
        }
        catch (Exception exception)
        {
            _logBuilder.AppendLine("Unable to restore the current version, it is kept in : " + backup);
            _logBuilder.AppendLine(exception.ToString());
        }
    }

    private static void DeleteBackup(string backup)
    {
        _movedFiles.Clear();
        _removedFolders.Clear();
        try
        {
            Directory.Delete(backup, true);
            _logBuilder.AppendLine("Backup folder deleted : " + backup);
        }
        catch (Exception exception)
        {
            _logBuilder.AppendLine("Unable to delete the backup folder (deleted at the next update) : " + exception.Message);
        }
    }

    /// <summary>
    /// Rights missing on the installation folder (nothing modified) : relaunched as administrator,
    /// or the current version is started again when it is not possible.
    /// </summary>
    private static async Task HandleMissingRightsAsync(string executablePath, string[] args)
    {
        _stopped = true;
        if (!Environment.IsPrivilegedProcess && Elevation.TryRelaunchElevated())
        {
            _logBuilder.AppendLine("Updater relaunched as administrator.");
            return;
        }
        _logBuilder.AppendLine("Update cancelled, nothing modified.");
        var box = MessageBoxManager.GetMessageBoxStandard("Update cancelled",
            "Administrator rights are required to update the installation folder. Nothing has been modified.",
            ButtonEnum.Ok, Icon.Error);
        await box.ShowAsync();
        try
        {
            ProcessStartInfo startInfo = new(executablePath);
            if (args.Length > 4)
            {
                startInfo.Arguments = args[4];
            }
            Process.Start(startInfo)?.Dispose();
        }
        catch (Exception exception)
        {
            _logBuilder.AppendLine(exception.ToString());
        }
    }

    // Removes what has been extracted (rollback) : everything except the archive, the updater files and the backup.
    private static void CleanInstallFolder(string folder, string backup)
    {
        _logBuilder.AppendLine("Cleaning install folder : " + folder);
        DirectoryInfo di = new(folder);

        foreach (FileInfo file in di.GetFiles())
        {
            if (!file.Name.Contains(".7z") && !_listFilesToSkip.Contains(file.Name))
            {
                file.Delete();
            }
            else
            {
                _logBuilder.AppendLine("Skipped : " + file.Name);
            }
        }
        foreach (DirectoryInfo dir in di.GetDirectories())
        {
            if (!string.Equals(dir.FullName, backup, StringComparison.OrdinalIgnoreCase))
            {
                dir.Delete(true);
            }
        }
        _logBuilder.AppendLine("Install folder successfully cleaned : " + folder);
    }

    private static void CloseApplication()
    {
        if (App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime) lifetime.Shutdown();
    }
}
