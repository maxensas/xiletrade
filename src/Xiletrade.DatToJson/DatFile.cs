using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace Xiletrade.DatToJson;

/// <summary>
/// Read-only view over a .datc64 file, without any schema.
/// </summary>
/// <remarks>
/// Layout: [u32 row count][row count * fixed-size rows][variable data section].
/// The variable data section begins with an 8 bytes marker (0xBB), right after the last row.
/// Strings and arrays stored in rows are 64-bit offsets relative to the start of this section.
/// Values are read in place: nothing is copied out of the file buffer.
/// </remarks>
internal sealed class DatFile
{
    /// <summary>Row index value used by the game for a null reference.</summary>
    internal const ulong NullRow = 0xFEFEFEFEFEFEFEFE;

    internal const int StringSize = 8;
    internal const int Int32Size = 4;
    internal const int ForeignRowSize = 16;

    private const int RowCountSize = 4;
    private const int MarkerSize = 8;
    private const byte MarkerByte = 0xBB;

    private readonly byte[] _data;
    private readonly int _variableStart;

    internal int RowCount { get; }
    internal int RowLength { get; }

    private DatFile(byte[] data, int rowCount, int rowLength)
    {
        _data = data;
        RowCount = rowCount;
        RowLength = rowLength;
        _variableStart = RowCountSize + rowCount * rowLength;
    }

    /// <summary>
    /// Parses the header of a .datc64 file. The buffer is kept as is and must not be modified afterwards.
    /// </summary>
    /// <exception cref="InvalidDataException">The buffer is not a valid .datc64 file.</exception>
    internal static DatFile Parse(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);
        if (data.Length < RowCountSize + MarkerSize)
        {
            throw new InvalidDataException("File too small to be a datc64 file.");
        }

        var rowCount = BinaryPrimitives.ReadUInt32LittleEndian(data);
        if (rowCount > int.MaxValue)
        {
            throw new InvalidDataException($"Invalid row count: {rowCount}.");
        }

        var rowLength = FindRowLength(data, (int)rowCount);
        if (rowLength < 0)
        {
            throw new InvalidDataException("Variable data section not found.");
        }
        return new DatFile(data, (int)rowCount, rowLength);
    }

    /// <summary>
    /// The variable section can only begin at 4 + rowCount * rowLength.
    /// Candidate row lengths are tried from the smallest one: the first position holding the marker wins.
    /// Only (file size / row count) positions are checked, and bytes inside rows are never mistaken for the marker
    /// unless they sit exactly on such a position.
    /// </summary>
    /// <returns>The row length, or -1 when no marker is found.</returns>
    private static int FindRowLength(ReadOnlySpan<byte> data, int rowCount)
    {
        Span<byte> marker = stackalloc byte[MarkerSize];
        marker.Fill(MarkerByte);

        if (rowCount is 0)
        {
            return data.Slice(RowCountSize, MarkerSize).SequenceEqual(marker) ? 0 : -1;
        }

        var maxRowLength = (data.Length - RowCountSize - MarkerSize) / rowCount;
        for (var rowLength = 0; rowLength <= maxRowLength; rowLength++)
        {
            var position = RowCountSize + rowCount * rowLength;
            if (data.Slice(position, MarkerSize).SequenceEqual(marker))
            {
                return rowLength;
            }
        }
        return -1;
    }

    private ReadOnlySpan<byte> Variable => _data.AsSpan(_variableStart);

    internal ReadOnlySpan<byte> GetRow(int row) => _data.AsSpan(RowCountSize + row * RowLength, RowLength);

    internal int ReadInt32(int row, int offset)
        => BinaryPrimitives.ReadInt32LittleEndian(GetRow(row).Slice(offset, Int32Size));

    /// <summary>
    /// Reads a foreign row field: { u64 rowIndex; u64 unused }.
    /// A null reference is 16 bytes of 0xFE, a valid one has its second value at zero.
    /// </summary>
    /// <returns>False when the bytes do not look like a foreign row field.</returns>
    internal bool TryReadForeignRow(int row, int offset, out ulong rowIndex)
    {
        var field = GetRow(row).Slice(offset, ForeignRowSize);
        rowIndex = BinaryPrimitives.ReadUInt64LittleEndian(field);
        var unused = BinaryPrimitives.ReadUInt64LittleEndian(field[8..]);
        return unused == (rowIndex is NullRow ? NullRow : 0UL);
    }

    /// <summary>
    /// Reads a string field as UTF-16 characters, without its terminator and without allocating.
    /// </summary>
    /// <returns>False when the field does not point to a valid string.</returns>
    internal bool TryReadString(int row, int offset, out ReadOnlySpan<char> text)
    {
        var reference = BinaryPrimitives.ReadUInt64LittleEndian(GetRow(row).Slice(offset, StringSize));
        return TryReadText(reference, out text);
    }

    /// <summary>
    /// Allocates the string of a field, to be used only when the value is really needed.
    /// </summary>
    internal string ReadString(int row, int offset)
    {
        if (!TryReadString(row, offset, out var text))
        {
            throw new InvalidDataException($"Row {row}, offset {offset}: not a valid string.");
        }
        return new string(text);
    }

    /// <summary>
    /// Text is stored as UTF-16LE and ends with two null characters.
    /// The end is found with a vectorized search, then the characters are checked:
    /// random bytes almost never form well paired surrogates, so a wrong offset is rejected.
    /// </summary>
    private bool TryReadText(ulong reference, out ReadOnlySpan<char> text)
    {
        text = default;
        var variable = Variable;
        // The marker occupies the beginning of the section: no text can start there.
        if (reference < MarkerSize || reference >= (ulong)variable.Length)
        {
            return false;
        }

        // Little-endian layout, so the bytes can be seen as characters directly.
        var chars = MemoryMarshal.Cast<byte, char>(variable[(int)reference..]);
        var end = chars.IndexOf("\0\0");
        if (end < 0)
        {
            return false;
        }

        var candidate = chars[..end];
        if (!HasValidSurrogates(candidate))
        {
            return false;
        }
        text = candidate;
        return true;
    }

    private static bool HasValidSurrogates(ReadOnlySpan<char> text)
    {
        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (!char.IsSurrogate(c))
            {
                continue;
            }
            if (!char.IsHighSurrogate(c) || i + 1 >= text.Length || !char.IsLowSurrogate(text[i + 1]))
            {
                return false;
            }
            i++;
        }
        return true;
    }
}
