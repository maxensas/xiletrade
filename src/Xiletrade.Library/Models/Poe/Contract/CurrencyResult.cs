using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract;

public sealed class CurrencyResult
{
    [JsonPropertyName("result")]
    public CurrencyResultData[] Result { get; set; } = null;
}
