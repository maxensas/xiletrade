using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract;

public sealed class Equipment
{
    [JsonPropertyName("disabled")]
    public bool Disabled { get; set; } = false;

    [JsonPropertyName("filters")]
    public EquipmentFilters Filters { get; set; } = new EquipmentFilters();
}
