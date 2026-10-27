namespace Xiletrade.DatToJson;

internal sealed class CalibrationOptions
{
    /// <summary>
    /// Minimum share of sampled keys of the new file that already existed in the snapshot.
    /// Lower than <see cref="ColumnThreshold"/> because a patch adds new rows.
    /// </summary>
    public double KeyThreshold { get; init; } = 0.75;

    /// <summary>Minimum share of matching values, on rows joined by key, to accept an offset.</summary>
    public double ColumnThreshold { get; init; } = 0.90;

    /// <summary>Other offsets scoring within this margin of the best one are reported as ambiguous.</summary>
    public double AmbiguityMargin { get; init; } = 0.05;

    /// <summary>Maximum number of rows read to score one offset.</summary>
    public int SampleSize { get; init; } = 4096;

    /// <summary>Minimum number of non-empty reference values needed to trust a score.</summary>
    public int MinInformativeRows { get; init; } = 10;
}

internal enum ColumnStatus
{
    Unchanged,
    Moved,
    Failed
}

internal readonly record struct CandidateScore(int Offset, double Score);

internal sealed record ColumnResult(
    string Name,
    ColumnStatus Status,
    int OldOffset,
    int NewOffset,
    double Score,
    IReadOnlyList<CandidateScore> Alternatives,
    string? Message = null)
{
    /// <summary>The column was found, but other offsets were almost as good.</summary>
    internal bool IsAmbiguous => Status is not ColumnStatus.Failed && Alternatives.Count > 0;
}

internal sealed class TableResult(string name, int oldRowLength)
{
    internal string Name { get; } = name;
    internal int OldRowLength { get; } = oldRowLength;
    internal int NewRowLength { get; set; } = -1;
    internal List<ColumnResult> Columns { get; } = [];
    internal string? Error { get; set; }

    internal bool Succeeded => Error is null && Columns.TrueForAll(static c => c.Status is not ColumnStatus.Failed);
}

internal sealed class CalibrationResult
{
    internal List<TableResult> Tables { get; } = [];
    internal List<string> Errors { get; } = [];

    /// <summary>Updated schema, only set when every table was calibrated.</summary>
    internal GameSchema? Schema { get; set; }

    /// <summary>Snapshot of the new game version, only set when every table was calibrated.</summary>
    internal DatSnapshot? Snapshot { get; set; }

    internal bool Succeeded => Schema is not null && Snapshot is not null;
    internal bool HasWarnings => Tables.Exists(static t => t.Columns.Exists(static c => c.IsAmbiguous));
}

/// <summary>
/// Finds the used columns again after a game update, by comparing the new dat files with the snapshot
/// taken on the previous version. Only offsets are searched: the types of the columns added by the
/// developers do not need to be known.
/// </summary>
/// <remarks>
/// For each table:
/// 1. The key column (e.g. Id) is located: the offset whose values best match the old keys.
/// 2. Old and new rows are joined on the key.
/// 3. Each other column is located: the offset whose values best match the old values on joined rows.
///    Columns keep their relative order, so a column is only searched after the previous one.
/// When no offset is good enough, the column is reported as failed and nothing is written:
/// the change needs a human decision (type changed, column removed, meaning changed...).
/// </remarks>
internal sealed class SchemaCalibrator(CalibrationOptions options)
{
    private const double Invalid = -1;

    private readonly record struct RowPair(int OldRow, int NewRow);

    internal CalibrationResult Calibrate(GameSchema schema, DatSnapshot reference, IReadOnlyDictionary<string, DatFile> files)
    {
        var result = new CalibrationResult();
        if (reference.Game != schema.Game)
        {
            result.Errors.Add($"The snapshot is for PoE {reference.Game}, the schema for PoE {schema.Game}.");
            return result;
        }

        var updated = schema.Clone();
        var keysByTable = new Dictionary<string, ulong[]>(StringComparer.Ordinal);

        foreach (var table in updated.Tables)
        {
            var tableResult = CalibrateTable(table, reference, files, keysByTable, out var dat);
            result.Tables.Add(tableResult);
            if (tableResult.Succeeded && dat is not null)
            {
                table.RowLength = dat.RowLength;
                keysByTable.Add(table.Name, ReadKeys(dat, table.Columns[0].Offset));
            }
        }

        if (!result.Tables.TrueForAll(static t => t.Succeeded))
        {
            return result;
        }

        var snapshot = BuildSnapshot(updated, files, result.Errors);
        if (snapshot is not null)
        {
            result.Schema = updated;
            result.Snapshot = snapshot;
        }
        return result;
    }

    private TableResult CalibrateTable(TableSchema table, DatSnapshot reference,
        IReadOnlyDictionary<string, DatFile> files, Dictionary<string, ulong[]> keysByTable, out DatFile? dat)
    {
        var result = new TableResult(table.Name, table.RowLength);
        if (!files.TryGetValue(table.Name, out dat))
        {
            result.Error = "Dat file not found.";
            return result;
        }
        result.NewRowLength = dat.RowLength;

        if (!reference.Tables.TryGetValue(table.Name, out var snapshot))
        {
            result.Error = "No reference values for this table: run the snapshot command first.";
            return result;
        }
        foreach (var column in table.Columns)
        {
            if (!snapshot.Columns.ContainsKey(column.Name))
            {
                result.Error = $"No reference values for column {column.Name}: run the snapshot command first.";
                return result;
            }
        }

        // 1. Key column: share of sampled new keys that already existed.
        // (an out parameter can not be captured by the scoring lambdas, hence this local)
        DatFile file = dat;
        var key = table.Columns[0];
        var oldKeys = snapshot.Columns[key.Name];
        var oldKeySet = new HashSet<ulong>(oldKeys);
        oldKeySet.Remove(ColumnReader.Empty);
        var sampledRows = Sample(file.RowCount, options.SampleSize);

        var keyResult = Locate(key, file, 0, options.KeyThreshold, null,
            offset => ScoreKey(file, offset, sampledRows, oldKeySet));
        result.Columns.Add(keyResult);
        if (keyResult.Status is ColumnStatus.Failed)
        {
            return result;
        }
        key.Offset = keyResult.NewOffset;

        // 2. Join old and new rows on the key.
        var pairs = MatchRows(oldKeys, ReadKeys(file, key.Offset), options.SampleSize);

        // 3. Other columns, searched after the previous one to keep their order.
        var minOffset = key.End;
        for (var i = 1; i < table.Columns.Count; i++)
        {
            var column = table.Columns[i];
            ulong[]? referencedKeys = null;
            if (column.Type is ColumnType.ForeignRow && !keysByTable.TryGetValue(column.References!, out referencedKeys))
            {
                result.Columns.Add(new ColumnResult(column.Name, ColumnStatus.Failed, column.Offset, -1, 0, [],
                    $"Referenced table {column.References} was not calibrated."));
                return result;
            }

            var oldValues = snapshot.Columns[column.Name];
            var columnResult = Locate(column, file, minOffset, options.ColumnThreshold, referencedKeys,
                offset => ScoreColumn(file, column.Type, offset, pairs, oldValues, referencedKeys));
            result.Columns.Add(columnResult);
            if (columnResult.Status is ColumnStatus.Failed)
            {
                return result;
            }
            column.Offset = columnResult.NewOffset;
            minOffset = column.End;
        }
        return result;
    }

    /// <summary>
    /// Keeps the current offset when it still scores well, otherwise tries every offset from <paramref name="minOffset"/>.
    /// The chosen offset is then checked on every row of the file.
    /// </summary>
    private ColumnResult Locate(ColumnSchema column, DatFile dat, int minOffset, double threshold,
        ulong[]? referencedKeys, Func<int, double> score)
    {
        var size = column.Type.Size();
        var oldOffset = column.Offset;

        if (oldOffset >= minOffset && oldOffset + size <= dat.RowLength)
        {
            var current = score(oldOffset);
            if (current >= threshold)
            {
                return CheckAllRows(new ColumnResult(column.Name, ColumnStatus.Unchanged, oldOffset, oldOffset, current, []),
                    dat, column.Type, referencedKeys);
            }
        }

        List<CandidateScore> candidates = [];
        var best = new CandidateScore(-1, Invalid);
        for (var offset = minOffset; offset + size <= dat.RowLength; offset++)
        {
            var value = score(offset);
            if (value <= Invalid)
            {
                continue;
            }
            candidates.Add(new CandidateScore(offset, value));
            // Strictly greater: on equal scores the lowest offset wins, which keeps the column order.
            if (value > best.Score)
            {
                best = new CandidateScore(offset, value);
            }
        }

        if (best.Score < threshold)
        {
            candidates.Sort(static (a, b) => b.Score.CompareTo(a.Score));
            var top = candidates.Count > 3 ? candidates.GetRange(0, 3) : candidates;
            return new ColumnResult(column.Name, ColumnStatus.Failed, oldOffset, -1, Math.Max(best.Score, 0), top,
                $"No offset reaches {threshold:P0}.");
        }

        List<CandidateScore> alternatives = [];
        foreach (var candidate in candidates)
        {
            if (candidate.Offset != best.Offset && candidate.Score >= threshold
                && candidate.Score >= best.Score - options.AmbiguityMargin)
            {
                alternatives.Add(candidate);
            }
        }

        var status = best.Offset == oldOffset ? ColumnStatus.Unchanged : ColumnStatus.Moved;
        return CheckAllRows(new ColumnResult(column.Name, status, oldOffset, best.Offset, best.Score, alternatives),
            dat, column.Type, referencedKeys);
    }

    /// <summary>Scores only use a sample of rows: the chosen offset must also be valid on every row.</summary>
    private static ColumnResult CheckAllRows(ColumnResult result, DatFile dat, ColumnType type, ulong[]? referencedKeys)
    {
        for (var row = 0; row < dat.RowCount; row++)
        {
            if (!ColumnReader.TryRead(dat, row, result.NewOffset, type, referencedKeys, out _))
            {
                return result with
                {
                    Status = ColumnStatus.Failed,
                    Message = $"Offset {result.NewOffset} is not a valid {type} on row {row}."
                };
            }
        }
        return result;
    }

    private double ScoreKey(DatFile dat, int offset, int[] rows, HashSet<ulong> oldKeys)
    {
        int informative = 0, matches = 0;
        foreach (var row in rows)
        {
            if (!ColumnReader.TryRead(dat, row, offset, ColumnType.String, null, out var value))
            {
                return Invalid;
            }
            if (value is ColumnReader.Empty)
            {
                continue;
            }
            informative++;
            if (oldKeys.Contains(value))
            {
                matches++;
            }
        }
        return Ratio(matches, informative);
    }

    private double ScoreColumn(DatFile dat, ColumnType type, int offset, RowPair[] pairs, ulong[] oldValues, ulong[]? referencedKeys)
    {
        int informative = 0, matches = 0;
        foreach (var (oldRow, newRow) in pairs)
        {
            if (!ColumnReader.TryRead(dat, newRow, offset, type, referencedKeys, out var value))
            {
                return Invalid;
            }
            var expected = oldValues[oldRow];
            if (expected is ColumnReader.Empty)
            {
                continue;
            }
            informative++;
            if (value == expected)
            {
                matches++;
            }
        }
        return Ratio(matches, informative);
    }

    private double Ratio(int matches, int informative)
        => informative < options.MinInformativeRows ? Invalid : (double)matches / informative;

    /// <summary>
    /// Pairs of rows with the same key in both versions, at most <paramref name="maxPairs"/>.
    /// Duplicated keys are ignored: they can not identify a row.
    /// </summary>
    private static RowPair[] MatchRows(ulong[] oldKeys, ulong[] newKeys, int maxPairs)
    {
        var oldIndex = IndexUniqueKeys(oldKeys);
        var newIndex = IndexUniqueKeys(newKeys);

        var pairs = new List<RowPair>(Math.Min(maxPairs, oldKeys.Length));
        foreach (var oldRow in Sample(oldKeys.Length, maxPairs))
        {
            var key = oldKeys[oldRow];
            if (key is not ColumnReader.Empty
                && oldIndex[key] == oldRow
                && newIndex.TryGetValue(key, out var newRow) && newRow >= 0)
            {
                pairs.Add(new RowPair(oldRow, newRow));
            }
        }
        return [.. pairs];
    }

    /// <summary>Key => row, or -1 when the key appears more than once.</summary>
    private static Dictionary<ulong, int> IndexUniqueKeys(ulong[] keys)
    {
        var index = new Dictionary<ulong, int>(keys.Length);
        for (var row = 0; row < keys.Length; row++)
        {
            var key = keys[row];
            if (key is ColumnReader.Empty)
            {
                continue;
            }
            if (!index.TryAdd(key, row))
            {
                index[key] = -1;
            }
        }
        return index;
    }

    /// <summary>Evenly spread row indexes, at most <paramref name="max"/>.</summary>
    private static int[] Sample(int count, int max)
    {
        var size = Math.Min(count, max);
        var rows = new int[size];
        var step = (double)count / size;
        for (var i = 0; i < size; i++)
        {
            rows[i] = (int)(i * step);
        }
        return rows;
    }

    private static ulong[] ReadKeys(DatFile dat, int offset)
    {
        var keys = new ulong[dat.RowCount];
        for (var row = 0; row < keys.Length; row++)
        {
            ColumnReader.TryRead(dat, row, offset, ColumnType.String, null, out keys[row]);
        }
        return keys;
    }

    /// <summary>
    /// Reads the used columns of every table with the schema as is, checks every value and returns the snapshot.
    /// Used after a successful calibration, and alone to create the first snapshot or accept a manual fix.
    /// </summary>
    /// <returns>Null when an error was added to <paramref name="errors"/>.</returns>
    internal static DatSnapshot? BuildSnapshot(GameSchema schema, IReadOnlyDictionary<string, DatFile> files, List<string> errors)
    {
        var snapshot = new DatSnapshot(schema.Game);
        var keysByTable = new Dictionary<string, ulong[]>(StringComparer.Ordinal);
        var errorCount = errors.Count;

        foreach (var table in schema.Tables)
        {
            if (!files.TryGetValue(table.Name, out var dat))
            {
                errors.Add($"{table.Name}: dat file not found.");
                continue;
            }
            if (dat.RowLength != table.RowLength)
            {
                errors.Add($"{table.Name}: row length is {dat.RowLength} bytes, the schema says {table.RowLength}. "
                    + "Run the calibrate command, or fix the schema by hand.");
                continue;
            }

            var columns = ReadColumns(table, dat, keysByTable, errors);
            if (columns is null)
            {
                continue;
            }
            keysByTable.Add(table.Name, columns[table.Key]);
            snapshot.Tables.Add(table.Name, new TableSnapshot(table.Name, dat.RowCount, columns));
        }
        return errors.Count == errorCount ? snapshot : null;
    }

    private static Dictionary<string, ulong[]>? ReadColumns(TableSchema table, DatFile dat,
        Dictionary<string, ulong[]> keysByTable, List<string> errors)
    {
        var columns = new Dictionary<string, ulong[]>(table.Columns.Count, StringComparer.Ordinal);
        foreach (var column in table.Columns)
        {
            ulong[]? referencedKeys = null;
            if (column.Type is ColumnType.ForeignRow && !keysByTable.TryGetValue(column.References!, out referencedKeys))
            {
                errors.Add($"{table.Name}.{column.Name}: referenced table {column.References} has errors.");
                return null;
            }

            var values = new ulong[dat.RowCount];
            for (var row = 0; row < values.Length; row++)
            {
                if (!ColumnReader.TryRead(dat, row, column.Offset, column.Type, referencedKeys, out values[row]))
                {
                    errors.Add($"{table.Name}.{column.Name}: offset {column.Offset} is not a valid {column.Type} on row {row}.");
                    return null;
                }
            }
            columns.Add(column.Name, values);
        }
        return columns;
    }
}
