using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xiletrade.Library.Models.Poe.Contract;

namespace Xiletrade.Library.Models.Application.Serialization.Converter;

internal class IntegerJsonConverter : JsonConverter<IntegerId>
{
    public override IntegerId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return new() { Id = reader.GetInt32() };
    }

    public override void Write(Utf8JsonWriter writer, IntegerId value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.Id);
    }
}
