using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract;

public sealed class FilterResultOptions
{
    [JsonPropertyName("id")]
    public IntegerId ID { get; set; } // This property can be a string OR an integer.

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}
