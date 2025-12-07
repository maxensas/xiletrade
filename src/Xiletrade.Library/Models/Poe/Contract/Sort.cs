using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract;

public sealed class Sort
{
    [JsonPropertyName("price")]
    public string Price { get; set; }
}
