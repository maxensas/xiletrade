using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract.One;

[DataContract]
public sealed class Sockets
{
    [DataMember(Name = "min", EmitDefaultValue = false)]
    [JsonPropertyName("min")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Min { get; set; }

    [DataMember(Name = "max", EmitDefaultValue = false)]
    [JsonPropertyName("max")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Max { get; set; }

    [DataMember(Name = "r", EmitDefaultValue = false)]
    [JsonPropertyName("r")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Red { get; set; }

    [DataMember(Name = "g", EmitDefaultValue = false)]
    [JsonPropertyName("g")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Green { get; set; }

    [DataMember(Name = "b", EmitDefaultValue = false)]
    [JsonPropertyName("b")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Blue { get; set; }

    [DataMember(Name = "w", EmitDefaultValue = false)]
    [JsonPropertyName("w")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? White { get; set; }
}
