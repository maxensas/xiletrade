using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable.SourceGeneration;

public class ValueTupleListConverter : JsonConverter<List<(string, int)>>
{
    public override List<(string, int)> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var result = new List<(string, int)>();

        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException("Expected start of array");

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
                break;

            if (reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException("Expected inner array");

            reader.Read();
            string item1 = reader.GetString();

            reader.Read();
            int item2 = reader.GetInt32();

            reader.Read();
            if (reader.TokenType != JsonTokenType.EndArray)
                throw new JsonException("Expected end of inner array");

            result.Add((item1, item2));
        }

        return result;
    }

    public override void Write(Utf8JsonWriter writer, List<(string, int)> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        foreach (var (item1, item2) in value)
        {
            writer.WriteStartArray();
            writer.WriteStringValue(item1);
            writer.WriteNumberValue(item2);
            writer.WriteEndArray();
        }

        writer.WriteEndArray();
    }
}
