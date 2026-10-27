using System.Buffers.Binary;
using System.Text;
using Xiletrade.DatToJson;

namespace Xiletrade.Test.CI.SchemaCalibration;

internal enum FieldKind
{
    String,
    Int32,
    ForeignRow,
    Padding
}

/// <summary>
/// One field of a synthetic row. Padding fields stand for the columns the generator does not read.
/// </summary>
internal sealed record TestField(string Name, FieldKind Kind, int PaddingSize = 0)
{
    internal int Size => Kind switch
    {
        FieldKind.String => DatFile.StringSize,
        FieldKind.Int32 => DatFile.Int32Size,
        FieldKind.ForeignRow => DatFile.ForeignRowSize,
        _ => PaddingSize
    };

    internal static TestField Str(string name) => new(name, FieldKind.String);
    internal static TestField Int(string name) => new(name, FieldKind.Int32);
    internal static TestField Foreign(string name) => new(name, FieldKind.ForeignRow);
    internal static TestField Pad(string name, int size) => new(name, FieldKind.Padding, size);
}

/// <summary>
/// Writes synthetic .datc64 files with the same layout as the game files.
/// </summary>
internal static class DatTestBuilder
{
    private static readonly byte[] Marker = [0xBB, 0xBB, 0xBB, 0xBB, 0xBB, 0xBB, 0xBB, 0xBB];

    /// <param name="rows">
    /// Field name => value: string, int, int for a foreign row index (missing = null reference),
    /// byte[] for a padding field (missing = zeros).
    /// </param>
    internal static byte[] Build(IReadOnlyList<TestField> fields, IReadOnlyList<IReadOnlyDictionary<string, object>> rows)
    {
        using var fixedPart = new MemoryStream();
        using var variable = new MemoryStream();
        variable.Write(Marker);
        var stringOffsets = new Dictionary<string, long>();
        Span<byte> buffer = stackalloc byte[16];

        foreach (var row in rows)
        {
            foreach (var field in fields)
            {
                row.TryGetValue(field.Name, out var value);
                switch (field.Kind)
                {
                    case FieldKind.String:
                        var text = value as string ?? string.Empty;
                        if (!stringOffsets.TryGetValue(text, out var offset))
                        {
                            offset = variable.Position;
                            variable.Write(Encoding.Unicode.GetBytes(text));
                            variable.Write(new byte[4]); // terminator
                            stringOffsets.Add(text, offset);
                        }
                        BinaryPrimitives.WriteUInt64LittleEndian(buffer, (ulong)offset);
                        fixedPart.Write(buffer[..8]);
                        break;

                    case FieldKind.Int32:
                        BinaryPrimitives.WriteInt32LittleEndian(buffer, value is int number ? number : 0);
                        fixedPart.Write(buffer[..4]);
                        break;

                    case FieldKind.ForeignRow:
                        var isNull = value is not int;
                        BinaryPrimitives.WriteUInt64LittleEndian(buffer, isNull ? DatFile.NullRow : (ulong)(int)value);
                        BinaryPrimitives.WriteUInt64LittleEndian(buffer[8..], isNull ? DatFile.NullRow : 0);
                        fixedPart.Write(buffer[..16]);
                        break;

                    default:
                        fixedPart.Write(value as byte[] ?? new byte[field.PaddingSize]);
                        break;
                }
            }
        }

        var file = new byte[4 + fixedPart.Length + variable.Length];
        BinaryPrimitives.WriteUInt32LittleEndian(file, (uint)rows.Count);
        fixedPart.ToArray().CopyTo(file, 4);
        variable.ToArray().CopyTo(file, 4 + fixedPart.Length);
        return file;
    }

    internal static DatFile Dat(IReadOnlyList<TestField> fields, IReadOnlyList<IReadOnlyDictionary<string, object>> rows)
        => DatFile.Parse(Build(fields, rows));

    internal static int OffsetOf(IReadOnlyList<TestField> fields, string name)
    {
        var offset = 0;
        foreach (var field in fields)
        {
            if (field.Name == name)
            {
                return offset;
            }
            offset += field.Size;
        }
        throw new ArgumentException($"Unknown field {name}", nameof(name));
    }

    internal static int RowLength(IReadOnlyList<TestField> fields) => fields.Sum(static f => f.Size);

    /// <summary>Schema of a synthetic table: the first column is the key.</summary>
    internal static TableSchema Table(string name, IReadOnlyList<TestField> fields, params string[] columns)
    {
        var table = new TableSchema
        {
            Name = name,
            RowLength = RowLength(fields),
            Key = columns[0]
        };
        foreach (var column in columns)
        {
            var field = fields.Single(f => f.Name == column);
            table.Columns.Add(new ColumnSchema
            {
                Name = column,
                Offset = OffsetOf(fields, column),
                Type = field.Kind switch
                {
                    FieldKind.String => ColumnType.String,
                    FieldKind.Int32 => ColumnType.Int32,
                    FieldKind.ForeignRow => ColumnType.ForeignRow,
                    _ => throw new ArgumentException($"{column} is a padding field.", nameof(columns))
                }
            });
        }
        return table;
    }
}
