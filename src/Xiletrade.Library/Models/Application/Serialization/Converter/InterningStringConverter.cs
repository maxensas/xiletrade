using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xiletrade.Library.Services;

namespace Xiletrade.Library.Models.Application.Serialization.Converter;

public sealed class InterningStringConverter : JsonConverter<string>
{
    private readonly DataManagerService _dm;

    public InterningStringConverter(DataManagerService dm)
    {
        _dm = dm ?? throw new ArgumentNullException(nameof(dm));
    }

    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string value = reader.GetString();

        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return _dm.Json.Intern(value.AsSpan());
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}