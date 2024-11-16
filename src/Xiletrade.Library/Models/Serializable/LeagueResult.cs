using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class LeagueResult
{
    [DataMember(Name = "id")]
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [DataMember(Name = "text")]
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}
