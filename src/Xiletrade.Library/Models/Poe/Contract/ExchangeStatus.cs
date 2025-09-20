using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract;

[DataContract]
public sealed class ExchangeStatus
{
    [DataMember(Name = "option")]
    [JsonPropertyName("option")]
    public string Option { get; set; } = "online";
}
