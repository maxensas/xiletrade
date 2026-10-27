using System.Globalization;
using System.Text;

namespace Xiletrade.DatToJson;

internal enum JsonFileStatus
{
    Unchanged,
    Changed,
    Added
}

/// <param name="Path">Relative to the output folder, with '/' (e.g. "en-US/Mods.json").</param>
/// <param name="Removed">Entries removed from the previous generation.</param>
/// <param name="Added">Entries added by this generation.</param>
internal sealed record JsonFileChange(string Path, JsonFileStatus Status, int Removed, int Added);

/// <summary>
/// Writes the json files, after comparing each of them with the previous generation, before it is replaced.
/// </summary>
/// <remarks>
/// A file identical to the previous generation is not written again.
/// The differences are written to <paramref name="diff"/> in the git unified format, one entry per line (see <see cref="JsonDiff"/>).
/// </remarks>
/// <param name="diff">Where to write the differences, null to only count them.</param>
internal sealed class JsonChanges(Stream? diff)
{
    internal List<JsonFileChange> Files { get; } = [];

    /// <param name="outputDirectory">Root of the json files, for the paths shown in the diff.</param>
    /// <param name="path">File to write.</param>
    /// <param name="json">New content, UTF-8 without byte order mark: written with one, as the historical generator did.</param>
    internal void Write(string outputDirectory, string path, ReadOnlyMemory<byte> json)
    {
        var relative = Path.GetRelativePath(outputDirectory, path).Replace('\\', '/');
        var exists = File.Exists(path);
        ReadOnlyMemory<byte> previous = exists ? File.ReadAllBytes(path) : ReadOnlyMemory<byte>.Empty;
        var preamble = Encoding.UTF8.Preamble;
        var hadPreamble = previous.Span.StartsWith(preamble);
        if (hadPreamble)
        {
            previous = previous[preamble.Length..];
        }

        if (exists && hadPreamble && previous.Span.SequenceEqual(json.Span))
        {
            Files.Add(new JsonFileChange(relative, JsonFileStatus.Unchanged, 0, 0));
            return;
        }

        var stats = JsonDiff.Compare(previous, json, diff, relative, newFile: !exists);
        Files.Add(new JsonFileChange(relative, exists ? JsonFileStatus.Changed : JsonFileStatus.Added, stats.Removed, stats.Added));

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        using var stream = File.Create(path);
        stream.Write(preamble);
        stream.Write(json.Span);
    }

    /// <summary>Markdown summary of the changes, for the report.</summary>
    /// <param name="diffLocation">Where the differences are written, already formatted, null when they are not.</param>
    internal void AppendReport(StringBuilder report, string? diffLocation)
    {
        var unchanged = Files.Count(static f => f.Status is JsonFileStatus.Unchanged);
        report.AppendLine().AppendLine("### Json changes").AppendLine();
        report.Append(CultureInfo.InvariantCulture, $"Unchanged files: **{unchanged}**, changed or new files: **{Files.Count - unchanged}**.").AppendLine();
        if (diffLocation is not null)
        {
            report.Append("Differences, one entry per line: ").Append(diffLocation).AppendLine(".");
        }
        if (Files.Count == unchanged)
        {
            return;
        }

        report.AppendLine().AppendLine("| File | Status | Entries removed | Entries added |");
        report.AppendLine("|---|---|---|---|");
        foreach (var file in Files)
        {
            if (file.Status is JsonFileStatus.Unchanged)
            {
                continue;
            }
            report.Append(CultureInfo.InvariantCulture,
                $"| {file.Path} | {(file.Status is JsonFileStatus.Added ? "**new**" : "changed")} | {file.Removed} | {file.Added} |").AppendLine();
        }
    }
}
