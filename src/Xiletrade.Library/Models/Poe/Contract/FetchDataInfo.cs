using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract;

public sealed class FetchDataInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("listing")]
    public FetchDataListing Listing { get; set; } = new FetchDataListing();

    [JsonPropertyName("item")]
    public ItemDataApi Item { get; set; } = new();
}
