using System.Text.Json;
using System.Text.Json.Serialization;

namespace Xiletrade.DatToJson;

/// <summary>
/// Types of the columns read for Xiletrade. Only what is needed is supported.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<ColumnType>))]
internal enum ColumnType
{
    String,
    Int32,
    ForeignRow
}

internal static class ColumnTypeExtensions
{
    internal static int Size(this ColumnType type) => type switch
    {
        ColumnType.String => DatFile.StringSize,
        ColumnType.Int32 => DatFile.Int32Size,
        ColumnType.ForeignRow => DatFile.ForeignRowSize,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };
}

/// <summary>
/// Minimal schema of one game: only the columns used by Xiletrade, with their byte offset in a row.
/// </summary>
internal sealed class GameSchema
{
    /// <summary>1 for PoE 1, 2 for PoE 2.</summary>
    public int Game { get; set; }

    /// <summary>Tables in calibration order: a referenced table must come before the tables referencing it.</summary>
    public List<TableSchema> Tables { get; set; } = [];

    internal GameSchema Clone() => new()
    {
        Game = Game,
        Tables = Tables.ConvertAll(static t => t.Clone())
    };
}

internal sealed class TableSchema
{
    /// <summary>Lower case dat name, without extension (e.g. "mods").</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Row length of the game version the schema was calibrated on. Informative only.</summary>
    public int RowLength { get; set; }

    /// <summary>
    /// Name of the column whose values identify a row across game versions (e.g. "Id").
    /// It must be the first column.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>Used columns, ordered by offset.</summary>
    public List<ColumnSchema> Columns { get; set; } = [];

    internal ColumnSchema? FindColumn(string name)
    {
        foreach (var column in Columns)
        {
            if (column.Name == name)
            {
                return column;
            }
        }
        return null;
    }

    internal TableSchema Clone() => new()
    {
        Name = Name,
        RowLength = RowLength,
        Key = Key,
        Columns = Columns.ConvertAll(static c => c.Clone())
    };
}

internal sealed class ColumnSchema
{
    public string Name { get; set; } = string.Empty;
    public ColumnType Type { get; set; }
    public int Offset { get; set; }

    /// <summary>Referenced table name, only for <see cref="ColumnType.ForeignRow"/>.</summary>
    public string? References { get; set; }

    internal int End => Offset + Type.Size();

    internal ColumnSchema Clone() => new()
    {
        Name = Name,
        Type = Type,
        Offset = Offset,
        References = References
    };
}

/// <summary>
/// Source generated serialization: no reflection, compatible with trimming and native AOT.
/// </summary>
[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(GameSchema))]
internal sealed partial class SchemaJsonContext : JsonSerializerContext
{
}

internal static class SchemaFile
{
    internal static GameSchema Load(string path)
    {
        using var stream = File.OpenRead(path);
        var schema = JsonSerializer.Deserialize(stream, SchemaJsonContext.Default.GameSchema)
            ?? throw new InvalidDataException($"Empty schema file: {path}");
        var errors = Validate(schema);
        if (errors.Count > 0)
        {
            throw new InvalidDataException($"Invalid schema {path}:{Environment.NewLine}" + string.Join(Environment.NewLine, errors));
        }
        return schema;
    }

    internal static void Save(GameSchema schema, string path)
    {
        using var stream = File.Create(path);
        JsonSerializer.Serialize(stream, schema, SchemaJsonContext.Default.GameSchema);
    }

    /// <summary>
    /// Checks the rules the calibrator relies on.
    /// </summary>
    internal static List<string> Validate(GameSchema schema)
    {
        List<string> errors = [];
        HashSet<string> previousTables = new(StringComparer.Ordinal);

        foreach (var table in schema.Tables)
        {
            if (table.Columns.Count is 0)
            {
                errors.Add($"{table.Name}: no column.");
            }
            else if (table.Columns[0].Name != table.Key)
            {
                errors.Add($"{table.Name}: the key column '{table.Key}' must be the first column.");
            }
            else if (table.Columns[0].Type is not ColumnType.String)
            {
                errors.Add($"{table.Name}: the key column must be a string.");
            }

            var previousEnd = 0;
            foreach (var column in table.Columns)
            {
                if (column.Offset < previousEnd)
                {
                    errors.Add($"{table.Name}.{column.Name}: columns must be ordered by offset and must not overlap.");
                }
                if (table.RowLength > 0 && column.End > table.RowLength)
                {
                    errors.Add($"{table.Name}.{column.Name}: column ends after the row length.");
                }
                if (column.Type is ColumnType.ForeignRow
                    && (column.References is null || !previousTables.Contains(column.References)))
                {
                    errors.Add($"{table.Name}.{column.Name}: a foreign row must reference a table declared before.");
                }
                previousEnd = column.End;
            }
            previousTables.Add(table.Name);
        }
        return errors;
    }
}
