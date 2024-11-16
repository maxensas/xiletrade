using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class Misc
{
    [DataMember(Name = "disabled")]
    [JsonPropertyName("disabled")]
    public bool Disabled { get; set; } = false;

    [DataMember(Name = "filters")]
    [JsonPropertyName("filters")]
    public MiscFilters Filters { get; set; } = new MiscFilters();
}
