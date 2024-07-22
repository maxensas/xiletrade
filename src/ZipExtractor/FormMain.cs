using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using ZipExtractor.Properties;
using SharpCompress;
using SharpCompress.Readers;
using SharpCompress.Common;
using SharpCompress.Archives.Rar;
using SharpCompress.Archives.SevenZip;

namespace ZipExtractor
{
    public partial class FormMain : Form
    {
        private const int MaxRetries = 2;
        private static byte[] configFile;
        private static string configFilePath;
        private BackgroundWorker _backgroundWorker;
        private readonly StringBuilder _logBuilder = new StringBuilder();

        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Shown(object sender, EventArgs e)
        {
            _logBuilder.AppendLine(DateTime.Now.ToString("F"));
            _logBuilder.AppendLine();
            _logBuilder.AppendLine("Xiletrade updater (ZipExtractor) started with following command line arguments.");

            string[] args = Environment.GetCommandLineArgs();
            for (var index = 0; index < args.Length; index++)
            {
                var arg = args[index];
                _logBuilder.AppendLine($"[{index}] {arg}");
            }

            _logBuilder.AppendLine();

            if (args.Length >= 4)
            {
                string executablePath = args[3];

                // Extract all the files.
                _backgroundWorker = new BackgroundWorker
                {
                    WorkerReportsProgress = true,
                    WorkerSupportsCancellation = true
                };

                _backgroundWorker.DoWork += (_, eventArgs) =>
                {
                    foreach (var process in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(executablePath)))
                    {
                        try
                        {
                            if (process.MainModule is {FileName: { }} && process.MainModule.FileName.Equals(executablePath))
                            {
                                _logBuilder.AppendLine("Waiting for application process to exit...");

                                _backgroundWorker.ReportProgress(0, "Waiting for application to exit...");
                                process.WaitForExit();
                            }
                        }
                        catch (Exception exception)
                        {
                            Debug.WriteLine(exception.Message);
                        }
                    }

                    _logBuilder.AppendLine("BackgroundWorker started successfully.");

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
                        MessageBox.Show("The installation folder must be named 'Xiletrade' in order to update the application properly", "Bad installation folder name",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    if (args[1].ToLower().EndsWith(".7z"))
                    {
                        IReader reader = null;
                        var opts = new SharpCompress.Readers.ReaderOptions();
                        if (args.Length >= 4)
                        {
                            opts.Password = args[3];
                        }

                        SevenZipArchive archive = SevenZipArchive.Open(args[1], opts);
                        var entries = archive.Entries;
                        _logBuilder.AppendLine($"Found total of {entries.Count} files and folders inside the 7z file.");

                        try
                        {
                            LoadConfig();
                            CleanInstallFolder(Directory.GetCurrentDirectory());

                            reader = archive.ExtractAllEntries();
                            
                            int count = archive.Entries.Count;
                            int index = 0;
                            int progress = 0;
                            string currentFile = "";
                            while (reader.MoveToNextEntry())
                            {
                                if (!reader.Entry.IsDirectory)
                                {
                                    if (_backgroundWorker.CancellationPending)
                                    {
                                        eventArgs.Cancel = true;
                                        return;
                                    }

                                    currentFile = string.Format(Resources.CurrentFileExtracting, reader.Entry.ToString());
                                    _backgroundWorker.ReportProgress(progress, currentFile);

                                    if (Path.GetFileName(reader.Entry.ToString()).Equals(Path.GetFileName(args[0]))) // Skip Update.exe file
                                    {
                                        progress = (index + 1) * 100 / count;
                                        index++;
                                        _logBuilder.AppendLine($"Skipping {reader.Entry} [{progress}%]");
                                        continue;
                                    }
                                    
                                    reader.WriteEntryToDirectory(path, new ExtractionOptions()
                                    {
                                        ExtractFullPath = true,
                                        Overwrite = true
                                    });

                                    progress = (index + 1) * 100 / count;
                                    _backgroundWorker.ReportProgress(progress, currentFile);
                                    _logBuilder.AppendLine($"{currentFile} [{progress}%]");

                                    index++;
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            _logBuilder.AppendLine();
                            _logBuilder.AppendLine(exception.ToString());

                            MessageBox.Show(exception.Message, exception.GetType().ToString(),
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        finally
                        {
                            archive.Dispose();
                        }
                    }
                    else
                    {
                        var archive = ZipFile.OpenRead(args[1]);
                        var entries = archive.Entries;
                        _logBuilder.AppendLine($"Found total of {entries.Count} files and folders inside the zip file.");

                        try
                        {
                            LoadConfig();
                            CleanInstallFolder(Directory.GetCurrentDirectory());

                            int progress = 0;
                            for (var index = 0; index < entries.Count; index++)
                            {
                                if (_backgroundWorker.CancellationPending)
                                {
                                    eventArgs.Cancel = true;
                                    break;
                                }

                                var entry = entries[index];

                                string currentFile = string.Format(Resources.CurrentFileExtracting, entry.FullName);
                                _backgroundWorker.ReportProgress(progress, currentFile);
                                int retries = 0;
                                bool notCopied = true;
                                while (notCopied)
                                {
                                    string filePath = String.Empty;
                                    try
                                    {
                                        filePath = Path.Combine(path, entry.FullName);
                                        if (!entry.IsDirectory())
                                        {
                                            var parentDirectory = Path.GetDirectoryName(filePath);
                                            if (!Directory.Exists(parentDirectory))
                                            {
                                                Directory.CreateDirectory(parentDirectory);
                                            }
                                            entry.ExtractToFile(filePath, true);
                                        }
                                        notCopied = false;
                                    }
                                    catch (IOException exception)
                                    {
                                        const int errorSharingViolation = 0x20;
                                        const int errorLockViolation = 0x21;
                                        var errorCode = Marshal.GetHRForException(exception) & 0x0000FFFF;
                                        if (errorCode == errorSharingViolation || errorCode == errorLockViolation)
                                        {
                                            retries++;
                                            if (retries > MaxRetries)
                                            {
                                                throw;
                                            }

                                            List<Process> lockingProcesses = null;
                                            if (Environment.OSVersion.Version.Major >= 6 && retries >= 2)
                                            {
                                                try
                                                {
                                                    lockingProcesses = FileUtil.WhoIsLocking(filePath);
                                                }
                                                catch (Exception)
                                                {
                                                    // ignored
                                                }
                                            }

                                            if (lockingProcesses == null)
                                            {
                                                Thread.Sleep(5000);
                                            }
                                            else
                                            {
                                                foreach (var lockingProcess in lockingProcesses)
                                                {
                                                    var dialogResult = MessageBox.Show(
                                                        string.Format(Resources.FileStillInUseMessage,
                                                            lockingProcess.ProcessName, filePath),
                                                        Resources.FileStillInUseCaption,
                                                        MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
                                                    if (dialogResult == DialogResult.Cancel)
                                                    {
                                                        throw;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            throw;
                                        }
                                    }
                                }

                                progress = (index + 1) * 100 / entries.Count;
                                _backgroundWorker.ReportProgress(progress, currentFile);

                                _logBuilder.AppendLine($"{currentFile} [{progress}%]");
                            }
                        }
                        finally
                        {
                            archive.Dispose();
                        }
                    }
                };

                _backgroundWorker.ProgressChanged += (_, eventArgs) =>
                {
                    progressBar.Value = eventArgs.ProgressPercentage;
                    textBoxInformation.Text = eventArgs.UserState?.ToString();
                    if (textBoxInformation.Text != null)
                    {
                        textBoxInformation.SelectionStart = textBoxInformation.Text.Length;
                        textBoxInformation.SelectionLength = 0;
                    }
                };

                _backgroundWorker.RunWorkerCompleted += (_, eventArgs) =>
                {
                    try
                    {
                        if (eventArgs.Error != null)
                        {
                            throw eventArgs.Error;
                        }

                        if (!eventArgs.Cancelled)
                        {
                            textBoxInformation.Text = @"Finished";
                            try
                            {
                                _logBuilder.AppendLine("Application files extracted successfully.");

                                if (File.Exists(args[1]))
                                {
                                    File.Delete(args[1]);
                                    _logBuilder.AppendLine("Update file deleted successfully : " + args[1]);
                                }

                                SaveBackupConfig();

                                string[] files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.tmp");

                                if (files != null)
                                {
                                    for (int i=0; i < files.Length ;i++)
                                    {
                                        File.Delete(files[i]);
                                        _logBuilder.AppendLine("Temp file deleted successfully : " + files[i]);
                                    }
                                }
                                
                                ProcessStartInfo processStartInfo = new ProcessStartInfo(executablePath);
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

                                MessageBox.Show(exception.Message, exception.GetType().ToString(),
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        _logBuilder.AppendLine();
                        _logBuilder.AppendLine(exception.ToString());

                        MessageBox.Show(exception.Message, exception.GetType().ToString(),
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        _logBuilder.AppendLine();
                        Application.Exit();
                    }
                };

                _backgroundWorker.RunWorkerAsync();
            }
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            _backgroundWorker?.CancelAsync();

            _logBuilder.AppendLine();
            File.AppendAllText(Path.Combine(Directory.GetCurrentDirectory(), "Update.log"), _logBuilder.ToString());
            // AppDomain.CurrentDomain.BaseDirectory // Give Temp directory
        }

        private void LoadConfig()
        {
            string[] files = Directory.GetFiles(Directory.GetCurrentDirectory() + "\\Data", "Config.json");

            if (files != null)
            {
                for (int i = 0; i < files.Length; i++) // only Config.json , not DefaultConfig.json
                {
                    configFile = File.ReadAllBytes(files[i]);
                    configFilePath = files[i];
                }
            }
        }

        private void SaveBackupConfig()
        {
            if (configFile.Length > 0 && configFilePath.Length > 0)
            {
                if (!File.Exists(configFilePath))
                {
                    File.WriteAllBytes(configFilePath, configFile);
                    _logBuilder.AppendLine("Config file saved successfully : " + configFilePath);
                }
                
                string suffix = ".bak";
                File.WriteAllBytes(configFilePath + suffix, configFile);
                _logBuilder.AppendLine("Config file backup saved successfully : " + configFilePath + suffix);
            }
        }

        private void CleanInstallFolder(string folder)
        {
            _logBuilder.AppendLine("Cleaning install folder : " + folder);
            DirectoryInfo di = new DirectoryInfo(folder);

            foreach (FileInfo file in di.GetFiles())
            {
                if (!file.Name.Contains("Update.exe") && !file.Name.Contains(".7z") && !file.Name.Contains(".zip"))
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
            _logBuilder.AppendLine("Install folder successfully cleaned : " + folder);
        }
    }
}
