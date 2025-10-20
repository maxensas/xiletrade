using System;
using System.Buffers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xiletrade.Library.Services;

namespace Xiletrade.Library.Models.Application.Serialization.Converter;

public class InterningStringConverter : JsonConverter<string>
{
    private readonly DataManagerService _dm;

    public InterningStringConverter(DataManagerService dm)
    {
        _dm = dm ?? throw new ArgumentNullException(nameof(dm));
    }
    
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        ReadOnlySpan<byte> utf8Bytes;

        byte[] rentedBuffer = null;

        if (reader.HasValueSequence)
        {
            int length = checked((int)reader.ValueSequence.Length);
            rentedBuffer = ArrayPool<byte>.Shared.Rent(length);

            reader.ValueSequence.CopyTo(rentedBuffer);

            utf8Bytes = rentedBuffer.AsSpan(0, length);
        }
        else
        {
            utf8Bytes = reader.ValueSpan;
        }

        int charCount = Encoding.UTF8.GetCharCount(utf8Bytes);
        char[] rentedChars = ArrayPool<char>.Shared.Rent(charCount);

        try
        {
            int actualChars = Encoding.UTF8.GetChars(utf8Bytes, rentedChars);
            ReadOnlySpan<char> charSpan = rentedChars.AsSpan(0, actualChars);
            return _dm.Json.Intern(charSpan);
        }
        catch
        {
            return string.Empty;
        }
        finally
        {
            ArrayPool<char>.Shared.Return(rentedChars);
            if (rentedBuffer is not null)
                ArrayPool<byte>.Shared.Return(rentedBuffer);
        }
    }
    /*
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string value = reader.GetString();

        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return _dm.Intern(value.AsSpan());
    }
    */
    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}