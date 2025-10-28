using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xiletrade.Library.Models.Poe.Contract;

namespace Xiletrade.Library.Models.Application.Serialization.Converter;

public class FlexibleStringConverter : JsonConverter<FlexibleString>
{
    public override FlexibleString Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.String => new FlexibleString() { Value = reader.GetString() },
            JsonTokenType.Number => new FlexibleString() { Value = reader.GetDecimal().ToString(CultureInfo.InvariantCulture) },
            JsonTokenType.Null => null,
            _ => throw new JsonException($"Unexpected token parsing string. Got {reader.TokenType}")
        };
    }

    public override void Write(Utf8JsonWriter writer, FlexibleString value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}
