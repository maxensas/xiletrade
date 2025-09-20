using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract.Two;

public sealed class MiscTwo
{
    [JsonPropertyName("disabled")]
    public bool Disabled { get; set; } = true;

    [JsonPropertyName("filters")]
    public MiscFiltersTwo Filters { get; set; } = new MiscFiltersTwo();
}
