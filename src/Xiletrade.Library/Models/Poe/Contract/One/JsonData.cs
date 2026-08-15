using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract.One;

public sealed class JsonData(string market)
{
    [JsonPropertyName("query")]
    public Query Query { get; set; } = new(market);

    [JsonPropertyName("sort")]
    public Sort Sort { get; set; } = new();
}
