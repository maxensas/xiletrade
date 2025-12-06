using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract;

public sealed class FilterData
{
    [JsonPropertyName("result")]
    public FilterResult[] Result { get; set; } = null;
}
