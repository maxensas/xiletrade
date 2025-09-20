using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract;

[DataContract]
public sealed class Stats
{
    [DataMember(Name = "type")]
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [DataMember(Name = "filters")]
    [JsonPropertyName("filters")]
    public StatsFilters[] Filters { get; set; }

    [DataMember(Name = "value")]
    [JsonPropertyName("value")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax Value { get; set; }

    [DataMember(Name = "disabled")]
    [JsonPropertyName("disabled")]
    public bool Disabled { get; set; }
}
