using System.Text.Json.Serialization;
using Xiletrade.Library.Models.Ninja.Contract.Exchange;

namespace Xiletrade.Library.Models.Ninja.Contract;

public sealed class NinjaItemDataLine
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("itemId")]
    public string ItemId { get; set; }

    [JsonPropertyName("detailsId")]
    public string DetailsId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("baseType")]
    public string BaseType { get; set; }

    [JsonPropertyName("category")]
    public string Category { get; set; }

    [JsonPropertyName("primaryValue")]
    public double PrimaryValue { get; set; }

    [JsonPropertyName("listingCount")]
    public int ListingCount { get; set; }

    [JsonPropertyName("corrupted")]
    public bool Corrupted { get; set; }

    [JsonPropertyName("sparkline")]
    public NinjaSparkLine Sparkline { get; set; }
}
