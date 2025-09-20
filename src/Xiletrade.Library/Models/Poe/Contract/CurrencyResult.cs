using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract;

[DataContract]
public sealed class CurrencyResult
{
    [DataMember(Name = "result")]
    [JsonPropertyName("result")]
    public CurrencyResultData[] Result { get; set; } = null;
}
