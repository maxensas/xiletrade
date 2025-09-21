using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Ninja.Contract.Two;

public sealed class NinjaCurrencyItem
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("image")]
    public string Image { get; set; }

    [JsonPropertyName("category")]
    public string Category { get; set; }
}
