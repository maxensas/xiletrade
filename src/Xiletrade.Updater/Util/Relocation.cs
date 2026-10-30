using SharpCompress.Archives.SevenZip;
using SharpCompress.Common;
using SharpCompress.Readers;
using System;
using System.Diagnostics;
using System.IO;

namespace Xiletrade.Updater.Util;

/// <summary>
/// The updater must not run from the installation folder it updates : its own files would be locked.
/// Launched from elsewhere than <see cref="Folder"/> (Update.exe extracted and run in place by the application, any release),
/// it extracts its files (exe + native libraries) from the archive into <see cref="Folder"/>, runs this copy and exits.
/// </summary>
/// <remarks>
/// Called before any Avalonia / SkiaSharp call : the native libraries are not loaded during this step.
/// The folder is deleted by the application at its next launch.
/// </remarks>
internal static class Relocation
{
    internal static readonly string Folder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Xiletrade", "Updater");

    private static readonly string[] _files = ["Update.exe", "av_libglesv2.dll", "libHarfBuzzSharp.dll", "libSkiaSharp.dll"];

    /// <summary>True when this process runs from <see cref="Folder"/> : no installed file is locked.</summary>
    internal static bool IsRelocated { get; } = string.Equals(
        Path.GetDirectoryName(Environment.ProcessPath), Folder, StringComparison.OrdinalIgnoreCase);

    /// <param name="args">Command line arguments : the archive is the first one.</param>
    /// <returns>True when the relocated copy has been started : this process must exit.
    /// False : the updater runs in place, as before (its own files are skipped).</returns>
    internal static bool TryRelaunch(string[] args)
    {
        if (IsRelocated || args.Length < 1
            || !args[0].EndsWith(".7z", StringComparison.OrdinalIgnoreCase) || !File.Exists(args[0]))
        {
            return false;
        }
        try
        {
            if (Directory.Exists(Folder))
            {
                Directory.Delete(Folder, true);
            }
            Directory.CreateDirectory(Folder);
            if (!ExtractUpdater(args[0]))
            {
                return false; // archive without the complete updater
            }

            var startInfo = new ProcessStartInfo(Path.Combine(Folder, _files[0]))
            {
                UseShellExecute = false, // same (elevated) token : no new UAC prompt
                WorkingDirectory = Environment.CurrentDirectory // the updater works on its current directory : installation folder
            };
            foreach (var arg in args)
            {
                startInfo.ArgumentList.Add(arg);
            }
            using var process = Process.Start(startInfo);
            return process is not null;
        }
        catch (Exception)
        {
            return false; // a previous copy still in use, I/O error... : run in place
        }
    }

    private static bool ExtractUpdater(string archiveFile)
    {
        using var archive = SevenZipArchive.OpenArchive(archiveFile);
        using IReader reader = archive.ExtractAllEntries();
        var remaining = _files.Length;
        while (remaining > 0 && reader.MoveToNextEntry())
        {
            if (reader.Entry.IsDirectory || !IsUpdaterFile(Path.GetFileName(reader.Entry.ToString().AsSpan())))
            {
                continue;
            }
            reader.WriteEntryToDirectory(Folder, new ExtractionOptions()
            {
                ExtractFullPath = false,
                Overwrite = true
            });
            remaining--;
        }
        return remaining is 0;
    }

    private static bool IsUpdaterFile(ReadOnlySpan<char> name)
    {
        foreach (var file in _files)
        {
            if (name.Equals(file, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }
}
