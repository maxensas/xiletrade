using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Application.Configuration.DTO;

[DataContract(Name = "regex")]
public sealed class ConfigRegex
{
    [DataMember(Name = "id")]
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [DataMember(Name = "name")]
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [DataMember(Name = "regex")]
    [JsonPropertyName("regex")]
    public string Regex { get; set; }
}
