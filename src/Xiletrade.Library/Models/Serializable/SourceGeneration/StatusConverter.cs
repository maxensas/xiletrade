using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Serializable.SourceGeneration;

public sealed class StatusConverter : JsonConverter<OnlineStatus>
{
    public override OnlineStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // If the token is a boolean, we return null (or handle as you want)
        if (reader.TokenType == JsonTokenType.True || reader.TokenType == JsonTokenType.False)
        {
            reader.GetBoolean(); // Consume the value
            return new() { Status = Strings.Status.Offline}; // Or throw / log / return special object
        }

        // If it's an object, deserialize as usual
        if (reader.TokenType == JsonTokenType.StartObject)
        {
            var obj = JsonSerializer.Deserialize<OnlineStatus>(ref reader, options);
            return obj.Status is null ? new() { Status = Strings.Status.Online } : obj;
        }

        throw new JsonException("Expected a JSON object or boolean value.");
    }

    //TODO
    public override void Write(Utf8JsonWriter writer, OnlineStatus value, JsonSerializerOptions options)
    {
        if (value is not null)
        {
            if (value.Status is Strings.Status.Offline)
            {
                writer.WriteBooleanValue(false);
            }
            if (value.Status is Strings.Status.Online)
            {
                writer.WriteNullValue();
            }
            JsonSerializer.Serialize(writer, value, options);
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}
