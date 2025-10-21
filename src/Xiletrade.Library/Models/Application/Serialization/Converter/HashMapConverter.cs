using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xiletrade.Library.Models.Poe.Domain;

namespace Xiletrade.Library.Models.Application.Serialization.Converter;

public class HashMapConverter : JsonConverter<List<HashMap>>
{
    public override List<HashMap> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var list = new List<HashMap>();
        using (var doc = JsonDocument.ParseValue(ref reader))
        {
            foreach (var item in doc.RootElement.EnumerateArray())
            {
                string id = item[0].GetString();

                //var values = item[1].EnumerateArray().Select(x => x.GetInt32()).ToList();
                var values = new List<int>();
                if (item[1].ValueKind is not JsonValueKind.Null && item[1].ValueKind is JsonValueKind.Array)
                {
                    values = [.. item[1].EnumerateArray().Select(x => x.GetInt32())];
                }
                list.Add(new() { Id = id, Values = values });
            }
        }
        return list;
    }

    public override void Write(Utf8JsonWriter writer, List<HashMap> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (var stat in value)
        {
            writer.WriteStartArray();
            writer.WriteStringValue(stat.Id);

            writer.WriteStartArray();
            foreach (var val in stat.Values)
            {
                writer.WriteNumberValue(val);
            }
            writer.WriteEndArray();

            writer.WriteEndArray();
        }
        writer.WriteEndArray();
    }
}
