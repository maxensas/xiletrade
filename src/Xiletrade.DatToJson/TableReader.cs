namespace Xiletrade.DatToJson;

/// <summary>
/// Reads the columns of one dat file through its minimal schema, by column name.
/// </summary>
internal sealed class TableReader
{
    private readonly DatFile _dat;
    private readonly TableSchema _table;
    private readonly string _path;

    /// <exception cref="InvalidDataException">The file does not have the row length the schema was calibrated on.</exception>
    internal TableReader(DatFile dat, TableSchema table, string path)
    {
        if (dat.RowLength != table.RowLength)
        {
            throw new InvalidDataException($"{path}: row length is {dat.RowLength} bytes, the schema says {table.RowLength}. "
                + "Run the calibrate command first.");
        }
        _dat = dat;
        _table = table;
        _path = path;
    }

    internal int RowCount => _dat.RowCount;

    /// <summary>Offset of a column, to be resolved once before reading rows.</summary>
    internal int Offset(string column)
        => _table.FindColumn(column)?.Offset
            ?? throw new InvalidDataException($"Column {column} is missing in the schema of {_table.Name}.");

    /// <summary>Text of a string field, without allocating.</summary>
    internal ReadOnlySpan<char> Text(int row, int offset)
        => _dat.TryReadString(row, offset, out var text)
            ? text
            : throw new InvalidDataException($"{_path}: row {row}, offset {offset} is not a valid string.");

    /// <summary>
    /// Allocated string of a field. Carriage returns are removed, as the historical CSV based generator did.
    /// </summary>
    internal string String(int row, int offset)
    {
        var text = Text(row, offset);
        if (text.IsEmpty)
        {
            return string.Empty;
        }
        return text.Contains('\r')
            ? new string(text).Replace("\r", string.Empty, StringComparison.Ordinal)
            : new string(text);
    }

    internal int Int32(int row, int offset) => _dat.ReadInt32(row, offset);

    /// <summary>Referenced row index, or -1 for a null reference.</summary>
    internal int ForeignRow(int row, int offset)
    {
        if (!_dat.TryReadForeignRow(row, offset, out var index))
        {
            throw new InvalidDataException($"{_path}: row {row}, offset {offset} is not a valid foreign row.");
        }
        return index is DatFile.NullRow || index > int.MaxValue ? -1 : (int)index;
    }
}
