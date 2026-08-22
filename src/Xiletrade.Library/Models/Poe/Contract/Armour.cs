using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract;

public sealed class Armour
{
    [JsonPropertyName("disabled")]
    public bool Disabled { get; set; }

    [JsonPropertyName("filters")]
    public ArmourFilters Filters { get; set; } = new();
}
