using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class LeagueData
{
    [DataMember(Name = "result")]
    [JsonPropertyName("result")]
    public LeagueResult[] Result { get; set; } = null;
}
