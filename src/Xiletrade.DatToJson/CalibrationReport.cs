using System.Globalization;
using System.Text;

namespace Xiletrade.DatToJson;

/// <summary>
/// Markdown reports, readable in a console and usable later as a pull request or issue body.
/// </summary>
internal static class CalibrationReport
{
    internal static string ForCalibration(CalibrationResult result, int game)
    {
        var sb = new StringBuilder();
        sb.Append("# PoE ").Append(game).AppendLine(" schema calibration").AppendLine();
        sb.AppendLine(result.Succeeded
            ? result.HasWarnings
                ? "Result: **schema updated, with ambiguous columns to check**"
                : "Result: **schema updated**"
            : "Result: **manual action required, nothing was written**");
        sb.AppendLine();

        foreach (var error in result.Errors)
        {
            sb.Append("- ").AppendLine(error);
        }
        if (result.Errors.Count > 0)
        {
            sb.AppendLine();
        }

        sb.AppendLine("| Table | Row length | Column | Status | Offset | Score | Notes |");
        sb.AppendLine("|---|---|---|---|---|---|---|");
        foreach (var table in result.Tables)
        {
            var rowLength = table.NewRowLength < 0 || table.NewRowLength == table.OldRowLength
                ? table.OldRowLength.ToString(CultureInfo.InvariantCulture)
                : $"{table.OldRowLength} → {table.NewRowLength}";

            if (table.Error is not null)
            {
                sb.Append("| ").Append(table.Name).Append(" | ").Append(rowLength)
                  .Append(" | | **error** | | | ").Append(table.Error).AppendLine(" |");
                continue;
            }
            foreach (var column in table.Columns)
            {
                sb.Append("| ").Append(table.Name)
                  .Append(" | ").Append(rowLength)
                  .Append(" | ").Append(column.Name)
                  .Append(" | ").Append(StatusText(column))
                  .Append(" | ").Append(OffsetText(column))
                  .Append(" | ").Append(column.Score.ToString("P1", CultureInfo.InvariantCulture))
                  .Append(" | ").Append(NotesText(column))
                  .AppendLine(" |");
            }
        }
        return sb.ToString();
    }

    /// <summary>
    /// Report of the snapshot command: the first values of each column, to check by eye that the offsets are right.
    /// </summary>
    internal static string ForSnapshot(GameSchema schema, IReadOnlyDictionary<string, DatFile> files, List<string> errors, int game)
    {
        var sb = new StringBuilder();
        sb.Append("# PoE ").Append(game).AppendLine(" schema snapshot").AppendLine();
        sb.AppendLine(errors.Count is 0 ? "Result: **snapshot written**" : "Result: **errors, nothing was written**");
        sb.AppendLine();
        foreach (var error in errors)
        {
            sb.Append("- ").AppendLine(error);
        }
        if (errors.Count > 0)
        {
            sb.AppendLine();
        }

        sb.AppendLine("| Table | Rows | Column | Offset | First values |");
        sb.AppendLine("|---|---|---|---|---|");
        foreach (var table in schema.Tables)
        {
            if (!files.TryGetValue(table.Name, out var dat))
            {
                continue;
            }
            foreach (var column in table.Columns)
            {
                sb.Append("| ").Append(table.Name)
                  .Append(" | ").Append(dat.RowCount)
                  .Append(" | ").Append(column.Name)
                  .Append(" | ").Append(column.Offset)
                  .Append(" | ").Append(FirstValues(dat, column, 3))
                  .AppendLine(" |");
            }
        }
        return sb.ToString();
    }

    private static string StatusText(ColumnResult column) => column.Status switch
    {
        ColumnStatus.Unchanged => "unchanged",
        ColumnStatus.Moved => "**moved**",
        ColumnStatus.Failed => "**FAILED**",
        _ => column.Status.ToString()
    };

    private static string OffsetText(ColumnResult column) => column.Status switch
    {
        ColumnStatus.Moved => $"{column.OldOffset} → {column.NewOffset}",
        ColumnStatus.Failed => $"{column.OldOffset} → ?",
        _ => column.OldOffset.ToString(CultureInfo.InvariantCulture)
    };

    private static string NotesText(ColumnResult column)
    {
        var sb = new StringBuilder(column.Message);
        if (column.Alternatives.Count > 0)
        {
            if (sb.Length > 0)
            {
                sb.Append(' ');
            }
            sb.Append(column.Status is ColumnStatus.Failed ? "Best candidates: " : "Ambiguous, also matching: ");
            for (var i = 0; i < column.Alternatives.Count; i++)
            {
                var candidate = column.Alternatives[i];
                if (i > 0)
                {
                    sb.Append(", ");
                }
                sb.Append(candidate.Offset).Append(" (")
                  .Append(candidate.Score.ToString("P1", CultureInfo.InvariantCulture)).Append(')');
            }
        }
        return sb.ToString();
    }

    private static string FirstValues(DatFile dat, ColumnSchema column, int count)
    {
        var sb = new StringBuilder();
        var found = 0;
        for (var row = 0; row < dat.RowCount && found < count; row++)
        {
            var text = ReadDisplayValue(dat, row, column);
            if (text is null)
            {
                continue;
            }
            if (found > 0)
            {
                sb.Append(", ");
            }
            // Keep the markdown table valid.
            sb.Append('`').Append(text.Replace('|', '/').Replace('\n', ' ')).Append('`');
            found++;
        }
        return sb.ToString();
    }

    /// <summary>Value as text, or null when it is empty or invalid.</summary>
    private static string? ReadDisplayValue(DatFile dat, int row, ColumnSchema column)
    {
        switch (column.Type)
        {
            case ColumnType.String:
                return dat.TryReadString(row, column.Offset, out var text) && !text.IsEmpty
                    ? new string(text)
                    : null;
            case ColumnType.Int32:
                return dat.ReadInt32(row, column.Offset).ToString(CultureInfo.InvariantCulture);
            case ColumnType.ForeignRow:
                return dat.TryReadForeignRow(row, column.Offset, out var index) && index is not DatFile.NullRow
                    ? "row " + index.ToString(CultureInfo.InvariantCulture)
                    : null;
            default:
                return null;
        }
    }
}
