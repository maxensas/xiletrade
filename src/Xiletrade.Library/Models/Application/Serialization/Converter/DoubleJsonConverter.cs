using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xiletrade.Library.Models.Prices.Contract;

namespace Xiletrade.Library.Models.Application.Serialization.Converter;

internal class DoubleJsonConverter : JsonConverter<ConfidenceScore>
{
    public override ConfidenceScore Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is JsonTokenType.String)
        {
            _ = double.TryParse(reader.GetString(), NumberStyles.Float
                , CultureInfo.InvariantCulture, out double score);
            return new ConfidenceScore{Score = score};
        }
        if (reader.TokenType is JsonTokenType.Number)
        {
            if (reader.TryGetDouble(out double doubleNumber))
            {
                return new ConfidenceScore { Score = doubleNumber };
            }
            if (reader.TryGetInt32(out int integerNumber))
            {
                return new ConfidenceScore { Score = integerNumber };
            }
        }
        return null;
    }

    public override void Write(Utf8JsonWriter writer, ConfidenceScore value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.Score);
    }
}
