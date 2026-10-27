using System.Text;

namespace Xiletrade.DatToJson;

/// <summary>
/// Fingerprints of the used columns of one table, taken on the game version the schema was calibrated on.
/// </summary>
internal sealed class TableSnapshot(string name, int rowCount, Dictionary<string, ulong[]> columns)
{
    internal string Name { get; } = name;
    internal int RowCount { get; } = rowCount;

    /// <summary>Column name => one fingerprint per row (see <see cref="ColumnReader"/>).</summary>
    internal IReadOnlyDictionary<string, ulong[]> Columns { get; } = columns;
}

/// <summary>
/// Reference data used to find the columns again after a game update.
/// Stored as a small binary file next to the schema: values are hashes, so the file contains no game text.
/// </summary>
internal sealed class DatSnapshot
{
    private const uint Magic = 0x4E534458; // "XDSN"
    private const ushort Version = 1;

    internal int Game { get; }
    internal Dictionary<string, TableSnapshot> Tables { get; } = new(StringComparer.Ordinal);

    internal DatSnapshot(int game)
    {
        Game = game;
    }

    internal static DatSnapshot Load(string path)
    {
        using var stream = File.OpenRead(path);
        using var reader = new BinaryReader(stream, Encoding.UTF8);

        if (reader.ReadUInt32() != Magic)
        {
            throw new InvalidDataException($"Not a schema snapshot: {path}");
        }
        var version = reader.ReadUInt16();
        if (version != Version)
        {
            throw new InvalidDataException($"Unsupported snapshot version {version}: {path}");
        }

        var snapshot = new DatSnapshot(reader.ReadInt32());
        var tableCount = reader.ReadInt32();
        for (var t = 0; t < tableCount; t++)
        {
            var name = reader.ReadString();
            var rowCount = reader.ReadInt32();
            var columnCount = reader.ReadInt32();
            var columns = new Dictionary<string, ulong[]>(columnCount, StringComparer.Ordinal);
            for (var c = 0; c < columnCount; c++)
            {
                var columnName = reader.ReadString();
                var values = new ulong[rowCount];
                for (var r = 0; r < values.Length; r++)
                {
                    values[r] = reader.ReadUInt64();
                }
                columns.Add(columnName, values);
            }
            snapshot.Tables.Add(name, new TableSnapshot(name, rowCount, columns));
        }
        return snapshot;
    }

    internal void Save(string path)
    {
        using var stream = File.Create(path);
        using var writer = new BinaryWriter(stream, Encoding.UTF8);

        writer.Write(Magic);
        writer.Write(Version);
        writer.Write(Game);
        writer.Write(Tables.Count);
        foreach (var table in Tables.Values)
        {
            writer.Write(table.Name);
            writer.Write(table.RowCount);
            writer.Write(table.Columns.Count);
            foreach (var (columnName, values) in table.Columns)
            {
                writer.Write(columnName);
                foreach (var value in values)
                {
                    writer.Write(value);
                }
            }
        }
    }
}
