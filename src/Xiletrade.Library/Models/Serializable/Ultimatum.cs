using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class Ultimatum
{
    [DataMember(Name = "disabled")]
    [JsonPropertyName("disabled")]
    public bool Disabled { get; set; } = false;

    [DataMember(Name = "filters")]
    [JsonPropertyName("filters")]
    public UltimatumFilters Filters { get; set; } = new UltimatumFilters();
}
