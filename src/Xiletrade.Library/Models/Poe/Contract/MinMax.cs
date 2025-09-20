using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract;

[DataContract]
public sealed class MinMax
{
    [DataMember(Name = "min", EmitDefaultValue = false)]
    [JsonPropertyName("min")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Min { get; set; }

    [DataMember(Name = "max", EmitDefaultValue = false)]
    [JsonPropertyName("max")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Max { get; set; }

    [DataMember(Name = "weight", EmitDefaultValue = false)]
    [JsonPropertyName("weight")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Weight { get; set; }

    [DataMember(Name = "option", EmitDefaultValue = false)]
    [JsonPropertyName("option")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Option { get; set; } // was object
}
