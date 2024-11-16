using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class LicenceData
{
    [DataMember(Name = "licence")]
    [JsonPropertyName("licence")]
    public string Licence { get; set; } = null;
}
