using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xiletrade.Library.Models.Poe.Contract;
using Xiletrade.Library.Services;

namespace Xiletrade.Library.Models.Application.Serialization.Converter;

public class ModAffixConverter : JsonConverter<List<ModAffix>>
{
    private static IServiceProvider _serviceProvider;

    public ModAffixConverter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

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
                    var context = _serviceProvider.GetRequiredService<DataManagerService>().Json.DefaultContext;
                    var mod = JsonSerializer.Deserialize(ref reader, context.ModAffix);
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
        var context = _serviceProvider.GetRequiredService<DataManagerService>().Json.DefaultContext;
        JsonSerializer.Serialize(writer, value, context.ModAffix);
    }
}
