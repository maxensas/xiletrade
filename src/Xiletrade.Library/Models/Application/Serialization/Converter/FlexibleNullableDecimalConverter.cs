using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Application.Serialization.Converter;

public class FlexibleNullableDecimalConverter : JsonConverter<decimal?>
{
    public override decimal? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is JsonTokenType.Null)
            return null;

        if (reader.TokenType is JsonTokenType.String)
        {
            var str = reader.GetString();
            if (string.IsNullOrWhiteSpace(str))
            {
                return null;
            }
            if (decimal.TryParse(str, out var value))
            {
                return value;
            }
        }

        if (reader.TokenType is JsonTokenType.Number)
            return reader.GetDecimal();

        throw new JsonException("Unexpected JSON type for a nullable decimal");
    }

    public override void Write(Utf8JsonWriter writer, decimal? value, JsonSerializerOptions options)
    {
        if (value is null)
            writer.WriteNullValue();
        else
            writer.WriteNumberValue(value.Value);
    }
}
