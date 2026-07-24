using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract;

public sealed class ItemTypeResult
{
    [JsonPropertyName("result")]
    public ItemTypeResultData[] Result { get; set; } = null;
}
