using System.Text.Json.Serialization;
using Xiletrade.Library.Models.Poe.Contract.Two;

namespace Xiletrade.Library.Models.Poe.DTO.Two;

public sealed class MiscTwo
{
    [JsonPropertyName("disabled")]
    public bool Disabled { get; set; } = true;

    [JsonPropertyName("filters")]
    public MiscFiltersTwo Filters { get; set; } = new MiscFiltersTwo();
}
