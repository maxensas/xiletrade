using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract;

[DataContract]
public sealed class FilterResultOptions
{
    [DataMember(Name = "id")]
    [JsonPropertyName("id")]
    public IntegerId ID { get; set; } // This property can be a string OR an integer.

    [DataMember(Name = "text")]
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}
