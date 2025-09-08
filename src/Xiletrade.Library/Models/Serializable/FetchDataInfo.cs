using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class FetchDataInfo
{
    [DataMember(Name = "id")]
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [DataMember(Name = "listing")]
    [JsonPropertyName("listing")]
    public FetchDataListing Listing { get; set; } = new FetchDataListing();

    [DataMember(Name = "item")]
    [JsonPropertyName("item")]
    public ItemDataApi Item { get; set; } = new();
}
