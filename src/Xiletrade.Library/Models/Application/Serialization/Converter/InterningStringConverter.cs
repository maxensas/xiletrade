using System;
using System.Buffers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Application.Serialization.Converter;

/// <summary>
/// Reads the json strings through the cache, from their UTF-8 bytes : a known value is neither decoded nor allocated.
/// </summary>
public sealed class InterningStringConverter(JsonHelper helper) : JsonConverter<string>
{
    private const int StackLimit = 256;

    private readonly JsonHelper _helper = helper;

    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Most values : the bytes of the json are the value itself (no escape), nothing is copied.
        if (!reader.HasValueSequence && !reader.ValueIsEscaped)
        {
            return _helper.Intern(reader.ValueSpan);
        }

        // Escaped value (\", \u00e9...) : unescaped by the reader into a buffer, the UTF-8 result is never longer than the source.
        int maxLength = reader.HasValueSequence ? checked((int)reader.ValueSequence.Length) : reader.ValueSpan.Length;
        byte[] rented = null;
        Span<byte> buffer = maxLength <= StackLimit ? stackalloc byte[StackLimit] : (rented = ArrayPool<byte>.Shared.Rent(maxLength));
        try
        {
            int length = reader.CopyString(buffer);
            return _helper.Intern(buffer[..length]);
        }
        finally
        {
            if (rented is not null)
            {
                ArrayPool<byte>.Shared.Return(rented);
            }
        }
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}
