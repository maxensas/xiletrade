using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class NinjaItemContract
{
    [DataMember(Name = "lines")]
    [JsonPropertyName("lines")]
    public NinjaItemLines[] Lines { get; set; } = null;
}
