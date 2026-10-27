using System.Text;
using System.Text.Json;

namespace Xiletrade.DatToJson;

/// <summary>
/// Non interactive commands, usable from a script or a GitHub workflow.
/// </summary>
/// <remarks>
/// snapshot : rewrites the snapshot with the schema as is (first use, or after a manual fix of the schema).
/// calibrate: finds the used columns again after a game update, then rewrites the schema and the snapshot.
/// export   : writes the json files of every language with the schema as is, after comparing them with the previous ones.
/// update   : calibrate, then export when the calibration succeeded.
/// fullupdate: every step in one command: snapshot when there is none yet, calibrate, export,
///             and the differences of the json files written in the report.
/// Game files come either from a local Content.ggpk (--ggpk) or from the patch CDN (--cdn).
/// --game all runs the command for PoE 1 then PoE 2, with one report and one diff.
/// </remarks>
internal static class Cli
{
    internal const int Success = 0;
    internal const int ManualActionRequired = 1;
    internal const int Error = 2;

    private const string Snapshot = "snapshot";
    private const string Calibrate = "calibrate";
    private const string Export = "export";
    private const string Update = "update";
    private const string FullUpdate = "fullupdate";

    /// <summary>Value of --cdn asking the patch server for the url of the current game version.</summary>
    private const string CurrentVersion = "current";

    internal static async Task<int> RunAsync(string[] args)
    {
        if (!CliOptions.TryParse(args, out var options, out var parseError))
        {
            Console.Error.WriteLine(parseError);
            PrintUsage();
            return Error;
        }

        if (!OozLibrary.TryRegister())
        {
            Console.Error.WriteLine($"Native decompressor not found: {OozLibrary.FilePath}");
            Console.Error.WriteLine("It is built with CMake by the first build of the project: build it again without -p:SkipOozBuild=true (see README).");
            return Error;
        }

        var report = new StringBuilder();
        var code = Success;
        using (var diff = OpenDiff(options))
        {
            // Each game is run even when the previous one failed: the exit code is the worst one.
            foreach (var game in options.Games)
            {
                code = Math.Max(code, await RunGameAsync(game, options, diff, report));
            }
        }

        if (options.ReportPath is not null)
        {
            File.WriteAllText(options.ReportPath, report.ToString());
            Console.WriteLine($"Report written : {options.ReportPath}");
        }
        if (options.DiffPath is not null && WritesJson(options.Command))
        {
            Console.WriteLine($"Diff written   : {options.DiffPath}");
        }
        return code;
    }

    private static bool WritesJson(string command) => command is Export or Update or FullUpdate;

    /// <summary>The differences of the json files, for the commands that write them.</summary>
    private static FileStream? OpenDiff(CliOptions options)
        => options.DiffPath is not null && WritesJson(options.Command)
            ? new FileStream(options.DiffPath, FileMode.Create, FileAccess.Write, FileShare.Read, bufferSize: 1 << 16)
            : null;

    private static async Task<int> RunGameAsync(int game, CliOptions options, Stream? diff, StringBuilder report)
    {
        var gameReport = new StringBuilder();
        // Differences of this game: copied to the diff file, and into the report for fullupdate.
        using var gameDiff = WritesJson(options.Command) && (diff is not null || options.Command is FullUpdate) ? new MemoryStream() : null;
        var code = Error;
        try
        {
            var schemaPath = options.GetSchemaPath(game);
            var schema = SchemaFile.Load(schemaPath);
            if (schema.Game != game)
            {
                gameReport.Append("# PoE ").Append(game).AppendLine(" error").AppendLine()
                    .Append(schemaPath).Append(" is a PoE ").Append(schema.Game).AppendLine(" schema.");
                return Error;
            }

            using (var source = await OpenSourceAsync(options, game))
            {
                Console.WriteLine($"PoE {game} game files : {source.Description}");
                code = options.Command switch
                {
                    Snapshot => RunSnapshot(schema, source, options, game, gameReport),
                    Calibrate => RunCalibrate(schema, source, options, game, gameReport, out _),
                    Export => RunExport(schema, source, options, gameDiff, gameReport),
                    Update => RunUpdate(schema, source, options, game, gameDiff, gameReport),
                    _ => RunFullUpdate(schema, source, options, game, gameDiff, gameReport)
                };
                gameReport.AppendLine().Append("Game files: ").AppendLine(source.Description);
            }

            if (gameDiff is not null && diff is not null)
            {
                gameDiff.Position = 0;
                gameDiff.CopyTo(diff);
            }

            // The json files are written: the downloaded bundles are no longer needed.
            if (WritesJson(options.Command) && options.CdnUrl is not null && options.IsDefaultCache)
            {
                EmptyCache(options.GetCachePath(game));
            }
            return code;
        }
        catch (Exception ex) when (ex is IOException or InvalidDataException or UnauthorizedAccessException or JsonException
            or HttpRequestException or System.Net.Sockets.SocketException)
        {
            Console.Error.WriteLine($"PoE {game}: {ex.Message}");
            gameReport.AppendLine().Append("# PoE ").Append(game).AppendLine(" error").AppendLine().AppendLine(ex.Message);
            return Error;
        }
        finally
        {
            Console.WriteLine();
            Console.WriteLine(gameReport);
            report.Append(gameReport);
            // The differences can be long: only in the report file, not in the console.
            // "No change" is only stated when the export ran (not after a calibration that needs a manual action).
            if (options.Command is FullUpdate && gameDiff is not null && (gameDiff.Length > 0 || code is Success))
            {
                AppendDiff(report, gameDiff, game);
            }
            report.AppendLine();
        }
    }

    /// <summary>Differences of the json files of one game, as a diff block of the markdown report.</summary>
    private static void AppendDiff(StringBuilder report, MemoryStream gameDiff, int game)
    {
        report.AppendLine().Append("## PoE ").Append(game).AppendLine(" json diff").AppendLine();
        if (gameDiff.Length is 0)
        {
            report.AppendLine("No change.");
            return;
        }

        var text = Encoding.UTF8.GetString(gameDiff.GetBuffer(), 0, (int)gameDiff.Length);
        // The fence must be longer than any backtick sequence of the content.
        var fence = text.Contains("```", StringComparison.Ordinal) ? "``````" : "```";
        report.Append(fence).AppendLine("diff").Append(text);
        if (!text.EndsWith('\n'))
        {
            report.AppendLine();
        }
        report.AppendLine(fence);
    }

    /// <summary>
    /// Every step: snapshot when the game has none yet (first run), then calibrate and export.
    /// </summary>
    private static int RunFullUpdate(GameSchema schema, IDatSource source, CliOptions options, int game, Stream? diff, StringBuilder report)
    {
        if (!File.Exists(options.GetSnapshotPath(game)))
        {
            report.Append("No snapshot for PoE ").Append(game).AppendLine(": created from the schema as is, check the first values below.").AppendLine();
            var code = RunSnapshot(schema, source, options, game, report);
            if (code is not Success)
            {
                return code;
            }
            report.AppendLine();
        }
        return RunUpdate(schema, source, options, game, diff, report);
    }

    private static async Task<IDatSource> OpenSourceAsync(CliOptions options, int game)
    {
        if (options.GgpkPath is not null)
        {
            return new GgpkDatSource(options.GgpkPath);
        }

        var cdnUrl = options.CdnUrl;
        if (cdnUrl is CurrentVersion)
        {
            Console.WriteLine($"Asking the PoE {game} patch server for the current version...");
            cdnUrl = await CdnDatSource.GetCurrentCdnUrlAsync(game);
        }
        var cachePath = options.GetCachePath(game);
        Console.WriteLine($"Download cache : {cachePath}");
        return new CdnDatSource(cdnUrl!, cachePath);
    }

    /// <summary>Deletes the default download cache, once the source that used it is disposed.</summary>
    private static void EmptyCache(string cachePath)
    {
        try
        {
            if (Directory.Exists(cachePath))
            {
                Directory.Delete(cachePath, recursive: true);
                Console.WriteLine($"Download cache emptied : {cachePath}");
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Console.WriteLine($"Download cache not emptied ({ex.Message}): {cachePath}");
        }
    }

    private static int RunSnapshot(GameSchema schema, IDatSource source, CliOptions options, int game, StringBuilder report)
    {
        var files = ReadEnglishFiles(schema, source);
        List<string> errors = [];
        var snapshot = SchemaCalibrator.BuildSnapshot(schema, files, errors);
        report.Append(CalibrationReport.ForSnapshot(schema, files, errors, game));

        if (snapshot is null)
        {
            return ManualActionRequired;
        }
        var snapshotPath = options.GetSnapshotPath(game);
        snapshot.Save(snapshotPath);
        Console.WriteLine($"Snapshot written : {snapshotPath}");
        return Success;
    }

    /// <param name="calibrated">The updated schema, null when nothing was written.</param>
    private static int RunCalibrate(GameSchema schema, IDatSource source, CliOptions options, int game,
        StringBuilder report, out GameSchema? calibrated)
    {
        calibrated = null;
        var snapshotPath = options.GetSnapshotPath(game);
        if (!File.Exists(snapshotPath))
        {
            report.Append("# PoE ").Append(game).AppendLine(" schema calibration").AppendLine()
                .Append("Snapshot not found: ").AppendLine(snapshotPath)
                .AppendLine("Create it first with the snapshot command, on a game version the schema is right for (see README).");
            return Error;
        }
        var reference = DatSnapshot.Load(snapshotPath);
        var files = ReadEnglishFiles(schema, source);

        var result = new SchemaCalibrator(new CalibrationOptions()).Calibrate(schema, reference, files);
        report.Append(CalibrationReport.ForCalibration(result, game));

        if (!result.Succeeded)
        {
            return ManualActionRequired;
        }
        calibrated = result.Schema!;
        var schemaPath = options.GetSchemaPath(game);
        SchemaFile.Save(calibrated, schemaPath);
        result.Snapshot!.Save(snapshotPath);
        Console.WriteLine($"Schema written   : {schemaPath}");
        Console.WriteLine($"Snapshot written : {snapshotPath}");
        return Success;
    }

    private static int RunExport(GameSchema schema, IDatSource source, CliOptions options, Stream? diff, StringBuilder report)
    {
        Console.WriteLine($"Output : {options.OutputPath}");
        var changes = new JsonChanges(diff);
        var result = new JsonExporter(schema, source, options.OutputPath, changes).Export();

        report.AppendLine().Append("# PoE ").Append(schema.Game).AppendLine(" json export").AppendLine();
        report.Append("Files generated: **").Append(result.Files.Count).AppendLine("**").AppendLine();
        foreach (var warning in result.Warnings)
        {
            report.Append("- ").AppendLine(warning);
        }
        changes.AppendReport(report, options.Command is FullUpdate ? "below" : options.DiffPath is null ? null : $"`{options.DiffPath}`");

        return result.Files.Count > 0 ? Success : ManualActionRequired;
    }

    private static int RunUpdate(GameSchema schema, IDatSource source, CliOptions options, int game, Stream? diff, StringBuilder report)
    {
        var code = RunCalibrate(schema, source, options, game, report, out var calibrated);
        if (code is not Success || calibrated is null)
        {
            report.AppendLine().AppendLine("Calibration needs a manual action: nothing was exported.");
            return code;
        }
        return RunExport(calibrated, source, options, diff, report);
    }

    /// <summary>Reads the english dat file of every table of the schema.</summary>
    private static Dictionary<string, DatFile> ReadEnglishFiles(GameSchema schema, IDatSource source)
    {
        var files = new Dictionary<string, DatFile>(schema.Tables.Count, StringComparer.Ordinal);
        foreach (var table in schema.Tables)
        {
            var path = GameData.GetDatPath(schema.Game, GameData.English, table.Name);
            var data = source.Read(path);
            if (data is null)
            {
                Console.WriteLine($"Not found in game files : {path}");
                continue;
            }
            files.Add(table.Name, DatFile.Parse(data));
        }
        return files;
    }

    private static void PrintUsage()
    {
        Console.Error.WriteLine("Usage: Xiletrade.DatToJson <snapshot|calibrate|export|update|fullupdate> --game <1|2|all>");
        Console.Error.WriteLine("         (--ggpk <Content.ggpk> | --cdn <current|url> [--cache <directory>])");
        Console.Error.WriteLine("         [--schemas <directory> | --schema <file> --snapshot <file>]");
        Console.Error.WriteLine("         [--output <directory>] [--report <file.md>] [--diff <file.diff>]");
        Console.Error.WriteLine("--game all: PoE 1 then PoE 2, with --cdn current and --schemas only.");
        Console.Error.WriteLine("--diff: differences between the previous and the new json files, one entry per line (export, update, fullupdate).");
        Console.Error.WriteLine("fullupdate: snapshot if missing, calibrate, export; the differences are written in the report (--report required).");
        Console.Error.WriteLine("Default --schemas: the Schemas folder of the project (poe<game>.schema.json, poe<game>.snapshot.bin).");
        Console.Error.WriteLine("Default --output: Data/Lang next to the executable.");
        Console.Error.WriteLine("Default download cache: bin/ooz/temp/poe<game> of the project, emptied after export and update.");
    }

    private sealed class CliOptions
    {
        internal string Command { get; private init; } = string.Empty;
        internal int[] Games { get; private init; } = [];
        internal string? GgpkPath { get; private init; }
        internal string? CdnUrl { get; private init; }
        internal string OutputPath { get; private init; } = string.Empty;
        internal string? ReportPath { get; private init; }
        internal string? DiffPath { get; private init; }

        /// <summary>The cache was not given with --cache: it is emptied after export and update.</summary>
        internal bool IsDefaultCache { get; private init; }

        private string? _cachePath;
        private string? _schemaPath;
        private string? _snapshotPath;
        private string _schemasDirectory = string.Empty;

        internal string GetCachePath(int game) => _cachePath ?? Path.Combine(GetDefaultCacheRoot(), $"poe{game}");
        internal string GetSchemaPath(int game) => _schemaPath ?? Path.Combine(_schemasDirectory, $"poe{game}.schema.json");
        internal string GetSnapshotPath(int game) => _snapshotPath ?? Path.Combine(_schemasDirectory, $"poe{game}.snapshot.bin");

        internal static bool TryParse(string[] args, out CliOptions options, out string error)
        {
            options = new CliOptions();
            error = string.Empty;
            if (args.Length is 0 || args[0] is not (Snapshot or Calibrate or Export or Update or FullUpdate))
            {
                error = $"The first argument must be '{Snapshot}', '{Calibrate}', '{Export}', '{Update}' or '{FullUpdate}'.";
                return false;
            }

            int[]? games = null;
            string? ggpk = null, cdn = null, cache = null, schemas = null, schema = null, snapshot = null,
                output = null, report = null, diff = null;
            for (var i = 1; i < args.Length; i += 2)
            {
                if (i + 1 >= args.Length)
                {
                    error = $"Missing value for {args[i]}.";
                    return false;
                }
                var value = args[i + 1];
                switch (args[i])
                {
                    case "--game":
                        games = value switch
                        {
                            "1" => [1],
                            "2" => [2],
                            "all" => [1, 2],
                            _ => null
                        };
                        if (games is null)
                        {
                            error = "--game must be 1, 2 or all.";
                            return false;
                        }
                        break;
                    case "--ggpk": ggpk = value; break;
                    case "--cdn": cdn = value; break;
                    case "--cache": cache = value; break;
                    case "--schemas": schemas = value; break;
                    case "--schema": schema = value; break;
                    case "--snapshot": snapshot = value; break;
                    case "--output": output = value; break;
                    case "--report": report = value; break;
                    case "--diff": diff = value; break;
                    default:
                        error = $"Unknown option {args[i]}.";
                        return false;
                }
            }

            if (games is null)
            {
                error = "--game is required.";
                return false;
            }
            if ((ggpk is null) == (cdn is null))
            {
                error = "Give either --ggpk or --cdn.";
                return false;
            }
            if (cdn is not null && cdn is not CurrentVersion && !cdn.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                && !cdn.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            {
                error = $"--cdn must be '{CurrentVersion}' or a CDN url.";
                return false;
            }
            if (games.Length > 1 && (ggpk is not null || cdn is not CurrentVersion))
            {
                error = $"--game all needs --cdn {CurrentVersion}: a Content.ggpk or a CDN url is for one game only.";
                return false;
            }
            if (args[0] is FullUpdate && report is null)
            {
                error = $"{FullUpdate} writes the differences of the json files in the report: --report is required.";
                return false;
            }
            if (games.Length > 1 && (schema is not null || snapshot is not null))
            {
                error = "--game all takes the schemas and snapshots of both games from a folder: use --schemas, or nothing for the default one.";
                return false;
            }

            options = new CliOptions
            {
                Command = args[0],
                Games = games,
                GgpkPath = ggpk is null ? null : Path.GetFullPath(ggpk),
                CdnUrl = cdn,
                IsDefaultCache = cache is null,
                OutputPath = Path.GetFullPath(output ?? Path.Combine(AppContext.BaseDirectory, "Data", "Lang")),
                ReportPath = report is null ? null : Path.GetFullPath(report),
                DiffPath = diff is null ? null : Path.GetFullPath(diff),
                _cachePath = cache is null ? null : Path.GetFullPath(cache),
                _schemaPath = schema is null ? null : Path.GetFullPath(schema),
                _snapshotPath = snapshot is null ? null : Path.GetFullPath(snapshot),
                _schemasDirectory = Path.GetFullPath(schemas ?? GetDefaultSchemasDirectory())
            };
            return true;
        }

        /// <summary>
        /// Schemas folder of the project, given by the build (RuntimeHostConfigurationOption in the project file):
        /// the versioned files are read and updated in place. Otherwise the copy next to the executable.
        /// </summary>
        private static string GetDefaultSchemasDirectory()
            => AppContext.GetData(SchemasDirectoryKey) as string is { Length: > 0 } configured
                ? configured
                : Path.Combine(AppContext.BaseDirectory, "Schemas");

        private const string SchemasDirectoryKey = "Xiletrade.DatToJson.SchemasDirectory";

        /// <summary>
        /// bin/ooz/temp of the project, given by the build (RuntimeHostConfigurationOption in the project file),
        /// otherwise a temp folder next to the executable.
        /// </summary>
        private static string GetDefaultCacheRoot()
            => AppContext.GetData(CacheDirectoryKey) as string is { Length: > 0 } configured
                ? configured
                : Path.Combine(AppContext.BaseDirectory, "temp");

        private const string CacheDirectoryKey = "Xiletrade.DatToJson.CacheDirectory";
    }
}
