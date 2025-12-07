using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract.One;

public sealed class Misc
{
    [JsonPropertyName("disabled")]
    public bool Disabled { get; set; } = true;

    [JsonPropertyName("filters")]
    public MiscFilters Filters { get; set; } = new MiscFilters();
}
