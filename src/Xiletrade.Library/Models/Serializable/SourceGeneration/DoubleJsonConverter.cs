using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable.SourceGeneration;

internal class DoubleJsonConverter : JsonConverter<object>
{
    public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetDouble();
    }

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        if (double.TryParse(value.ToString(), out double valDouble))
        {
            writer.WriteNumberValue(valDouble);
            return;
        }
        if (int.TryParse(value.ToString(), out int valInt))
        {
            writer.WriteNumberValue(valInt);
            return;
        }
        writer.WriteNumberValue(0);
    }
}
