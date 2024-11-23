using Avalonia.Controls;
using System.Collections.Generic;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Compression;
using System.IO;
using System.Runtime.InteropServices;
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

namespace Xiletrade.Updater.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private static string _installText = "Extracting files...";
    [ObservableProperty]
    private static string _currentFile = string.Empty;
    [ObservableProperty]
    private static int _progressB = 0;

    private const int MAX_RETRIES = 2;
    private static byte[]? _configFile;
    private static string? _configFilePath;
    private static Task? _extractTask;
    private static CancellationTokenSource? _extractTokenSource;
    private static readonly StringBuilder _logBuilder = new();
    private static readonly string _extracting = "Extracting {0}";
    //private static readonly string _fileStillInUseCaption = "Unable to update the file!";
    //private static readonly string _fileStillInUseMessage = "{0} is still open and it is using \"{1}\". Please close the process manually and press Retry.";
    private static readonly List<string> _listFilesToSkip = new(){ "Update.exe", "av_libglesv2.dll", "libHarfBuzzSharp.dll", "libSkiaSharp.dll" };

    [RelayCommand]
    private void Launch()
    {
        _extractTask = ExtractFiles();
        //Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => ExtractFiles(), Avalonia.Threading.DispatcherPriority.ContextIdle);
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
                var opts = new SharpCompress.Readers.ReaderOptions();
                if (args.Length >= 4)
                {
                    opts.Password = args[3];
                }

                SevenZipArchive archive = SevenZipArchive.Open(args[1], opts);
                var entries = archive.Entries;
                _logBuilder.AppendLine($"Found total of {entries.Count} files and folders inside the 7z archive.");

                try
                {
                    LoadConfig();
                    CleanInstallFolder(Directory.GetCurrentDirectory());

                    reader = archive.ExtractAllEntries();

                    int count = archive.Entries.Count;
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
                }
            }
        }, token);

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

    private static void CleanInstallFolder(string folder)
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
            dir.Delete(true);
        }
        //_extractTask?.IsCompletedSuccessfully
        _logBuilder.AppendLine("Install folder successfully cleaned : " + folder);
    }

    private static void CloseApplication()
    {
        if (App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime) lifetime.Shutdown();
    }
}
