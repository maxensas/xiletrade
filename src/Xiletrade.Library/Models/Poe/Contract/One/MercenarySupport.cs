using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract.One;

public sealed class MercenarySupport
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("tier")]
    public int Tier { get; set; }
}
