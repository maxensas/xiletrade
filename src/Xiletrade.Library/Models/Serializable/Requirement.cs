using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable;

public sealed class Requirement
{
    [JsonPropertyName("disabled")]
    public bool Disabled { get; set; } = true;

    [JsonPropertyName("filters")]
    public RequirementFilters Filters { get; set; } = new RequirementFilters();
}
