using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xiletrade.Library.Models.Poe.Contract;
using Xiletrade.Library.Models.Serialization.SourceGeneration;

namespace Xiletrade.Library.Models.Application.Serialization.Converter;

public class BuiltInSupportConverter(SourceGenerationContext context) : JsonConverter<ItemBuiltInSupport>
{
    private readonly SourceGenerationContext _context = context;

    public override ItemBuiltInSupport Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.String => new ItemBuiltInSupport { Description = reader.GetString() },
            JsonTokenType.StartObject => JsonSerializer.Deserialize(ref reader, _context.ItemBuiltInSupport),
            JsonTokenType.Null => null,
            _ => throw new JsonException($"Unexpected type : {reader.TokenType}")
        };
    }

    public override void Write(Utf8JsonWriter writer, ItemBuiltInSupport value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, _context.ItemBuiltInSupport);
    }
}
