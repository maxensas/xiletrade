using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract.One;

public sealed class JsonData
{
    [JsonPropertyName("query")]
    public Query Query { get; set; } = new();

    [JsonPropertyName("sort")]
    public Sort Sort { get; set; } = new();
}
