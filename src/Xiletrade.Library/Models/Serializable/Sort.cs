using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class Sort
{
    [DataMember(Name = "price")]
    [JsonPropertyName("price")]
    public string Price { get; set; }
}
