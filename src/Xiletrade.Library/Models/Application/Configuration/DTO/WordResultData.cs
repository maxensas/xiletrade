using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Application.Configuration.DTO;

[DataContract]
public sealed class WordResultData
{
    [DataMember(Name = "name")] // Text2
    [JsonPropertyName("name")]
    public string Name { get; set; } = null;

    [DataMember(Name = "name_en")] // Text
    [JsonPropertyName("name_en")]
    public string NameEn { get; set; } = null;
}
