using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Application.Serialization.Converter;

internal class IntegerJsonConverter : JsonConverter<object>
{
    public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetInt32();
    }

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(Convert.ToInt32(value));
    }
}
