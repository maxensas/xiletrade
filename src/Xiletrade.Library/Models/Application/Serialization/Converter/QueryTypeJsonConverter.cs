using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xiletrade.Library.Models.Poe.Contract;
using Xiletrade.Library.Models.Poe.Contract.One;
using Xiletrade.Library.Models.Serialization.SourceGeneration;

namespace Xiletrade.Library.Models.Application.Serialization.Converter;

internal class QueryTypeJsonConverter : JsonConverter<QueryType>
{
    private readonly SourceGenerationContext _context;

    public QueryTypeJsonConverter(SourceGenerationContext context)
    {
        _context = context;
    }

    public override QueryType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var bytes = reader.GetBytesFromBase64();
        using MemoryStream ms = new(bytes);
        GemTransfigured gem = JsonSerializer.Deserialize(ms, _context.GemTransfigured);
        //GemTransfigured gem = JsonSerializer.Deserialize<GemTransfigured>(ms);
        if (gem is not null)
        {
            return new QueryType() { Type = gem };
        }
        return new() { Type = reader.GetString() };
    }

    public override void Write(Utf8JsonWriter writer, QueryType value, JsonSerializerOptions options)
    {
        if (value.Type is GemTransfigured gem)
        {
            /*
            writer.WriteStartObject();
            writer.WriteString("option", gem.Option);
            writer.WriteString("discriminator", gem.Discriminator);
            writer.WriteEndObject();
            writer.Flush();
            */
            var jsonString = JsonSerializer.Serialize(value.Type, _context.GemTransfigured);
            writer.WriteRawValue(jsonString);

            return;
        }
        writer.WriteStringValue(value.Type.ToString());
    }
}
