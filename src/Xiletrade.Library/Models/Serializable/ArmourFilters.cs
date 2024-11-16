using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class ArmourFilters
{
    [DataMember(Name = "ar")]
    [JsonPropertyName("ar")]
    public MinMax Armour { get; set; } = new MinMax();

    [DataMember(Name = "es")]
    [JsonPropertyName("es")]
    public MinMax Energy { get; set; } = new MinMax();

    [DataMember(Name = "ev")]
    [JsonPropertyName("ev")]
    public MinMax Evasion { get; set; } = new MinMax();

    [DataMember(Name = "ward")]
    [JsonPropertyName("ward")]
    public MinMax Ward { get; set; } = new MinMax();
}
