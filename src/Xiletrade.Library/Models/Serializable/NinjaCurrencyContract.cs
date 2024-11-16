using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class NinjaCurrencyContract
{
    [DataMember(Name = "lines")]
    [JsonPropertyName("lines")]
    public NinjaCurLines[] Lines { get; set; } = null;

    [DataMember(Name = "currencyDetails")]
    [JsonPropertyName("currencyDetails")]
    public NinjaCurDetails[] Details { get; set; } = null;
}
