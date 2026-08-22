using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract.Two;

public sealed class JsonDataTwo(string market)
{
    [JsonPropertyName("query")]
    public QueryTwo Query { get; set; } = new(market);

    [JsonPropertyName("sort")]
    public Sort Sort { get; set; } = new();
}
