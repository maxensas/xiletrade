using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xiletrade.Library.Models.Poe.Contract;
using Xiletrade.Library.Models.Serialization.SourceGeneration;

namespace Xiletrade.Library.Models.Application.Serialization.Converter;

public class ModAffixConverter(SourceGenerationContext context) : JsonConverter<List<ModAffix>>
{
    private readonly SourceGenerationContext _context = context;

    public override List<ModAffix> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is not JsonTokenType.StartArray)
            throw new JsonException();

        var result = new List<ModAffix>();

        while (reader.Read())
        {
            if (reader.TokenType is JsonTokenType.EndArray)
                return result;

            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    result.Add(new ModAffix{ Text = reader.GetString()});
                    break;

                case JsonTokenType.StartObject:
                    var mod = JsonSerializer.Deserialize(ref reader, _context.ModAffix);
                    result.Add(mod);
                    break;

                default:
                    throw new JsonException($"Unexpected type : {reader.TokenType}");
            }
        }

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, List<ModAffix> value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, _context.ModAffix);
    }
}
