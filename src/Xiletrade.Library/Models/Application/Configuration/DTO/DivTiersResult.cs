using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Application.Configuration.DTO;

[DataContract]
public sealed class DivTiersResult
{
    [DataMember(Name = "tag")]
    [JsonPropertyName("tag")]
    public string Tag { get; set; } = null;
   
    [DataMember(Name = "tier")]
    [JsonPropertyName("tier")]
    public string Tier { get; set; } = null;
}
