using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xiletrade.Library.Models.Poe.Contract;

namespace Xiletrade.Library.Models.Application.Serialization.Converter;

public class ItemExtendedOrEmptyArrayConverter : JsonConverter<ItemExtended>
{
    public override ItemExtended Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Case 1: Empty array []
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            reader.Read(); // Move past [
            if (reader.TokenType != JsonTokenType.EndArray)
            {
                throw new JsonException("Expected empty array, but array was not empty.");
            }
            return null; // Interpret empty array as null
        }

        // Case 2: JSON object
        if (reader.TokenType == JsonTokenType.StartObject)
        {
            return JsonSerializer.Deserialize<ItemExtended>(ref reader, options);
        }

        throw new JsonException("Expected JSON object or empty array.");
    }

    public override void Write(Utf8JsonWriter writer, ItemExtended value, JsonSerializerOptions options)
    {
        if (value != null)
        {
            JsonSerializer.Serialize(writer, value, options);
        }
        else
        {
            // Serialize null as empty array
            writer.WriteStartArray();
            writer.WriteEndArray();
        }
    }
}
