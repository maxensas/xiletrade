using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class WordResultData
{
    [DataMember(Name = "name")] // Text2
    [JsonPropertyName("name")]
    public string Name { get; set; } = null;

    [DataMember(Name = "nameEn")] // Text
    [JsonPropertyName("nameEn")]
    public string NameEn { get; set; } = null;
}
