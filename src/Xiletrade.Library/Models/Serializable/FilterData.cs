using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class FilterData
{
    [DataMember(Name = "result")]
    [JsonPropertyName("result")]
    public FilterResult[] Result { get; set; } = null;
}
