using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract.One;

public sealed class Weapon
{
    [JsonPropertyName("disabled")]
    public bool Disabled { get; set; } = true;

    [JsonPropertyName("filters")]
    public WeaponFilters Filters { get; set; } = new WeaponFilters();
}
