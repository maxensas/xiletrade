using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Xiletrade.Library.Models.Serializable.SourceGeneration;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class FilterResultOptions
{
    [DataMember(Name = "id")]
    [JsonPropertyName("id")]
    [JsonConverter(typeof(IntegerJsonConverter))]
    public object ID { get; set; } = 0; // This property can be a string OR an integer.

    [DataMember(Name = "text")]
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}
