using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Application.Serialization.Converter;

public sealed class InterningStringConverter(JsonHelper helper) : JsonConverter<string>
{
    private readonly JsonHelper _helper = helper;

    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string value = reader.GetString();

        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return _helper.Intern(value.AsSpan());
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}