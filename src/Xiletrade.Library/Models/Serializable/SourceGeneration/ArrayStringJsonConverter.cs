using System.Text.Json;
using System;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable.SourceGeneration;

// warning IL3050 incompatible with AOT, Converter used only for poe.prices response.
internal class ArrayStringJsonConverter : JsonConverter<object[][]>
{
    public override object[][] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return JsonSerializer.Deserialize<object[][]>(ref reader, options);
    }

    public override void Write(Utf8JsonWriter writer, object[][] value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options);
    }
}
