using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable.SourceGeneration;

internal class QueryTypeJsonConverter : JsonConverter<object>
{
    public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var bytes = reader.GetBytesFromBase64();
        using MemoryStream ms = new(bytes);
        GemTransfigured gem = JsonSerializer.Deserialize<GemTransfigured>(ms);
        if (gem is not null)
        {
            return gem;
        }
        return reader.GetString();
    }

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        if (value is GemTransfigured gem)
        {
            writer.WriteStartObject();
            writer.WriteString("option", gem.Option);
            writer.WriteString("discriminator", gem.Discriminator);
            writer.WriteEndObject();
            writer.Flush();
            return;
        }
        writer.WriteStringValue(value.ToString());
    }
}
