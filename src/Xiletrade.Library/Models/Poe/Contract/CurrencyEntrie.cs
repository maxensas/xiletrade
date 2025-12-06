using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract;

public sealed class CurrencyEntrie
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = null;

    [JsonPropertyName("text")]
    public string Text { get; set; } = null;

    [JsonPropertyName("image")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Img { get; set; } = null;
}
